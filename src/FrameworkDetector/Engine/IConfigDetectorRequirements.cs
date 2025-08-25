// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace FrameworkDetector.Engine;

public interface IConfigDetectorRequirements
{
    IConfigDetectorRequirements Required(Func<DetectorCheckList, DetectorCheckList> checks);

    IConfigDetectorRequirements Optional(string subtitle, Func<DetectorCheckList, DetectorCheckList> checks);

    DetectorDefinition BuildDefinition();
}