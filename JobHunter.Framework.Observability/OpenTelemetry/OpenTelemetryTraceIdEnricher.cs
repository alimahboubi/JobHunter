using OpenTelemetry.Trace;
using Serilog.Core;
using Serilog.Events;

namespace JobHunter.Framework.Observability.OpenTelemetry;

public class OpenTelemetryTraceIdEnricher: ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var traceId = Tracer.CurrentSpan.Context.TraceId.ToHexString();
        if (!string.IsNullOrWhiteSpace(traceId))
        {
            var traceIdProperty = new LogEventProperty("TraceId", new ScalarValue(traceId));
            logEvent.AddOrUpdateProperty(traceIdProperty);
        }
    }
}