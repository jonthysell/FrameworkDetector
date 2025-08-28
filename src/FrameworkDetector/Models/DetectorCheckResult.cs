// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;
using System.Text.Json.Nodes;

namespace FrameworkDetector.Models;

public enum DetectorCheckStatus
{
    None,
    InProgress,
    Canceled,
    CompletedPassed,
    CompletedFailed,
}

public class DetectorCheckResult
{
    public ICheckDefinition Detector;

    public DetectorCheckStatus Status { get; set; } = DetectorCheckStatus.None;

    public JsonObject? ExtraData { get; set; } = null;

    public DetectorCheckResult(ICheckDefinition detectorCheck)
    {
        Detector = detectorCheck;
    }
}
