// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Management;


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
        var shellHWND = NativeMethods.GetShellWindow();

        var windows = new List<ProcessWindowMetadata>();

        NativeMethods.EnumWindows((hWnd, lParam) =>
        {
            if (hWnd == shellHWND)
            {
                return true;
            }

            try
            {
                if (NativeMethods.GetWindowThreadProcessId(hWnd, out var processID) && processID == process.Id)
                {
                    var classNameSB = new StringBuilder(1024);
                    var classNameFound = NativeMethods.GetClassName(hWnd, classNameSB, classNameSB.Capacity);

                    var textLength = NativeMethods.GetWindowTextLength(hWnd);
                    var textSB = new StringBuilder(textLength + 1);
                    var textFound = NativeMethods.GetWindowText(hWnd, textSB, textSB.Capacity);

                    var isVisible = NativeMethods.IsWindowVisible(hWnd);

                    if (classNameFound || textFound)
                    {
                        windows.Add(new ProcessWindowMetadata(classNameFound ? classNameSB.ToString() : null,
                                                              textFound ? textSB.ToString() : null,
                                                              isVisible));
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return windows;
    }
}

public record ProcessWindowMetadata(string? ClassName, string? Text, bool? IsVisible) { } 

internal partial class NativeMethods
{
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    // Have to use DllImport due to lack of out string marshalling
    [DllImport("user32.dll")]
    public static extern bool GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [LibraryImport("user32.dll")]
    public static partial IntPtr GetShellWindow();

    [LibraryImport("user32.dll", EntryPoint = "GetWindowTextLengthW")]
    public static partial int GetWindowTextLength(IntPtr hWnd);

    // Have to use DllImport due to lack of out string marshalling
    [DllImport("user32.dll")]
    public static extern bool GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowThreadProcessId(IntPtr hWnd, out int processId);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsWindowVisible(IntPtr hWnd);

    
}