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
            .Required("mscorlib", checks => checks
                .ContainsLoadedModule(@"mscorlib(\.ni)?\.dll"))
            .Optional("CLR", checks => checks
                .ContainsLoadedModule(@"clr\.dll"))
            .Optional("clrjit", checks => checks
                .ContainsLoadedModule(@"clrjit\.dll"))
            .Optional("mscorjit", checks => checks
                .ContainsLoadedModule(@"mscorjit\.dll"))
            .BuildDefinition();
    }
}
