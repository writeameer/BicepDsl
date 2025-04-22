using Bicep.Core.Parsing;
using Bicep.Core.Syntax;

namespace BicepDsl.Config;


public class Workflow
{
    public string Name { get; set; } = "default";
    public Dictionary<string, string> Parameters { get; set; } = [];
    public List<OrkaTool> Steps { get; set; } = [];


    public static void PrintWorkflow(Workflow workflow)
    {
        Console.WriteLine($"Workflow Name: {workflow.Name}");
        Console.WriteLine($"Steps: {workflow.Steps.Count}");
        foreach (var step in workflow.Steps)
        {
            Console.WriteLine($"- {step.Name} ({step.Provider})");
            foreach (var input in step.Inputs)
            {
                Console.WriteLine($"  - {input.Key}: {input.Value}");
            }
            if (step.DependsOn.Count != 0)
            {
                Console.WriteLine($"  - Depends on: {string.Join(", ", step.DependsOn)}");
            }
        }
    }

}

