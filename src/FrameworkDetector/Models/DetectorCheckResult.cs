// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Models;

public enum DetectorCheckStatus
{
    None,
    InProgress,
    Canceled,
    CompletedPassed,
    CompletedFailed,
    Error,
}

/// <summary>
/// Represents the status of an individual check of a specific detector.
/// </summary>
public interface IDetectorCheckResult
{
    public IDetector Detector { get; }

    public ICheckDefinition Check { get; }

    public DetectorCheckStatus Status { get; set; }
}

public record DetectorCheckResult<T>(
    IDetector Detector,
    ICheckDefinition Check
) : IDetectorCheckResult where T : struct
{
    public DetectorCheckStatus Status { get; set; } = DetectorCheckStatus.None;

    public T? ExtraMetadata { get; set; }
}
