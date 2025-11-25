using CG.Test.Editor.Models.Nodes;
using System.Windows.Navigation;

namespace CG.Test.Editor.Models.Types
{
    public class JsonValueType(Type type) : JsonTypeBase(type)
    {
        private readonly static Dictionary<Type, string> _typeNameMap = new()
        {
            { typeof(string), "String" },

            { typeof(bool), "Boolean" },

            { typeof(byte)  , "UInt8"  },
            { typeof(ushort), "UInt16" },
            { typeof(uint)  , "UInt32" },
            { typeof(ulong) , "UInt64" },

            { typeof(sbyte), "SInt8"  },
            { typeof(short), "SInt16" },
            { typeof(int)  , "SInt32" },
            { typeof(long) , "SInt64" },

            { typeof(float) , "Float32" },
            { typeof(double), "Float64" },
        };

        public override string Name { get; } = _typeNameMap[type];

        public override JsonValueNode Create()
        {
            object? resultValue = null;
            if (Type.IsPrimitive)
            {
                resultValue = Activator.CreateInstance(Type)!;
            }
            else if (Type == typeof(string))
            {
                resultValue = string.Empty;
            }
            return new JsonValueNode(resultValue!);   
        }

        public override bool IsConvertibleFrom(JsonTypeBase type) => type is JsonValueType valueType && valueType.Type == Type; 
    }
}
