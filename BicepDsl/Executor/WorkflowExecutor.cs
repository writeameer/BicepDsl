using BicepDsl.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicepDsl.Executor;

public static class WorkflowExecutor
{
    public static async Task ExecuteAsync(Workflow workflow)
    {
        foreach (var step in workflow.Steps)
        {
            Console.WriteLine($"\n== Executing: {step.Name} ({step.Provider})");

            try
            {
                if (step.Provider.StartsWith("orka.tool"))
                {
                    await CommandExecutor.ExecuteAsync(step);
                }
                else
                {
                    Console.WriteLine($"[SKIP] Unsupported provider type: {step.Provider}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Step '{step.Name}' failed: {ex.Message}");
            }
        }
    }
}
