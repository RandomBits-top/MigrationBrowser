using MigrationBrowser.Interfaces;
using System.Diagnostics;

namespace MigrationBrowser.Implementations
{
    /// <summary>
    /// Handles launching processes with specified arguments.
    /// </summary>
    internal class ProcessLauncher : IProcessLauncher
    {
        /// <summary>
        /// Launches a process with the specified executable path and arguments.
        /// </summary>
        /// <param name="executablePath">The path to the executable.</param>
        /// <param name="arguments">The command-line arguments to pass to the process.</param>
        public void Launch(string executablePath, string arguments)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                UseShellExecute = true
            });
        }
    }
}