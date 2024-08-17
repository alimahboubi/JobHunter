using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Framework.Observability;

public static class StartupExtension
{
    public static IServiceCollection AddMetadataService(this IServiceCollection service,ServiceLifetime lifetime=ServiceLifetime.Scoped)
    {
        service.Add(new ServiceDescriptor(typeof(IMetadataService), typeof(MetadataService), lifetime));
        return service;
    }
}