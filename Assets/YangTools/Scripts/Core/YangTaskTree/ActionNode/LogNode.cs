using UnityEngine;

/// <summary>
/// LOG节点
/// </summary>
public class LogNode : ActionNodeBase
{
    public LogLevel logLevel;
    public string logString;
    protected override void OnStart()
    {
        switch (logLevel)
        {
            case LogLevel.Info:
                Debug.Log(logString);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(logString);
                break;
            case LogLevel.Error:
                Debug.LogError(logString);
                break;
            default:
                break;
        }

        SetFinish(true);
    }

    protected override void OnCancel()
    {
    }
}
public enum LogLevel
{
    Info,
    Warning, 
    Error
}