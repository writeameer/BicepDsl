namespace BicepDsl.Config;

public class OrkaTool
{
    public string Name { get; set; } = "default"; 
    public string Provider { get; set; } = "";
    public Dictionary<string, string> Inputs { get; set; } = new(); 
    public List<string> DependsOn { get; set; } = new();
}

