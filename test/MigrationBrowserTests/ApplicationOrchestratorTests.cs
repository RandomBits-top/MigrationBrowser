using MigrationBrowser.Implementations;
using MigrationBrowser.Interfaces;
using NSubstitute;
using Shouldly;

namespace MigrationBrowserTests
{
    public class ApplicationOrchestratorTests
    {
        private readonly IRegistryManager _mockRegistryManager;
        private readonly IUserInteraction _mockUserInteraction;
        private readonly IProcessLauncher _mockProcessLauncher;
        private readonly ApplicationOrchestrator _orchestrator;

        public ApplicationOrchestratorTests()
        {
            _mockRegistryManager = Substitute.For<IRegistryManager>();
            _mockUserInteraction = Substitute.For<IUserInteraction>();
            UrlValidator urlValidator = new();
            _mockProcessLauncher = Substitute.For<IProcessLauncher>();

            _orchestrator = new ApplicationOrchestrator(
                _mockRegistryManager,
                _mockUserInteraction,
                urlValidator,
                _mockProcessLauncher);
        }

        #region Registration Mode Tests

        [Fact]
        public void Execute_WithRegisterFlag_CallsRegisterHttpHttpsHandlers()
        {
            // Arrange
            string[] args = { "--register" };

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockRegistryManager.Received(1).RegisterHttpHttpsHandlers();
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithRegisterFlagAndSilent_DoesNotPromptUser()
        {
            // Arrange
            string[] args = { "--register", "--silent" };

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockRegistryManager.Received(1).RegisterHttpHttpsHandlers();
            _mockUserInteraction.DidNotReceive().PromptToOpenDefaultApps();
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithRegisterFlagWithoutSilent_PromptsUser()
        {
            // Arrange
            string[] args = { "--register" };

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockRegistryManager.Received(1).RegisterHttpHttpsHandlers();
            _mockUserInteraction.Received(1).PromptToOpenDefaultApps();
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithRegisterFlagWhenRegistrationFails_ReturnsError()
        {
            // Arrange
            string[] args = { "--register" };
            _mockRegistryManager.When(x => x.RegisterHttpHttpsHandlers())
                .Do(x => throw new UnauthorizedAccessException("Access denied"));

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockUserInteraction.Received(1).ShowError("Registration failed: Access denied");
            _mockUserInteraction.DidNotReceive().PromptToOpenDefaultApps();
            result.ShouldBe(1);
        }

        #endregion

        #region Browser Launch Mode - No Arguments

        [Fact]
        public void Execute_WithNoArguments_LaunchesEdgeWithoutUrl()
        {
            // Arrange
            string[] args = Array.Empty<string>();
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockRegistryManager.Received(1).GetEdgePath();
            _mockProcessLauncher.Received(1).Launch(edgePath, "");
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithNoArgumentsWhenEdgeNotFound_ReturnsError()
        {
            // Arrange
            string[] args = Array.Empty<string>();
            _mockRegistryManager.GetEdgePath().Returns((string?)null);

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockUserInteraction.Received(1).ShowError("Microsoft Edge not found in registry.");
            _mockProcessLauncher.DidNotReceive().Launch(Arg.Any<string>(), Arg.Any<string>());
            result.ShouldBe(1);
        }

        #endregion

        #region Browser Launch Mode - With Valid URL

        [Fact]
        public void Execute_WithValidHttpUrl_LaunchesEdgeWithUrl()
        {
            // Arrange
            string url = "http://example.com";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string>());

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockRegistryManager.Received(1).GetEdgePath();
            _mockRegistryManager.Received(1).LoadUrlPatterns();
            _mockProcessLauncher.Received(1).Launch(edgePath, $"\"{url}\"");
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithValidHttpsUrl_LaunchesEdgeWithUrl()
        {
            // Arrange
            string url = "https://example.com";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string>());

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockProcessLauncher.Received(1).Launch(edgePath, $"\"{url}\"");
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithUrlMatchingPattern_LaunchesEdgeInPrivateMode()
        {
            // Arrange
            string url = "https://private.example.com";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string> { "private\\.example\\.com" });

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockProcessLauncher.Received(1).Launch(edgePath, $"--inprivate \"{url}\"");
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithUrlNotMatchingPattern_LaunchesEdgeNormally()
        {
            // Arrange
            string url = "https://public.example.com";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string> { "private\\.example\\.com" });

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockProcessLauncher.Received(1).Launch(edgePath, $"\"{url}\"");
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithComplexUrl_LaunchesEdgeWithQuotedUrl()
        {
            // Arrange
            string url = "https://example.com/path?param1=value1&param2=value2";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string>());

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockProcessLauncher.Received(1).Launch(edgePath, $"\"{url}\"");
            result.ShouldBe(0);
        }

        #endregion

        #region Browser Launch Mode - Invalid URLs

        [Fact]
        public void Execute_WithInvalidUrl_ShowsErrorAndDoesNotLaunch()
        {
            // Arrange
            string[] args = { "not-a-valid-url" };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockUserInteraction.Received(1).ShowError("Invalid URL format provided.");
            _mockProcessLauncher.DidNotReceive().Launch(Arg.Any<string>(), Arg.Any<string>());
            result.ShouldBe(1);
        }

        [Fact]
        public void Execute_WithFtpUrl_ShowsErrorAndDoesNotLaunch()
        {
            // Arrange
            string[] args = { "ftp://example.com" };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockUserInteraction.Received(1).ShowError("Only HTTP and HTTPS protocols are supported. Received: ftp");
            _mockProcessLauncher.DidNotReceive().Launch(Arg.Any<string>(), Arg.Any<string>());
            result.ShouldBe(1);
        }

        [Fact]
        public void Execute_WithFileUrl_ShowsErrorAndDoesNotLaunch()
        {
            // Arrange
            string[] args = { "file:///c:/test.txt" };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockUserInteraction.Received(1).ShowError("Only HTTP and HTTPS protocols are supported. Received: file");
            _mockProcessLauncher.DidNotReceive().Launch(Arg.Any<string>(), Arg.Any<string>());
            result.ShouldBe(1);
        }

        #endregion

        #region Browser Launch - Process Launch Failures

        [Fact]
        public void Execute_WhenProcessLaunchFails_ShowsErrorAndReturnsError()
        {
            // Arrange
            string url = "https://example.com";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string>());
            _mockProcessLauncher.When(x => x.Launch(Arg.Any<string>(), Arg.Any<string>()))
                .Throw(new InvalidOperationException("Process start failed"));

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockUserInteraction.Received(1).ShowError("Failed to start Edge: Process start failed");
            result.ShouldBe(1);
        }

        [Fact]
        public void Execute_WhenProcessLaunchThrowsUnauthorizedException_ShowsErrorAndReturnsError()
        {
            // Arrange
            string url = "https://example.com";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string>());
            _mockProcessLauncher.When(x => x.Launch(Arg.Any<string>(), Arg.Any<string>()))
                .Throw(new UnauthorizedAccessException("Access denied"));

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockUserInteraction.Received(1).ShowError("Failed to start Edge: Access denied");
            result.ShouldBe(1);
        }

        #endregion

        #region URL Patterns Integration

        [Fact]
        public void Execute_WithMultiplePatterns_UsesInPrivateWhenAnyMatches()
        {
            // Arrange
            string url = "https://secure.example.com";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string> 
            { 
                "banking\\.com",
                "secure\\.example\\.com",
                "private\\.org"
            });

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockProcessLauncher.Received(1).Launch(edgePath, $"--inprivate \"{url}\"");
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithEmptyPatternList_LaunchesNormally()
        {
            // Arrange
            string url = "https://example.com";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string>());

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockProcessLauncher.Received(1).Launch(edgePath, $"\"{url}\"");
            result.ShouldBe(0);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Execute_WithUrlContainingQuotes_ProperlyEscapesQuotes()
        {
            // Arrange
            string url = "https://example.com/path?param=\"value\"";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string>());

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockProcessLauncher.Received(1).Launch(edgePath, Arg.Is<string>(s => s.Contains("\\\"")));
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithUrlContainingSpaces_ProperlyQuotesUrl()
        {
            // Arrange
            string url = "https://example.com/path with spaces";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string>());

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockProcessLauncher.Received(1).Launch(edgePath, Arg.Is<string>(s => s.StartsWith("\"") && s.EndsWith("\"")));
            result.ShouldBe(0);
        }

        [Fact]
        public void Execute_WithUrlHavingWhitespace_TrimsUrl()
        {
            // Arrange
            string url = "  https://example.com  ";
            string[] args = { url };
            string edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";
            _mockRegistryManager.GetEdgePath().Returns(edgePath);
            _mockRegistryManager.LoadUrlPatterns().Returns(new List<string>());

            // Act
            int result = _orchestrator.Execute(args);

            // Assert
            _mockProcessLauncher.Received(1).Launch(edgePath, "\"https://example.com\"");
            result.ShouldBe(0);
        }

        #endregion
    }
}