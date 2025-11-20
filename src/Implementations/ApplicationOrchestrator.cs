using MigrationBrowser.Interfaces;

namespace MigrationBrowser.Implementations
{
    /// <summary>
    /// Orchestrates the application flow by delegating to specialized components.
    /// This class contains the business logic and depends only on abstractions (interfaces)
    /// for testability, following the Humble Object pattern.
    /// </summary>
    internal class ApplicationOrchestrator
    {
        private readonly IRegistryManager _registryManager;
        private readonly IUserInteraction _userInteraction;
        private readonly UrlValidator _urlValidator;
        private readonly IProcessLauncher _processLauncher;

        public ApplicationOrchestrator(
            IRegistryManager registryManager,
            IUserInteraction userInteraction,
            UrlValidator urlValidator,
            IProcessLauncher processLauncher)
        {
            _registryManager = registryManager;
            _userInteraction = userInteraction;
            _urlValidator = urlValidator;
            _processLauncher = processLauncher;
        }

        /// <summary>
        /// Executes the application logic based on command-line arguments.
        /// </summary>
        public int Execute(string[] args)
        {
            bool silent = args.Length > 1 && args[1] == "--silent";

            // Handle registration mode
            if (args.Length >= 1 && args[0] == "--register")
            {
                return HandleRegistration(silent);
            }

            // Handle normal operation mode (launch browser)
            return HandleBrowserLaunch(args);
        }

        /// <summary>
        /// Handles the registration of HTTP/HTTPS protocol handlers.
        /// </summary>
        private int HandleRegistration(bool silent)
        {
            try
            {
                _registryManager.RegisterHttpHttpsHandlers();
            }
            catch (Exception ex)
            {
                _userInteraction.ShowError($"Registration failed: {ex.Message}");
                return 1;
            }

            if (!silent)
            {
                _userInteraction.PromptToOpenDefaultApps();
            }

            return 0;
        }

        /// <summary>
        /// Handles launching the browser with the provided URL or without arguments.
        /// </summary>
        private int HandleBrowserLaunch(string[] args)
        {
            // Get Edge path
            string? edgePath = _registryManager.GetEdgePath();
            if (string.IsNullOrEmpty(edgePath))
            {
                _userInteraction.ShowError("Microsoft Edge not found in registry.");
                return 1;
            }

            // Build arguments
            string? arguments = BuildBrowserArguments(args);
            if (arguments is null)
            {
                return 1; // Error already shown to user
            }

            // Launch browser
            try
            {
                _processLauncher.Launch(edgePath, arguments);
            }
            catch (Exception ex)
            {
                _userInteraction.ShowError($"Failed to start Edge: {ex.Message}");
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Builds the command-line arguments for launching the browser.
        /// </summary>
        private string? BuildBrowserArguments(string[] args)
        {
            // No URL provided
            if (args.Length == 0)
            {
                return "";
            }

            string url = args[0].Trim();

            // Validate URL
            if (!_urlValidator.ValidateUrl(url, out Uri? uri, out string? errorMessage))
            {
                _userInteraction.ShowError(errorMessage!);
                return null;
            }

            // Check if URL matches any patterns
            var patterns = _registryManager.LoadUrlPatterns();
            bool matches = _urlValidator.MatchesAnyPattern(url, patterns);

            // Build arguments based on pattern match
            string quotedUrl = ArgumentHelper.QuoteArgument(url);
            return matches ? $"--inprivate {quotedUrl}" : quotedUrl;
        }
    }
}