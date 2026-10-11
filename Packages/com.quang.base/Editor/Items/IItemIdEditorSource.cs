using System.Collections.Generic;

namespace Quang.Base.Editor
{
    public interface IItemIdEditorSource
    {
        IEnumerable<ItemIdEditorOption> GetOptions();
    }
}
