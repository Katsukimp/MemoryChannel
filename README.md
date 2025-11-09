# MemoryEventBus - High-Performance In-Memory Event Bus

High-performance in-memory event bus para .NET8 baseado em `System.Threading.Channels`, focado em throughput, baixa latência e observabilidade opcional.

## Motivação
MediatR e outros event dispatchers são ótimos para orquestração/pipelines, porém:
- Overhead de reflection / pipeline em cenários de altíssimo volume
- Falta de controle explícito sobre backpressure / profundidade de fila
- Observabilidade limitada sem customização

`MemoryEventBus` fornece um barramento **simples**, **rápido** e **observável** para comunicação interna em processos (in-process pub/sub). Não substitui mensagerias distribuídas.

## Principais Características
- Channel por tipo de evento (unbounded ou bounded)
- Publicação assíncrona e consumo com `BackgroundService`
- Métricas (counters, histogram, gauge) usando `System.Diagnostics.Metrics`
- Tracing opcional (ActivitySource)
- Retry com políticas plugáveis (Exponential / Linear)
- Error handling centralizado
- Health check opcional (profundidade + liveness de consumo)
- Baixo overhead (sem reflection para publish/consume)

## Quando Usar
Ideal para:
- Alto volume de eventos in-process
- Orquestração interna em microsserviço (antes de externalizar para fila real)
- Pipelines curtos de processamento
- Necessidade de métricas e tracing por tipo de evento

Evite usar se você precisa de:
- Persistência / durabilidade de mensagens
- Escala multi-processo / distribuída
- Ordenação global garantida
- Retries após reinício do processo

## Estrutura do Repositório
```
src/
 MemoryEventBus.Domain/ (Contratos e implementações base)
 MemoryEventBus.Infrastructure/ (Enhancements: métricas, tracing, retry, health)
 samples/MinimalSample/ (Exemplo mínimo)
 tests/MemoryEventBus.Tests/ (Testes unitários)
```

## Instalação
Enquanto não há pacote NuGet público:
1. Adicione referências de projeto onde for usar:
```
dotnet add <SeuProjeto> reference ../MemoryEventBus.Domain/MemoryEventBus.Domain.csproj
dotnet add <SeuProjeto> reference ../MemoryEventBus.Infrastructure/MemoryEventBus.Infrastructure.csproj
```
2. Registre no `Program.cs` ou no builder do Host:
```csharp
services.AddMemoryEventBus(o =>
{
 o.EnableMetrics = true; // default true
 o.EnableTracing = false; // habilite se estiver usando Activity/OTel
 o.EnableHealthChecks = false; // registre health check se usar endpoints
});
```
Ou modo simplificado:
```csharp
services.AddMemoryEventBus(enableMetrics: true);
```

## Exemplo Mínimo (baseado em samples/MinimalSample)
```csharp
var host = Host.CreateDefaultBuilder(args)
 .ConfigureServices(s =>
 {
 s.AddMemoryEventBus(enableMetrics: true);
 s.AddHostedService<SampleConsumer>();
 }).Build();

await host.StartAsync();
var manager = host.Services.GetRequiredService<IEventChannelManager>();
await manager.TryWriteAsync(new SampleEvent { EventId = Guid.NewGuid().ToString(), EventName = "SampleEvent", OccurredOn = DateTime.UtcNow });
```
Consumer:
```csharp
public sealed class SampleEvent : DomainEvent { }
public sealed class SampleConsumer(IEventChannelManager manager, ILogger<SampleConsumer> logger) : BackgroundService
{
 private readonly Channel<SampleEvent> _channel = manager.GetOrCreateChannel<SampleEvent>();
 protected override async Task ExecuteAsync(CancellationToken ct)
 {
 await foreach (var ev in _channel.Reader.ReadAllAsync(ct))
 logger.LogInformation("Consumed {EventName} {Id}", ev.EventName, ev.EventId);
 }
}
```

