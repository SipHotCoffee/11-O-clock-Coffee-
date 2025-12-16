using CG.Test.Editor.FrontEnd.Models;
using CG.Test.Editor.FrontEnd.ViewModels;
using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Visitors
{
    public class NodeParserVisitor(FileInstanceViewModel editor, NodeViewModelBase? parent, ILogger<string> logger, JsonNode? sourceNode) : VisitorBase<SchemaTypeBase, NodeViewModelBase?>
    {
        private readonly FileInstanceViewModel _editor = editor;

        private readonly ILogger<string> _logger = logger;

        public NodeViewModelBase? Parent { get; set; } = parent;

        public JsonNode? SourceNode { get; set; } = sourceNode;

        public NumberNodeViewModel? Visit(SchemaNumberType numberType)
        {
			if (SourceNode is JsonValue valueNode)
			{
				if (valueNode.TryGetValue<double>(out var value))
				{
                    return new NumberNodeViewModel(_editor, Parent, numberType, value);
				}
				else
				{
					_logger.Log($"Failed to convert value of '{valueNode.GetType()}' to '{typeof(double)}'.");
				}
			}
			else
			{
				_logger.Log($"'{SourceNode}' must be of type '{typeof(JsonValue)}' and the value must be convertible to type '{typeof(double)}'.");
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
					_logger.Log($"Failed to convert value of '{valueNode.GetType()}' to '{typeof(long)}'.");
				}
			}
			else
			{
				_logger.Log($"'{SourceNode}' must be of type '{typeof(JsonValue)}' and the value must be convertible to type '{typeof(long)}'.");
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
					_logger.Log($"Failed to convert value of '{valueNode.GetType()}' to '{typeof(string)}'.");
				}
			}
			else
			{
				_logger.Log($"'{SourceNode}' must be of type '{typeof(JsonValue)}' and the value must be convertible to type '{typeof(string)}'.");
			}
			return null;
		}

        public ArrayNodeViewModel? Visit(SchemaArrayType arrayType)
        {
			if (SourceNode is JsonArray arrayNode)
			{
				var result = new ArrayNodeViewModel(_editor, Parent, arrayType);
				var elementVisitor = new NodeParserVisitor(_editor, result, _logger, null);
				foreach (var node in arrayNode)
				{
					elementVisitor.SourceNode = node;
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
				_logger.Log($"'{SourceNode}' must be of type '{typeof(JsonArray)}'.");
			}
			return null;
		}

		public ObjectNodeViewModel? Visit(SchemaObjectType objectType)
		{
			if (SourceNode is JsonObject objectNode)
			{
				var result = new ObjectNodeViewModel(_editor, Parent, objectType);
				var nodeVisitor = new NodeParserVisitor(_editor, result, _logger, null);
				foreach (var property in objectType.Properties)
				{
					if (objectNode.TryGetPropertyValue(property.Name, out var node))
					{
						nodeVisitor.SourceNode = node;
						var childNode = property.Type.Visit(nodeVisitor);
						if (childNode is not null)
						{
							result.Nodes.Add(new KeyValuePair<string, NodeViewModelBase>(property.Name, childNode));
						}
					}
				}
				return result;
			}
			else
			{
				_logger.Log($"'{SourceNode}' must be of type '{typeof(JsonArray)}'.");
			}
			return null;
		}

		public NodeViewModelBase? Visit(SchemaVariantType variantType)
		{
			var messages = new List<string>();
			var logger = new CollectionLogger<string>(messages);
			
			foreach (var possibleType in variantType.PossibleTypes)
			{
				var nodeViewModel = possibleType.Visit(new NodeParserVisitor(_editor, Parent, logger, SourceNode));
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
