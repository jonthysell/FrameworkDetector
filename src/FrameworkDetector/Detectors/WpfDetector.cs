// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DetectorChecks;

namespace FrameworkDetector.Detectors;

public class WpfDetector : Detector 
{
    public override string Name => nameof(WpfDetector);

    public override string Description => "Windows Presentation Framework";

    public override string FrameworkId => "WPF";

    public WpfDetector()
    {
        _processChecks.Add(new LoadedModulePresentDetectorCheck("PresentationFramework.dll", true));
        _processChecks.Add(new LoadedModulePresentDetectorCheck("PresentationCore.dll", true));
    }
}
