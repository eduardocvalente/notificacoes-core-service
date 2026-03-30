namespace NotificacoesService.Application.Options;

public sealed class BrokerOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string MatriculaRealizadaQueue { get; set; } = "matricula-realizada";
    public string NotaLancadaQueue { get; set; } = "nota-lancada";
    public string AlunoAtualizadoQueue { get; set; } = "aluno-atualizado";
    public string Exchange { get; set; } = "escola.events";
}
