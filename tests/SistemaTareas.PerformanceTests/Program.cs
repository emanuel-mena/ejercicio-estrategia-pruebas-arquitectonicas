using System.Collections.Concurrent;
using System.Net.Http.Json;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;

var baseUrl = Environment.GetEnvironmentVariable("SISTEMA_TAREAS_BASE_URL")
    ?? "http://localhost:5080";
var durationSeconds = int.TryParse(
    Environment.GetEnvironmentVariable("PERFORMANCE_DURATION_SECONDS"),
    out var configuredDuration)
    ? configuredDuration
    : 60;

using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
var pendingTasks = await client.GetFromJsonAsync<TareaDto[]>("/api/tareas/pendientes")
    ?? [];

if (pendingTasks.Length < 100)
{
    Console.Error.WriteLine(
        "Se requieren al menos 100 tareas pendientes. Inicie la app con DemoData__ExtraPendingTasks=10000.");
    return 2;
}

var taskIds = new ConcurrentQueue<Guid>(pendingTasks.Select(task => task.Id));
var scenario = Scenario.Create("asignar_tarea", async _ =>
    {
        if (!taskIds.TryDequeue(out var taskId))
        {
            return Response.Fail("NO_TASKS", "No quedan tareas pendientes para asignar.", 0, 0);
        }

        using var response = await client.PostAsJsonAsync(
            $"/api/tareas/{taskId}/asignacion",
            new { usuarioId = Guid.Parse("10000000-0000-0000-0000-000000000001") });
        await Task.Delay(100);

        return response.IsSuccessStatusCode
            ? Response.Ok()
            : Response.Fail(statusCode: response.StatusCode.ToString());
    })
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.KeepConstant(copies: 10, during: TimeSpan.FromSeconds(durationSeconds)))
    .WithThresholds(
        Threshold.Create(scenarioStats => scenarioStats.Ok.Latency.Percent95 <= 500),
        Threshold.Create(scenarioStats => scenarioStats.Ok.Latency.Percent99 <= 1000),
        Threshold.Create(scenarioStats => scenarioStats.Fail.Request.Percent <= 1));

var stats = NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFolder(Path.Combine(Environment.CurrentDirectory, "artifacts", "performance"))
    .WithReportFormats(ReportFormat.Html)
    .Run();

var scenarioStats = stats.ScenarioStats.Single(value => value.ScenarioName == "asignar_tarea");
var p95 = scenarioStats.Ok.Latency.Percent95;
var p99 = scenarioStats.Ok.Latency.Percent99;
var errorRate = scenarioStats.Fail.Request.Percent;

Console.WriteLine($"Umbrales: p95={p95:F2} ms, p99={p99:F2} ms, errores={errorRate:F2}%");
return stats.Thresholds.Any(threshold => threshold.IsFailed) ? 1 : 0;

internal sealed record TareaDto(Guid Id);
