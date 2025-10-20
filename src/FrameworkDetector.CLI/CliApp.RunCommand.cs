// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using System.CommandLine;
using System.CommandLine.Parsing;

namespace FrameworkDetector.CLI;

public partial class CliApp
{
    /// <summary>
    /// A command which runs the specified executable/package before inspecting it.
    /// </summary>
    /// <returns><see cref="Command"/></returns>
    private Command GetRunCommand()
    {
        Option<string?> pathOption = new("--exePath", "-exe")
        {
            Description = "The full path of the program to run.",
        };

        Option<string?> packageOption = new("--packageFullName", "-pkg")
        {
            Description = "The full name of the package to run. Must be available to the current user (unless process is running as admin).",
        };

        Option<string?> aumidOption = new("--applicationUserModelId", "-aumid")
        {
            Description = "The app user model id of the app to run. Must be available to the current user (unless process is running as admin).",
        };

        Option<int?> waitTimeOption = new("--waitTime", "-wait")
        {
            Description = "The time in milliseconds to wait after starting the program before inspecting it. Default is 2000.",
        };

        Option<string?> outputFileOption = new("--outputFile", "-o")
        {
            Description = "Save the inspection report as JSON to the given filename.",
        };

        var command = new Command("run", "Inspect a process/package provided to run first")
        {
            pathOption,
            packageOption,
            aumidOption,
            waitTimeOption,
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

            var exepath = parseResult.GetValue(pathOption);
            var packageFullName = parseResult.GetValue(packageOption);
            var aumid = parseResult.GetValue(aumidOption);
            var waitTime = parseResult.GetValue(waitTimeOption) ?? 2000;
            var outputFilename = parseResult.GetValue(outputFileOption);

            if (exepath is not null)
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting program at \"{0}\"...", exepath);
                    process = Process.Start(exepath);
                }
                catch { }

                if (process is null)
                {
                    PrintError("Unable to find/start program at \"{0}\".", exepath);
                    return (int)ExitCode.ArgumentParsingError;
                }

                return (int)await InspectStartedProcessAsync(process, waitTime, outputFilename, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(packageFullName))
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting \"{0}\" app of package \"{1}\"...", aumid ?? "default", packageFullName);
                    process = await Process.StartByPackageFullNameAsync(packageFullName, aumid, cancellationToken);
                }
                catch { }

                if (process is null)
                {
                    PrintError("Unable to start \"{0}\" app of package \"{1}\".", aumid ?? "default", packageFullName);
                    return (int)ExitCode.ArgumentParsingError;
                }

                return (int)await InspectStartedProcessAsync(process, waitTime, outputFilename, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(aumid))
            {
                Process? process = null;

                try
                {
                    PrintInfo("Starting \"{0}\" app...", aumid);
                    process = await Process.StartByApplicationModelUserIdAsync(aumid, cancellationToken);
                }
                catch { }

                if (process is null)
                {
                    PrintError("Unable to start \"{0}\" app.", aumid);
                    return (int)ExitCode.ArgumentParsingError;
                }

                return (int)await InspectStartedProcessAsync(process, waitTime, outputFilename, cancellationToken);
            }

            PrintError("Missing command arguments.");
            await command.Parse("-h").InvokeAsync();

            return (int)ExitCode.ArgumentParsingError;
        });

        return command;
    }

    private async Task<ExitCode> InspectStartedProcessAsync(Process process, int waitTime, string? outputFilename, CancellationToken cancellationToken)
    {
        PrintInfo($"Process {process.ProcessName}({process.Id}) started...");

        if (waitTime > 0)
        {
            PrintInfo("Waiting an additional {0}ms before inspecting...", waitTime);
            await Task.Delay(waitTime, cancellationToken);
        }

        if (await InspectProcessAsync(process, outputFilename, cancellationToken))
        {
            return ExitCode.Success;
        }

        return ExitCode.InspectFailed;
    }
}
