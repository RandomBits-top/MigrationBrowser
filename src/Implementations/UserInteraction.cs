using MigrationBrowser.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MigrationBrowser.Implementations
{
    /// <summary>
    /// Handles user interaction operations including message boxes and system settings.
    /// </summary>
    internal class UserInteraction : IUserInteraction
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        /// <summary>
        /// Displays an error message box to the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        public void ShowError(string message)
        {
            MessageBox(IntPtr.Zero, message, "MigrationBrowser", 0x10);
        }

        /// <summary>
        /// Prompts the user to open the Default Apps settings.
        /// </summary>
        public void PromptToOpenDefaultApps()
        {
            int result = MessageBox(IntPtr.Zero,
                "MigrationBrowser is registered. Do you want to open Default apps settings now so you can select it as the default browser?",
                "MigrationBrowser - Set as Default",
                0x4 | 0x30); // Yes/No + Question

            if (result == 6) // Yes
                OpenDefaultAppsSettings();
        }

        /// <summary>
        /// Opens the Windows Default Apps settings page.
        /// </summary>
        private void OpenDefaultAppsSettings()
        {
            try
            {
                Process.Start(new ProcessStartInfo("ms-settings:defaultapps") { UseShellExecute = true });
            }
            catch
            {
                try
                {
                    Process.Start(new ProcessStartInfo("control.exe", "/name Microsoft.DefaultPrograms") { UseShellExecute = true });
                }
                catch
                {
                    MessageBox(IntPtr.Zero, "Unable to open Default Apps settings. Please open Settings → Apps → Default apps and select MigrationBrowser.", "MigrationBrowser", 0x10);
                }
            }
        }
    }
}