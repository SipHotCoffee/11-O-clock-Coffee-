namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public abstract class SchemaTypeBase
    {
        public virtual bool IsValueType => false;

        public abstract bool IsConvertibleFrom(SchemaTypeBase sourceType);
    }
}
