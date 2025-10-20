// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using YamlDotNet.Serialization;

using FrameworkDetector.Engine;

namespace FrameworkDetector.CLI;

public record DocMetadata
{
    [YamlMember(Alias = "id")]
    public string? FrameworkId { get; set; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    // TODO: Actual Uri type not supported in AOT by YamlDotNet without explicit converter
    // https://github.com/aaubry/YamlDotNet/issues/1030
    public string? Source { get; init; }

    public string? Website { get; init; }

    public DetectorCategory Category { get; init; }

    // TODO: Converter to list of strings as CSV
    public string? Keywords { get; init; }

    [YamlMember(Alias = "ms.date")]
    public DateTimeOffset? Date { get; init; }

    public string? Author { get; init; }
}