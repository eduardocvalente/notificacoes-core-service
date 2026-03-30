namespace NotificacoesService.Application.DTOs;

/// <summary>Representa uma notificação no histórico do destinatário.</summary>
public sealed record NotificacaoResponse(
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    Guid Id,

    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    Guid DestinatarioId,

    /// <example>aluno@escola.com.br</example>
    string Email,

    /// <example>MatriculaConfirmada</example>
    string Tipo,

    /// <example>Confirmação de Matrícula</example>
    string Assunto,

    /// <example>Enviada</example>
    string Status,

    /// <example>1</example>
    int TentativasEnvio,

    /// <example>2025-08-01T14:30:00Z</example>
    DateTime CriadaEm,

    /// <example>2025-08-01T14:30:05Z</example>
    DateTime? EnviadaEm,

    /// <example>null</example>
    string? MotivoFalha);
