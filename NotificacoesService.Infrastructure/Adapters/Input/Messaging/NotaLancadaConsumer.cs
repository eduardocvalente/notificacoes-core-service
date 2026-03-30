using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;
using NotificacoesService.Application.Options;
using NotificacoesService.Application.Ports.Input;
using NotificacoesService.Infrastructure.Adapters.Input.Messaging.Events;

namespace NotificacoesService.Infrastructure.Adapters.Input.Messaging;

public sealed class NotaLancadaConsumer : ConsumerBase<NotaLancadaEvent>
{
    private readonly BrokerOptions _brokerOptions;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NotaLancadaConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<BrokerOptions> brokerOptions,
        ILogger<NotaLancadaConsumer> logger)
        : base(logger, scopeFactory)
    {
        _brokerOptions = brokerOptions.Value;
    }

    protected override async IAsyncEnumerable<NotaLancadaEvent> ReceberEventosAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _brokerOptions.Host,
            Port = _brokerOptions.Port,
            VirtualHost = _brokerOptions.VirtualHost,
            UserName = _brokerOptions.Username,
            Password = _brokerOptions.Password
        };

        var canal = Channel.CreateUnbounded<NotaLancadaEvent>();

        await using var connection = await factory.CreateConnectionAsync(ct);
        await using var rabbitChannel = await connection.CreateChannelAsync(cancellationToken: ct);

        await rabbitChannel.QueueDeclareAsync(
            queue: _brokerOptions.NotaLancadaQueue,
            durable: true, exclusive: false, autoDelete: false,
            arguments: null, cancellationToken: ct);

        await rabbitChannel.BasicQosAsync(
            prefetchSize: 0, prefetchCount: 10, global: false, ct);

        var consumer = new AsyncEventingBasicConsumer(rabbitChannel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var @event = JsonSerializer.Deserialize<NotaLancadaEvent>(json, JsonOptions);

                if (@event is not null)
                    await canal.Writer.WriteAsync(@event, ct);

                await rabbitChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao deserializar NotaLancadaEvent.");
                await rabbitChannel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await rabbitChannel.BasicConsumeAsync(
            queue: _brokerOptions.NotaLancadaQueue,
            autoAck: false, consumer: consumer, cancellationToken: ct);

        Logger.LogInformation(
            "Aguardando mensagens na fila {Queue}.", _brokerOptions.NotaLancadaQueue);

        await foreach (var @event in canal.Reader.ReadAllAsync(ct))
            yield return @event;
    }

    protected override async Task<Result> ProcessarEventoAsync(
        NotaLancadaEvent evento,
        IServiceProvider services,
        string correlationId,
        CancellationToken ct)
    {
        var input = new NotaLancadaInput(
            AlunoId: evento.AlunoId,
            EmailAluno: evento.EmailAluno,
            NomeAluno: evento.NomeAluno,
            NomeDisciplina: evento.NomeDisciplina,
            Nota: evento.Nota,
            DataLancamento: evento.DataLancamento,
            CorrelationId: correlationId);

        var useCase = services.GetRequiredService<IProcessarNotaLancadaUseCase>();
        return await useCase.ExecutarAsync(input, ct);
    }
}
