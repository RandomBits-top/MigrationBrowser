using System.Text.RegularExpressions;

namespace MigrationBrowser.Implementations
{
    /// <summary>
    /// Handles URL validation and pattern matching operations.
    /// </summary>
    internal class UrlValidator
    {
        /// <summary>
        /// Validates that the URL has proper format and supported protocol.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <param name="uri">The parsed URI if validation succeeds.</param>
        /// <param name="errorMessage">The error message if validation fails.</param>
        /// <returns>True if validation succeeds, false otherwise.</returns>
        public bool ValidateUrl(string url, out Uri? uri, out string? errorMessage)
        {
            uri = null;
            errorMessage = null;

            // Validate URL format
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                errorMessage = "Invalid URL format provided.";
                return false;
            }

            // Validate protocol
            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                errorMessage = $"Only HTTP and HTTPS protocols are supported. Received: {uri.Scheme}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the URL matches any of the provided regex patterns.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <param name="patterns">The list of regex patterns to match against.</param>
        /// <returns>True if the URL matches any pattern, false otherwise.</returns>
        public bool MatchesAnyPattern(string url, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                try
                {
                    if (Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)))
                    {
                        return true;
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    // Skip patterns that timeout
                    continue;
                }
            }

            return false;
        }
    }
}