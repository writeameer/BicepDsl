using BicepDsl.Config;
using BicepDsl.Runtime;
using BicepDsl.Tools;


HostApplicationBuilder CreateAppHost(Workflow workflow) 
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Logging.AddConsole(consoleLogOptions =>
    {
        // Configure all logs to go to stderr
        consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
    });


    var tools = new ToolLoader().LoadfromConfig(workflow);


    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly()
        .WithTools(tools);
    return builder;
}

var workflow = ConfigLoader.ReadWorkflow("orka.bicep");
var code = OrkaToolGenerator.GenerateStaticToolClass(workflow);
File.WriteAllText("GeneratedOrkaTools.txt", code);
var assembly = RuntimeToolCompiler.Compile(code, out var compileErrors);

if (assembly == null)
{
    Console.WriteLine("❌ Compilation failed:");
    compileErrors.ForEach(Console.WriteLine);
    return;
}

var builder = CreateAppHost(workflow);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(assembly);

var app = builder.Build();
await app.RunAsync();