using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace JobHunter.Framework.Observability.OpenTelemetry;

public static class TelemetryConfigurations
{
    private static OpenTelemetryBuilder AddOpenTelemetryMetrics(
        this OpenTelemetryBuilder builder,
        ObservabilityConfig config,
        OpenTelemetryMetricsConfig? metricConfig,
        Action<MeterProviderBuilder>? configurator = null
    )
    {
        if (metricConfig is null)
        {
            return builder;
        }

        return builder.WithMetrics(metrics =>
        {
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: config.ServiceName, serviceVersion: config.ServiceVersion);
            if (config.AdditionalAttributes is not null)
                resourceBuilder.AddAttributes(config.AdditionalAttributes);
            metrics.SetResourceBuilder(resourceBuilder);
            if (metricConfig.Meters is not null && metricConfig.IsEnabled)
            {
                metrics.AddMeter(metricConfig.Meters);
            }


            metrics.AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation();

            if (metricConfig.IsProcessMetricsEnabled)
            {
                metrics.AddProcessInstrumentation();
            }

            if (metricConfig.UseConsoleExplorer)
            {
                metrics.AddConsoleExporter();
            }

            metrics.AddMeter("OpenAI.*");

            if (metricConfig.OtlpExporterEndpoint is not null)
            {
                metrics.AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(metricConfig.OtlpExporterEndpoint);
                    o.Headers = metricConfig.OtlpExporterHeaders;
                });
            }

            configurator?.Invoke(metrics);
        });
    }

    private static OpenTelemetryBuilder AddOpenTelemetryTracing(
        this OpenTelemetryBuilder builder,
        ObservabilityConfig config,
        OpenTelemetryTracingConfig? tracingConfig,
        Action<TracerProviderBuilder>? configurator = null
    )
    {
        if (tracingConfig is null || !tracingConfig.IsEnabled)
        {
            return builder;
        }

        return builder
            .WithTracing(opt =>
            {
                var resourceBuilder = ResourceBuilder.CreateDefault()
                    .AddService(serviceName: config.ServiceName, serviceVersion: config.ServiceVersion);
                if (config.AdditionalAttributes is not null)
                    resourceBuilder.AddAttributes(config.AdditionalAttributes);
                opt
                    .AddSource(config.ServiceName, "Soshyant.OMS.Framework.Messaging.Stan")
                    .SetResourceBuilder(resourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation();

                if (tracingConfig.IsElasticSearchClientTraceEnabled)
                {
                    opt.AddElasticsearchClientInstrumentation();
                }

                if (tracingConfig.IsQuartzTraceEnabled)
                {
                    opt.AddQuartzInstrumentation();
                }

                if (tracingConfig.IsEntityFrameWorkTraceEnabled)
                {
                    opt.AddEntityFrameworkCoreInstrumentation();
                }


                if (tracingConfig.UseConsoleExplorer)
                {
                    opt.AddConsoleExporter();
                }

                if (tracingConfig.IsSqlTraceEnabled)
                {
                    opt.AddSqlClientInstrumentation(instrumentationOptions =>
                    {
                        if (tracingConfig.SqlTracingConfig is null) return;

                        instrumentationOptions.SetDbStatementForStoredProcedure =
                            tracingConfig.SqlTracingConfig.SetDbStatementForStoredProcedure;
                        instrumentationOptions.SetDbStatementForText =
                            tracingConfig.SqlTracingConfig.SetDbStatementForText;
                        instrumentationOptions.EnableConnectionLevelAttributes =
                            tracingConfig.SqlTracingConfig.EnableConnectionLevelAttributes;
                    });
                }

                opt.AddSource("OpenAI.*");

                if (tracingConfig.OtlpExporterEndpoint is not null)
                {
                    opt.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(tracingConfig.OtlpExporterEndpoint);
                        o.Headers = tracingConfig.OtlpExporterHeaders;
                    });
                }

                configurator?.Invoke(opt);
            });
    }


    private static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        ObservabilityConfig config,
        Action<TracerProviderBuilder>? tracerConfigurator = null,
        Action<MeterProviderBuilder>? meterConfigurator = null,
        Action<OpenTelemetryLoggerOptions>? loggerConfigurator = null
    )
    {
        if (config.OpenTelemetry is not null && config.OpenTelemetry.IsEnabled)
        {
            services
                .AddOpenTelemetry()
                .AddOpenTelemetryTracing(config, config.OpenTelemetry.Tracing, tracerConfigurator)
                .AddOpenTelemetryMetrics(config, config.OpenTelemetry.Metrics, meterConfigurator);
        }

        return services;
    }

    public static IServiceCollection AddOpenTelemetryServices(
        this IServiceCollection services, ObservabilityConfig? config,
        Action<TracerProviderBuilder>? tracerConfigurator = null,
        Action<MeterProviderBuilder>? meterConfigurator = null,
        Action<OpenTelemetryLoggerOptions>? loggerConfigurator = null
    )
    {
        if (config is null || !config.IsEnabled)
        {
            return services;
        }

        return services
            .AddOpenTelemetry(config, tracerConfigurator, meterConfigurator, loggerConfigurator);
    }


    public static IHostBuilder UseOpenTelemetryServices(
        this IHostBuilder host,
        ObservabilityConfig config
    )
    {
        ArgumentNullException.ThrowIfNull(config);

        if (!config.IsEnabled)

        {
            return host;
        }

        if (config.OpenTelemetry?.Logging is not null && config.OpenTelemetry.Logging.IsEnabled)
        {
            host.ConfigureLogging(logging => logging
                .AddOpenTelemetry(opt =>
                {
                    var resourceBuilder = ResourceBuilder.CreateDefault()
                        .AddService(serviceName: config.ServiceName, serviceVersion: config.ServiceVersion);
                    if (config.AdditionalAttributes is not null)
                        resourceBuilder.AddAttributes(config.AdditionalAttributes);

                    if (config.OpenTelemetry.Logging.OtlpExporterEndpoint is not null)
                    {
                        opt.AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(config.OpenTelemetry.Logging.OtlpExporterEndpoint);
                            o.Headers = config.OpenTelemetry.Logging.OtlpExporterHeaders;
                        });
                    }

                    if (config.OpenTelemetry.Logging.UseConsoleExplorer)
                    {
                        opt.AddConsoleExporter();
                    }

                    opt.SetResourceBuilder(resourceBuilder);

                    opt.IncludeFormattedMessage = config.OpenTelemetry.Logging.IncludeFormattedMessage;

                    opt.IncludeScopes = config.OpenTelemetry.Logging.IncludeScope;

                    opt.ParseStateValues = config.OpenTelemetry.Logging.ParseStateValue;
                })
            );
        }

        return host;
    }
}