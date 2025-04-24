using BicepDsl.Config;
using System.Text;

namespace BicepDsl.Tools;

public static class OrkaToolGenerator
{
    public static string GenerateStaticToolClass(Workflow workflow, string @namespace = "BicepDsl.Generated")
    {
        var sb = new StringBuilder();

        sb.AppendLine("using ModelContextProtocol.Server;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.IO;");
        sb.AppendLine("using System;");
        sb.AppendLine("using BicepDsl.Executor;");
        sb.AppendLine("using BicepDsl.Config;");
        sb.AppendLine();
        sb.AppendLine($"namespace {@namespace};");
        sb.AppendLine();
        sb.AppendLine("public static class GeneratedOrkaTools");
        sb.AppendLine("{");

        foreach (var step in workflow.Steps)
        {
            if (!step.Provider.Trim('\'').StartsWith("orka.tool") || !step.Inputs.TryGetValue("command", out var raw))
                continue;

            var name = step.Name;
            var provider = step.Provider.Trim('\'');
            var command = raw.Trim('\'');

            sb.AppendLine("    [McpServerTool]");
            sb.AppendLine($"    [Description(\"Orka tool generated for CLI command: {command}\")]");
            sb.AppendLine($"    public static string {name}(string command)");
            sb.AppendLine("    {");
            sb.AppendLine($"        File.AppendAllText(\"c:\\\\temp\\\\orka-tool-call.log\", $\"[{{DateTime.Now:HH:mm:ss}}] Tool '{name}' called with command: {{command}}\\n\");");
            sb.AppendLine("        return CommandExecutor.ExecuteSync(new OrkaTool");
            sb.AppendLine("        {");
            sb.AppendLine($"            Name = \"{name}\",");
            sb.AppendLine($"            Provider = \"{provider}\",");
            sb.AppendLine("            Inputs = new Dictionary<string, string>");
            sb.AppendLine("            {");
            sb.AppendLine("                { \"command\", command }");
            sb.AppendLine("            }");
            sb.AppendLine("        }) ?? \"No output\";");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}
