using BicepDsl.Config;
using ModelContextProtocol.Server;

namespace BicepDsl.Tools;

public class ToolLoader
{
    private static readonly string toolLog = "c:\\temp\\orka-toolloader.log";

    private void Log(string message)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(toolLog)!);
        File.AppendAllText(toolLog, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
    }

    public List<McpServerTool> LoadfromConfig(Workflow workflow)
    {
        var tools = new List<McpServerTool>();

        foreach (var step in workflow.Steps)
        {
            if (step.Provider.Trim('\'').StartsWith("orka.tool") &&
                step.Inputs.TryGetValue("command", out var commandRaw))
            {
                var command = commandRaw.Trim('\'');

                var options = new McpServerToolCreateOptions
                {
                    Name = step.Name,
                    Title = step.Name,
                    Description = $"Orka CLI tool for command: {command}"
                };

                // 🔐 Capture command locally (for closure safety)
                var capturedCommand = command;

                // ✅ Register a strongly typed string tool (because schema is unsupported in your version)
                var tool = McpServerTool.Create(
                (Func<string[], CancellationToken, Task<string>>)(async (cmd, ct) =>
                {
                    File.AppendAllText("c:\\temp\\orka-delegate-fired.log", $"[{DateTime.Now:HH:mm:ss}] Tool fired with command: {cmd}\n");
                    await Task.CompletedTask;
                    return $"✅ Orka heard you say: {cmd}";
                }),
                options);


                Log($"✅ Registered: {step.Name} as typed-command tool → {command}");
                tools.Add(tool);
            }
        }

        Log($"🎯 Total tools registered: {tools.Count}");
        return tools;
    }
}
