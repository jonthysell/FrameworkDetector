// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.DetectorChecks;

public class LoadedModulePresentDetectorCheck : ProcessDetectorCheck
{
    public override string Name => $"{nameof(LoadedModulePresentDetectorCheck)}({ModuleName})";

    public override string Description => $"Detect \"{ModuleName}\" in Process.LoadedModules";

    public readonly string ModuleName;

    public LoadedModulePresentDetectorCheck(string moduleName, bool isRequired = true) : base(isRequired)
    {
        ModuleName = moduleName;
    }

    protected override async Task<DetectorCheckStatus> RunCheckAsync(CancellationToken cancellationToken)
    {
        if (Process is null)
        {
            throw new ArgumentNullException(nameof(Process));
        }

        Result.Status = DetectorCheckStatus.InProgress;

        foreach (var processModule in Process.Modules.Cast<ProcessModule>())
        {
            await Task.Yield();

            if (cancellationToken.IsCancellationRequested)
            {
                Result.Status = DetectorCheckStatus.Canceled;
                break;
            }

            if (processModule.ModuleName.Equals(ModuleName, StringComparison.InvariantCultureIgnoreCase))
            {
                Result.Status = DetectorCheckStatus.CompletedPassed;
                break;
            }
        }

        if (Result.Status == DetectorCheckStatus.InProgress)
        {
            Result.Status = DetectorCheckStatus.CompletedFailed;
        }

        return Result.Status;
    }
}
