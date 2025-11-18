namespace MigrationBrowser
{
    /// <summary>
    /// Main entry point for MigrationBrowser application.
    /// Orchestrates the application flow by delegating to specialized components.
    /// </summary>
    internal static class Program
    {
        static int Main(string[] args)
        {
            var registryManager = new RegistryManager();
            var userInteraction = new UserInteraction();
            var urlValidator = new UrlValidator();
            var browserLauncher = new EdgeBrowserLauncher();

            bool silent = args.Length > 1 && args[1] == "--silent";

            // Handle registration mode
            if (args.Length >= 1 && args[0] == "--register")
            {
                return HandleRegistration(registryManager, userInteraction, silent);
            }

            // Handle normal operation mode (launch browser)
            return HandleBrowserLaunch(args, registryManager, userInteraction, urlValidator, browserLauncher);
        }

        /// <summary>
        /// Handles the registration of HTTP/HTTPS protocol handlers.
        /// </summary>
        private static int HandleRegistration(RegistryManager registryManager, UserInteraction userInteraction, bool silent)
        {
            try
            {
                registryManager.RegisterHttpHttpsHandlers();
            }
            catch (Exception ex)
            {
                userInteraction.ShowError($"Registration failed: {ex.Message}");
                return 1;
            }

            if (!silent)
            {
                userInteraction.PromptToOpenDefaultApps();
            }

            return 0;
        }

        /// <summary>
        /// Handles launching the browser with the provided URL or without arguments.
        /// </summary>
        private static int HandleBrowserLaunch(
            string[] args,
            RegistryManager registryManager,
            UserInteraction userInteraction,
            UrlValidator urlValidator,
            EdgeBrowserLauncher browserLauncher)
        {
            // Get Edge path
            string? edgePath = registryManager.GetEdgePath();
            if (string.IsNullOrEmpty(edgePath))
            {
                userInteraction.ShowError("Microsoft Edge not found in registry.");
                return 1;
            }

            // Build arguments
            string? arguments = BuildBrowserArguments(args, registryManager, userInteraction, urlValidator);
            if (arguments is null)
            {
                return 1; // Error already shown to user
            }

            // Launch browser
            try
            {
                browserLauncher.Launch(edgePath, arguments);
            }
            catch (Exception ex)
            {
                userInteraction.ShowError($"Failed to start Edge: {ex.Message}");
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Builds the command-line arguments for launching the browser.
        /// </summary>
        private static string? BuildBrowserArguments(
            string[] args,
            RegistryManager registryManager,
            UserInteraction userInteraction,
            UrlValidator urlValidator)
        {
            // No URL provided
            if (args.Length == 0)
            {
                return "";
            }

            string url = args[0].Trim();

            // Validate URL
            if (!urlValidator.ValidateUrl(url, out Uri? uri, out string? errorMessage))
            {
                userInteraction.ShowError(errorMessage!);
                return null;
            }

            // Check if URL matches any patterns
            var patterns = registryManager.LoadUrlPatterns();
            bool matches = urlValidator.MatchesAnyPattern(url, patterns);

            // Build arguments based on pattern match
            string quotedUrl = ArgumentHelper.QuoteArgument(url);
            return matches ? $"--inprivate {quotedUrl}" : quotedUrl;
        }
    }
}