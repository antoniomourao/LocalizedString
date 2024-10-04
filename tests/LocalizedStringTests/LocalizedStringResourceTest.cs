using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LocalizedString.Tests
{
    public class LocalizedStringResourcesTests
    {
        private readonly ILogger<LocalizedStringResources> _loggerMock;
        private readonly ILocalizedStringOptions _optionsMock;
        private readonly LocalizedStringResources _localizedStringResources;

        public LocalizedStringResourcesTests()
        {
            _loggerMock = Substitute.For<ILogger<LocalizedStringResources>>();
            _optionsMock = Substitute.For<ILocalizedStringOptions>();
            _optionsMock.ResourcesPath.Returns("Resources");
            _optionsMock.ResourcesExtension.Returns("strings");
            _optionsMock.SupportedCultures.Returns(new[] { "en-US", "fr-FR" });

            _localizedStringResources = new LocalizedStringResources(_optionsMock, _loggerMock);
        }

        [Theory]
        [InlineData("", "en-US")]
        [InlineData("es-ES", "en-US")]
        [InlineData("en-US", "en-US")]
        [InlineData("fr-FR", "fr-FR")]
        public void GetCultureName_WhenNotSupported_ShouldReturnValidCulture(string cultureName, string expectedCultureName)
        {
            // Arrange

            // Act
            var result = _localizedStringResources.getValidCultureName(cultureName);

            // Assert
            Assert.Equal(result, expectedCultureName);
        }

        [Theory]
        [InlineData("", "en-US")]
        [InlineData("pt-PT", "en-US")]
        [InlineData("en-US", "en-US")]
        [InlineData("fr-FR", "fr-FR")]
        public void SetCultureName_ValidCultureName_SetsValidCultureName(string cultureName, string expectedCultureName)
        {
            // Arrange

            // Act
            _localizedStringResources.SetCultureName(cultureName);

            // Assert
            Assert.Equal(_localizedStringResources.GetCurrentCultureName, expectedCultureName);
        }

        [Theory]
        [InlineData("json")]
        [InlineData("strings")]
        public void Write_ValidContent_WritesToFile(string resourceExtension)
        {
            // Arrange
            _optionsMock.ResourcesExtension.Returns(resourceExtension);
            var baseName = "test";
            var content = new Dictionary<string, string> { { "key", "value" } };
            var filePath = "Resources";
            var fileName = Path.Combine(filePath, $"test.{_optionsMock.ResourcesExtension}");

            Directory.CreateDirectory(filePath);
            if (File.Exists(fileName)) File.Delete(fileName);

            // Act
            _localizedStringResources.Write(baseName, content);

            // Assert
            Assert.True(File.Exists(fileName));
            var writtenContent = _localizedStringResources.readResourceTypeContent(fileName);
            Assert.Equal(content, writtenContent);
        }

        [Theory]
        [InlineData("json")]
        [InlineData("strings")]
        public void Read_ValidBaseName_ReturnsContent(string resourceExtension)
        {
            // Arrange
            _optionsMock.ResourcesExtension.Returns(resourceExtension);
            var localizedStringResources = new LocalizedStringResources(_optionsMock, _loggerMock);

            var baseName = "test";
            var content = new Dictionary<string, string> { { "key", "value" } };
            var filePath = "Resources";
            var fileName = Path.Combine(filePath, $"test.{_optionsMock.ResourcesExtension}");

            Directory.CreateDirectory(filePath);
            if (File.Exists(fileName)) File.Delete(fileName);
            var fileContent = localizedStringResources.processResourceTypeContent(content);
            File.WriteAllText(fileName, fileContent);

            // Act
            var result = localizedStringResources.Read(baseName);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public void Read_InvalidBaseName_ReturnsEmptyDictionary()
        {
            // Arrange
            var baseName = "nonexistent";

            // Act
            var result = _localizedStringResources.Read(baseName);

            // Assert
            Assert.Empty(result);
        }
    }
}