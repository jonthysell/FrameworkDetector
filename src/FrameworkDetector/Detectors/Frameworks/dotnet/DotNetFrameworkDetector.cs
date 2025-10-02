// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.Engine;

namespace FrameworkDetector.Detectors;

public class DotNetFrameworkDetector : IDetector 
{
    public string Name => nameof(DotNetFrameworkDetector);

    public string Description => "Microsoft .NET Framework";

    public string FrameworkId => "DotNetFramework";

    public DetectorCategory Category => DetectorCategory.Framework;

    public DotNetFrameworkDetector()
    {
    }
    
    public DetectorDefinition CreateDefinition()
    {
        return this.Create()
            .Required("CLR Module", checks => checks
                .ContainsLoadedModule("clr.dll", productName: "Microsoft® .NET Framework"))
            // OR
            .Required("mscorlib Module", checks => checks
                .ContainsLoadedModule("mscorlib.dll", productName: "Microsoft® .NET Framework", checkForNgenModule: true))
            .Optional("Extra Modules", checks => checks
                .ContainsLoadedModule("clrjit.dll", productName: "Microsoft® .NET Framework")
                .ContainsLoadedModule("mscorjit.dll", productName: "Microsoft® .NET Framework"))
            .BuildDefinition();
    }
}
