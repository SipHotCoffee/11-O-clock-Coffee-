namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public abstract class LinkedSchemaTypeBase
    {
        public virtual bool IsValueType => false;

        public abstract bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType);
    }
}
