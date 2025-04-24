using BicepDsl.Config;
using System.Diagnostics;

public static class CommandExecutor
{
    private static readonly string logPath = "c:\\temp\\orka-exec.log";

    private static void Log(string message)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
    }

    public static async Task<string?> ExecuteAsync(OrkaTool step)
    {
        if (!step.Inputs.TryGetValue("command", out var raw))
        {
            Log("⚠️ No command provided.");
            return "No command found.";
        }

        var command = raw.Trim('\'');

        Log($"🛠 Executing: {command}");

        try
        {
            var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            if (process == null)
            {
                Log("❌ Failed to start process.");
                return "Failed to start process.";
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            Log("✅ Process exited.");
            Log($"📤 Output: {output}");
            Log($"📛 Error: {error}");

            File.AppendAllText("c:\\temp\\orka-tool-params.log", $"[{DateTime.Now:HH:mm:ss}] 🔍 Received command param: {command}\n");


            return string.IsNullOrWhiteSpace(error) ? output : $"stderr: {error}";
        }
        catch (Exception ex)
        {
            Log($"🔥 Exception: {ex}");
            return $"Exception: {ex.Message}";
        }
    }

    public static string? ExecuteSync(OrkaTool step)
    {
        return ExecuteAsync(step).GetAwaiter().GetResult();
    }
}
