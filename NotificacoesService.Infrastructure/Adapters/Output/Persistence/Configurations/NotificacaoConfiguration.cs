using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NotificacoesService.Infrastructure.Adapters.Output.Persistence.Configurations;

public sealed class NotificacaoConfiguration : IEntityTypeConfiguration<NotificacaoEntity>
{
    public void Configure(EntityTypeBuilder<NotificacaoEntity> builder)
    {
        builder.ToTable("notificacoes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.DestinatarioId)
            .HasColumnName("destinatario_id")
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Assunto)
            .HasColumnName("assunto")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Corpo)
            .HasColumnName("corpo")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.TentativasEnvio)
            .HasColumnName("tentativas_envio")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.CriadaEm)
            .HasColumnName("criada_em")
            .IsRequired();

        builder.Property(x => x.EnviadaEm)
            .HasColumnName("enviada_em");

        builder.Property(x => x.MotivoFalha)
            .HasColumnName("motivo_falha");

        builder.HasIndex(x => x.DestinatarioId)
            .HasDatabaseName("ix_notificacoes_destinatario_id");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_notificacoes_status");
    }
}
