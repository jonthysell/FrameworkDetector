// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace FrameworkDetector.Models;

public enum DetectorStatus
{
    None,
    InProgress,
    Canceled,
    Completed,
}

public class DetectorResult
{
    public required string DetectorName { get; set; }

    public required string DetectorVersion { get; set; }

    public required string FrameworkId { get; set; }

    public bool FrameworkFound { get; set; } = false;

    public DetectorStatus Status { get; set; } = DetectorStatus.None;

    public List<IDetectorCheckResult> CheckResults { get; } = [];
}
