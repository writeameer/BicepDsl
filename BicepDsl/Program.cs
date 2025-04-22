using BicepDsl.Config;
using BicepDsl.Executor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Text.Json.Nodes;


var workflow = ConfigLoader.ReadWorkflow("orka.bicep");
var providers = ConfigLoader.ReadProviders("providers.bicep");


var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});





// Register dynamic tools from orka.bicep
var tools = new List<McpServerTool>();


foreach (var step in workflow.Steps)
{
    if (step.Provider.Trim('\'').StartsWith("orka.tool") &&
        step.Inputs.TryGetValue("command", out var commandRaw))
    {
        var command = commandRaw.Trim('\'');

        var options = new McpServerToolCreateOptions { 
            Name = step.Name,
            Description = $"List azure resource groups"
        };

        // Fix for CS8917: Explicitly specify the delegate type for the lambda function.
        var tool = McpServerTool.Create((Func<string[], CancellationToken, Task<string>>)(async (args, ct) =>
        {
            var output = await CommandExecutor.ExecuteAsync(step);
            return output ?? "";
        }),options);

        tools.Add(tool);
    }
}

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithTools(tools);



await builder.Build().RunAsync();

