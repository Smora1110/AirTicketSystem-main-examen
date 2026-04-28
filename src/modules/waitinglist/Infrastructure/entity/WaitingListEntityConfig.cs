// src/modules/waitinglist/Infrastructure/entity/WaitingListEntityConfig.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirTicketSystem.modules.waitinglist.Infrastructure.entity;

public class WaitingListEntityConfig : IEntityTypeConfiguration<WaitingListEntity>
{
    public void Configure(EntityTypeBuilder<WaitingListEntity> builder)
    {
        builder.ToTable("lista_espera");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(w => w.ReservaId).HasColumnName("reserva_id").IsRequired();
        builder.Property(w => w.VueloId).HasColumnName("vuelo_id").IsRequired();
        builder.Property(w => w.FechaRegistro).HasColumnName("fecha_registro").IsRequired();
        builder.Property(w => w.Prioridad).HasColumnName("prioridad").IsRequired();
        builder.Property(w => w.Estado).HasColumnName("estado").HasMaxLength(15)
            .IsRequired().HasDefaultValue("PENDIENTE");

        builder.ToTable(t => t.HasCheckConstraint("chk_estado_lista_espera",
            "estado IN ('PENDIENTE','PROMOVIDO','CANCELADO')"));

        builder.HasIndex(w => new { w.ReservaId, w.VueloId }).IsUnique();

        builder.HasOne(w => w.Reserva)
            .WithMany()
            .HasForeignKey(w => w.ReservaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Vuelo)
            .WithMany()
            .HasForeignKey(w => w.VueloId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
