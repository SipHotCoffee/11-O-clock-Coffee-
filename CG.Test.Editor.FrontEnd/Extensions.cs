using CG.Test.Editor.FrontEnd.Models;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Numerics;
using System.Reflection;

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
	}
}
