using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MigrationBrowser
{
    internal static class Program
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
            bool silent = args.Length > 1 && args[1] == "--silent";

            if (args.Length >= 1 && args[0] == "--register")
            {
                // Create per-user registration entries so Settings will list this app as a candidate.
                try
                {
                    RegisterHttpHttpsHandlers();
                }
                catch (Exception ex)
                {
                    MessageBox(IntPtr.Zero, $"Registration failed: {ex.Message}", "MigrationBrowser", 0x10);
                    return 1;
                }

                if (!silent)
                {
                    // Interactive flow: prompt to open Default Apps so user can make MigrationBrowser the default.
                    PromptToOpenDefaultApps();
                }

                // In silent mode do not show UI; return success after creating HKCU entries.
                return 0;
            }

            // Normal operation: open Edge with or without URL
            string? edgePath = GetEdgePath();
            if (string.IsNullOrEmpty(edgePath))
            {
                MessageBox(IntPtr.Zero, "Microsoft Edge not found in registry.", "MigrationBrowser", 0x10);
                return 1;
            }

            string arguments;
            if (args.Length == 0)
            {
                arguments = "";
            }
            else
            {
                string url = args[0].Trim();
                var patterns = LoadUrlPatterns();
                bool matches = patterns.Any(p => Regex.IsMatch(url, p, RegexOptions.IgnoreCase));
                arguments = matches ? $"--inprivate \"{url}\"" : $"\"{url}\"";
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
                return 1;
            }

            return 0;
        }

        // ----------------------------------------------------------------
        // Create per-user ProgId + Capabilities so Settings lists the app
        // ----------------------------------------------------------------
        private static void RegisterHttpHttpsHandlers()
        {
            string exePath = Process.GetCurrentProcess().MainModule!.FileName;
            string command = $"\"{exePath}\" \"%1\"";

            // 1) ProgId (protocol handler) under HKCU\Software\Classes\<ProgId>
            using (var progIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}"))
            {
                progIdKey.SetValue("", $"URL:{ProgId} Protocol");
                progIdKey.SetValue("URL Protocol", "");
                using var iconKey = progIdKey.CreateSubKey("DefaultIcon");
                iconKey.SetValue("", $"\"{exePath}\",1");
                using var shellKey = progIdKey.CreateSubKey(@"shell\open\command");
                shellKey.SetValue("", command);
            }

            // 2) Capabilities under HKCU\Software\<AppRegRoot>\Capabilities
            string capabilitiesRoot = $@"{AppRegRoot}\Capabilities";
            using (var capKey = Registry.CurrentUser.CreateSubKey(capabilitiesRoot))
            {
                capKey.SetValue("ApplicationName", "MigrationBrowser");
                capKey.SetValue("ApplicationDescription", "Handles web links and opens matching URLs in InPrivate mode");
            }

            // 3) URLAssociations under Capabilities
            using (var urlAssoc = Registry.CurrentUser.CreateSubKey($@"{capabilitiesRoot}\URLAssociations"))
            {
                urlAssoc.SetValue("http", ProgId);
                urlAssoc.SetValue("https", ProgId);
            }

            // 4) Tell Windows where to find the Capabilities (RegisteredApplications)
            using (var regApps = Registry.CurrentUser.CreateSubKey(@"Software\RegisteredApplications"))
            {
                regApps.SetValue("MigrationBrowser", $@"{capabilitiesRoot}");
            }
        }

        // ----------------------------------------------------------------
        // Interactive helper to open Default Apps settings
        // ----------------------------------------------------------------
        private static void PromptToOpenDefaultApps()
        {
            int result = MessageBox(IntPtr.Zero,
                "MigrationBrowser is registered. Do you want to open Default apps settings now so you can select it as the default browser?",
                "MigrationBrowser - Set as Default",
                0x4 | 0x30); // Yes/No + Question

            if (result == 6) // Yes
                OpenDefaultAppsSettings();
        }

        private static void OpenDefaultAppsSettings()
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

        // ----------------------------------------------------------------
        // Load URL patterns from registry
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
        // Get Edge path from registry
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

        // ----------------------------------------------------------------
        // Minimal MessageBox P/Invoke
        // ----------------------------------------------------------------
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        // Safe argument quoting helper (use when building Arguments)
        private static string QuoteArgument(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return "\"\"";
            // Basic safe quoting for command line: escape embedded quotes
            return "\"" + arg.Replace("\"", "\\\"") + "\"";
        }
    }
}