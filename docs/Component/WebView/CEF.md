# Chromium Embedded Framework (CEF)

## Summary

The Chromium Embedded Framework (CEF) is a simple framework for embedding Chromium-based browsers in other applications.

**Website:** [CEF Site](https://bitbucket.org/chromiumembedded/cef/)

### Languages

**Framework Languages:** C, C++

**App Languages:** C, C++, Delphi, Go, Java, Python, C#, Visual Basic and other .NET languages

### OS Support

Windows, macOS, Linux

### Dependencies

CEF depends on [Google Chromium](https://www.chromium.org/Home/).

### Canonical Apps

- [GOG Galaxy](https://www.gog.com/galaxy)
- [Steam Client](https://store.steampowered.com/about/download)

## How to Detect

**Implementation:** [CEFDetector](/src/FrameworkDetector/Detectors/Component/WebView/CEFDetector.cs)

### Runtime Detection

The following module should be loaded by the running process:

1. `libecef.dll`

The specific version of CEF can be gotten by checking the FileVersion of the loaded module.

### Static Detection

It is not possible to definitively determine the use of CEF by an app by detecting the presence or absence of the aforementioned module file(s) within the app's binaries. In the simplest case, any of the aforementioned module(s) could have been included mistakenly.

## Resources

- [Official Site](https://bitbucket.org/chromiumembedded/cef/)
- [CefSharp](https://github.com/cefsharp/CefSharp)
