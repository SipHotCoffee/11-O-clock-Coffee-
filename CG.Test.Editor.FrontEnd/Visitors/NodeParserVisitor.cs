using CG.Test.Editor.FrontEnd.Models;
using CG.Test.Editor.FrontEnd.ViewModels;
using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Visitors
{
	public class NodeParsingMessage(string message, IEnumerable<object> path)
	{
		public string Message { get; } = message;

		public IEnumerable<object> Path { get; } = path;
	}

    public class NodeParserVisitor(FileInstanceViewModel editor, IEnumerable<object> currentPath, NodeViewModelBase? parent, ILogger<NodeParsingMessage> logger, JsonNode? sourceNode) : Visitor<NodeParserVisitor, SchemaTypeBase, NodeViewModelBase?>
    {
        private readonly FileInstanceViewModel _editor = editor;

        private readonly ILogger<NodeParsingMessage> _logger = logger;

        public NodeViewModelBase? Parent { get; set; } = parent;

		public IEnumerable<object> CurrentPath { get; set; } = currentPath;

		public JsonNode? SourceNode { get; set; } = sourceNode;

		private void LogMessage(string message)
		{
			_logger.Log(new NodeParsingMessage(message, CurrentPath));
		}

        public NumberNodeViewModel? Visit(SchemaNumberType numberType)
        {
			if (SourceNode is JsonValue valueNode)
			{
				if (valueNode.TryGetValue<double>(out var value))
				{
					if (value >= numberType.Minimum && value <= numberType.Maximum)
					{
						return new NumberNodeViewModel(_editor, Parent, numberType, value);
					}
					else
					{
						LogMessage($"Value must be between '{numberType.Minimum}' and '{numberType.Maximum}'");
					}
				}
				else
				{
					LogMessage($"Failed to convert value of '{valueNode.GetType()}' to '{typeof(double)}'.");
				}
			}
			else
			{
				LogMessage($"Expecting node of type '{typeof(JsonValue)}' and the value must be convertible to type '{typeof(double)}'.");
			}
            return null;
		}

		public IntegerNodeViewModel? Visit(SchemaIntegerType integerType)
		{
			if (SourceNode is JsonValue valueNode)
			{
				if (valueNode.TryGetValue<long>(out var value))
				{
					return new IntegerNodeViewModel(_editor, Parent, integerType, value);
				}
				else
				{
					LogMessage($"Failed to convert value of '{valueNode.GetType()}' to '{typeof(long)}'.");
				}
			}
			else
			{
				LogMessage($"Expecting node of type '{typeof(JsonValue)}' and the value must be convertible to type '{typeof(long)}'.");
			}
			return null;
		}

		public EnumNodeViewModel? Visit(SchemaEnumType enumType)
		{
			if (SourceNode is JsonValue valueNode)
			{
				if (valueNode.TryGetValue<string>(out var value))
				{
					if (enumType.TryFindIndex(value, out var index))
					{
						return new EnumNodeViewModel(_editor, Parent, enumType, index);
					}
					else
					{
						LogMessage($"No enum member is called '{value}'. Accepted values: [{string.Join(", ", enumType.PossibleValues)}]");
					}
				}
				else
				{
					LogMessage($"Failed to convert value of '{valueNode.GetType()}' to '{typeof(bool)}'.");
				}
			}
			else
			{
				LogMessage($"Expecting node of type '{typeof(JsonValue)}' and the value must be convertible to type '{typeof(bool)}'.");
			}
			return null;
		}

		public BooleanNodeViewModel? Visit(SchemaBooleanType booleanType)
		{
			if (SourceNode is JsonValue valueNode)
			{
				if (valueNode.TryGetValue<bool>(out var value))
				{
					return new BooleanNodeViewModel(_editor, Parent, booleanType, value);
				}
				else
				{
					LogMessage($"Failed to convert value of '{valueNode.GetType()}' to '{typeof(bool)}'.");
				}
			}
			else
			{
				LogMessage($"Expecting node of type '{typeof(JsonValue)}' and the value must be convertible to type '{typeof(bool)}'.");
			}
			return null;
		}

		public StringNodeViewModel? Visit(SchemaStringType stringType)
		{
			if (SourceNode is JsonValue valueNode)
			{
				if (valueNode.TryGetValue<string>(out var value))
				{
					return new StringNodeViewModel(_editor, Parent, stringType, value);
				}
				else
				{
					LogMessage($"Failed to convert value of '{valueNode.GetType()}' to '{typeof(string)}'.");
				}
			}
			else
			{
				LogMessage($"Expecting node of type '{typeof(JsonValue)}' and the value must be convertible to type '{typeof(string)}'.");
			}
			return null;
		}

        public ArrayNodeViewModel? Visit(SchemaArrayType arrayType)
        {
			if (SourceNode is JsonArray arrayNode)
			{
				var result = new ArrayNodeViewModel(_editor, Parent, arrayType);
				var elementVisitor = new NodeParserVisitor(_editor, CurrentPath, result, _logger, null);
                for (var i = 0; i < arrayNode.Count; i++)
				{
                    var node = arrayNode[i];
                    elementVisitor.SourceNode = node;
					elementVisitor.CurrentPath = CurrentPath.Append(i);
					var nodeViewModel = arrayType.ElementType.Visit(elementVisitor);

					if (nodeViewModel is not null)
					{
						result.Elements.Add(nodeViewModel);
					}
				}
				return result;
			}
			else
			{
				LogMessage($"Expecting node of type '{typeof(JsonArray)}'.");
			}
			return null;
		}

		public ObjectNodeViewModel? Visit(SchemaObjectType objectType)
		{
			if (SourceNode is JsonObject objectNode)
			{
				var result = new ObjectNodeViewModel(_editor, Parent, objectType);
				var nodeVisitor = new NodeParserVisitor(_editor, CurrentPath, result, _logger, null);
				foreach (var property in objectType.Properties)
				{
					if (objectNode.TryGetPropertyValue(property.Name, out var node))
					{
						nodeVisitor.SourceNode = node;
						nodeVisitor.CurrentPath = CurrentPath.Append(property.Name);
						var childNode = property.Type.Visit(nodeVisitor);
						if (childNode is not null)
						{
							result.Nodes.Add(new KeyValuePair<string, NodeViewModelBase>(property.Name, childNode));
						}
					}
					else
					{
						LogMessage($"Value for property '{property.Name}', has not been found.");
					}
				}
				return result;
			}
			else
			{
				LogMessage($"Expecting node of type '{typeof(JsonObject)}'.");
			}
			return null;
		}

		public NodeViewModelBase? Visit(SchemaVariantType variantType)
		{
			var messages = new List<NodeParsingMessage>();
			var logger = new CollectionLogger<NodeParsingMessage>(messages);
			
			foreach (var possibleType in variantType.PossibleTypes)
			{
				messages.Clear();
				var nodeViewModel = possibleType.Visit(new NodeParserVisitor(_editor, CurrentPath, Parent, logger, SourceNode));
				if (nodeViewModel is not null && messages.Count == 0)
				{
					return nodeViewModel;
				}
			}

			foreach (var message in messages)
			{
				_logger.Log(message);
			}
			return null;
		}
	}
}
