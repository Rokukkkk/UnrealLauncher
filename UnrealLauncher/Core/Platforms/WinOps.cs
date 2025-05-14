using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace UnrealLauncher.Core.Platforms;

[SuppressMessage("Interoperability", "CA1416")]
public static class WinOps
{
    public static RegistryKey? GetRegistryLocalMachine(string keyPath)
    {
        return Registry.LocalMachine.OpenSubKey(keyPath);
    }

    public static RegistryKey? GetRegistryClassesRoot(string keyPath)
    {
        return Registry.ClassesRoot.OpenSubKey(keyPath);
    }
}