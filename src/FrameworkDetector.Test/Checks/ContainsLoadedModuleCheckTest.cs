// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static FrameworkDetector.Checks.ContainsLoadedModuleCheck;
using FrameworkDetector.Models;

namespace FrameworkDetector.Test.Checks;

[TestClass]
public class ContainsLoadedModuleCheckTest() : CheckTestBase<ContainsLoadedModuleArgs, ContainsLoadedModuleData>(GetCheckRegistrationInfo)
{
    [TestMethod]
    [DataRow("")]
    [DataRow("TestModuleName.dll")]
    public async Task ContainsLoadedModuleCheck_FilenameFoundTest(string filename)
    {
        await RunFilenameCheck([filename], filename, DetectorCheckStatus.CompletedPassed, filename);
    }

    [TestMethod]
    [DataRow("", "TestModuleName.dll")]
    [DataRow("TestModuleName.dll", "WrongModuleName.dll")]
    public async Task ContainsLoadedModuleCheck_FilenameNotFoundTest(string actualFilename, string filenameToCheck)
    {
        await RunFilenameCheck([actualFilename], filenameToCheck, DetectorCheckStatus.CompletedFailed, null);
    }

    private async Task RunFilenameCheck(string[] actualFilenames, string filenameToCheck, DetectorCheckStatus expectedCheckStatus, string? expectedFilename)
    {
        var actualLoadedModules = actualFilenames.Select(filename => new WindowsBinaryMetadata(filename)).ToArray();
        var args = new ContainsLoadedModuleArgs(filenameToCheck);

        ContainsLoadedModuleData? expectedOutput = expectedFilename is not null ? new ContainsLoadedModuleData(new WindowsBinaryMetadata(expectedFilename)) : null;

        var cts = new CancellationTokenSource();

        await RunTest(actualLoadedModules, args, expectedCheckStatus, expectedOutput, cts.Token);
    }

    private async Task RunTest(WindowsBinaryMetadata[]? actualLoadedModules, ContainsLoadedModuleArgs args, DetectorCheckStatus expectedCheckStatus, ContainsLoadedModuleData? expectedOutput, CancellationToken cancellationToken)
    {
        var dataSources = GetTestProcessDataSource(new ProcessMetadata(nameof(ContainsLoadedModuleCheckTest), LoadedModules: actualLoadedModules));
        await RunCheck_ValidArgsAsync(dataSources, args, expectedCheckStatus, expectedOutput, cancellationToken);
    }
}
