// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using System.CommandLine;
using System.CommandLine.Parsing;

using FrameworkDetector.Engine;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which inspects a single running process on the system.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetInspectAllCommand()
    {
        Option<string?> outputFolderOption = new("--outputFolder")
        {
            Description = "Save the inspection reports as JSON to the given folder name. Each file will be named by the process id.",
        };

        // See: https://learn.microsoft.com/dotnet/api/system.diagnostics.process.mainwindowhandle
        Option<bool?> filterWindowProcesses = new("--filterWindowProcesses")
        {
            Description = "Filters processes by those that are more likely to be applications with a MainWindowHandle. Default is true.",
        };

        Option<bool> includeChildrenOption = new("--includeChildren")
        {
            Description = "Include the children processes of an inspected process.",
        };

        Option<bool> verboseOption = new("--verbose", "--v")
        {
            Description = "Print verbose output.",
        };

        var command = new Command("all", "Inspect all running processes")
        {
            filterWindowProcesses,
            includeChildrenOption,
            outputFolderOption,
            verboseOption,
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

            var outputFolderName = parseResult.GetValue(outputFolderOption);
            var verbose = parseResult.GetValue(verboseOption);
            var filterProcesses = parseResult.GetValue(filterWindowProcesses) ?? true;
            var includeChildren = parseResult.GetValue(includeChildrenOption);

            // Create output folder (if specified) for output
            if (!string.IsNullOrEmpty(outputFolderName) && !System.IO.Directory.Exists(outputFolderName))
            {
                System.IO.Directory.CreateDirectory(outputFolderName);
            }

            var processes = Process.GetProcesses();
            List<Process> processesToInspect = new();

            // 1. Add all processes if we're not filtering to MainWindows (not default)
            if (!filterProcesses)
            {
                processesToInspect.AddRange(processes);
            }
            else
            {
                foreach (var process in processes)
                {
                    try
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            // Wait for the process to be ready (in case it just started).
                            // TODO: If this is too slow we can probably ignore, as we assume that all apps we want to inspect will already be running...
                            process.WaitForInputIdle();
                            processesToInspect.Add(process);
                        }
                    }
                    catch
                    {
                        // Ignore processes we can't access
                    }
                }
            }

            // 2. Run against all the processes (one-by-one for now)
            ExitCode result = ExitCode.Success;
            int count = 0;
            int fails = 0;
            foreach (var process in processesToInspect)
            {
                string? outputFilename = string.IsNullOrEmpty(outputFolderName) ? null : Path.Combine(outputFolderName, $"{process.ProcessName}_{process.Id}.json");
                PrintInfo("Inspecting process {0}({1}) {2:00.0}%", process.ProcessName, process.Id, 100.0 * count++ / processesToInspect.Count);
                if (!await InspectProcessAsync(process, includeChildren, verbose, outputFilename, cancellationToken))
                {
                    PrintError("Failed to inspect process {0}({1}).", process.ProcessName, process.Id);
                    // Set error, but continue
                    result = ExitCode.InspectFailed;
                    fails++;
                }
            }

            // 3. Summary
            if (fails == 0)
            {
                PrintInfo("Successfully inspected all {0} processes.", processesToInspect.Count);
            }
            else
            {
                PrintError("Failed to inspect {0}/{1} processes.", fails, processesToInspect.Count);
            }

            return (int)result;
        });

        return command;
    }
}
