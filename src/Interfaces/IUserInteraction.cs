namespace MigrationBrowser.Interfaces
{
    /// <summary>
    /// Abstraction for user interaction operations to enable testing.
    /// </summary>
    public interface IUserInteraction
    {
        /// <summary>
        /// Displays an error message to the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        void ShowError(string message);

        /// <summary>
        /// Prompts the user to open the Default Apps settings.
        /// </summary>
        void PromptToOpenDefaultApps();
    }
}