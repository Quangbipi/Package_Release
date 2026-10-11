using Quang.Base.Persistence;

namespace Quang.IAP
{
    public interface IPurchaseLedger
    {
        bool IsAvailable { get; }

        SaveRecoveryState LoadRecovery { get; }

        PurchaseLedgerState State { get; }

        PurchaseLedgerWriteResult RecordObserved(PurchaseTransactionRecord record);

        PurchaseLedgerWriteResult UpdateState(string transactionId, PurchaseTransactionState state, string code = null);

        bool ContainsCompletedTransaction(string transactionId);
    }
}
