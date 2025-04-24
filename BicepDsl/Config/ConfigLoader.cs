using Bicep.Core.Parsing;
using Bicep.Core.Syntax;
using BicepDsl;
using BicepDsl.Config;
using System.Reflection;

namespace BicepDsl.Config;



public static class ConfigLoader
{
    public static string AssemblyDirectory
    {
        get
        {
            string? location = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(location))
            {
                throw new InvalidOperationException("Location is null or empty. Unable to determine the assembly directory.");
            }

            return Path.GetDirectoryName(location) ?? throw new InvalidOperationException("Failed to determine the assembly directory.");
        }
    }

    public static Workflow ReadWorkflow(string fileName)
    {
        var filePath = Path.Combine(AssemblyDirectory, fileName);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Workflow file not found: {filePath}");
            Environment.Exit(1);
            return null!;
        }
        string sourceText;
        try
        {
            sourceText = File.ReadAllText(filePath);
        }
        catch
        {
            Console.WriteLine($"Failed to read workflow file: {filePath}");
            Environment.Exit(1);
            return null!;
        }

        var parser = new Parser(sourceText);
        var program = parser.Program();

        var workflow = new Workflow();

        foreach (var declaration in program.Declarations)
        {
            if (declaration is ParameterDeclarationSyntax param)
            {
                var paramName = param.Name.IdentifierName;
                var defaultVal = param.Modifier is ParameterDefaultValueSyntax def
                    ? def.DefaultValue.ToString()
                    : "";

                workflow.Parameters[paramName] = defaultVal;
            }
        }

        foreach (var declaration in program.Declarations)
        {
            if (declaration is not ResourceDeclarationSyntax resource) continue;

            var tool = new OrkaTool
            {
                Name = resource.Name.IdentifierName,
                Provider = resource.Type.ToString().Trim('\''),
            };

            if (resource.Value is ObjectSyntax body &&
                body.TryGetPropertyByName("properties")?.Value is ObjectSyntax props &&
                props.TryGetPropertyByName("input")?.Value is ObjectSyntax input)
            {
                foreach (var prop in input.Properties)
                {
                    var key = prop.TryGetKeyText();
                    var val = prop.Value.ToString();
                    if (key != null)
                    {
                        tool.Inputs[key] = val;
                    }
                    else
                    {
                        Console.WriteLine("Warning: Encountered a null key in input properties. Skipping this property.");
                    }
                    if (key != null)
                    {
                        tool.Inputs[key] = val;
                    }
                    else
                    {
                        Console.WriteLine("Warning: Encountered a null key in input properties. Skipping this property.");
                    }
                    //tool.Inputs[key] = val;
                }
            }

            if (resource.Value is ObjectSyntax dependsBody &&
                dependsBody.TryGetPropertyByName("dependsOn")?.Value is ArraySyntax dependsOn)
            {
                foreach (var item in dependsOn.Items)
                {
                    tool.DependsOn.Add(item.ToString());
                }
            }

            workflow.Steps.Add(tool);
        }

        return workflow;
    }

    public static Dictionary<string, Provider> ReadProviders(string fileName)
    {
        var providers = new Dictionary<string, Provider>();
        var filePath = Path.Combine(AssemblyDirectory, fileName);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Providers file not found: {filePath}");
            Environment.Exit(1);
            return providers;
        }
        string sourceText;
        try
        {
            sourceText = File.ReadAllText(filePath);
        }
        catch
        {
            Console.WriteLine($"Failed to read providers file: {filePath}");
            Environment.Exit(1);
            return providers;
        }

        var parser = new Parser(sourceText);
        var program = parser.Program();

        foreach (var declaration in program.Declarations)
        {
            if (declaration is not ResourceDeclarationSyntax resource) continue;

            var provider = new Provider
            {
                Name = resource.Name.IdentifierName,
                Type = resource.Type.ToString(),
            };

            if (resource.Value is ObjectSyntax body &&
                body.TryGetPropertyByName("properties")?.Value is ObjectSyntax props &&
                props.TryGetPropertyByName("config")?.Value is ObjectSyntax config)
            {
                foreach (var prop in config.Properties)
                {
                    if (prop.TryGetKeyText() is string key && key != null)
                    {
                        provider.Config[key] = prop.Value.ToString();
                    }
                    else
                    {
                        Console.WriteLine("Warning: Encountered a null key in config properties. Skipping this property.");
                    }
                    //provider.Config[prop.TryGetKeyText()] = prop.Value.ToString();
                }
            }

            providers[provider.Name] = provider;
        }

        return providers;
    }
}
