using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LocalizedString.Tests;

public class LocalizedStringReaderTests
{
    [Fact]
    public void Indexer_ReturnsLocalizedString()
    {
        // Arrange
        var mockFactory = Substitute.For<ILocalizedStringFactory>();
        var mockLogger = Substitute.For<ILogger<LocalizedStringReader<object>>>();
        var mockLocalizer = Substitute.For<ILocalizedStringReader>();

        mockLocalizer[Arg.Any<string>()].Returns(callInfo => $"Localized_{callInfo.Arg<string>()}");
        mockFactory.Create(typeof(object)).Returns(mockLocalizer);

        var reader = new LocalizedStringReader<object>(mockFactory, mockLogger);

        // Act
        var result = reader["TestString"];

        // Assert
        Assert.Equal("Localized_TestString", result);
    }

    [Fact]
    public void Indexer_WithArguments_ReturnsFormattedLocalizedString()
    {
        // Arrange
        var mockFactory = Substitute.For<ILocalizedStringFactory>();
        var mockLogger = Substitute.For<ILogger<LocalizedStringReader<object>>>();
        var mockLocalizer = Substitute.For<ILocalizedStringReader>();

        mockLocalizer[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(callInfo => $"Localized_{callInfo.Arg<string>()}_{string.Join("_", callInfo.Arg<object[]>())}");
        mockFactory.Create(typeof(object)).Returns(mockLocalizer);

        var reader = new LocalizedStringReader<object>(mockFactory, mockLogger);

        // Act
        var result = reader["TestString", 1, 2, 3];

        // Assert
        Assert.Equal("Localized_TestString_1_2_3", result);
    }

    [Fact]
    public void Indexer_ReturnsLocalizedString_FromDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<string, string>
            {
                { "TestString", "Localized_TestString" }
            };
        var reader = new LocalizedStringReader(dictionary);

        // Act
        var result = reader["TestString"];

        // Assert
        Assert.Equal("Localized_TestString", result);
    }

    [Fact]
    public void Indexer_WithArguments_ReturnsFormattedLocalizedString_FromDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<string, string>
            {
                { "TestString", "Localized_TestString_{0}_{1}_{2}" }
            };
        var reader = new LocalizedStringReader(dictionary);

        // Act
        var result = reader["TestString", 1, 2, 3];

        // Assert
        Assert.Equal("Localized_TestString_1_2_3", result);
    }

    [Fact]
    public void GetAllStrings_ReturnsAllKeys_FromDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<string, string>
            {
                { "String1", "Localized_String1" },
                { "String2", "Localized_String2" },
                { "String3", "Localized_String3" }
            };
        var reader = new LocalizedStringReader(dictionary);

        // Act
        var result = reader.GetAllStrings(false);

        // Assert
        Assert.Equal(dictionary.Keys, result);
    }
}