using System.Diagnostics;
using NUnit.Framework;

namespace Chirp.Playwright.Tests;

[SetUpFixture]
public class PlaywrightTestBase
{
    private static Process? _serverProcess;
    public static string BaseUrl { get; private set; } = "http://localhost:5273";

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
            return;

        var solutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run --project src/Chirp.Web/Chirp.Web.csproj",
            WorkingDirectory = solutionDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        _serverProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start Chirp.Web process.");

        var serverReady = false;
        var timeout = DateTime.UtcNow.AddSeconds(30);

        while (!serverReady && DateTime.UtcNow < timeout)
        {
            var line = _serverProcess.StandardOutput.ReadLine();
            if (line?.Contains("Now listening on:", StringComparison.OrdinalIgnoreCase) == true)
            {
                serverReady = true;
            }
        }

        if (!serverReady)
        {
            _serverProcess.Kill(true);
            throw new TimeoutException("Server did not start within 30 seconds");
        }
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill(true);
            _serverProcess.Dispose();
        }
    }
}
