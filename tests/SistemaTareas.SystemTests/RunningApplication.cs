using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace SistemaTareas.SystemTests;

internal sealed class RunningApplication : IAsyncDisposable
{
    private readonly Process _process;
    private readonly string _databasePath;
    private readonly StringBuilder _output;

    private RunningApplication(Process process, string baseUrl, string databasePath, StringBuilder output)
    {
        _process = process;
        BaseUrl = baseUrl;
        _databasePath = databasePath;
        _output = output;
    }

    public string BaseUrl { get; }

    public static async Task<RunningApplication> StartAsync()
    {
        var root = FindRepositoryRoot();
        var port = GetAvailablePort();
        var baseUrl = $"http://127.0.0.1:{port}";
        var databasePath = Path.Combine(Path.GetTempPath(), $"sistema-tareas-{Guid.NewGuid():N}.db");
        var output = new StringBuilder();

        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(Path.Combine(root, "src", "SistemaTareas.Web", "SistemaTareas.Web.csproj"));
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--no-launch-profile");
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(baseUrl);
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
        startInfo.Environment["ConnectionStrings__Tareas"] = $"Data Source={databasePath}";

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, args) => AppendLine(output, args.Data);
        process.ErrorDataReceived += (_, args) => AppendLine(output, args.Data);

        if (!process.Start())
        {
            throw new InvalidOperationException("No fue posible iniciar la aplicación web.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        var application = new RunningApplication(process, baseUrl, databasePath, output);

        try
        {
            await application.WaitUntilHealthyAsync();
            return application;
        }
        catch
        {
            await application.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
        }

        _process.Dispose();

        await DeleteDatabaseFilesAsync();
    }

    private async Task WaitUntilHealthyAsync()
    {
        using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        var deadline = DateTimeOffset.UtcNow.AddSeconds(45);

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (_process.HasExited)
            {
                throw new InvalidOperationException($"La aplicación terminó antes de iniciar.\n{_output}");
            }

            try
            {
                using var response = await client.GetAsync("/health");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(200);
        }

        throw new TimeoutException($"La aplicación no respondió a tiempo.\n{_output}");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SistemaTareas.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("No se encontró la raíz de la solución.");
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static void AppendLine(StringBuilder builder, string? value)
    {
        if (value is not null)
        {
            lock (builder)
            {
                builder.AppendLine(value);
            }
        }
    }

    private async Task DeleteDatabaseFilesAsync()
    {
        var paths = new[] { _databasePath, $"{_databasePath}-wal", $"{_databasePath}-shm" };

        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                foreach (var path in paths.Where(File.Exists))
                {
                    File.Delete(path);
                }

                return;
            }
            catch (IOException) when (attempt < 19)
            {
                await Task.Delay(100);
            }
        }
    }
}
