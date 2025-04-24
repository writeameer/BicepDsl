using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace BicepDsl.Runtime;

public static class RuntimeToolCompiler
{
    public static Assembly? Compile(string sourceCode, out List<string> errors)
    {
        errors = new();

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Load core assembly references from currently loaded AppDomain
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .ToList();

        // Ensure System.ComponentModel.Primitives is included for [Description]
        var coreLibPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var primitivesPath = Path.Combine(coreLibPath, "System.ComponentModel.Primitives.dll");
        if (File.Exists(primitivesPath))
        {
            assemblies.Add(Assembly.LoadFrom(primitivesPath));
        }

        var references = assemblies
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            assemblyName: "OrkaGeneratedTools",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            errors.AddRange(result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            return null;
        }

        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }
}
