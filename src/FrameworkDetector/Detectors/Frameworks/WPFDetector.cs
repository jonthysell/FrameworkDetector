// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

/// <summary>
/// Detector for the Windows Presentation Framework (WPF).
/// Built according to docs/WPF.md.
/// </summary>
public class WPFDetector : IDetector 
{
    public string Name => nameof(WPFDetector);

    public string Description => "Windows Presentation Framework";

    public string FrameworkId => "WPF";

    public DetectorCategory Category => DetectorCategory.Framework;

    public WPFDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        // WPF
        return this.Create()
            .Required("Presentation Framework", checks => checks
                .ContainsModule("PresentationFramework.dll"))
            // OR
            .Required("Presentation Core", checks => checks
                .ContainsModule("PresentationCore.dll"))
            .BuildDefinition();
    }
}
