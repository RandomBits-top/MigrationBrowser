using Microsoft.Win32;
using MigrationBrowser.Interfaces;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MigrationBrowser.Implementations
{
    /// <summary>
    /// Manages all registry-related operations for MigrationBrowser.
    /// </summary>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    internal class RegistryManager : IRegistryManager
    {
        private const string UrlPatternsKey = @"Software\MigrationBrowser\UrlPatterns";
        private const string EdgeAppPathKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe";
        private const string AppRegRoot = @"Software\MigrationBrowser";
        private const string ProgId = "MigrationBrowser";

        /// <summary>
        /// Creates per-user registration entries for HTTP/HTTPS protocol handlers.
        /// </summary>
        public void RegisterHttpHttpsHandlers()
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

        /// <summary>
        /// Loads URL patterns from the registry.
        /// </summary>
        /// <returns>A list of valid regex patterns.</returns>
        public List<string> LoadUrlPatterns()
        {
            var list = new List<string>();
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(UrlPatternsKey);
                if (key != null)
                {
                    foreach (string name in key.GetValueNames())
                    {
                        string? val = key.GetValue(name) as string;
                        if (string.IsNullOrWhiteSpace(val)) continue;
                        
                        string pattern = val.Trim();
                        // Validate regex pattern before adding
                        try
                        {
                            // Test pattern with timeout to ensure it's valid and not catastrophic
                            new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(10));
                            list.Add(pattern);
                        }
                        catch (ArgumentException)
                        {
                            // Skip invalid regex patterns
                        }
                        catch (RegexMatchTimeoutException)
                        {
                            // Skip patterns that are too complex
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>
        /// Retrieves the Microsoft Edge executable path from the registry.
        /// </summary>
        /// <returns>The path to Edge if found and valid, otherwise null.</returns>
        public string? GetEdgePath()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(EdgeAppPathKey);
                string? edgePath = key?.GetValue(null) as string;

                // Validate the path exists and is actually Edge
                if (!string.IsNullOrEmpty(edgePath) && File.Exists(edgePath))
                {
                    try
                    {
                        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(edgePath);
                        if (versionInfo.ProductName?.Contains("Microsoft Edge", StringComparison.OrdinalIgnoreCase) ?? false)
                        {
                            return edgePath;
                        }
                    }
                    catch
                    {
                        // If we can't verify it's Edge, don't use it
                    }
                }

                return null;
            }
            catch { return null; }
        }
    }
}