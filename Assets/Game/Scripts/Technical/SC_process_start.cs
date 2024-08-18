using UnityEngine;
using System.Diagnostics;

public static class ProcessStart
{
    /// <summary>
    /// Opens notepad, depending on operating system.
    /// </summary>
    /// <param name="filePath">File path.</param>
    public static void OpenTextEditor(string filePath = "")
    {
        if (!string.IsNullOrEmpty(filePath) && !System.IO.File.Exists(filePath))
        {
            UnityEngine.Debug.LogError("File doesn't exist: " + filePath);
            return;
        }

#if UNITY_STANDALONE_WIN
        Process.Start("notepad.exe", filePath);
#elif UNITY_STANDALONE_OSX
        Process.Start("open", "-a TextEdit " + filePath);
#elif UNITY_STANDALONE_LINUX
        Process.Start("gedit", filePath);
#endif
    }

    /// <summary>
    /// Opens file explorer, depending on operating system.
    /// </summary>
    /// <param name="folderPath">Folder path.</param>
    public static void OpenFileExplorer(string folderPath)
    {
        if (!string.IsNullOrEmpty(folderPath) && !System.IO.Directory.Exists(folderPath))
        {
            UnityEngine.Debug.LogError("Folder doesn't exist: " + folderPath);
            return;
        }

#if UNITY_STANDALONE_WIN
        Process.Start("explorer.exe", folderPath);
#elif UNITY_STANDALONE_OSX
        Process.Start("open", folderPath);
#elif UNITY_STANDALONE_LINUX
        Process.Start("xdg-open", folderPath);
#endif
    }
}

