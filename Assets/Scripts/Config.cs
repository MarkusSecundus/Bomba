using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Extensions
{
    public static TimeSpan Multiply(this TimeSpan self, long factor) => TimeSpan.FromTicks(self.Ticks * factor);

    public static IEnumerable<(T Value, int Index)> Enumerate<T>(this IEnumerable<T> self)
    {
        int t = 0;
        foreach(var e in self)
            yield return (e, t++);
    }
    public static IEnumerable<T> Repeat<T>(this T self, int count)
    {
        for (int t = 0; t < count; ++t)
            yield return self;
    }
    public static string RepeatString(this string c, int count) => string.Concat(c.Repeat(count));
}

[System.Serializable] public struct SerializableDateTime
{
    public int Year;
    public int Month;
    public int Day;
    public int Hour;
    public int Minute;
    public int Second;

    public DateTime ToDateTime() => new DateTime(Year, Month, Day, Hour, Minute, Second, 0);
    public static SerializableDateTime FromDateTime(DateTime d) => new SerializableDateTime {Year = d.Year, Month = d.Month, Day = d.Day, Hour = d.Hour, Minute = d.Minute, Second = d.Second };
}


[System.Serializable] public class Config 
{
    public SerializableDateTime ExplosionTime;
    public string Password;
    public FailPenaltyDefinition[] FailPenalties = Array.Empty<FailPenaltyDefinition>();
    public int NumberOfAttemptsFailedSoFar = 0;

    [System.Serializable] public struct FailPenaltyDefinition
    {
        public uint MaxAttempts;
        [SerializeField] double _penaltySeconds;
        public TimeSpan PenaltyTimeSpan { get => TimeSpan.FromSeconds(_penaltySeconds); set => _penaltySeconds = value.TotalSeconds; }
    }

    public static Config Default => new Config
    {
        ExplosionTime = SerializableDateTime.FromDateTime(DateTime.Now + TimeSpan.FromDays(1)),
        Password = "1234",
        FailPenalties = new FailPenaltyDefinition[]
        {
            new FailPenaltyDefinition
            {
                MaxAttempts = 3,
                PenaltyTimeSpan = TimeSpan.FromSeconds(10)
            },
            new FailPenaltyDefinition
            {
                MaxAttempts = 5,
                PenaltyTimeSpan = TimeSpan.FromSeconds(30)
            },
            new FailPenaltyDefinition
            {
                MaxAttempts = 10,
                PenaltyTimeSpan = TimeSpan.FromMinutes(2)
            },
            new FailPenaltyDefinition
            {
                MaxAttempts = 10,
                PenaltyTimeSpan = TimeSpan.FromMinutes(30)
            },
            new FailPenaltyDefinition
            {
                PenaltyTimeSpan = TimeSpan.FromHours(1.5d)
            },
        }
    };

    public DateTime ComputeActualExplosionTime()
    {
        var ret = ExplosionTime.ToDateTime();
        long fails = NumberOfAttemptsFailedSoFar;
        foreach(var (p, i) in FailPenalties.Enumerate())
        {
            if (fails <= 0) break;
            var failsCurrent = (i >= FailPenalties.Length-1)?fails: Math.Min(fails, p.MaxAttempts);
            ret -= p.PenaltyTimeSpan.Multiply(failsCurrent);
            fails -= failsCurrent;
        }
        return ret;
    }


    public const string ConfigFilePath = "config.json";
    public static string FullConfigFilePath => Path.Combine(Application.persistentDataPath, ConfigFilePath);

    public static Config Load()
    {
        var fullPath = FullConfigFilePath;
        if (!File.Exists(fullPath))
        {
            Debug.Log($"Creating default config");
            var ret = Default;
            ret.Save();
            return ret;
        }
        Debug.Log($"Loading config from '{fullPath}'");
        var configJson = File.ReadAllText(fullPath);
        return JsonUtility.FromJson<Config>(configJson);
    }
    public void Save()
    {
        var fullPath = FullConfigFilePath;
        var configJson = JsonUtility.ToJson(this, true);
        Debug.Log($"Saving config to {fullPath} - value is:\n'{configJson}'");
        File.WriteAllText(fullPath, configJson);
    }
}
