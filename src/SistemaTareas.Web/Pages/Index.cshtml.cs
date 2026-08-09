using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaTareas.Application.Queries;

namespace SistemaTareas.Web.Pages;

public sealed class IndexModel(IConsultaTareas consulta) : PageModel
{
    public IReadOnlyList<TareaResumen> Tareas { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Tareas = await consulta.ListarTareasAsync(cancellationToken);
    }
}

