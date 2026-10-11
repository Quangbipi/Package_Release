using Quang.Base;

namespace Quang.IAP
{
    public interface IPurchaseCatalogProvider
    {
        int Version { get; }

        bool TryGet(PurchaseProductId id, out PurchaseCatalogEntry entry);

        bool TryResolveStoreId(string storeName, string storeProductId, out PurchaseCatalogEntry entry);
    }
}
