// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.DetectorChecks;

public abstract class PathDetectorCheck : IDetectorCheck
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public bool IsRequired { get; protected set; }

    public DetectorCheckResult Result { get; protected set; } = null;

    public string? Path { get; protected set; } = null;

    protected PathDetectorCheck(bool isRequired)
    {
        IsRequired = isRequired;
        Result = new DetectorCheckResult(this);
    }

    public async Task<DetectorCheckStatus> RunCheckAsync(string path, CancellationToken cancellationToken)
    {
        Path = path;
        return await RunCheckAsync(cancellationToken);
    }

    public virtual async Task<DetectorCheckStatus> RunCheckAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Path))
        {
            throw new ArgumentNullException(nameof(Path));
        }

        return DetectorCheckStatus.None;
    }
}
