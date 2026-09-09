#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

/// <summary>
/// Unity 编辑器运行模式截图工具
/// </summary>
[InitializeOnLoad]
internal static class RuntimeScreenshotEditor
{
    /// <summary>
    /// 截图保存目录名称
    /// </summary>
    private const string ScreenshotFolderName = "UnityScreenshots";

    /// <summary>
    /// 截图文件名前缀
    /// </summary>
    private const string ScreenshotFilePrefix = "Screenshot_";

    private static bool capturePending;

    /// <summary>
    /// 注册编辑器运行时截图快捷键
    /// </summary>
    static RuntimeScreenshotEditor()
    {
    }

    /// <summary>
    /// 响应编辑器快捷键 M
    /// </summary>
    [Shortcut("Runtime Screenshot/Capture", KeyCode.M)]
    private static void CaptureByShortcut()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isPaused || capturePending)
        {
            return;
        }

        capturePending = true;
        EditorApplication.delayCall += CaptureScreenshot;
    }

    /// <summary>
    /// 在编辑器主线程执行截图
    /// </summary>
    private static void CaptureScreenshot()
    {
        capturePending = false;
        if (!EditorApplication.isPlaying || EditorApplication.isPaused)
        {
            return;
        }

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(desktopPath))
        {
            Debug.LogError("截图失败：无法获取当前用户桌面路径");
            return;
        }

        try
        {
            string screenshotFolderPath = Path.Combine(desktopPath, ScreenshotFolderName);
            Directory.CreateDirectory(screenshotFolderPath);

            string fileName = $"{ScreenshotFilePrefix}{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
            string screenshotPath = GetAvailableScreenshotPath(screenshotFolderPath, fileName);
            int width = Screen.width;
            int height = Screen.height;

            ScreenCapture.CaptureScreenshot(screenshotPath, 1);
            Debug.LogError($"截图已保存：{screenshotPath} 分辨率：{width}x{height}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"截图失败：{exception.Message}");
        }
    }

    /// <summary>
    /// 获取不会覆盖已有文件的截图路径
    /// </summary>
    /// <param name="folderPath">截图目录</param>
    /// <param name="fileName">默认文件名</param>
    /// <returns>可用截图路径</returns>
    private static string GetAvailableScreenshotPath(string folderPath, string fileName)
    {
        string filePath = Path.Combine(folderPath, fileName);
        if (!File.Exists(filePath))
        {
            return filePath;
        }

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        int suffix = 1;
        do
        {
            filePath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_{suffix}{extension}");
            suffix++;
        } while (File.Exists(filePath));

        return filePath;
    }
}
#endif
