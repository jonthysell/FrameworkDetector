// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
using FrameworkDetector.Models;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects a single running process on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectCommand()
    {
        Option<int?> pidOption = new("--processId", "-pid")
        {
            Description = "The PID of the process to inspect.",
        };

        Option<string?> processNameOption = new("--processName")
        {
            Description = "The name of the process to inspect.",
        };

        Option<string?> outputFileOption = new("--outputFile", "-o")
        {
            Description = "Save the inspection report as JSON to the given filename.",
        };

        var command = new Command("inspect", "Inspect a given process")
        {
            pidOption,
            processNameOption,
            outputFileOption,
        };
        command.TreatUnmatchedTokensAsErrors = true;

        command.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
        {
            if (parseResult.Errors.Count > 0)
            {
                // Display any command argument errors
                foreach (ParseError parseError in parseResult?.Errors ?? Array.Empty<ParseError>())
                {
                    PrintError(parseError.Message);
                }

                return (int)ExitCode.ArgumentParsingError;
            }

            var processId = parseResult.GetValue(pidOption);
            var processName = parseResult.GetValue(processNameOption);
            var outputFilename = parseResult.GetValue(outputFileOption);

            if (processId is not null)
            {
                if (await InspectProcessAsync(Process.GetProcessById(processId.Value), outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }
            }
            else if (!string.IsNullOrWhiteSpace(processName))
            {
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    PrintError("Unable to find process with name \"{0}\".", processName);
                }
                else if (processes.Length > 1)
                {
                    //TODO: figure out how to handle inspecting multiple processes and how to output the results.
                    PrintWarning("More than one process with name \"{0}\":", processName);
                    foreach (var process in processes)
                    {
                        PrintWarning("  {0}({1})", process.ProcessName, process.Id);
                    }

                    if (processes.TryGetRootProcess(out var rootProcess) && rootProcess is not null)
                    {
                        PrintWarning("Determined root process {0}({1}).\n", rootProcess.ProcessName, rootProcess.Id);
                        if (await InspectProcessAsync(rootProcess, outputFilename, cancellationToken))
                        {
                            return (int)ExitCode.Success;
                        }
                    }
                    else
                    {
                        PrintError("Please run again with the PID of the specific process you wish to inspect.");
                    }
                }
                else if (await InspectProcessAsync(processes[0], outputFilename, cancellationToken))
                {
                    return (int)ExitCode.Success;
                }

                return (int)ExitCode.InspectFailed;
            }

            PrintError("Missing command arguments.");
            await command.Parse("-h").InvokeAsync();

            return (int)ExitCode.ArgumentParsingError;
        });

        return command;
    }

    /// Encapsulation of initializing datasource and grabbing engine reference to kick-off a detection against all registered detectors (see ConfigureServices)
    private async Task<bool> InspectProcessAsync(Process process, string? outputFilename, CancellationToken cancellationToken)
    {
        // TODO: Probably have this elsewhere to be called
        var target = $"process {process.ProcessName}({process.Id}){(IncludeChildren ? " (and children)" : "")}";

        PrintInfo("Preparing to inspect {0}...", target);

        if (!process.IsAccessible())
        {
            PrintError("Cannot access {0} to inspect" + (!WindowsIdentity.IsRunningAsAdmin ? ", try running as Administrator." : "."), target);
            return false;
        }

        if (WaitForInputIdle)
        {
            PrintInfo("Waiting for input idle for {0}", target);
            if (!await process.TryWaitForIdleAsync(cancellationToken))
            {
                PrintError("Waiting for input idle for {0} failed, try running again.", target);
                return false;
            }
        }

        if (Verbosity > VerbosityLevel.Quiet)
        {
            Console.Write($"Inspecting {target}:");
        }

        var processDataSources = new List<ProcessDataSource>() { new ProcessDataSource(process) };
        if (IncludeChildren)
        {
            processDataSources.AddRange(process.GetChildProcesses().Select(p => new ProcessDataSource(p)));
        }

        DataSourceCollection sources = new(processDataSources.ToArray());

        DetectionEngine engine = Services.GetRequiredService<DetectionEngine>();
        engine.DetectionProgressChanged += (s, e) =>
        {
            if (Verbosity > VerbosityLevel.Quiet)
            {
                Console.Write($"\rInspecting {target}: {e.Progress:000.0}%");
            }
        };

        ToolRunResult result = await engine.DetectAgainstSourcesAsync(sources, cancellationToken);

        if (Verbosity > VerbosityLevel.Quiet)
        {
            Console.WriteLine();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            PrintWarning("Inspection was canceled prior to completion.");
            Console.WriteLine();
        }

        PrintResult(result);

        TrySaveOutput(result, outputFilename);

        // TODO: Return false on failure
        return true;
    }
}
