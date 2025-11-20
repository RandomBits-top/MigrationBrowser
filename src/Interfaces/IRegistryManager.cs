namespace MigrationBrowser.Interfaces
{
    /// <summary>
    /// Abstraction for registry-related operations to enable testing.
    /// </summary>
    public interface IRegistryManager
    {
        /// <summary>
        /// Creates per-user registration entries for HTTP/HTTPS protocol handlers.
        /// </summary>
        void RegisterHttpHttpsHandlers();

        /// <summary>
        /// Loads URL patterns from the registry.
        /// </summary>
        /// <returns>A list of valid regex patterns.</returns>
        List<string> LoadUrlPatterns();

        /// <summary>
        /// Retrieves the Microsoft Edge executable path from the registry.
        /// </summary>
        /// <returns>The path to Edge if found and valid, otherwise null.</returns>
        string? GetEdgePath();
    }
}