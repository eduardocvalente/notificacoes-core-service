namespace NotificacoesService.Application.DTOs;

public sealed record EmailMessage(
    string Para,
    string NomeDestinatario,
    string Assunto,
    string Corpo
);
