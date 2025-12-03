using CG.Test.Editor.FrontEnd.ViewModels;
using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models
{
    public abstract class SchemaTypeBase
    {
        public abstract bool IsConvertibleFrom(SchemaTypeBase sourceType);
    }
}
