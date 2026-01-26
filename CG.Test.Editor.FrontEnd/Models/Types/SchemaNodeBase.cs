using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.FrontEnd.Models.Types
{
	public abstract class SchemaNodeBase(SchemaNodeBase? parent)
    {
        private LinkedSchemaTypeBase? _resolvedType = null;

        public SchemaNodeBase? Parent { get; } = parent;

		public bool TryResolve(IReadOnlyDictionary<string, SchemaNodeBase> nodeMap, Dictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
        {
            if (_resolvedType is not null)
            {
                type = _resolvedType;
                return true;
            }

            if (OnTryResolve(nodeMap, definedTypes, out type))
            {
                _resolvedType = type;
                return true;
            }

            type = null;
            return false;
        }

		protected abstract bool OnTryResolve(IReadOnlyDictionary<string, SchemaNodeBase> nodeMap, Dictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type);
    }

	public class SchemaDefinedNode(SchemaNodeBase? parent, LinkedSchemaTypeBase type) : SchemaNodeBase(parent)
	{
        public LinkedSchemaTypeBase Type { get; } = type;

		protected override bool OnTryResolve(IReadOnlyDictionary<string, SchemaNodeBase> nodeMap, Dictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
        {
            type = Type;
            return true;
        }
    }

	public class SchemaTypeReferenceNode(SchemaNodeBase? parent, string typeName) : SchemaNodeBase(parent)
    {
        public string TypeName { get; } = typeName;

		protected override bool OnTryResolve(IReadOnlyDictionary<string, SchemaNodeBase> nodeMap, Dictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
        {
			if (definedTypes.TryGetValue(TypeName, out type))
			{
				return true;
			}

            type = new LinkedSchemaSymbolType(TypeName, definedTypes);
            return true;
		}
    }

    public class SchemaObjectNode(SchemaNodeBase? parent, string name) : SchemaNodeBase(parent)
    {
        public string Name { get; } = name;

        public Dictionary<string, SchemaNodeBase> PropertyNodes { get; } = [];

		protected override bool OnTryResolve(IReadOnlyDictionary<string, SchemaNodeBase> nodeMap, Dictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
        {
            if (definedTypes.TryGetValue(Name, out type))
            {
                return true;
            }

            var schemaProperties = new List<LinkedSchemaProperty>(PropertyNodes.Count);
            foreach (var (propertyName, propertyNode) in PropertyNodes)
            {
                if (propertyNode.TryResolve(nodeMap, definedTypes, out var propertyType))
                {
                    schemaProperties.Add(new LinkedSchemaProperty()
                    {
                        Index = schemaProperties.Count,
                        Name  = propertyName,
                        Type  = propertyType,
                    });
                }
                else
                {
                    type = null;
                    return false;
                }
            }

            type = new LinkedSchemaObjectType(Name, schemaProperties);
            definedTypes.TryAdd(Name, type);
            return true;
        }
    }

    public class SchemaVariantNode(SchemaNodeBase? parent, string name) : SchemaNodeBase(parent)
    {
        public string Name { get; } = name;

        public List<SchemaNodeBase> PossibleTypeNodes { get; } = [];

		protected override bool OnTryResolve(IReadOnlyDictionary<string, SchemaNodeBase> nodeMap, Dictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
        {
			if (definedTypes.TryGetValue(Name, out type))
			{
				return true;
			}

			var possibleTypes = new List<LinkedSchemaTypeBase>();
            foreach (var possibleTypeNode in PossibleTypeNodes)
            {
				if (possibleTypeNode.TryResolve(nodeMap, definedTypes, out var possibleType))
                {
                    possibleTypes.Add(possibleType);
                }
                else
                {
                    type = null;
                    return false;
                }
			}

            type = new LinkedSchemaVariantType(Name, possibleTypes);
			definedTypes.TryAdd(Name, type);
			return true;
        }
    }

	public class SchemaArrayNode(SchemaNodeBase? parent, SchemaNodeBase elementTypeNode) : SchemaNodeBase(parent)
	{
        public SchemaNodeBase ElementTypeNode { get; } = elementTypeNode;

		protected override bool OnTryResolve(IReadOnlyDictionary<string, SchemaNodeBase> nodeMap, Dictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
        {
            if (ElementTypeNode.TryResolve(nodeMap, definedTypes, out var elementType))
            {
                type = new LinkedSchemaArrayType(elementType);
                return true;
            }
            type = null;
            return false;
		}
	}

	public class SchemaReferenceNode(SchemaNodeBase? parent, SchemaNodeBase targetTypeNode) : SchemaNodeBase(parent)
	{
		public SchemaNodeBase TargetTypeNode { get; } = targetTypeNode;

		protected override bool OnTryResolve(IReadOnlyDictionary<string, SchemaNodeBase> nodeMap, Dictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
        {
			if (TargetTypeNode.TryResolve(nodeMap, definedTypes, out var targetType))
			{
				type = new LinkedSchemaReferenceType(targetType);
				return true;
			}
			type = null;
			return false;
		}
	}

}
