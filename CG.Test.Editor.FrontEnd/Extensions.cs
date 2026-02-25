using CG.Test.Editor.FrontEnd.Models.Types;
using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.Visitors;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd
{
    public static class Extensions
    {
		extension(IEnumerable enumerable)
		{
			public IEnumerable<NodeViewModelBase> ClonedNodes(NodeViewModelBase? parent) => enumerable.OfType<NodeViewModelBase>().Select((node) => node.Clone(parent));
		}

		extension<TInteger>(TInteger) where TInteger : IBinaryInteger<TInteger>, IMinMaxValue<TInteger>
		{
            public static SchemaIntegerType GetIntegerSchema(ParameterInfo? parameter)
            {
				var rangeAttribute = parameter?.GetCustomAttribute<RangeAttribute>();

				if (rangeAttribute is not null)
				{
					return new SchemaIntegerType((int)rangeAttribute.Minimum, (int)rangeAttribute.Maximum, default);
				}

                return new SchemaIntegerType(long.CreateTruncating(TInteger.MinValue), long.CreateTruncating(TInteger.MaxValue), default);
            }
        }

		extension<TFloat>(TFloat) where TFloat : IFloatingPoint<TFloat>, IMinMaxValue<TFloat>
		{
            public static SchemaNumberType GetFloatSchema(ParameterInfo? parameter)
            {
				var rangeAttribute = parameter?.GetCustomAttribute<RangeAttribute>();

				if (rangeAttribute is not null)
				{
					return new SchemaNumberType(((IConvertible)rangeAttribute.Minimum).ToDouble(CultureInfo.CurrentCulture), ((IConvertible)rangeAttribute.Maximum).ToDouble(CultureInfo.CurrentCulture), default);
				}

				return new SchemaNumberType(double.CreateTruncating(TFloat.MinValue), double.CreateTruncating(TFloat.MaxValue), default);
            }
        }

		extension(Type type)
		{
			public SchemaTypeBase GetSchemaFromType(ParameterInfo? parameter)
			{
				     if (type == typeof(byte))    { return    byte.GetIntegerSchema(parameter); }
				else if (type == typeof(ushort))  { return  ushort.GetIntegerSchema(parameter); }
				else if (type == typeof(uint))    { return    uint.GetIntegerSchema(parameter); }
				else if (type == typeof(ulong))   { return   ulong.GetIntegerSchema(parameter); }
				else if (type == typeof(sbyte))   { return   sbyte.GetIntegerSchema(parameter); }
				else if (type == typeof(short))   { return   short.GetIntegerSchema(parameter); }
				else if (type == typeof(int))     { return     int.GetIntegerSchema(parameter); }
				else if (type == typeof(long))    { return    long.GetIntegerSchema(parameter); }
				else if (type == typeof(float))   { return   float.GetFloatSchema(parameter);   }
				else if (type == typeof(double))  { return  double.GetFloatSchema(parameter);   }
				else if (type == typeof(decimal)) { return decimal.GetFloatSchema(parameter);   }
				else if (type == typeof(bool))
				{
					return new SchemaBooleanType(default);
				}
				else if (type == typeof(string))
				{
					return new SchemaStringType(int.MaxValue, string.Empty);
				}
				else if (type.IsArray)
				{
					return new SchemaArrayType((type.GetElementType() ?? typeof(object)).GetSchemaFromType(null), int.MinValue, int.MaxValue);
				}
				else if (type.IsAssignableTo(typeof(IEnumerable)))
				{
					if (type.GenericTypeArguments.Length > 0)
					{
						return new SchemaArrayType(type.GenericTypeArguments[0].GetSchemaFromType(null), int.MinValue, int.MaxValue);
					}
					else
					{
						return new SchemaArrayType(typeof(object).GetSchemaFromType(null), int.MinValue, int.MaxValue);
					}
				}
				else
				{
					var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
					var properties = new Dictionary<string, LinkedSchemaProperty>();
                    var parameters = constructor.GetParameters();
					return new SchemaObjectType(type.Name, parameters.Select((parameter, index) => new LinkedSchemaProperty()
					{ 
						Index = index,
						Name = parameter.Name ?? string.Empty,
						Type = parameter.ParameterType.GetSchemaFromType(parameter)
					}));
				}
			}
		}

		extension(SchemaTypeBase type)
		{
			public IEnumerable<SchemaObjectType> EnumerateObjectTypes()
			{
				if (type is SchemaObjectType objectType)
				{
					yield return objectType;
				}
				else if (type is SchemaVariantType variantType)
				{
					foreach (var possibleType in variantType.PossibleTypes)
					{
						foreach (var possibleObjectType in possibleType.EnumerateObjectTypes())
						{
							yield return possibleObjectType;
						}
					}
				}
				else if (type is SchemaSymbolType symbolType)
				{
					foreach (var possibleObjectType in symbolType.LinkedType.EnumerateObjectTypes())
					{
						yield return possibleObjectType;
					}
				}
			}
		}

		extension(JsonObject objectNode)
		{
			public bool TryParseSchemaDefinitions(ILogger<SchemaParsingMessage> logger, Dictionary<string, SchemaTypeBase> namedTypes)
			{
				foreach (var (typeName, typeNode) in objectNode)
				{
					if (typeNode is null)
					{
						continue;
					}

					if (typeNode.TryParseSchemaType(logger, namedTypes, out var type))
					{
						namedTypes.Add(typeName, type);
					}
					else
					{
						return false;
					}
				}
				return true;
			}

			public bool TryGetValue<TValue>(string propertyName, [MaybeNullWhen(false)] out TValue value)
			{
				value = default;
				return objectNode.TryGetPropertyValue(propertyName, out var childNode) && childNode is JsonValue childValueNode && childValueNode.TryGetValue(out value);
			}

			public bool TryGetArray(string propertyName, out JsonArray array)
			{
				if (objectNode.TryGetPropertyValue(propertyName, out var childNode) && childNode is JsonArray childArrayNode)
				{
					array = childArrayNode;
					return true;
				}
				array = [];
				return false;
			}

			public bool TryGetValue<TValue>(string propertyName, ILogger<string> logger, [MaybeNullWhen(false)] out TValue value)
			{
				if (objectNode.TryGetPropertyValue(propertyName, out var childNode))
				{
					if (childNode is JsonValue childValueNode && childValueNode.TryGetValue(out value))
					{
						return true;
					}
					else
					{
						logger.Log($"Property of name: '{propertyName}', must be a '{typeof(JsonValue)}' of type '{typeof(TValue)}'.");
					}
				}
				else
				{
					logger.Log($"Missing property of name: '{propertyName}'.");
				}
				value = default;
				return false;
			}

			public bool TryGetValue<TValue>(string propertyName, ILogger<SchemaParsingMessage> logger, [MaybeNullWhen(false)] out TValue value)
			{
				if (objectNode.TryGetPropertyValue(propertyName, out var childNode))
				{
					if (childNode is JsonValue childValueNode && childValueNode.TryGetValue(out value))
					{
						return true;
					}
					else
					{
						logger.Log(new SchemaParsingMessage($"Property of name: '{propertyName}', must be a '{typeof(JsonValue)}' of type '{typeof(TValue)}'.", objectNode));
					}
				}
				value = default;
				return false;
			}

			private bool TryParseSchemaBooleanType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (!objectNode.TryGetValue<bool>("default", logger, out var defaultValue))
				{
					defaultValue = default;
				}

				type = new SchemaBooleanType(defaultValue);
				return true;
			}

			private bool TryParseSchemaIntegerType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (!objectNode.TryGetValue<long>("minimum", logger, out var minimum))
				{
					minimum = long.MinValue;
				}

				if (!objectNode.TryGetValue<long>("maximum", logger, out var maximum))
				{
					maximum = long.MaxValue;
				}

				if (!objectNode.TryGetValue<long>("default", logger, out var defaultValue))
				{
					defaultValue = Math.Clamp(default, minimum, maximum);
				}

				type = new SchemaIntegerType(minimum, maximum, defaultValue);
				return true;
			}

			private bool TryParseSchemaNumberType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (!objectNode.TryGetValue<double>("minimum", logger, out var minimum))
				{
					minimum = double.MinValue;
				}

				if (!objectNode.TryGetValue<double>("maximum", logger, out var maximum))
				{
					maximum = double.MaxValue;
				}

				if (!objectNode.TryGetValue<double>("default", logger, out var defaultValue))
				{
					defaultValue = Math.Clamp(default, minimum, maximum);
				}

				type = new SchemaNumberType(minimum, maximum, defaultValue);
				return true;
			}

			private bool TryParseSchemaStringType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("enum", out var node) && node is JsonArray arrayNode)
				{
					var successful = true;
					type = new SchemaEnumType(arrayNode.OfType<JsonValue>().Select((valueNode) =>
					{
						if (valueNode.TryGetValue<string>(out var elementName))
						{
							return elementName;
						}
						else
						{
							logger.Log(new SchemaParsingMessage("Enum value must be of type 'string'.", valueNode));
							successful = false;
							return string.Empty;
						}
					}));
					return successful;
				}

				if (!objectNode.TryGetValue<int>("maxLength", logger, out var maxLength))
				{
					maxLength = int.MaxValue;
				}

				if (!objectNode.TryGetValue<string>("default", logger, out var defaultValue))
				{
					defaultValue = string.Empty;
				}

				type = new SchemaStringType(maxLength, defaultValue);
				return true;
			}

			private bool TryParseSchemaObjectType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> registeredTypes, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("properties", out var propertiesNode))
				{
					if (propertiesNode is JsonObject propertiesObjectNode)
					{
						if (propertiesObjectNode.TryGetPropertyValue("$type", out var typeNode) &&
							typeNode is JsonObject typeObjectNode && typeObjectNode.TryGetValue<string>("const", logger, out var typeName))
						{
							var properties = new List<LinkedSchemaProperty>();
							var index = 0;
							foreach (var (propertyName, propertyNode) in propertiesObjectNode)
							{
								if (propertyName == "$type")
								{
									continue;
								}

								if (propertyNode is not null && propertyNode.TryParseSchemaType(logger, registeredTypes, out var propertyType))
								{
									properties.Add(new LinkedSchemaProperty()
									{
										Name  = propertyName,
										Type  = propertyType,
										Index = index++,
									});
								}
								else
								{
									type = null;
									return false;
								}
							}
					
							type = new SchemaObjectType(typeName, properties);

							return true;
						}
						else
						{
							logger.Log(new SchemaParsingMessage($"Object properties must all have a '$type' of type: '{typeof(string)}'", propertiesObjectNode));
						}
					}
					else
					{
						logger.Log(new SchemaParsingMessage($"Property of name 'properties' in object type, must be of type: '{typeof(JsonObject)}'", propertiesNode!));
					}
				}
				else if (objectNode.TryGetPropertyValue("oneOf", out var oneOfNode))
				{
					if (oneOfNode is JsonArray arrayNode)
					{
						var possibleTypes = new List<SchemaTypeBase>();

						foreach (var node in arrayNode)
						{
							if (node is not null && node.TryParseSchemaType(logger, registeredTypes, out var possibleType))
							{
								possibleTypes.Add(possibleType);
							}
							else
							{
								type = null;
								return false;
							}
						}

						type = new SchemaVariantType(string.Empty, possibleTypes);
						return true;
					}
					else
					{
						logger.Log(new SchemaParsingMessage($"Property of name 'oneOf' in object type, must be of type: '{typeof(JsonArray)}'", oneOfNode!));
					}
				}
				else
				{
					logger.Log(new SchemaParsingMessage($"Node of type 'object' must contain a 'properties' property, or a 'oneOf' array.", objectNode));
				}
				type = null;
				return false;
			}


			private bool TryParseSchemaArrayType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> registeredTypes, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("items", out var itemsNode) && itemsNode is not null)
				{
					if (itemsNode.TryParseSchemaType(logger, registeredTypes, out var elementType))
					{
						if (!objectNode.TryGetValue<int>("minItems", logger, out var minimumItemCount))
						{
							minimumItemCount = 0;
						}

						if (!objectNode.TryGetValue<int>("maxItems", logger, out var maximumItemCount))
						{
							maximumItemCount = int.MaxValue;
						}

						type = new SchemaArrayType(elementType, minimumItemCount, maximumItemCount);
						return true;
					}
				}
				else
				{
					logger.Log(new SchemaParsingMessage($"Node of type 'array' must contain an 'items' object.", objectNode));
				}
				type = null;
				return false;
			}

			private bool TryParseSchemaInternalReferenceType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> registeredTypes, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("target", out var targetNode) && targetNode is not null)
				{
					if (targetNode.TryParseSchemaType(logger, registeredTypes, out var elementType))
					{
						type = new SchemaReferenceType(elementType);
						return true;
					}

					logger.Log(new SchemaParsingMessage($"Target type of 'reference' node is not available: '{targetNode["$ref"]?.GetValue<string>()}'", objectNode));
				}
				else
				{
					logger.Log(new SchemaParsingMessage($"Node of type 'reference' must contain a 'target' object.", objectNode));
				}
				type = null;
				return false;
			}

			private bool TryParseSchemaExternalReferenceType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> registeredTypes, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("target", out var targetNode) && targetNode is not null)
				{
					if (targetNode.TryParseSchemaType(logger, registeredTypes, out var elementType))
					{
						type = new SchemaExternalReferenceType(elementType);
						return true;
					}

					logger.Log(new SchemaParsingMessage($"Target type of 'external reference' node is not available: '{targetNode["$ref"]?.GetValue<string>()}'", objectNode));
				}
				else
				{
					logger.Log(new SchemaParsingMessage($"Node of type 'external reference' must contain a 'target' object.", objectNode));
				}
				type = null;
				return false;
			}
		}

		extension(JsonNode node)
		{

			public bool TryParseSchemaType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> typeDefinitions, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (node is JsonObject objectNode)
				{
					if (objectNode.TryGetPropertyValue("$ref", out var refNode) && refNode is JsonValue refValueNode && refValueNode.TryGetValue<string>(out var path))
					{
						var pathTokens = path.Split('/');
						JsonNode? currentNode = node.Root;
						for (var i = 1; i < pathTokens.Length; i++)
						{
							currentNode = currentNode?[pathTokens[i]];
						}

						var typeName = pathTokens[^1];
						type = new SchemaSymbolType(typeName, typeDefinitions);
						return true;
					}
					else if (objectNode.TryGetPropertyValue("type", out var childNode) && childNode is not null)
					{
						if (childNode is JsonValue childValueNode && childValueNode.TryGetValue<string>(out var typeName))
						{
							switch (typeName)
							{
								case "boolean":
									return objectNode.TryParseSchemaBooleanType          (logger, out type);
								case "integer":
									return objectNode.TryParseSchemaIntegerType          (logger, out type);
								case "number":								             
									return objectNode.TryParseSchemaNumberType           (logger, out type);
								case "string":								             
									return objectNode.TryParseSchemaStringType           (logger, out type);
								case "object":								             
									return objectNode.TryParseSchemaObjectType           (logger, typeDefinitions, out type);
								case "array":								             
									return objectNode.TryParseSchemaArrayType            (logger, typeDefinitions, out type);
								case "reference":								         
									return objectNode.TryParseSchemaInternalReferenceType(logger, typeDefinitions, out type);
								case "external":
									return objectNode.TryParseSchemaExternalReferenceType(logger, typeDefinitions, out type);
							}
						}
						else
						{
							logger.Log(new SchemaParsingMessage("Expecting 'type' node to be a string", childNode));
						}
					}
				}
				else
				{
					logger.Log(new SchemaParsingMessage($"Expecting '{typeof(JsonObject)}' for type schema.", node));
				}

				type = null;
				return false;
			}
		}
	}
}
