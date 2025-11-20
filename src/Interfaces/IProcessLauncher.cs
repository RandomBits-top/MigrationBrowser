namespace MigrationBrowser.Interfaces
{
    /// <summary>
    /// Abstraction for process launching operations to enable testing.
    /// </summary>
    public interface IProcessLauncher
    {
        /// <summary>
        /// Launches a process with the specified executable path and arguments.
        /// </summary>
        /// <param name="executablePath">The path to the executable.</param>
        /// <param name="arguments">The command-line arguments to pass to the process.</param>
        void Launch(string executablePath, string arguments);
    }
}