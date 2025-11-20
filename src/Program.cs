using MigrationBrowser.Implementations;
using MigrationBrowser.Interfaces;

namespace MigrationBrowser
{
    /// <summary>
    /// Main entry point for MigrationBrowser application.
    /// This is a "Humble Object" that only handles infrastructure concerns
    /// (creating concrete instances) and delegates all business logic to ApplicationOrchestrator.
    /// </summary>
    internal static class Program
    {
        static int Main(string[] args)
        {
            // Create concrete implementations (infrastructure)
            IRegistryManager registryManager = new RegistryManager();
            IUserInteraction userInteraction = new UserInteraction();
            UrlValidator urlValidator = new UrlValidator();
            IProcessLauncher processLauncher = new ProcessLauncher();

            // Create orchestrator with dependencies (business logic)
            var orchestrator = new ApplicationOrchestrator(
                registryManager,
                userInteraction,
                urlValidator,
                processLauncher);

            // Execute application logic
            return orchestrator.Execute(args);
        }
    }
}