// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static FrameworkDetector.Checks.ContainsActiveWindowCheck;
using FrameworkDetector.Models;

namespace FrameworkDetector.Test.Checks;

[TestClass]
public class ContainsActiveWindowCheckTest() : CheckTestBase<ContainsActiveWindowArgs, ContainsActiveWindowData>(GetCheckRegistrationInfo)
{
    [TestMethod]
    [DataRow("")]
    [DataRow("TestWindowClassName")]
    public async Task ContainsActiveWindowCheck_WindowClassFoundTest(string className)
    {
        await RunWindowClassTest([className], className, DetectorCheckStatus.CompletedPassed, className);
    }

    [TestMethod]
    [DataRow("", "TestWindowClassName")]
    [DataRow("TestWindowClassName", "WrongWindowClassName")]
    public async Task ContainsActiveWindowCheck_WindowClassNotFoundTest(string actualClassName, string classNameToCheck)
    {
        await RunWindowClassTest([actualClassName], classNameToCheck, DetectorCheckStatus.CompletedFailed, null);
    }

    private async Task RunWindowClassTest(string[] actualWindowClassNames, string classNameToCheck, DetectorCheckStatus expectedCheckStatus, string? expectedWindowClassName)
    {
        var actualWindows = actualWindowClassNames.Select(className => new ProcessWindowMetadata(className)).ToArray();
        var args = new ContainsActiveWindowArgs(classNameToCheck);

        ContainsActiveWindowData? expectedOutput = expectedWindowClassName is not null ? new ContainsActiveWindowData(new ProcessWindowMetadata(expectedWindowClassName)) : null;

        var cts = new CancellationTokenSource();

        await RunTest(actualWindows, args, expectedCheckStatus, expectedOutput, cts.Token);
    }

    private async Task RunTest(ProcessWindowMetadata[]? actualWindows, ContainsActiveWindowArgs args, DetectorCheckStatus expectedCheckStatus, ContainsActiveWindowData? expectedOutput, CancellationToken cancellationToken)
    {
        var dataSources = GetTestProcessDataSource(new ProcessMetadata(nameof(ContainsActiveWindowCheckTest), ActiveWindows: actualWindows));
        await RunCheck_ValidArgsAsync(dataSources, args, expectedCheckStatus, expectedOutput, cancellationToken);
    }
}
