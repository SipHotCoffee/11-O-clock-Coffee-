using CG.Test.Editor.FrontEnd.Models.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace CG.Test.Editor.FrontEnd
{
    public static class Extensions
    {
		extension<TInteger>(TInteger) where TInteger : IBinaryInteger<TInteger>, IMinMaxValue<TInteger>
		{
            public static SchemaIntegerType GetIntegerSchema(ParameterInfo? parameter)
            {
				var rangeAttribute = parameter?.GetCustomAttribute<RangeAttribute>();

				if (rangeAttribute is not null)
				{
					return new SchemaIntegerType((int)rangeAttribute.Minimum, (int)rangeAttribute.Maximum);
				}

                return new SchemaIntegerType(long.CreateTruncating(TInteger.MinValue), long.CreateTruncating(TInteger.MaxValue));
            }
        }

		extension<TFloat>(TFloat) where TFloat : IFloatingPoint<TFloat>, IMinMaxValue<TFloat>
		{
            public static SchemaNumberType GetFloatSchema(ParameterInfo? parameter)
            {
				var rangeAttribute = parameter?.GetCustomAttribute<RangeAttribute>();

				if (rangeAttribute is not null)
				{
					return new SchemaNumberType(((IConvertible)rangeAttribute.Minimum).ToDouble(CultureInfo.CurrentCulture), ((IConvertible)rangeAttribute.Maximum).ToDouble(CultureInfo.CurrentCulture));
				}

				return new SchemaNumberType(double.CreateTruncating(TFloat.MinValue), double.CreateTruncating(TFloat.MaxValue));
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
					return new SchemaBooleanType();
				}
				else if (type == typeof(string))
				{
					return new SchemaStringType(int.MaxValue);
				}
				else if (type.IsArray)
				{
					return new SchemaArrayType((type.GetElementType() ?? typeof(object)).GetSchemaFromType(null));
				}
				else if (type.IsAssignableTo(typeof(IEnumerable)))
				{
					if (type.GenericTypeArguments.Length > 0)
					{
						return new SchemaArrayType(type.GenericTypeArguments[0].GetSchemaFromType(null));
					}
					else
					{
						return new SchemaArrayType(typeof(object).GetSchemaFromType(null));
					}
				}
				else
				{
					var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
					var properties = new Dictionary<string, SchemaProperty>();
                    var parameters = constructor.GetParameters();
					return new SchemaObjectType(type.Name, parameters.Select((parameter, index) => new SchemaProperty()
					{ 
						Index = index,
						Name = parameter.Name ?? string.Empty,
						Type = parameter.ParameterType.GetSchemaFromType(parameter)
					}));
				}
			}
		}

		extension(JsonArray arrayNode)
		{

			public IEnumerable<SchemaTypeBase> EnumerateVariantTypes(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> registeredTypes, DictionaryQueue<string, JsonObject> typesToResolve)
			{
				foreach (var node in arrayNode)
				{
					if (node is not null && node.TryParseSchemaType(logger, registeredTypes, typesToResolve, out var type))
					{
						yield return type;
					}
				}
			}
		}

		extension(JsonObject objectNode)
		{

			public void ParseDefinitions(ILogger<SchemaParsingMessage> logger, Dictionary<string, SchemaTypeBase> types)
			{
				var typesToResolve = new DictionaryQueue<string, JsonObject>();

				foreach (var (typeName, typeNode) in objectNode)
				{
					if (typeNode is JsonObject childObjectNode)
					{
						typesToResolve.Enqueue(typeName, childObjectNode);
					}
				}

				while (typesToResolve.TryDequeue(out var typeName, out var typeObjectNode))
				{
					if (typeObjectNode.TryGetPropertyValue("oneOf", out var oneOfNode) && oneOfNode is JsonArray oneOfArrayNode)
					{
						foreach (var possibleTypeNode in oneOfArrayNode)
						{
							
						}
					}
					else if (typeObjectNode.TryGetPropertyValue("properties", out var propertiesNode) && propertiesNode is JsonObject propertiesObjectNode)
					{
						var properties = new List<SchemaProperty>();

						var index = 0;
						foreach (var (propertyName, propertyTypeNode) in propertiesObjectNode)
						{
							if (propertyName != "$type")
							{
								if (propertyTypeNode is not null && propertyTypeNode.TryParseSchemaType(logger, types, typesToResolve, out var type))
								{
									properties.Add(new SchemaProperty()
									{
										Index = index++,
										Name = propertyName,
										Type = type,
									});
								}
								else
								{
									typesToResolve.Enqueue(typeName, typeObjectNode);
									if (propertyTypeNode is JsonObject propertyTypeObjectNode)
									{
										typesToResolve.Enqueue(propertyName, propertyTypeObjectNode);
									}
								}
							}
						}
					}
				}
			}

			private bool TryGetValue<TValue>(string propertyName, ILogger<SchemaParsingMessage> logger, [MaybeNullWhen(false)] out TValue value)
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

			public IEnumerable<SchemaProperty> EnumerateProperties(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> registeredTypes, DictionaryQueue<string, JsonObject> typesToResolve)
			{
				var index = 0;
				foreach (var pair in objectNode)
				{
					if (pair.Key != "$type")
					{
						if (pair.Value is not null && pair.Value.TryParseSchemaType(logger, registeredTypes, typesToResolve, out var type))
						{
							yield return new SchemaProperty()
							{
								Name = pair.Key,
								Type = type,
								Index = index++,
							};
						}
					}
				}
			}

			public bool TryParseSchemaIntegerType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (!objectNode.TryGetValue<long>("minimum", logger, out var minimum))
				{
					minimum = long.MinValue;
				}

				if (!objectNode.TryGetValue<long>("maximum", logger, out var maximum))
				{
					maximum = long.MaxValue;
				}

				type = new SchemaIntegerType(minimum, maximum);
				return true;
			}

			public bool TryParseSchemaNumberType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (!objectNode.TryGetValue<double>("minimum", logger, out var minimum))
				{
					minimum = double.MinValue;
				}

				if (!objectNode.TryGetValue<double>("maximum", logger, out var maximum))
				{
					maximum = double.MaxValue;
				}
				type = new SchemaNumberType(minimum, maximum);
				return true;
			}

			public bool TryParseSchemaStringType(ILogger<SchemaParsingMessage> logger, [NotNullWhen(true)] out SchemaTypeBase? type)
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
				type = new SchemaStringType(maxLength);
				return true;
			}

			public bool TryParseSchemaObjectType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> registeredTypes, DictionaryQueue<string, JsonObject> typesToResolve, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("properties", out var propertiesNode))
				{
					if (propertiesNode is JsonObject propertiesObjectNode)
					{
						if (propertiesObjectNode.TryGetPropertyValue("$type", out var typeNode) &&
							typeNode is JsonObject typeObjectNode && typeObjectNode.TryGetValue<string>("const", logger, out var typeName))
						{
							var beforeFailCount = typesToResolve.Count;
							var properties = propertiesObjectNode.EnumerateProperties(logger, registeredTypes, typesToResolve);							
							type = new SchemaObjectType(typeName, properties);
							var afterFailCount = typesToResolve.Count;

							if (afterFailCount > beforeFailCount)
							{
								type = null;
								return false;
							}

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
						var beforeFailCount = typesToResolve.Count;
						type = new SchemaVariantType(arrayNode.EnumerateVariantTypes(logger, registeredTypes, typesToResolve));
						var afterFailCount = typesToResolve.Count;
						
						if (afterFailCount > beforeFailCount)
						{
							type = null;
							return false;
						}

						return true;
					}
					else
					{
						logger.Log(new SchemaParsingMessage($"Property of name 'oneOf' in object type, must be of type: '{typeof(JsonArray)}'", oneOfNode!));
					}
				}
				else
				{
					logger.Log(new SchemaParsingMessage($"Node of type 'object' must contain a 'properties' object or a 'oneOf' array.", objectNode));
				}
				type = null;
				return false;
			}

			public bool TryParseSchemaArrayType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> registeredTypes, DictionaryQueue<string, JsonObject> typesToResolve, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("items", out var itemsNode) && itemsNode is not null)
				{
					if (itemsNode.TryParseSchemaType(logger, registeredTypes, typesToResolve, out var elementType))
					{
						type = new SchemaArrayType(elementType);
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

			public bool TryParseSchemaReferenceType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> registeredTypes, DictionaryQueue<string, JsonObject> typesToResolve, [NotNullWhen(true)] out SchemaTypeBase? type)
			{
				if (objectNode.TryGetPropertyValue("target", out var targetNode) && targetNode is not null)
				{
					if (targetNode.TryParseSchemaType(logger, registeredTypes, typesToResolve, out var elementType))
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
		}

		extension(JsonNode node)
		{
			public bool TryParseSchemaType(ILogger<SchemaParsingMessage> logger, IReadOnlyDictionary<string, SchemaTypeBase> typeDefinitions, DictionaryQueue<string, JsonObject> typesToResolve, [NotNullWhen(true)] out SchemaTypeBase? type)
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
						return typeDefinitions.TryGetValue(typeName, out type);
					}
					else if (objectNode.TryGetPropertyValue("type", out var childNode) && childNode is not null)
					{
						if (childNode is JsonValue childValueNode && childValueNode.TryGetValue<string>(out var typeName))
						{
							switch (typeName)
							{
								case "boolean":
									type = new SchemaBooleanType();
									return true;
								case "integer":
									return objectNode.TryParseSchemaIntegerType  (logger, out type);
								case "number":								     
									return objectNode.TryParseSchemaNumberType   (logger, out type);
								case "string":								     
									return objectNode.TryParseSchemaStringType   (logger, out type);
								case "object":								     
									return objectNode.TryParseSchemaObjectType   (logger, typeDefinitions, typesToResolve, out type);
								case "array":								     
									return objectNode.TryParseSchemaArrayType    (logger, typeDefinitions, typesToResolve, out type);
								case "reference":
									return objectNode.TryParseSchemaReferenceType(logger, typeDefinitions, typesToResolve, out type);
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
