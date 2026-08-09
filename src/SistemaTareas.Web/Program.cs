using SistemaTareas.Application;
using SistemaTareas.Infrastructure;
using SistemaTareas.Infrastructure.Persistence;
using SistemaTareas.Infrastructure.Seed;
using SistemaTareas.Web.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddApplication();

var connectionString = builder.Configuration.GetConnectionString("Tareas")
    ?? "Data Source=App_Data/tareas.db";

if (connectionString.Contains("App_Data", StringComparison.OrdinalIgnoreCase))
{
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
}

builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapAsignacionEndpoints();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TareasDbContext>();
    var extraTasks = builder.Configuration.GetValue<int>("DemoData:ExtraPendingTasks");
    await DatabaseSeeder.InitializeAsync(dbContext, extraTasks);
}

await app.RunAsync();

public partial class Program;

