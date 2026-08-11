using Xunit;
using Microsoft.Playwright;

namespace SistemaTareas.SystemTests;

public sealed class AsignarTareaSystemTests
{
    [Fact]
    public async Task UsuarioAsignaTareaDesdeLaInterfazYElCambioPersiste()
    {
        await using var application = await RunningApplication.StartAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        await page.GotoAsync(application.BaseUrl);
        await page.GetByTestId("assign-link").First.ClickAsync();
        await page.GetByTestId("assignee-select").SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await page.GetByTestId("assign-submit").ClickAsync();

        await Assertions.Expect(page.GetByTestId("success-alert"))
            .ToContainTextAsync("se asignó correctamente");

        await page.ReloadAsync();
        await Assertions.Expect(page.GetByTestId("assigned-message"))
            .ToContainTextAsync("Ana Rodríguez");
    }
}
