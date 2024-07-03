using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(99)]
public class Logger : MonoBehaviour, IDisposable
{
    public const string LogFilePath = "gamelog.txt";
    public static string FullLogFilePath => Path.Combine(Application.persistentDataPath, LogFilePath);
    public const float FlushIntervalSeconds = 1f;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(FlushIntervalSeconds);
        Flush();
    }

    StringBuilder _logs = new StringBuilder();

    public void Message(string message)
    {
        _logs.AppendLine($"{DateTime.Now}: {message}");
    }

    public void Flush()
    {
        if (_logs.Length <= 0) return;
        File.AppendAllText(FullLogFilePath, _logs.ToString());
        _logs.Clear();
    }
    private void OnEnable() => Flush();
    private void OnDisable() => Flush();
    private void OnDestroy() => Flush();
    public void Dispose() => Flush();
}
