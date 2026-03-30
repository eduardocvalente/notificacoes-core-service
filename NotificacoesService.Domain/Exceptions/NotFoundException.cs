namespace NotificacoesService.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string recurso, object id)
        : base($"'{recurso}' com identificador '{id}' não foi encontrado.") { }
}
