using NotificacoesService.Application.DTOs;
using NotificacoesService.Application.Ports.Output;
using NotificacoesService.Domain.Enums;

namespace NotificacoesService.Infrastructure.Adapters.Output.Templates;

public sealed class RazorTemplateRenderer : ITemplateRenderer
{
    public Task<string> RenderizarAsync(TipoNotificacao tipo, object dados, CancellationToken ct)
    {
        var html = tipo switch
        {
            TipoNotificacao.MatriculaConfirmada => RenderizarMatriculaConfirmada(dados),
            TipoNotificacao.NotaDisponivel => RenderizarNotaDisponivel(dados),
            TipoNotificacao.AtualizacaoCadastral => RenderizarAtualizacaoCadastral(dados),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de notificação sem template definido.")
        };

        return Task.FromResult(html);
    }

    private static string RenderizarMatriculaConfirmada(object dados)
    {
        if (dados is MatriculaRealizadaInput input)
        {
            return $"""
                <!DOCTYPE html>
                <html lang="pt-BR">
                <head><meta charset="utf-8" /><title>Confirmação de Matrícula</title></head>
                <body style="font-family: Arial, sans-serif; color: #333;">
                  <h2 style="color: #1a73e8;">Confirmação de Matrícula</h2>
                  <p>Olá, <strong>{EscapeHtml(input.NomeAluno)}</strong>!</p>
                  <p>Sua matrícula no curso <strong>{EscapeHtml(input.NomeCurso)}</strong> foi confirmada com sucesso.</p>
                  <p><strong>Data da matrícula:</strong> {input.DataMatricula:dd/MM/yyyy}</p>
                  <hr />
                  <p style="font-size: 0.85em; color: #888;">Sistema Escolar — mensagem automática, não responda este e-mail.</p>
                </body>
                </html>
                """;
        }

        return "<html><body><p>Matrícula confirmada.</p></body></html>";
    }

    private static string RenderizarNotaDisponivel(object dados)
    {
        if (dados is NotaLancadaInput input)
        {
            return $"""
                <!DOCTYPE html>
                <html lang="pt-BR">
                <head><meta charset="utf-8" /><title>Nota Disponível</title></head>
                <body style="font-family: Arial, sans-serif; color: #333;">
                  <h2 style="color: #1a73e8;">Nota Disponível</h2>
                  <p>Olá, <strong>{EscapeHtml(input.NomeAluno)}</strong>!</p>
                  <p>A nota da disciplina <strong>{EscapeHtml(input.NomeDisciplina)}</strong> já está disponível.</p>
                  <p><strong>Nota:</strong> {input.Nota:F2}</p>
                  <p><strong>Data de lançamento:</strong> {input.DataLancamento:dd/MM/yyyy}</p>
                  <hr />
                  <p style="font-size: 0.85em; color: #888;">Sistema Escolar — mensagem automática, não responda este e-mail.</p>
                </body>
                </html>
                """;
        }

        return "<html><body><p>Nota disponível.</p></body></html>";
    }

    private static string RenderizarAtualizacaoCadastral(object dados)
    {
        if (dados is AlunoAtualizadoInput input)
        {
            return $"""
                <!DOCTYPE html>
                <html lang="pt-BR">
                <head><meta charset="utf-8" /><title>Atualização Cadastral</title></head>
                <body style="font-family: Arial, sans-serif; color: #333;">
                  <h2 style="color: #1a73e8;">Atualização Cadastral</h2>
                  <p>Olá, <strong>{EscapeHtml(input.NomeAluno)}</strong>!</p>
                  <p>Seus dados cadastrais foram atualizados com sucesso.</p>
                  <p><strong>Campo atualizado:</strong> {EscapeHtml(input.CampoAtualizado)}</p>
                  <p><strong>Data:</strong> {input.DataAtualizacao:dd/MM/yyyy HH:mm}</p>
                  <p>Se você não reconhece esta alteração, entre em contato com a instituição imediatamente.</p>
                  <hr />
                  <p style="font-size: 0.85em; color: #888;">Sistema Escolar — mensagem automática, não responda este e-mail.</p>
                </body>
                </html>
                """;
        }

        return "<html><body><p>Cadastro atualizado.</p></body></html>";
    }

    private static string EscapeHtml(string value) =>
        value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}
