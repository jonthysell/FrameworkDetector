// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Models;

namespace FrameworkDetector.Inputs;

/// <summary>
/// An <see cref="IInputType"/> which represents a loose executable of an application binary to analyze.
/// </summary>
public record ExecutableInput(WindowsModuleMetadata ExecutableMetadata,
                              ImportedFunctionsMetadata[] ImportedFunctions,
                              ExportedFunctionsMetadata[] ExportedFunctions,
                              WindowsModuleMetadata[] ImportedModules,
                              WindowsModuleMetadata[] DotNetModules,
                              IReadOnlyDictionary<string, IReadOnlyList<object>> CustomData) 
    : IEquatable<ExecutableInput>,
      IImportedFunctionsDataSource, 
      IExportedFunctionsDataSource,
      IModulesDataSource,
      ICustomDataSource,
      IInputTypeFactory<FileInfo>,
      IInputType<FileInfo>
{
    [JsonIgnore]
    public string InputGroup => "executables";

    public static async Task<IInputType> CreateAndInitializeDataSourcesAsync(FileInfo executable, bool? isLoaded, CustomDataFactoryCollection<FileInfo>? customDataFactories, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get executable's own metadata
        var metadata = WindowsModuleMetadata.GetMetadata(executable.FullName, isLoaded == true);

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get functions imported by the executable
        var importedFunctions = executable.GetImportedFunctionsMetadata();

        // Loop over Imported Functions to get ImportedModules
        HashSet<WindowsModuleMetadata> importedModules = new();
        foreach (var function in importedFunctions)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();

            var moduleName = function.ModuleName;

            // Module names extracted from imported functions do not contain paths, check path of executable
            if (!Path.IsPathFullyQualified(moduleName))
            {
                var moduleFullPath = Path.GetFullPath(moduleName, Path.GetDirectoryName(executable.FullName) ?? "");
                if (Path.Exists(moduleFullPath))
                {
                    moduleName = moduleFullPath;
                }
            }

            var moduleMetadata = WindowsModuleMetadata.GetMetadata(moduleName, false);
            importedModules.Add(moduleMetadata);
        }

        // Loop over targets in .deps.json file to produce DotNetModules
        HashSet<WindowsModuleMetadata> dotnetModules = new();
        var depsJsonPath = Path.ChangeExtension(executable.FullName, ".deps.json");
        if (File.Exists(depsJsonPath))
        {
            var executablePath = Path.GetDirectoryName(executable.FullName);

            var depsJson = File.ReadAllText(depsJsonPath);
            var depsJsonDoc = JsonDocument.Parse(depsJson);
            if (depsJsonDoc.RootElement.TryGetProperty("targets", out var targets) && targets.ValueKind == JsonValueKind.Object)
            {
                foreach (var target in targets.EnumerateObject())
                {
                    if (target.Value.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var item in target.Value.EnumerateObject())
                        {
                            if (item.Value.ValueKind == JsonValueKind.Object)
                            {
                                // Get dotnet modules from runtime
                                if (item.Value.TryGetProperty("runtime", out var runtime) && runtime.ValueKind == JsonValueKind.Object)
                                {
                                    foreach (var runtimeDep in runtime.EnumerateObject())
                                    {
                                        var filename = Path.GetFileName(runtimeDep.Name);
                                        var fileVersion = runtimeDep.Value.ValueKind == JsonValueKind.Object && runtimeDep.Value.TryGetProperty("fileVersion", out var fileVersionElement) ? fileVersionElement.ToString() : null;

                                        WindowsModuleMetadata? module = null;

                                        // Try to look for local file
                                        if (executablePath is not null)
                                        {
                                            var localPath = Path.Combine(executablePath, filename);
                                            module = WindowsModuleMetadata.GetMetadata(localPath, false);
                                        }

                                        // Fallback with just data from .deps.json
                                        module ??= module = new WindowsModuleMetadata(filename, FileVersion: fileVersion, IsLoaded: false);

                                        dotnetModules.Add(module);
                                    }
                                }

                                // Get native modules from native
                                if (item.Value.TryGetProperty("native", out var native) && runtime.ValueKind == JsonValueKind.Object)
                                {
                                    foreach (var nativeDep in native.EnumerateObject())
                                    {
                                        var filename = Path.GetFileName(nativeDep.Name);
                                        var fileVersion = nativeDep.Value.ValueKind == JsonValueKind.Object && nativeDep.Value.TryGetProperty("fileVersion", out var fileVersionElement) ? fileVersionElement.ToString() : null;

                                        WindowsModuleMetadata? module = null;

                                        // Try to look for local file
                                        if (executablePath is not null)
                                        {
                                            var localPath = Path.Combine(executablePath, filename);
                                            module = WindowsModuleMetadata.GetMetadata(localPath, false);
                                        }

                                        // Fallback with just data from .deps.json
                                        module ??= module = new WindowsModuleMetadata(filename, FileVersion: fileVersion, IsLoaded: false);

                                        dotnetModules.Add(module);
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Get functions exposed by executable
        var exportedFunctions = executable.GetExportedFunctionsMetadata();

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // Load CustomData
        var customData = customDataFactories is not null ? await customDataFactories.CreateCustomDataAsync(executable, isLoaded, cancellationToken) : new Dictionary<string, IReadOnlyList<object>>(0);

        // No async initialization needed here yet, so just construct
        return new ExecutableInput(metadata,
                                   importedFunctions.OrderBy(f => f.ModuleName).ToArray(),
                                   exportedFunctions.OrderBy(f => f.Name).ToArray(),
                                   importedModules.OrderBy(m => m.FileName).ToArray(),
                                   dotnetModules.OrderBy(m => m.FileName).ToArray(),
                                   customData);
    }

    public override int GetHashCode() => ExecutableMetadata.GetHashCode();

    public virtual bool Equals(ExecutableInput? input)
    {
        if (input is null)
        {
            return false;
        }

        return ExecutableMetadata == input.ExecutableMetadata;
    }

    public IEnumerable<ImportedFunctionsMetadata> GetImportedFunctions() => ImportedFunctions;

    public IEnumerable<ExportedFunctionsMetadata> GetExportedFunctions() => ExportedFunctions;

    public IEnumerable<WindowsModuleMetadata> GetModules() => ImportedModules.Union(DotNetModules);

    public IEnumerable<object> GetCustomData(string key) => CustomData.TryGetValue(key, out var values) ? values : Enumerable.Empty<object>();
}
