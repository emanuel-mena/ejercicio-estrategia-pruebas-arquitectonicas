using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaTareas.Application.UseCases.CrearTarea;

namespace SistemaTareas.Web.Pages;

public sealed class CrearTareaModel(ICrearTareaUseCase crearTarea) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(160, ErrorMessage = "El título no puede superar 160 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [BindProperty]
    [StringLength(800, ErrorMessage = "La descripción no puede superar 800 caracteres.")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await crearTarea.ExecuteAsync(
            new CrearTareaCommand(Titulo, Descripcion),
            cancellationToken);

        TempData["Success"] = result.Mensaje;
        return RedirectToPage("/Index");
    }
}