## Consumidores Avançados
Use a classe base para retry, métricas e tracing:
```csharp
public class PaymentConfirmedConsumer : BaseEventConsumer<PaymentConfirmedEvent>
{
 public PaymentConfirmedConsumer(IEventChannelManager mgr, ILogger<BaseEventConsumer<PaymentConfirmedEvent>> log,
 IEventBusErrorHandler errors, IRetryPolicy retry, IEventBusMetrics? metrics, EventBusActivitySource? tracing)
 : base(mgr, log, errors, retry, metrics, tracing) { }

 protected override ValueTask ProcessEventAsync(PaymentConfirmedEvent @event, CancellationToken ct)
 {
 // Lógica de negócio
 return ValueTask.CompletedTask;
 }
}
```

## Bounded Channels (Backpressure)
```csharp
var channel = manager.GetOrCreateBoundedChannel<MyEvent>(capacity:500);
```
Quando `FullMode = Wait` o produtor aguarda vaga (suaviza pico). Ajuste capacidade conforme memória e SLA.

## Retry Policies
Implementação padrão disponível em `RetryPolicies.cs`:
- `ExponentialBackoffRetryPolicy`
- `LinearRetryPolicy`
Ambas respeitam exceções não-transientes (Argument/InvalidOperation => não faz retry).

Para customizar implemente `IRetryPolicy`.

## Métricas Expostas
Meter: `MemoryEventBus`
- Counter `eventbus_events_published_total`
- Counter `eventbus_events_consumed_total`
- Counter `eventbus_events_failed_total`
- Counter `eventbus_retry_attempts_total`
- Histogram `eventbus_event_processing_duration_seconds`
- ObservableGauge `eventbus_channel_depth`

Integração OpenTelemetry (exemplo rápido):
```csharp
builder.Services.AddOpenTelemetry()
 .WithMetrics(m => m.AddMeter("MemoryEventBus"));
```

## Tracing
Ative com `EnableTracing = true`. Produz `Activity` para publish/consume (tags:
- `eventbus.publish.success`
- `eventbus.consume.success`
- `eventbus.consume.error`
)
Integre com OTel: `AddSource("MemoryEventBus")` (se ajustar ActivitySource para nome fixo).

## Health Check
Registre com `EnableHealthChecks = true` e depois exponha em `MapHealthChecks`. O health check avalia profundidade e estados internos conforme `EventBusHealthOptions`.

## Error Handling
Override de comportamento via implementação de `IEventBusErrorHandler` (ex: logging estruturado, DLQ externa, métricas customizadas).

## Encerramento / Drain
`BaseEventConsumer` tenta drenar eventos restantes em `StopAsync` para reduzir perda durante shutdown gracioso.

## Testes
Executar:
```
dotnet test
```
Inclui testes de concorrência e gerenciamento de channels.

## Roadmap
- [ ] Publicar pacote NuGet
- [ ] Benchmarks comparativos automatizados vs MediatR
- [ ] Suporte a filtros/pipelines opcionais
- [ ] Suporte a prioridades de eventos
- [ ] Instrumentação OpenTelemetry dedicada (ActivitySource nomeado)

## Boas Práticas
- Separar contratos de evento em assembly compartilhado
- Evitar lógica pesada no consumer sem cancellation awareness
- Monitorar profundidade de canais para detectar backpressure
- Usar bounded channels quando risco de explosão de memória

## Limitações Atuais
- In-memory (não persiste)
- Não distribuído / não garante entrega em caso de crash
- Sem ordenação cross-type (apenas FIFO por channel)

## Comparação Resumida com MediatR
| Aspecto | MemoryEventBus | MediatR |
|---------|----------------|---------|
| Throughput | Alto (channels) | Médio/Alto |
| Pipelines/Behaviors | Manual | Integrado |
| Observabilidade | Métricas + opcional tracing | Extensível (custom) |
| Backpressure | Bounded channel | Não nativo |
| Distribuição | Não | Não |

## Licença
MIT (adicionar arquivo LICENSE se necessário).

## Contribuições
PRs são bem-vindos. Mantenha foco em simplicidade e baixo overhead.

---
Feedback e melhorias são bem-vindos.

