namespace RagDemoAPI.Configuration;

public class AzureOptions
{
    public const string Azure = "Azure";

    public string DeploymentName { get; set; }
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
    public string ModelId { get; set; }

}
