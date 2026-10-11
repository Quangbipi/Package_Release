using UnityEngine;

namespace Quang.Base
{
    public interface IPiggyBankService
    {
        int PiggyGoldProgress { get; }
        void IncreasePiggyProgress();
        void ResetPiggyProgress();
    }
}
