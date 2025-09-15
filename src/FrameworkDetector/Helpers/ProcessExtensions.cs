// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Management;
using Windows.Win32.Foundation;

namespace FrameworkDetector.DetectorChecks;

public static class ProcessExtensions
{
    // Adapted from https://stackoverflow.com/a/38614443
    public static IEnumerable<Process> GetChildProcesses(this Process process, bool recursive = false)
    {
        var children = new ManagementObjectSearcher(
                $"Select * From Win32_Process Where ParentProcessID={process.Id}")
            .Get()
            .Cast<ManagementObject>()
            .Select(mo =>
                Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));

        return recursive ? children.Union(children.Select(c => c.GetChildProcesses(recursive)).SelectMany(x => x)) : children;
    }

    public static IEnumerable<ProcessWindowMetadata> GetActiveWindowMetadata(this Process process)
    {
        var windows = new List<ProcessWindowMetadata>();

        HWND.EnumWindows(hwnd => 
        {
            try
            {
                if (hwnd.GetWindowThreadProcessId(out var processID) > 0 && processID == process.Id)
                {
                    var className = hwnd.GetClassName();
                    var windowText = hwnd.GetWindowText();

                    if (className is not null || windowText is not null)
                    {
                        windows.Add(new ProcessWindowMetadata(className,
                                                              windowText,
                                                              hwnd.IsWindowVisible()));
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        });

        return windows;
    }
}

public record ProcessWindowMetadata(string? ClassName, string? Text, bool? IsVisible) { } 
