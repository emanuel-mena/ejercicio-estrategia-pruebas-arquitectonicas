using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaTareas.Application.Queries;
using SistemaTareas.Application.UseCases.AsignarTarea;

namespace SistemaTareas.Web.Pages;

public sealed class AsignarTareaModel(
    IConsultaTareas consulta,
    IAsignarTareaUseCase asignarTarea) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid TareaId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Seleccione una persona responsable.")]
    public Guid? UsuarioId { get; set; }

    public TareaResumen? Tarea { get; private set; }

    public IReadOnlyList<SelectListItem> Usuarios { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await CargarDatosAsync(cancellationToken);
        return Tarea is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || UsuarioId is null)
        {
            await CargarDatosAsync(cancellationToken);
            return Page();
        }

        var result = await asignarTarea.ExecuteAsync(
            new AsignarTareaCommand(TareaId, UsuarioId.Value),
            cancellationToken);

        if (result.EsExitoso)
        {
            TempData["Success"] = result.Mensaje;
            return RedirectToPage("/AsignarTarea", new { tareaId = TareaId });
        }

        ModelState.AddModelError(string.Empty, result.Mensaje);
        await CargarDatosAsync(cancellationToken);
        return Page();
    }

    private async Task CargarDatosAsync(CancellationToken cancellationToken)
    {
        var tareas = await consulta.ListarTareasAsync(cancellationToken);
        Tarea = tareas.SingleOrDefault(tarea => tarea.Id == TareaId);

        var usuarios = await consulta.ListarUsuariosActivosAsync(cancellationToken);
        Usuarios = usuarios
            .Select(usuario => new SelectListItem(usuario.Nombre, usuario.Id.ToString()))
            .ToArray();
    }
}

