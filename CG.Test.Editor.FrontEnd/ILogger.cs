using System;
using System.Collections.Generic;
using System.Text;

namespace CG.Test.Editor.FrontEnd
{
    public interface ILogger<TItem>
    {
        void Log(TItem item);
    }
}
