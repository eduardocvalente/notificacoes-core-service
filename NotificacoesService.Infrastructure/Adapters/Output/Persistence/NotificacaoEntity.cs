namespace NotificacoesService.Infrastructure.Adapters.Output.Persistence;

public sealed class NotificacaoEntity
{
    public Guid Id { get; set; }
    public Guid DestinatarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TentativasEnvio { get; set; }
    public DateTime CriadaEm { get; set; }
    public DateTime? EnviadaEm { get; set; }
    public string? MotivoFalha { get; set; }
}
