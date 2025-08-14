// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace FrameworkDetector;

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

    public List<DetectorCheckResult> CheckResults { get; } = new List<DetectorCheckResult>();
}
