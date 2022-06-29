using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wiremock.OpenAPIValidator.Commands;

namespace Wiremock.OpenAPIValidator.Tests.Commands
{
    public class WiremockResponseReaderCommandHandlerTests
    {
        private WiremockResponseReaderCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _handler = new WiremockResponseReaderCommandHandler();
        }

        [Test]
        public async Task Handle_BadDirectory()
        {
            var response = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = string.Empty
            }, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
            });
        }

        [Test]
        public async Task Handle_BadParentDirectory()
        {
            var response = await _handler.Handle(new WiremockResponseReaderCommand
            {
                WiremockMappingPath = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory())
            }, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.Not.Null);
            });
        }
    }
}
