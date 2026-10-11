using Quang.Data.Persistence;
using NUnit.Framework;

namespace Quang.Persistence.Tests
{
    [TestFixture]
    public class OwnedRootLayoutTests
    {
        [Test]
        public void DirectoryNames_MatchesTheDirectoriesFileSaveStoreAndEvidenceStoreCreate()
        {
            CollectionAssert.AreEquivalent(
                new[] { "primary", "backup", "quarantine", "receipts" },
                OwnedRootLayout.DirectoryNames);
        }
    }
}
