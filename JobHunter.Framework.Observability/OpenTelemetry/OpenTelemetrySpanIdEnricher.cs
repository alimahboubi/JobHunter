using OpenTelemetry.Trace;
using Serilog.Core;
using Serilog.Events;

namespace JobHunter.Framework.Observability.OpenTelemetry;

public class OpenTelemetrySpanIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var spanId = Tracer.CurrentSpan.Context.SpanId.ToHexString();
        if (!string.IsNullOrWhiteSpace(spanId))
        {
            var spanIdProperty = new LogEventProperty("SpanId", new ScalarValue(spanId));
            logEvent.AddOrUpdateProperty(spanIdProperty);
        }
    }
}