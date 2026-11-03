using System.Collections.Generic;
using NUnit.Framework;

namespace Quang.Audio.Tests
{
    public sealed class SfxSourcePoolTests
    {
        private sealed class FakeSource
        {
            public bool Busy;
        }

        [Test]
        public void Rent_WhenFreeSourceExists_ReturnsFreeSource()
        {
            var a = new FakeSource { Busy = true };
            var b = new FakeSource();
            var pool = new SfxSourcePool<FakeSource>(new List<FakeSource> { a, b }, s => s.Busy);

            Assert.AreSame(b, pool.Rent());
        }

        [Test]
        public void Rent_WhenAllBusy_ReturnsOldest()
        {
            var a = new FakeSource();
            var b = new FakeSource();
            var pool = new SfxSourcePool<FakeSource>(new List<FakeSource> { a, b }, s => s.Busy);
            pool.Rent(); // a
            a.Busy = true;
            pool.Rent(); // b
            b.Busy = true;

            Assert.AreSame(a, pool.Rent());
            Assert.AreSame(b, pool.Rent());
        }
    }
}
