using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LocalizedString.Tests
{
    public class LocalizedStringFactoryTest
    {
        private readonly ILocalizedStringResources _mockResources;
        private readonly ILogger<LocalizedStringFactory> _mockLogger;
        private readonly LocalizedStringFactory _factory;

        public LocalizedStringFactoryTest()
        {
            _mockResources = Substitute.For<ILocalizedStringResources>();
            _mockLogger = Substitute.For<ILogger<LocalizedStringFactory>>();
            _factory = new LocalizedStringFactory(_mockResources, _mockLogger);
        }

        [Fact]
        public async Task Create_WithTypeResourceSource_CallsCreateWithFullName()
        {
            // Arrange
            var resourceSource = typeof(LocalizedStringFactoryTest);
            var expectedFullName = resourceSource.FullName;

            // Act
            var result = await _factory.Create(resourceSource);

            // Assert
            _mockResources.Received(1).Read(expectedFullName);
        }

        [Fact]
        public async Task Create_WithBaseName_LogsAndCreatesLocalizedStringReader()
        {
            // Arrange
            var baseName = "TestResource";
            var dictionary = new Dictionary<string, string> { { "key", "value" } };
            _mockResources.Read(baseName).Returns(dictionary);

            // Act
            var result = await _factory.Create(baseName);

            // Assert
            _mockLogger.Received().Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString().Contains($"Creating AppStringLocalizer for {baseName}")),
                null,
                Arg.Any<Func<object, Exception, string>>());

            _mockLogger.Received().Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString().Contains($"AppStringLocalizer for {baseName} created with {dictionary.Count} entries")),
                null,
                Arg.Any<Func<object, Exception, string>>());

            Assert.NotNull(result);
            Assert.IsType<LocalizedStringReader>(result);
        }
    }
}