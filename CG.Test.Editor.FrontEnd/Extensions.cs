using CG.Test.Editor.FrontEnd.Models;
using System.Collections;
using System.Numerics;
using System.Reflection;

namespace CG.Test.Editor.FrontEnd
{
    public static class Extensions
    {
		extension<TInteger>(TInteger) where TInteger : IBinaryInteger<TInteger>, IMinMaxValue<TInteger>
		{
			public static SchemaIntegerType GetIntegerSchema() 
				=> new(long.CreateTruncating(TInteger.MinValue), long.CreateTruncating(TInteger.MaxValue));
		}

		extension<TFloat>(TFloat) where TFloat : IFloatingPoint<TFloat>, IMinMaxValue<TFloat>
		{
			public static SchemaNumberType GetFloatSchema()
				=> new(double.CreateTruncating(TFloat.MinValue), double.CreateTruncating(TFloat.MaxValue));
		}

		extension(Type type)
		{
			public SchemaTypeBase GetSchemaFromType()
			{
				     if (type == typeof(byte))    { return    byte.GetIntegerSchema();         }
				else if (type == typeof(ushort))  { return  ushort.GetIntegerSchema();         }
				else if (type == typeof(uint))    { return    uint.GetIntegerSchema();         }
				else if (type == typeof(ulong))   { return   ulong.GetIntegerSchema();         }
				else if (type == typeof(sbyte))   { return   sbyte.GetIntegerSchema();         }
				else if (type == typeof(short))   { return   short.GetIntegerSchema();         }
				else if (type == typeof(int))     { return     int.GetIntegerSchema();         }
				else if (type == typeof(long))    { return    long.GetIntegerSchema();         }
				else if (type == typeof(float))   { return   float.GetFloatSchema();           }
				else if (type == typeof(double))  { return  double.GetFloatSchema();           }
				else if (type == typeof(decimal)) { return decimal.GetFloatSchema();           }
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
					return new SchemaArrayType((type.GetElementType() ?? typeof(object)).GetSchemaFromType());
				}
				else if (type.IsAssignableTo(typeof(IEnumerable)))
				{
					if (type.GenericTypeArguments.Length > 0)
					{
						return new SchemaArrayType(type.GenericTypeArguments[0].GetSchemaFromType());
					}
					else
					{
						return new SchemaArrayType(typeof(object).GetSchemaFromType());
					}
				}
				else
				{
					var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
					var properties = new Dictionary<string, SchemaProperty>();
                    var parameters = constructor.GetParameters();
                    for (var i = 0; i < parameters.Length; i++)
					{
                        var parameter = parameters[i];
                        properties.Add(parameter.Name ?? string.Empty, new SchemaProperty()
						{
							Index = i,
							Name = parameter.Name ?? string.Empty,
							Type = parameter.ParameterType.GetSchemaFromType()
						});
					}
					return new SchemaObjectType(type.Name, properties);
				}
			}
		}
	}
}
