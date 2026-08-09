using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTareas.Domain.Entities;

namespace SistemaTareas.Infrastructure.Persistence.Configurations;

internal sealed class TareaConfiguration : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> builder)
    {
        builder.ToTable("Tareas");
        builder.HasKey(tarea => tarea.Id);
        builder.Property(tarea => tarea.Titulo).HasMaxLength(160).IsRequired();
        builder.Property(tarea => tarea.Descripcion).HasMaxLength(800).IsRequired();
        builder.Property(tarea => tarea.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(tarea => tarea.Version).IsConcurrencyToken().IsRequired();

        builder
            .HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(tarea => tarea.UsuarioAsignadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tarea => tarea.Estado);
    }
}

