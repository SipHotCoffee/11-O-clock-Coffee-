using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DependencyPropertyToolkit
{
	public class ClassInfo
	{
		public string ClassName { get; set; }
		public string NamespaceName { get; set; }

		public List<IPropertySymbol> Properties { get; set; }
	}

    [Generator]
    public class DependencyPropertyGenerator : IIncrementalGenerator
    {
		private static readonly DiagnosticDescriptor PropertyMustBePartial =
			new DiagnosticDescriptor(
				id: "DPGEN001",
				title: "Property must be declared as partial",
				messageFormat: "The property '{0}' must be declared as 'partial' for dependency property generation",
				category: "DependencyPropertyGenerator",
				DiagnosticSeverity.Error,
				isEnabledByDefault: true);

		private static readonly DiagnosticDescriptor DefaultValueTypeMustMatchPropertyType =
			new DiagnosticDescriptor(
				id: "DPGEN002",
				title: "Attribute default value type, must match the property type",
				messageFormat: "Attribute default value type '{0}', must match property type '{1}'",
				category: "DependencyPropertyGenerator",
				DiagnosticSeverity.Error,
				isEnabledByDefault: true);

		private static bool InheritsFrom(INamedTypeSymbol type, string baseTypeName)
		{
			var current = type;
			while (current != null)
			{
				if (current.ToDisplayString() == baseTypeName)
				{
					return true;
				}
				current = current.BaseType;
			}
			return false;
		}

		public static string NormalizeType(ITypeSymbol type)
		{
			var nonAnnotated = type.WithNullableAnnotation(NullableAnnotation.None);

			var format = SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
				SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
				SymbolDisplayMiscellaneousOptions.UseSpecialTypes
			);

			return nonAnnotated.ToDisplayString(format);
		}


		private static string GetNamespace(ClassDeclarationSyntax classDeclaration)
		{
			// Walk up the syntax tree until we find a namespace declaration
			var parent = classDeclaration.Parent;
			while (parent != null)
			{
                switch (parent)
                {
                    case NamespaceDeclarationSyntax namespaceDeclaration:
                        return namespaceDeclaration.Name.ToString();

                    case FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclaration:
                        return fileScopedNamespaceDeclaration.Name.ToString();
                }
                parent = parent.Parent;
            }

            // No namespace found → class is in global namespace
            return null;
        }


		private bool IsStubProperty(IPropertySymbol property)
		{
			foreach (var syntaxRef in property.DeclaringSyntaxReferences)
			{
				if (syntaxRef.GetSyntax() is PropertyDeclarationSyntax propDecl)
				{
					return propDecl.AccessorList == null;
				}
			}
			return false;
		}

		private string ConvertToLiteral(object value)
		{
			if (value is string stringValue)
			{
				return $"\"{stringValue}\"";
			}
			else if (value is bool booleanValue)
			{
				return booleanValue ? "true" : "false";
			}
			else if (value is char charValue)
			{
				return $"'{charValue}'";
			}
			else
			{
				return value.ToString();
			}
		}

		private string GenerateStringFromProperty(SourceProductionContext sourceProductionContext, ClassInfo classInfo, IPropertySymbol property)
		{
			var foundAttribute = property.GetAttributes().First((attribute) => attribute.AttributeClass.ToDisplayString() == "DependencyPropertyToolkit.DependencyPropertyAttribute");

			var triggerFunctionName = $"Trigger{property.Name}Changed";

			var dependencyPropertyCode = $@"public static readonly System.Windows.DependencyProperty {property.Name}Property = System.Windows.DependencyProperty.Register(""{property.Name}"", typeof({NormalizeType(property.Type)}), typeof({classInfo.ClassName}), new System.Windows.PropertyMetadata({triggerFunctionName}));";

			var defaultValueArgument = foundAttribute.ConstructorArguments[0];
			if (defaultValueArgument.Value != null)
			{
				if (!defaultValueArgument.Type.Equals(property.Type, SymbolEqualityComparer.Default))
				{
					sourceProductionContext.ReportDiagnostic(Diagnostic.Create(DefaultValueTypeMustMatchPropertyType, property.Locations.FirstOrDefault(), foundAttribute.ConstructorArguments[0].Type, property.Type));
				}
				else
				{
					dependencyPropertyCode = $@"public static readonly System.Windows.DependencyProperty {property.Name}Property = System.Windows.DependencyProperty.Register(""{property.Name}"", typeof({NormalizeType(property.Type)}), typeof({classInfo.ClassName}), new System.Windows.PropertyMetadata({ConvertToLiteral(defaultValueArgument.Value)}, {triggerFunctionName}));";
				}
			}

			var result = $@"
		{dependencyPropertyCode}

		private static void {triggerFunctionName}(System.Windows.DependencyObject dependencyObject, System.Windows.DependencyPropertyChangedEventArgs e)
		{{
			(({classInfo.ClassName})dependencyObject).On{property.Name}Changed(({property.Type})e.OldValue, ({property.Type})e.NewValue);
		}}

		partial void On{property.Name}Changed({property.Type} oldValue, {property.Type} newValue);

		public partial {property.Type} {property.Name} 
		{{
			get => ({property.Type})GetValue({property.Name}Property);
			set => SetValue({property.Name}Property, value);
		}}";

			return result;
		}

		private string GenerateClassCode(SourceProductionContext sourceProductionContext, ClassInfo info)
		{
			return $@"
namespace {info.NamespaceName}
{{
	public partial class {info.ClassName}
	{{
		{string.Join("\n", info.Properties.Select((property) => GenerateStringFromProperty(sourceProductionContext, info, property)))}
	}}
}}
";
		}

        public void Initialize(IncrementalGeneratorInitializationContext generatorContext)
        {
			var candidates = generatorContext.SyntaxProvider.CreateSyntaxProvider(
				predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.Modifiers.Any(modifier => modifier.Text == "partial"),
				transform: (context, _) =>
				{
					var classDeclaration = (ClassDeclarationSyntax)context.Node;
					var model = context.SemanticModel;
					var symbol = model.GetDeclaredSymbol(classDeclaration);

					if (symbol is INamedTypeSymbol typeSymbol)
					{
						if (InheritsFrom(typeSymbol, "System.Windows.DependencyObject"))
						{
							return new ClassInfo()
							{
								ClassName = symbol.Name,
								NamespaceName = GetNamespace(classDeclaration),
								Properties = typeSymbol.GetMembers().OfType<IPropertySymbol>()
																	.Where((property) 
																		=> property.GetAttributes().Any((data) => data.AttributeClass.ToDisplayString() == "DependencyPropertyToolkit.DependencyPropertyAttribute")).ToList()
							};
						}
					}

					return null;
				}).Where((classInfo) => classInfo != null).Collect();

			// Emit generated partials
			generatorContext.RegisterSourceOutput(candidates, (sourceProductionContext, infos) =>
			{
				foreach (var info in infos.GroupBy(i => $"{i.NamespaceName}.{i.ClassName}").Select(g => g.First()))
				{
					var source = GenerateClassCode(sourceProductionContext, info);
					sourceProductionContext.AddSource($"{info.NamespaceName}.{info.ClassName}.Generator.cs", SourceText.From(source, Encoding.UTF8));
				}
			});
		}
    }

}
