
using CG.Test.Editor.Models.Types;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace CG.Test.Editor
{
    public static class Extensions
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT 
        { 
            public int X;
            public int Y; 
        }

        [DllImport("user32.dll")] 
        private static extern bool GetCursorPos(out POINT lpPoint);
        
        public static POINT GetMousePosition() 
        {
            GetCursorPos(out var lpPoint);
            return lpPoint;
        }

        //public static bool IsInteger(this EditorValueTypeCode typeCode)
        //{
        //    return typeCode switch
        //    {
        //        EditorValueTypeCode.UInt8  or
        //        EditorValueTypeCode.UInt16 or
        //        EditorValueTypeCode.UInt32 or
        //        EditorValueTypeCode.UInt64 or
        //        EditorValueTypeCode.SInt8  or
        //        EditorValueTypeCode.SInt16 or
        //        EditorValueTypeCode.SInt32 or
        //        EditorValueTypeCode.SInt64 => true,
        //        _ => false,
        //    };
        //}

        //public static bool IsNumber(this EditorValueTypeCode typeCode)
        //{
        //    return typeCode switch
        //    {
        //        EditorValueTypeCode.UInt8   or
        //        EditorValueTypeCode.UInt16  or 
        //        EditorValueTypeCode.UInt32  or 
        //        EditorValueTypeCode.UInt64  or 
        //        EditorValueTypeCode.SInt8   or 
        //        EditorValueTypeCode.SInt16  or 
        //        EditorValueTypeCode.SInt32  or 
        //        EditorValueTypeCode.SInt64  or
        //        EditorValueTypeCode.Float32 or 
        //        EditorValueTypeCode.Float64 => true,
        //        _ => false,
        //    };
        //}

        //public static EditorValueTypeCode GetEditorType<TValue>(this TValue value) => value switch
        //{
        //    byte   => EditorValueTypeCode.UInt8,
        //    ushort => EditorValueTypeCode.UInt16,
        //    uint   => EditorValueTypeCode.UInt32,
        //    ulong  => EditorValueTypeCode.UInt64,
        //    sbyte  => EditorValueTypeCode.SInt8,
        //    short  => EditorValueTypeCode.SInt16,
        //    int    => EditorValueTypeCode.SInt32,
        //    long   => EditorValueTypeCode.SInt64,
        //    float  => EditorValueTypeCode.Float32,
        //    double => EditorValueTypeCode.Float64,
        //    string => EditorValueTypeCode.String,
        //    bool   => EditorValueTypeCode.Boolean,
        //    _      => throw new NotImplementedException(),
        //};

        //public static void Process(this EditorValue value, Window ownerWindow, string name, Action<EditorObject> handleReferenceNavigattion)
        //{
        //    if (value.Type is EditorValueType valueType)
        //    { 
        //        if (valueType.TypeCode.IsNumber())
        //        {
        //            var numberDialog = new NumericValueDialog()
        //            {
        //                Title = $"Edit Value: '{name}'",
        //                Owner = ownerWindow,
        //            };

        //            switch (valueType.TypeCode)
        //            {
        //                case EditorValueTypeCode.UInt8:   numberDialog.IsIntegral = true;  numberDialog.Minimum =   byte.MinValue; numberDialog.Maximum =   byte.MaxValue; numberDialog.Value = (  byte)value.Value;  break;
        //                case EditorValueTypeCode.UInt16:  numberDialog.IsIntegral = true;  numberDialog.Minimum = ushort.MinValue; numberDialog.Maximum = ushort.MaxValue; numberDialog.Value = (ushort)value.Value;  break;
        //                case EditorValueTypeCode.UInt32:  numberDialog.IsIntegral = true;  numberDialog.Minimum =   uint.MinValue; numberDialog.Maximum =   uint.MaxValue; numberDialog.Value = (  uint)value.Value;  break;
        //                case EditorValueTypeCode.UInt64:  numberDialog.IsIntegral = true;  numberDialog.Minimum =  ulong.MinValue; numberDialog.Maximum =  ulong.MaxValue; numberDialog.Value = ( ulong)value.Value;  break;
        //                case EditorValueTypeCode.SInt8:   numberDialog.IsIntegral = true;  numberDialog.Minimum =  sbyte.MinValue; numberDialog.Maximum =  sbyte.MaxValue; numberDialog.Value = ( sbyte)value.Value;  break;
        //                case EditorValueTypeCode.SInt16:  numberDialog.IsIntegral = true;  numberDialog.Minimum =  short.MinValue; numberDialog.Maximum =  short.MaxValue; numberDialog.Value = ( short)value.Value;  break;
        //                case EditorValueTypeCode.SInt32:  numberDialog.IsIntegral = true;  numberDialog.Minimum =    int.MinValue; numberDialog.Maximum =    int.MaxValue; numberDialog.Value = (   int)value.Value;  break;
        //                case EditorValueTypeCode.SInt64:  numberDialog.IsIntegral = true;  numberDialog.Minimum =   long.MinValue; numberDialog.Maximum =   long.MaxValue; numberDialog.Value = (  long)value.Value;  break;
        //                case EditorValueTypeCode.Float32: numberDialog.IsIntegral = false; numberDialog.Minimum =  float.MinValue; numberDialog.Maximum =  float.MaxValue; numberDialog.Value = ( float)value.Value;  break;
        //                case EditorValueTypeCode.Float64: numberDialog.IsIntegral = false; numberDialog.Minimum = double.MinValue; numberDialog.Maximum = double.MaxValue; numberDialog.Value = (double)value.Value;  break;
        //            }

        //            if (numberDialog.ShowDialog() ?? false)
        //            {
        //                switch (valueType.TypeCode)
        //                {
        //                    case EditorValueTypeCode.UInt8:   value.Value = (byte)  numberDialog.Value; break;
        //                    case EditorValueTypeCode.UInt16:  value.Value = (ushort)numberDialog.Value; break;
        //                    case EditorValueTypeCode.UInt32:  value.Value = (uint)  numberDialog.Value; break;
        //                    case EditorValueTypeCode.UInt64:  value.Value = (ulong) numberDialog.Value; break;
        //                    case EditorValueTypeCode.SInt8:   value.Value = (sbyte) numberDialog.Value; break;
        //                    case EditorValueTypeCode.SInt16:  value.Value = (short) numberDialog.Value; break;
        //                    case EditorValueTypeCode.SInt32:  value.Value = (int)   numberDialog.Value; break;
        //                    case EditorValueTypeCode.SInt64:  value.Value = (long)  numberDialog.Value; break;
        //                    case EditorValueTypeCode.Float32: value.Value = (float) numberDialog.Value; break;
        //                    case EditorValueTypeCode.Float64: value.Value =         numberDialog.Value; break;
        //                }                
        //            }
        //        }
        //        else if (valueType.TypeCode == EditorValueTypeCode.String && value.Value is string stringValue)
        //        {
        //            var stringDialogViewModel = new StringValueDialogViewModel
        //            {
        //                Text = stringValue,
        //            };

        //            var stringDialog = new StringValueDialogView
        //            {
        //                Owner = ownerWindow,
        //                DataContext = stringDialogViewModel,
        //                Title = $"Edit Value: '{name}'",
        //            };

        //            if (stringDialog.ShowDialog() ?? false)
        //            {
        //                value.Value = stringDialogViewModel.Text;
        //            }
        //        }
        //        else if (valueType.TypeCode == EditorValueTypeCode.Boolean && value.Value is bool booleanValue)
        //        {
        //            value.Value = !booleanValue;
        //        }
        //    }
        //    else if (value.Type is EditorReferenceType referenceType && value.Value is ulong id)
        //    {
        //        bool Filter(EditorNode node) => node is EditorObject objectNode && referenceType.TypeArgument.ConvertibleFrom(objectNode.Type);

        //        var referenceDialogViewModel = new ReferenceDialogViewModel()
        //        {
        //            HasReference = value.Tree.TryGetObject(id, out var objectNode),
        //            SelectedNode = objectNode,
        //            Filter       = Filter
        //        };
               
        //        var root = new EditorNodeTreeNode(value.Root, Filter);
        //        referenceDialogViewModel.RootNodes.Add(root);
                
        //        var referenceDialog = new ReferenceDialog()
        //        {
        //            Owner = ownerWindow,
        //            DataContext = referenceDialogViewModel
        //        };

        //        if (referenceDialog.ShowDialog() == true)
        //        {
        //            value.Value = referenceDialogViewModel.HasReference ? ((EditorObject)referenceDialogViewModel.SelectedNode!).Id : 0UL;
        //        }

        //        if (referenceDialogViewModel.HasTargetLocation && referenceDialogViewModel.SelectedNode is EditorObject selectedObjectNode)
        //        {
        //            handleReferenceNavigattion(selectedObjectNode);
        //        }
        //    }
        //    else if (value.Type is EditorEnumType enumType && value.Value is int memberValue)
        //    {
        //        var viewModel = new EnumValueDialogViewModel()
        //        {
        //            EnumType = enumType,
        //            SelectedMember = enumType.Members.First((enumMember) => enumMember.Value == memberValue)
        //        };

        //        var enumValueDialog = new EnumValueDialogView()
        //        {
        //            Owner = ownerWindow,
        //            DataContext = viewModel,
        //            Title = $"Edit Value: '{name}'",
        //        };

        //        if (enumValueDialog.ShowDialog() == true)
        //        {
        //            value.Value = viewModel.SelectedMember.Value;
        //        }
        //    }
        //}

        //public static EditorStructType? GetFirstStructure(this EditorNode node)
        //{
        //    var children = new List<EditorNode>();

        //    foreach (var child in node.Children)
        //    {
        //        if (child.Type is EditorStructType structType)
        //        {
        //            return structType;
        //        }

        //        children.Add(child);
        //    }

        //    foreach (var child in children)
        //    {
        //        var firstStructure = child.GetFirstStructure();
        //        if (firstStructure is not null)
        //        {
        //            return firstStructure;
        //        }
        //    }

        //    return null;
        //}

        //public static void AddAllStructureTypes(this EditorNode node, ISet<EditorStructType> target)
        //{
        //    foreach (var child in node.Children)
        //    {
        //        child.AddAllStructureTypes(target);
        //        if (child.Type is EditorStructType structType)
        //        {
        //            target.Add(structType);
        //            foreach (var baseType in structType.AllBaseTypes)
        //            {
        //                target.Add(baseType);
        //            }
        //        }
        //    }
        //}

        public static JsonTypeBase ToJsonType(this Type type)
        {
            if (type.IsPrimitive || type == typeof(string))
            {
                return new JsonValueType(type);
            }
            else if (type.IsArray)
            {
                return new JsonArrayType(type.GetElementType()!);
            }
            else if(type.IsAssignableTo(typeof(IEnumerable)))
            {
                if (type.IsGenericType)
                {
                    return new JsonArrayType(type.GenericTypeArguments[0]);
                }
                else
                {
                    return new JsonArrayType(typeof(object));
                }
            }
            else
            {
                var parameters = type.GetConstructors().First().GetParameters();

                var fields = parameters.Select((parameter, index) =>
                {
                    return new JsonField()
                    {
                        Name = parameter.Name!,
                        Type = JsonTypeBase.Get(parameter.ParameterType),
                        Index = index,
                    };
                });

                return new JsonStructType(type, fields);
            }
        }

        public static IEnumerable<Type> EnumerateDerivedTypes(this Type baseType) 
            => baseType.Assembly.DefinedTypes.Where((type) => baseType != type && baseType.IsAssignableFrom(type));

        public static IEnumerable<Type> EnumerateImplementedDerivedTypes(this Type baseType)
          => baseType.Assembly.DefinedTypes.Where((type) => baseType != type && !type.IsAbstract && baseType.IsAssignableFrom(type));
    }
}
