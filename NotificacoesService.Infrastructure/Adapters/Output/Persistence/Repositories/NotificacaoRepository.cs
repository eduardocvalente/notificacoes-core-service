using Microsoft.EntityFrameworkCore;
using NotificacoesService.Domain.Entities;
using NotificacoesService.Domain.Enums;
using NotificacoesService.Domain.Ports.Output;
using NotificacoesService.Infrastructure.Adapters.Output.Persistence.Mappers;

namespace NotificacoesService.Infrastructure.Adapters.Output.Persistence.Repositories;

public sealed class NotificacaoRepository : INotificacaoRepository
{
    private readonly NotificacaoDbContext _context;

    public NotificacaoRepository(NotificacaoDbContext context)
    {
        _context = context;
    }

    public async Task<Notificacao?> ObterPorIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Notificacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? null : NotificacaoMapper.ToDomain(entity);
    }

    public async Task AdicionarAsync(Notificacao notificacao, CancellationToken ct)
    {
        var entity = NotificacaoMapper.ToEntity(notificacao);
        await _context.Notificacoes.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Notificacao notificacao, CancellationToken ct)
    {
        var entity = NotificacaoMapper.ToEntity(notificacao);
        _context.Notificacoes.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Notificacao>> ListarPorDestinatarioAsync(
        Guid destinatarioId, CancellationToken ct)
    {
        var entities = await _context.Notificacoes
            .AsNoTracking()
            .Where(x => x.DestinatarioId == destinatarioId)
            .OrderByDescending(x => x.CriadaEm)
            .ToListAsync(ct);

        return entities.ConvertAll(NotificacaoMapper.ToDomain).AsReadOnly();
    }

    public async Task<IReadOnlyList<Notificacao>> ListarPendentesPorTentativaAsync(
        int maxTentativas, CancellationToken ct)
    {
        var statusFalha = nameof(StatusNotificacao.Falha);

        var entities = await _context.Notificacoes
            .AsNoTracking()
            .Where(x => x.Status == statusFalha && x.TentativasEnvio < maxTentativas)
            .OrderBy(x => x.CriadaEm)
            .ToListAsync(ct);

        return entities.ConvertAll(NotificacaoMapper.ToDomain).AsReadOnly();
    }
}
