// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;

namespace FrameworkDetector;

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
    public readonly IDetectorCheck DetectorCheck;

    public DetectorCheckStatus Status = DetectorCheckStatus.None;

    public JsonObject? ExtraData = null;

    public DetectorCheckResult(IDetectorCheck detectorCheck)
    {
        DetectorCheck = detectorCheck;
    }

    public JsonObject AsJson()
    {
        var result = new JsonObject();
        result["name"] = DetectorCheck.Name;
        result["description"] = DetectorCheck.Description;
        result["isRequired"] = DetectorCheck.IsRequired;
        result["status"] = char.ToLower(Status.ToString()[0]) + Status.ToString()[1..];
        if (ExtraData is not null)
        {
            result["extraData"] = ExtraData;
        }
        return result;
    }

    public override string ToString()
    {
        return AsJson().ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
    }
}
