// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.CLI;

internal static class Program
{
    private readonly static CliApp App = new CliApp();

    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += App.CurrentDomain_UnhandledException;

        var cts = new CancellationTokenSource();
        return await App.RunAsync(args, cts.Token);
    }
}
