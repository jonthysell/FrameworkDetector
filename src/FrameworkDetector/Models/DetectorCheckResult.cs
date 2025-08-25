// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    public IDetectorCheck Detector;

    public DetectorCheckStatus Status { get; set; } = DetectorCheckStatus.None;

    public JsonObject? ExtraData { get; set; } = null;

    public DetectorCheckResult(IDetectorCheck detectorCheck)
    {
        Detector = detectorCheck;
    }
}
