// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// A command which runs the specified executable/package before inspecting it.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetRunCommand()
    {
        Option<string?> pathOption = new("--exePath", "--exe")
        {
            Description = "The full path of the program to run.",
        };

        Option<string?> packageOption = new("--packageName", "--pkg")
        {
            Description = "The name of the package to run.",
        };

        Option<int?> waitTimeOption = new("--waitTime", "--wait")
        {
            Description = "The time in milliseconds to wait after starting the program before inspecting it. Default is 2000.",
        };

        Option<string?> outputFileOption = new("--outputFile")
        {
            Description = "Save the inspection report as JSON to the given filename.",
        };

        Option<bool> includeChildrenOption = new("--includeChildren")
        {
            Description = "Include the children processes of an inspected process.",
        };

        Option<bool> verboseOption = new("--verbose", "--v")
        {
            Description = "Print verbose output.",
        };

        var command = new Command("run", "Inspect a process/package provided to run first")
        {
            pathOption,
            packageOption,
            waitTimeOption,
            includeChildrenOption,
            outputFileOption,
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

            var exepath = parseResult.GetValue(pathOption);
            var packageName = parseResult.GetValue(packageOption);
            var waitTime = parseResult.GetValue(waitTimeOption) ?? 2000;
            var outputFilename = parseResult.GetValue(outputFileOption);
            var verbose = parseResult.GetValue(verboseOption);
            var includeChildren = parseResult.GetValue(includeChildrenOption);

            if (exepath is not null)
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting program at \"{0}\"", exepath);
                    process = Process.Start(exepath);
                }
                catch (Exception)
                {
                    PrintError("Unable to find/start process at \"{0}\".", exepath);
                    return (int)ExitCode.ArgumentParsingError;
                }

                PrintInfo("Waiting for UI Idle of app");
                if (process?.WaitForInputIdle() == true && process?.Responding == true)                
                {
                    // Question: Should we wait first and then check for idle?
                    if (waitTime > 0)
                    {
                        PrintInfo("Waiting an additional {0}ms before inspecting...", waitTime);
                        Thread.Sleep(waitTime);
                    }

                    PrintInfo("Inspecting app...");
                    if (await InspectProcessAsync(process, includeChildren, verbose, outputFilename, cancellationToken))
                    {
                        return (int)ExitCode.Success;
                    }
                }                

                return (int)ExitCode.InspectFailed;
            }
            else if (!string.IsNullOrWhiteSpace(packageName))
            {
                // TODO: Implement package running
            }

            PrintError("Missing command arguments.");
            command.Parse("-h").Invoke();

            return (int)ExitCode.ArgumentParsingError;
        });

        return command;
    }
}
