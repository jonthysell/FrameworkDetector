// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FrameworkDetector.Engine;

public class DetectorCheckList
{
    internal List<ICheckDefinition> Checks { get; init; } = new();

    // Is this merged with the DetectorDefinition itself (with another special interface)?

    internal DetectorCheckList ContainsClass(string v)
    {
        // TODO: Move to it's own extension method file.
        Debug.WriteLine("ContainsClass NOT IMPLEMENTED YET");

        return this;
    }
    
    // TODO: How to make this accessible to extension but not detector?
    public void AddCheck(ICheckDefinition definition)
    {
        Checks.Add(definition);
    }
}