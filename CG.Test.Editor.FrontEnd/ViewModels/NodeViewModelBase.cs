using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public class NodeViewModelGenerator() : VisitorBase<SchemaTypeBase, NodeViewModelBase>
    {
        public ArrayNodeViewModel Visit(SchemaArrayType arrayType) => new ArrayNodeViewModel(arrayType);

        public ObjectNodeViewModel Visit(SchemaObjectType objectType) => new ObjectNodeViewModel(objectType);

        public NumberNodeViewModel Visit(SchemaNumberType numberType) => new NumberNodeViewModel(numberType, numberType.Minimum);

        public IntegerNodeViewModel Visit(SchemaIntegerType integerType) => new IntegerNodeViewModel(integerType, integerType.Minimum);

        public StringNodeViewModel Visit(SchemaStringType stringType) => new StringNodeViewModel(stringType, string.Empty);
    }

    public abstract partial class NodeViewModelBase : ObservableObject
    {
        public abstract SchemaTypeBase Type { get; }

        public abstract NodeViewModelBase Clone();
    }
}
