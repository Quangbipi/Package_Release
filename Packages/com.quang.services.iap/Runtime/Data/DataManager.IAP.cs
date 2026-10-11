using UnityEngine;

namespace Quang.Data
{
    using System;

    // com.quang.services.iap's own data registration. Compiled into Quang.Data via this folder's
    // Quang.Data.asmref, so DataManager keeps serializing iapData by the same field name the
    // DataManager prefab already references. Lives here, not in com.quang.data, because a project
    // without the IAP package installed has no IAPData type at all - com.quang.data referencing it
    // directly made the whole data package uncompilable without IAP.
    public partial class DataManager
    {
        [SerializeField]
        private IAPData iapData;

        partial void TryGetServiceSOData<T>(ref T result) where T : ScriptableObject
        {
            if (typeof(T) == typeof(IAPData))
            {
                result = iapData as T;
            }
        }
    }
}
