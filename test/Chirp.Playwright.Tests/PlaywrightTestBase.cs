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

        await Task.Delay(5000);
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill();
            _serverProcess.Dispose();
        }
    }
}
