namespace NotificacoesService.Application.Options;

public sealed class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
}
