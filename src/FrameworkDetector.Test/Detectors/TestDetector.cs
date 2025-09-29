// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using FrameworkDetector.Engine;

namespace FrameworkDetector.Test.Detectors;

public class TestDetector() : IDetector
{
    public string Name => nameof(TestDetector);

    public string Description => nameof(TestDetector);

    public string FrameworkId => nameof(TestDetector);

    public DetectorCategory Category => DetectorCategory.Framework;

    public DetectorDefinition CreateDefinition() => throw new NotImplementedException();
}
