using System.Diagnostics;
using BicepDsl.Config;

namespace BicepDsl.Executor;

public static class CommandExecutor
{
    public static async Task<String?> ExecuteAsync(OrkaTool step)
    {
        if (!step.Inputs.TryGetValue("command", out var raw))
        {
            Console.WriteLine("No 'command' input found.");
            return null;
        }

        var command = raw.Trim('\'');

        var psi = new ProcessStartInfo
        {
            FileName = Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd.exe" : "/bin/bash",
            Arguments = Environment.OSVersion.Platform == PlatformID.Win32NT ? $"/C {command}" : $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"[ERROR]: {error}");
        }
        else
        {
            Console.WriteLine(output);
        }

        return output;
    }
}
