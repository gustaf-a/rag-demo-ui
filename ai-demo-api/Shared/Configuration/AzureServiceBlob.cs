namespace Shared.Configuration;

public class AzureServiceBlob
{
    //TODO add connectionStrings in azure web service environment
    public string ConnectionString { get; set; }
    public string RootContainerName { get; set; }
}