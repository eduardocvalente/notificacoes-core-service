using Microsoft.Extensions.DependencyInjection;
using NotificacoesService.Application.Ports.Input;
using NotificacoesService.Application.UseCases;

namespace NotificacoesService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProcessarMatriculaRealizadaUseCase, ProcessarMatriculaRealizadaUseCase>();
        services.AddScoped<IProcessarNotaLancadaUseCase, ProcessarNotaLancadaUseCase>();
        services.AddScoped<IProcessarAlunoAtualizadoUseCase, ProcessarAlunoAtualizadoUseCase>();
        services.AddScoped<IReenviarNotificacaoUseCase, ReenviarNotificacaoUseCase>();
        services.AddScoped<IConsultarNotificacoesUseCase, ConsultarNotificacoesUseCase>();

        return services;
    }
}
