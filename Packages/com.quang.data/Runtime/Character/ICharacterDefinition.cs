using UnityEngine;

namespace Quang.Data
{
    public abstract class ICharacterDefinition : ScriptableObject
    {
        public string Id;

        public class BuildContext { }

        public abstract ICharacterStats BuildRuntime(BuildContext context = null);
    }
}
