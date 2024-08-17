namespace JobHunter.Framework.Observability;

public class MetadataService : IMetadataService
{
    private string _metadata;
    public string Get()
    {
        return _metadata;
    }

    public void Set(string metadata)
    {
        _metadata = metadata;
    }
}