// src/modules/reprogramacion/Infrastructure/entity/RescheduleHistoryEntityConfig.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirTicketSystem.modules.reprogramacion.Infrastructure.entity;

public class RescheduleHistoryEntityConfig : IEntityTypeConfiguration<RescheduleHistoryEntity>
{
    public void Configure(EntityTypeBuilder<RescheduleHistoryEntity> builder)
    {
        builder.ToTable("historial_reprogramacion");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(h => h.ReservaId).HasColumnName("reserva_id").IsRequired();
        builder.Property(h => h.VueloAnteriorId).HasColumnName("vuelo_anterior_id").IsRequired();
        builder.Property(h => h.VueloNuevoId).HasColumnName("vuelo_nuevo_id").IsRequired();
        builder.Property(h => h.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(h => h.Motivo).HasColumnName("motivo").HasMaxLength(300).IsRequired();
        builder.Property(h => h.UsuarioId).HasColumnName("usuario_id");

        builder.HasOne(h => h.Reserva)
            .WithMany()
            .HasForeignKey(h => h.ReservaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.VueloAnterior)
            .WithMany()
            .HasForeignKey(h => h.VueloAnteriorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.VueloNuevo)
            .WithMany()
            .HasForeignKey(h => h.VueloNuevoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
