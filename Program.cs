using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MigrationBrowser
{
    internal class Program
    {
        // --------------------------------------------------------------------
        // Configuration
        // --------------------------------------------------------------------
        private const string UrlPatternsKey = @"Software\MigrationBrowser\UrlPatterns";
        private const string EdgeAppPathKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe";
        private const string AppRegRoot = @"Software\MigrationBrowser";
        private static readonly string[] Protocols = { "http", "https" };
        private const string ProgId = "MigrationBrowser";

        // --------------------------------------------------------------------
        static int Main(string[] args)
        {
            // ------------------------------------------------------------
            // 1. Handle --register [--silent]
            // ------------------------------------------------------------
            bool silent = args.Length > 1 && args[1] == "--silent";

            if (args.Length >= 1 && args[0] == "--register")
            {
                RegisterHttpHttpsHandlers();

                if (silent)
                {
                    SetAsDefaultViaAPI();
                    Console.WriteLine("MigrationBrowser registered and set as default (silent mode).");
                }
                else
                {
                    PromptToSetAsDefault();
                    Console.WriteLine("Registration complete.");
                }
                return 0;
            }

            // ------------------------------------------------------------
            // 2. Normal operation: open Edge
            // ------------------------------------------------------------
            string? edgePath = GetEdgePath();
            if (string.IsNullOrEmpty(edgePath))
            {
                MessageBox(IntPtr.Zero, "Microsoft Edge not found in registry.", "MigrationBrowser", 0x10);
                return 1;
            }

            string arguments;

            if (args.Length == 0)
            {
                // No URL → open Edge normally
                arguments = "";
            }
            else
            {
                string url = args[0].Trim();

                var patterns = LoadUrlPatterns();
                bool matches = patterns.Any(p => Regex.IsMatch(url, p, RegexOptions.IgnoreCase));

                arguments = matches
                    ? $"--inprivate \"{url}\""
                    : $"\"{url}\"";
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = edgePath,
                    Arguments = arguments,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox(IntPtr.Zero, $"Failed to start Edge: {ex.Message}", "MigrationBrowser", 0x10);
            }

            return 0;
        }

        // ----------------------------------------------------------------
        // Manual registration
        // ----------------------------------------------------------------
        private static void RegisterHttpHttpsHandlers()
        {
            string exePath = Process.GetCurrentProcess().MainModule!.FileName;
            string command = $"\"{exePath}\" \"%1\"";

            using var progIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}");
            progIdKey.SetValue("", $"URL:{ProgId} Protocol");
            progIdKey.SetValue("URL Protocol", "");

            using var iconKey = progIdKey.CreateSubKey("DefaultIcon");
            iconKey.SetValue("", $"\"{exePath}\",1");

            using var shellKey = progIdKey.CreateSubKey(@"shell\open\command");
            shellKey.SetValue("", command);

            foreach (string proto in Protocols)
            {
                string userChoiceKey = $@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\{proto}\UserChoice";
                using var key = Registry.CurrentUser.CreateSubKey(userChoiceKey);
                key.SetValue("ProgId", ProgId);
                key.SetValue("Hash", "AAAAAAAAAAAAAAAAAAAAAA==");
            }
        }

        // ----------------------------------------------------------------
        // Prompt (only if not --silent)
        // ----------------------------------------------------------------
        private static void PromptToSetAsDefault()
        {
            int result = MessageBox(
                IntPtr.Zero,
                "Set MigrationBrowser as your default web browser?\n\n" +
                "This will allow it to handle all web links and apply InPrivate mode for matching URLs.",
                "MigrationBrowser - Set as Default",
                0x4 | 0x30); // Yes/No + Question

            if (result == 6) // Yes
            {
                try
                {
                    SetAsDefaultViaAPI();
                    MessageBox(IntPtr.Zero, "MigrationBrowser is now your default browser!", "Success", 0x40);
                }
                catch (Exception ex)
                {
                    MessageBox(IntPtr.Zero, $"Failed to set default: {ex.Message}\n\nPlease set it manually in Settings.", "Error", 0x10);
                }
            }
        }

        // ----------------------------------------------------------------
        // Silent: Set as default via API
        // ----------------------------------------------------------------
        private static void SetAsDefaultViaAPI()
        {
            var clsid = new Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376");
            var type = Type.GetTypeFromCLSID(clsid);
            dynamic shell = Activator.CreateInstance(type)!;

            foreach (string proto in Protocols)
            {
                shell.SetAppAsDefault(ProgId, proto, 0);
            }
        }

        // ----------------------------------------------------------------
        // COM & MessageBox
        // ----------------------------------------------------------------
        [ComImport, Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IApplicationAssociationRegistration
        {
            void SetAppAsDefault([MarshalAs(UnmanagedType.LPWStr)] string pszAppId,
                                 [MarshalAs(UnmanagedType.LPWStr)] string pszQuery,
                                 uint atQueryType);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        // ----------------------------------------------------------------
        // Load URL patterns
        // ----------------------------------------------------------------
        private static List<string> LoadUrlPatterns()
        {
            var list = new List<string>();
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(UrlPatternsKey);
                if (key != null)
                {
                    foreach (string name in key.GetValueNames())
                    {
                        string? val = key.GetValue(name) as string;
                        if (!string.IsNullOrWhiteSpace(val))
                            list.Add(val.Trim());
                    }
                }
            }
            catch { }
            return list;
        }

        // ----------------------------------------------------------------
        // Get Edge path
        // ----------------------------------------------------------------
        private static string? GetEdgePath()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(EdgeAppPathKey);
                return key?.GetValue(null) as string;
            }
            catch { return null; }
        }
    }
}