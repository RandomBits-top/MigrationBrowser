namespace MigrationBrowser.Implementations
{
    /// <summary>
    /// Provides utility methods for handling command-line arguments.
    /// </summary>
    internal static class ArgumentHelper
    {
        /// <summary>
        /// Safely quotes an argument for command-line usage.
        /// </summary>
        /// <param name="arg">The argument to quote.</param>
        /// <returns>The quoted argument string.</returns>
        public static string QuoteArgument(string arg)
        {
            if (string.IsNullOrEmpty(arg)) 
                return "\"\"";
            
            // Basic safe quoting for command line: escape embedded quotes
            return "\"" + arg.Replace("\"", "\\\"") + "\"";
        }
    }
}