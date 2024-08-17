namespace JobHunter.Framework.Observability;

public interface IMetadataService
{
    string Get();
    void Set(string metadata);
}