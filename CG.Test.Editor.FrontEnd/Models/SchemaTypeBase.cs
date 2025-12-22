namespace CG.Test.Editor.FrontEnd.Models
{
    public abstract class SchemaTypeBase
    {
        public virtual bool IsValueType => false;

        public abstract bool IsConvertibleFrom(SchemaTypeBase sourceType);
    }
}
