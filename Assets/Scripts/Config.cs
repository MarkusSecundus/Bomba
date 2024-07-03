using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

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
    public static string PadToLength(this string self, int length, string padding=" ", string format="{0}{1}")
    {
        if (self.Length >= length) return self;
        return string.Format(format, RepeatString(padding, length - self.Length), self);
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
    public string[] Passwords;
    public int MaxPasswordAttemptLength;
    public bool ShowStarsInsteadOfPasswordCharacters = true;
    public FailPenaltyDefinition[] FailPenalties = Array.Empty<FailPenaltyDefinition>();
    public int NumberOfAttemptsFailedSoFar = 0;
    public bool DidWin = false;

    [System.Serializable] public struct FailPenaltyDefinition
    {
        public int MaxAttempts;
        [SerializeField][FormerlySerializedAs("Penalty_Hours_Minutes_Seconds")] string Penalty_Hours_Minutes_Seconds;
        public TimeSpan PenaltyTimeSpan { get => TimeSpan.Parse(Penalty_Hours_Minutes_Seconds); set => Penalty_Hours_Minutes_Seconds = value.ToString(); }
    }

    public static Config Default => new Config
    {
        ExplosionTime = SerializableDateTime.FromDateTime(DateTime.Now + TimeSpan.FromDays(1)),
        Passwords = new string[] { "1234" },
        MaxPasswordAttemptLength = 6,
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
            var failsCurrent = (p.MaxAttempts <= 0  || i >= FailPenalties.Length-1)?fails: Math.Min(fails, p.MaxAttempts);
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

    [SerializeField] string[] _comment_cz = new string[] {
    $"${nameof(ExplosionTime)}.. Základní čas kdy nastane exploze - vůči němu se odečítají postihy (viz ${nameof(FailPenalties)}).",
    $"${nameof(Passwords)}.. Seznam hesel. Zadáním jednoho libovolného z nich bude bomba zneškodněna. Typicky obsahuje jediný záznam. Heslo smí sestávat pouze z číslic 0..9. Nesmí mít více znaků než je hodnota ${nameof(MaxPasswordAttemptLength)}.",
    $"${nameof(MaxPasswordAttemptLength)}... Kolikaznakový pokus o heslo může být v konzoli nanejvýš zadán. Pokud má skutečné heslo znaků více, nebude možné ho zadat! Mělo by být nanejvýš cca 30, jinak může text začít přetékat do ostatních prvků grafického rozhraní.",
    $"${nameof(ShowStarsInsteadOfPasswordCharacters)}... Hodnota `true`/`false` - pokud `true`, zadávané heslo bude vyhvězdičkované.",
    $"${nameof(FailPenalties)}...Odstupňovaný seznam postihů za chybné zadání hesla. Seřazeny od nejmírnějšího po nejtvrdší.",
    $" ...${nameof(FailPenaltyDefinition.MaxAttempts)}...Kolikrát smí postih být uplatněn, než postoupíme na následující, přísnější postih. Nastavte na 0 pro neomezené (tzn. finální stupeň postihu, na kterém už zůstaneme)",
    $" ...$Penalty_Hours_Minutes_Seconds...Čas, který se při každém chybném zadání odečte od základního času exploze, definovaného v ${nameof(ExplosionTime)}.",
    $"${nameof(NumberOfAttemptsFailedSoFar)}...Kolikrát se již hráč neúspěšně pokusil zadat heslo. Na základě tohoto čísla a seznamu ${nameof(FailPenalties)} se počítá postih, jehož odečtením od ${nameof(ExplosionTime)} získáme skutečný čas výbuchu. Vynulujte pro zručení postihu. (Do tohoto pole hra sama přičte 1 pokaždé když hráč zadá špatné heslo.)",
    $"${nameof(DidWin)}... Hodnota `true`/`false` - `true` pokud hráč již zadal správné heslo a bomba je tedy deaktivovaná. (Do tohoto pole hra sama zapíše `true` v okamžiku kdy hráč vyhrál)"
    };
    [SerializeField] string _comment_en = "TODO: write the comment";
}