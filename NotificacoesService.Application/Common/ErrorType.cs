namespace NotificacoesService.Application.Common;

public enum ErrorType
{
    Validation,   // entrada inválida → 400
    NotFound,     // recurso não encontrado → 404
    Conflict,     // estado inválido / regra de negócio violada → 409
    Unexpected    // erro técnico não mapeado → 500
}
