// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace FrameworkDetector;

public interface IDetectorCheck
{
    string Name { get; }

    string Description { get; }

    bool IsRequired { get; }

    DetectorCheckResult Result { get; }
}
