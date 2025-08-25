// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Models;

namespace FrameworkDetector.Engine;

public interface IDetectorCheck
{
    string Name { get; }

    string Description { get; }

    bool IsRequired { get; }

    DetectorCheckResult Result { get; }
}
