using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FrameworkDetector.Models;

public record ToolRunResult
{
    public string ToolName { get; }

    public string ToolVersion { get; }

    public string Timestamp { get; }

    public WindowsBinaryMetadata? ProcessMetadata { get; set; }

    public List<DetectorResult> Frameworks { get; set; } = new();

    public ToolRunResult(string toolName, string toolVersion)
    {
        ToolName = toolName;
        ToolVersion = toolVersion;
        Timestamp = DateTime.UtcNow.ToString("O");
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, DetectorJsonSerializerOptions.Options);
    }
}
