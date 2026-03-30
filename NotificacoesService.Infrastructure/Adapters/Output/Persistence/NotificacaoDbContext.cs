using Microsoft.EntityFrameworkCore;
using NotificacoesService.Infrastructure.Adapters.Output.Persistence.Configurations;

namespace NotificacoesService.Infrastructure.Adapters.Output.Persistence;

public sealed class NotificacaoDbContext : DbContext
{
    public NotificacaoDbContext(DbContextOptions<NotificacaoDbContext> options)
        : base(options) { }

    public DbSet<NotificacaoEntity> Notificacoes => Set<NotificacaoEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new NotificacaoConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
