using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
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
		extension<TInteger>(TInteger) where TInteger : IBinaryInteger<TInteger>, IMinMaxValue<TInteger>
		{
            public static LinkedSchemaIntegerType GetIntegerSchema(ParameterInfo? parameter)
            {
				var rangeAttribute = parameter?.GetCustomAttribute<RangeAttribute>();

				if (rangeAttribute is not null)
				{
					return new LinkedSchemaIntegerType((int)rangeAttribute.Minimum, (int)rangeAttribute.Maximum);
				}

                return new LinkedSchemaIntegerType(long.CreateTruncating(TInteger.MinValue), long.CreateTruncating(TInteger.MaxValue));
            }
        }

		extension<TFloat>(TFloat) where TFloat : IFloatingPoint<TFloat>, IMinMaxValue<TFloat>
		{
            public static LinkedSchemaNumberType GetFloatSchema(ParameterInfo? parameter)
            {
				var rangeAttribute = parameter?.GetCustomAttribute<RangeAttribute>();

				if (rangeAttribute is not null)
				{
					return new LinkedSchemaNumberType(((IConvertible)rangeAttribute.Minimum).ToDouble(CultureInfo.CurrentCulture), ((IConvertible)rangeAttribute.Maximum).ToDouble(CultureInfo.CurrentCulture));
				}

				return new LinkedSchemaNumberType(double.CreateTruncating(TFloat.MinValue), double.CreateTruncating(TFloat.MaxValue));
            }
        }

		extension(Type type)
		{
			public LinkedSchemaTypeBase GetSchemaFromType(ParameterInfo? parameter)
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
					return new LinkedSchemaBooleanType();
				}
				else if (type == typeof(string))
				{
					return new LinkedSchemaStringType(int.MaxValue);
				}
				else if (type.IsArray)
				{
					return new LinkedSchemaArrayType((type.GetElementType() ?? typeof(object)).GetSchemaFromType(null));
				}
				else if (type.IsAssignableTo(typeof(IEnumerable)))
				{
					if (type.GenericTypeArguments.Length > 0)
					{
						return new LinkedSchemaArrayType(type.GenericTypeArguments[0].GetSchemaFromType(null));
					}
					else
					{
						return new LinkedSchemaArrayType(typeof(object).GetSchemaFromType(null));
					}
				}
				else
				{
					var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
					var properties = new Dictionary<string, LinkedSchemaProperty>();
                    var parameters = constructor.GetParameters();
					return new LinkedSchemaObjectType(type.Name, parameters.Select((parameter, index) => new LinkedSchemaProperty()
					{ 
						Index = index,
						Name = parameter.Name ?? string.Empty,
						Type = parameter.ParameterType.GetSchemaFromType(parameter)
					}));
				}
			}
		}

		extension(LinkedSchemaTypeBase type)
		{

			public IEnumerable<LinkedSchemaObjectType> EnumerateObjectTypes()
			{
				if (type is LinkedSchemaObjectType objectType)
				{
					yield return objectType;
				}
				else if (type is LinkedSchemaVariantType variantType)
				{
					foreach (var possibleType in variantType.PossibleTypes)
					{
						foreach (var possibleObjectType in possibleType.EnumerateObjectTypes())
						{
							yield return possibleObjectType;
						}
					}
				}
				else if (type is LinkedSchemaSymbolType symbolType)
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
			public bool TryParseSchemaDefinitions(ILogger<SchemaParsingMessage> logger, Dictionary<string, LinkedSchemaTypeBase> namedTypes)
			{
				foreach (var (typeName, typeNode) in objectNode)
				{
					if (typeNode is null)
					{
						continue;
					}

					if (typeNode.TryParseLinkedSchemaType(logger, namedTypes, out var type))
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

			private bool TryParseSchemaIntegerType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
			{
				if (!objectNode.TryGetValue<long>("minimum", logger, out var minimum))
				{
					minimum = long.MinValue;
				}

				if (!objectNode.TryGetValue<long>("maximum", logger, out var maximum))
				{
					maximum = long.MaxValue;
				}

				type = new LinkedSchemaIntegerType(minimum, maximum);
				return true;
			}

			private bool TryParseSchemaNumberType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
			{
				if (!objectNode.TryGetValue<double>("minimum", logger, out var minimum))
				{
					minimum = double.MinValue;
				}

				if (!objectNode.TryGetValue<double>("maximum", logger, out var maximum))
				{
					maximum = double.MaxValue;
				}
				type = new LinkedSchemaNumberType(minimum, maximum);
				return true;
			}

			private bool TryParseSchemaStringType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("enum", out var node) && node is JsonArray arrayNode)
				{
					var successful = true;
					type = new LinkedSchemaEnumType(arrayNode.OfType<JsonValue>().Select((valueNode) =>
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
				type = new LinkedSchemaStringType(maxLength);
				return true;
			}

			private bool TryParseSchemaObjectType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, LinkedSchemaTypeBase> registeredTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
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

								if (propertyNode is not null && propertyNode.TryParseLinkedSchemaType(logger, registeredTypes, out var propertyType))
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
					
							type = new LinkedSchemaObjectType(typeName, properties);

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
						var possibleTypes = new List<LinkedSchemaTypeBase>();

						foreach (var node in arrayNode)
						{
							if (node is not null && node.TryParseLinkedSchemaType(logger, registeredTypes, out var possibleType))
							{
								possibleTypes.Add(possibleType);
							}
							else
							{
								type = null;
								return false;
							}
						}

						type = new LinkedSchemaVariantType(string.Empty, possibleTypes);
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


			private bool TryParseSchemaArrayType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, LinkedSchemaTypeBase> registeredTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("items", out var itemsNode) && itemsNode is not null)
				{
					if (itemsNode.TryParseLinkedSchemaType(logger, registeredTypes, out var elementType))
					{
						type = new LinkedSchemaArrayType(elementType);
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

			private bool TryParseSchemaReferenceType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, LinkedSchemaTypeBase> registeredTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("target", out var targetNode) && targetNode is not null)
				{
					if (targetNode.TryParseLinkedSchemaType(logger, registeredTypes, out var elementType))
					{
						type = new LinkedSchemaReferenceType(elementType);
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
		}

		extension(JsonNode node)
		{

			public bool TryParseLinkedSchemaType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, LinkedSchemaTypeBase> typeDefinitions, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
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
						type = new LinkedSchemaSymbolType(typeName, typeDefinitions);
						return true;
					}
					else if (objectNode.TryGetPropertyValue("type", out var childNode) && childNode is not null)
					{
						if (childNode is JsonValue childValueNode && childValueNode.TryGetValue<string>(out var typeName))
						{
							switch (typeName)
							{
								case "boolean":
									type = new LinkedSchemaBooleanType();
									return true;
								case "integer":
									return objectNode.TryParseSchemaIntegerType  (logger, out type);
								case "number":								     
									return objectNode.TryParseSchemaNumberType   (logger, out type);
								case "string":								     
									return objectNode.TryParseSchemaStringType   (logger, out type);
								case "object":								     
									return objectNode.TryParseSchemaObjectType   (logger, typeDefinitions, out type);
								case "array":								     
									return objectNode.TryParseSchemaArrayType    (logger, typeDefinitions, out type);
								case "reference":
									return objectNode.TryParseSchemaReferenceType(logger, typeDefinitions, out type);
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
