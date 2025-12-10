namespace CG.Test.Editor.FrontEnd.Models
{
    public abstract class SchemaTypeBase
    {
        public abstract bool IsConvertibleFrom(SchemaTypeBase sourceType);
    }
}
