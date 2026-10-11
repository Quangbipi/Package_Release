using Quang.Base;
using NUnit.Framework;
using UnityEngine;

namespace Quang.Data.Tests
{
    // Locator.Data is a private static field behind a public get/set property (Base
    // namespace, partial class spread across per-domain files) - ResetDataForTests()
    // was added to Locator.Data.cs (Packages/com.quang.data/Runtime/Base/Locator.Data.cs)
    // specifically to give tests a clean-slate hook, user-approved production change.
    public class LocatorCycleTests
    {
        private class FakeDataService : IDataService
        {
            public T GetData<T>(int index = 0) where T : class => null;
            public T GetSOData<T>() where T : ScriptableObject => null;
            public T GetUnit<T>(int type) where T : class => null;
            public void Save() { }
        }

        [TearDown]
        public void TearDown()
        {
            Quang.Base.Locator.ResetDataForTests();
        }

        [Test]
        public void Register_Resolve_ReturnsSameInstance()
        {
            var service = new FakeDataService();

            Quang.Base.Locator.Data = service;

            Assert.AreSame(service, Quang.Base.Locator.Data);
        }

        [Test]
        public void Reset_ClearsRegisteredInstance()
        {
            Quang.Base.Locator.Data = new FakeDataService();

            Quang.Base.Locator.ResetDataForTests();

            Assert.IsNull(Quang.Base.Locator.Data);
        }

        [Test]
        public void RegisterAfterReset_ResolvesNewInstance()
        {
            var first = new FakeDataService();
            Quang.Base.Locator.Data = first;
            Quang.Base.Locator.ResetDataForTests();

            var second = new FakeDataService();
            Quang.Base.Locator.Data = second;

            Assert.AreSame(second, Quang.Base.Locator.Data);
            Assert.AreNotSame(first, Quang.Base.Locator.Data);
        }
    }
}
