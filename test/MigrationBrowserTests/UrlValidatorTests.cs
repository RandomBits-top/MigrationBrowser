using MigrationBrowser.Implementations;
using Shouldly;

namespace MigrationBrowserTests
{
    public class UrlValidatorTests
    {
        private readonly UrlValidator _validator;

        public UrlValidatorTests()
        {
            _validator = new UrlValidator();
        }

        #region ValidateUrl Tests

        [Fact]
        public void ValidateUrl_WithValidHttpUrl_ReturnsTrue()
        {
            // Arrange
            string url = "http://example.com";

            // Act
            bool result = _validator.ValidateUrl(url, out Uri? uri, out string? errorMessage);

            // Assert
            result.ShouldBeTrue();
            uri.ShouldNotBeNull();
            uri.Scheme.ShouldBe("http");
            errorMessage.ShouldBeNull();
        }

        [Fact]
        public void ValidateUrl_WithValidHttpsUrl_ReturnsTrue()
        {
            // Arrange
            string url = "https://example.com";

            // Act
            bool result = _validator.ValidateUrl(url, out Uri? uri, out string? errorMessage);

            // Assert
            result.ShouldBeTrue();
            uri.ShouldNotBeNull();
            uri.Scheme.ShouldBe("https");
            errorMessage.ShouldBeNull();
        }

        [Fact]
        public void ValidateUrl_WithComplexUrl_ReturnsTrue()
        {
            // Arrange
            string url = "https://example.com:8080/path/to/resource?query=value&param=test#anchor";

            // Act
            bool result = _validator.ValidateUrl(url, out Uri? uri, out string? errorMessage);

            // Assert
            result.ShouldBeTrue();
            uri.ShouldNotBeNull();
            uri.Scheme.ShouldBe("https");
            errorMessage.ShouldBeNull();
        }

        [Fact]
        public void ValidateUrl_WithInvalidFormat_ReturnsFalse()
        {
            // Arrange
            string url = "not-a-valid-url";

            // Act
            bool result = _validator.ValidateUrl(url, out Uri? uri, out string? errorMessage);

            // Assert
            result.ShouldBeFalse();
            uri.ShouldBeNull();
            errorMessage.ShouldBe("Invalid URL format provided.");
        }

        [Fact]
        public void ValidateUrl_WithFtpProtocol_ReturnsFalse()
        {
            // Arrange
            string url = "ftp://example.com";

            // Act
            bool result = _validator.ValidateUrl(url, out Uri? uri, out string? errorMessage);

            // Assert
            result.ShouldBeFalse();
            uri.ShouldNotBeNull(); // URI is created but validation fails
            errorMessage.ShouldBe("Only HTTP and HTTPS protocols are supported. Received: ftp");
        }

        [Fact]
        public void ValidateUrl_WithFileProtocol_ReturnsFalse()
        {
            // Arrange
            string url = "file:///c:/test.txt";

            // Act
            bool result = _validator.ValidateUrl(url, out Uri? uri, out string? errorMessage);

            // Assert
            result.ShouldBeFalse();
            uri.ShouldNotBeNull();
            errorMessage.ShouldBe("Only HTTP and HTTPS protocols are supported. Received: file");
        }

        [Fact]
        public void ValidateUrl_WithMailtoProtocol_ReturnsFalse()
        {
            // Arrange
            string url = "mailto:test@example.com";

            // Act
            bool result = _validator.ValidateUrl(url, out Uri? uri, out string? errorMessage);

            // Assert
            result.ShouldBeFalse();
            uri.ShouldNotBeNull();
            errorMessage.ShouldBe("Only HTTP and HTTPS protocols are supported. Received: mailto");
        }

        #endregion

        #region MatchesAnyPattern Tests

        [Fact]
        public void MatchesAnyPattern_WithMatchingPattern_ReturnsTrue()
        {
            // Arrange
            string url = "https://example.com/test";
            var patterns = new List<string> { "example\\.com" };

            // Act
            bool result = _validator.MatchesAnyPattern(url, patterns);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void MatchesAnyPattern_WithMultiplePatternsOneMatches_ReturnsTrue()
        {
            // Arrange
            string url = "https://example.com/test";
            var patterns = new List<string> 
            { 
                "nomatch\\.com",
                "example\\.com",
                "another\\.com"
            };

            // Act
            bool result = _validator.MatchesAnyPattern(url, patterns);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void MatchesAnyPattern_WithNoMatchingPattern_ReturnsFalse()
        {
            // Arrange
            string url = "https://example.com/test";
            var patterns = new List<string> { "nomatch\\.com", "different\\.org" };

            // Act
            bool result = _validator.MatchesAnyPattern(url, patterns);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public void MatchesAnyPattern_WithEmptyPatternList_ReturnsFalse()
        {
            // Arrange
            string url = "https://example.com/test";
            var patterns = new List<string>();

            // Act
            bool result = _validator.MatchesAnyPattern(url, patterns);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public void MatchesAnyPattern_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            string url = "https://EXAMPLE.COM/test";
            var patterns = new List<string> { "example\\.com" };

            // Act
            bool result = _validator.MatchesAnyPattern(url, patterns);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void MatchesAnyPattern_WithComplexRegexPattern_ReturnsTrue()
        {
            // Arrange
            string url = "https://example.com/api/v1/users/123";
            var patterns = new List<string> { "/api/v\\d+/users/\\d+" };

            // Act
            bool result = _validator.MatchesAnyPattern(url, patterns);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void MatchesAnyPattern_WithWildcardPattern_ReturnsTrue()
        {
            // Arrange
            string url = "https://subdomain.example.com/test";
            var patterns = new List<string> { ".*\\.example\\.com" };

            // Act
            bool result = _validator.MatchesAnyPattern(url, patterns);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void MatchesAnyPattern_WithTimeoutPattern_SkipsAndContinues()
        {
            // Arrange - Create a catastrophically backtracking pattern that will timeout
            string url = "https://example.com/test";
            var patterns = new List<string> 
            { 
                "(a+)+b",  // This pattern can cause exponential backtracking
                "example\\.com"  // Valid pattern that should match
            };

            // Act
            bool result = _validator.MatchesAnyPattern(url, patterns);

            // Assert
            result.ShouldBeTrue(); // Should match the second pattern after first times out
        }

        #endregion
    }
}