using MigrationBrowser.Implementations;
using Shouldly;

namespace MigrationBrowserTests
{
    public class ArgumentHelperTests
    {
        [Fact]
        public void QuoteArgument_WithNullString_ReturnsEmptyQuotes()
        {
            // Arrange
            string? arg = null;

            // Act
            string result = ArgumentHelper.QuoteArgument(arg!);

            // Assert
            result.ShouldBe("\"\"");
        }

        [Fact]
        public void QuoteArgument_WithEmptyString_ReturnsEmptyQuotes()
        {
            // Arrange
            string arg = "";

            // Act
            string result = ArgumentHelper.QuoteArgument(arg);

            // Assert
            result.ShouldBe("\"\"");
        }

        [Fact]
        public void QuoteArgument_WithSimpleString_ReturnsQuotedString()
        {
            // Arrange
            string arg = "https://example.com";

            // Act
            string result = ArgumentHelper.QuoteArgument(arg);

            // Assert
            result.ShouldBe("\"https://example.com\"");
        }

        [Fact]
        public void QuoteArgument_WithStringContainingQuotes_EscapesQuotes()
        {
            // Arrange
            string arg = "test\"value\"here";

            // Act
            string result = ArgumentHelper.QuoteArgument(arg);

            // Assert
            result.ShouldBe("\"test\\\"value\\\"here\"");
        }

        [Fact]
        public void QuoteArgument_WithStringContainingSpaces_ReturnsQuotedString()
        {
            // Arrange
            string arg = "hello world test";

            // Act
            string result = ArgumentHelper.QuoteArgument(arg);

            // Assert
            result.ShouldBe("\"hello world test\"");
        }

        [Fact]
        public void QuoteArgument_WithComplexUrl_ReturnsQuotedUrl()
        {
            // Arrange
            string arg = "https://example.com/path?query=value&param=test";

            // Act
            string result = ArgumentHelper.QuoteArgument(arg);

            // Assert
            result.ShouldBe("\"https://example.com/path?query=value&param=test\"");
        }
    }
}