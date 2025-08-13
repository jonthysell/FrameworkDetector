// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Nodes;

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
    public required string DetectorName;

    public required string DetectorVersion;

    public required string FrameworkId;

    public bool FrameworkFound = false;

    public DetectorStatus Status = DetectorStatus.None;

    public readonly List<DetectorCheckResult> CheckResults = new List<DetectorCheckResult>();

    public JsonObject AsJson()
    {
        var result = new JsonObject();
        result["detectorName"] = DetectorName;
        result["detectorVersion"] = DetectorVersion;
        result["frameworkId"] = FrameworkId;
        result["frameworkFound"] = FrameworkFound;
        result["status"] = char.ToLower(Status.ToString()[0]) + Status.ToString()[1..];

        var checkResults = new JsonArray();
        foreach (var checkResult in CheckResults)
        {
            checkResults.Add(checkResult.AsJson());
        }

        result["checkResults"] = checkResults;

        return result;
    }

    public override string ToString()
    {
        return AsJson().ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
    }
}
