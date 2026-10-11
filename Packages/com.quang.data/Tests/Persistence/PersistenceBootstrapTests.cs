using System;
using Quang.Base.Persistence;
using Quang.Data.Persistence;
using NUnit.Framework;

namespace Quang.Data.Tests.Persistence
{
    public class PersistenceBootstrapTests
    {
        [Test]
        public void InstallDefault_DoesNotOverwriteExplicitServiceFactory()
        {
            Func<IPersistenceService> previous = Database.ServiceFactory;
            var service = new PersistenceService(new InMemorySaveStore());
            Func<IPersistenceService> explicitFactory = () => service;
            try
            {
                Database.ServiceFactory = explicitFactory;

                PersistenceBootstrap.InstallDefault();

                Assert.That(Database.ServiceFactory, Is.SameAs(explicitFactory));
            }
            finally
            {
                Database.ServiceFactory = previous;
            }
        }
    }
}
