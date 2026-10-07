# Shabarinath Floating Pen Tool V5.2

V5.2 fixes the remaining GitHub compile error from V5.1.

## Fix

The build error was:

- `The name 'Canvas' does not exist in the current context`

Cause:
`Canvas.SetLeft()` and `Canvas.SetTop()` are defined in `System.Windows.Controls`,
but that namespace was not imported after implicit global usings were disabled.

V5.2 explicitly imports:

```csharp
using System.Windows.Controls;
```

## Build

GitHub Actions workflow:

`Build Shabarinath Floating Pen Tool V5.2`

Artifact:

`Shabarinath-Floating-Pen-Tool-V5-2`

Installer:

`Shabarinath_Floating_Pen_Tool_V5_2_Setup.exe`