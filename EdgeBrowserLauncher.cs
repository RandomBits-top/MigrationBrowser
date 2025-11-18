using System.Diagnostics;

namespace MigrationBrowser
{
    /// <summary>
    /// Handles launching Microsoft Edge browser with specified arguments.
    /// </summary>
    internal class EdgeBrowserLauncher
    {
        /// <summary>
        /// Launches Microsoft Edge with the specified arguments.
        /// </summary>
        /// <param name="edgePath">The path to the Edge executable.</param>
        /// <param name="arguments">The command-line arguments to pass to Edge.</param>
        public void Launch(string edgePath, string arguments)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = edgePath,
                Arguments = arguments,
                UseShellExecute = true
            });
        }
    }
}