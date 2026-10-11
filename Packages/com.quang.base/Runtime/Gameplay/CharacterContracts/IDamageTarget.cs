using Quang.DesignPattern;
using UnityEngine;

namespace Quang.Base
{
    public interface IDamageTarget
    {
        Transform Tf { get; }
        CHARACTER_TYPE Type { get; }
        GameUnit Source { get; }
    }
}
