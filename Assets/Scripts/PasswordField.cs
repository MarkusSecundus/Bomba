using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PasswordField : MonoBehaviour
{
    string _value = "";
    public string Value { get => _value; private set 
            => _passwordField.text = _cfg.ShowStarsInsteadOfPasswordCharacters
                    ? _passwordPlaceholderChar.RepeatString((_value = value).Length)
                    : (_value = value);
                                                                                               } 

    [SerializeField] string _passwordPlaceholderChar = "H";

    [SerializeField] UnityEvent OnLengthLimitExceeded;
    [SerializeField] UnityEvent OnValidPasswordEntered;
    [SerializeField] UnityEvent OnInvalidPasswordEntered;
    [SerializeField] UnityEvent OnTimeOut;

    [SerializeField] TMP_Text _passwordField;
    [SerializeField] TMP_Text _countdownField;
    [SerializeField] TMP_Text _penaltyField;

    Config _cfg;
    DateTime _explosionTime;
    Logger _log;
    private void Awake()
    {
        _log = GameObject.FindAnyObjectByType<Logger>();
    }
    private void Start()
    {
        _log.Message("Game - Start()");
        _cfg = Config.Load();
        _explosionTime = _cfg.ComputeActualExplosionTime();
        Value = Value;
        if (_cfg.DidWin)
        {
            _log.Message("Opened while already having won");
            _log.Flush();
            OnValidPasswordEntered.Invoke();
        }
    }
    private void OnEnable()
    {
        _log.Message("Game - OnEnable()");
    }
    private void OnDisable()
    {
        _log.Message("Game - OnDisable()");
    }
    private void OnDestroy()
    {
        _log.Message("Game - OnDestroy()");
    }

    private void Update()
    {
        var timeRemaining = _explosionTime - DateTime.Now;
        _countdownField.text = PrintTimeSpan(timeRemaining);
        if (timeRemaining.Ticks < 0)
        {
            _log.Message($"Time out - negative time remaining: '{timeRemaining}'");
            _log.Flush();
            OnTimeOut.Invoke();
        }
    }
    public void AddText(string toAdd)
    {
        if (Value.Length >= _cfg.MaxPasswordAttemptLength) OnLengthLimitExceeded?.Invoke();
        else
        {
            Value += toAdd;
        }
    }

    public void SubmitValue()
    {
        if (string.IsNullOrEmpty(Value)) return;

        if(_cfg.Passwords.Contains(Value))
        {
            _log.Message($"Entered valid password: '{Value}'");
            _log.Flush();
            OnValidPasswordEntered.Invoke();
            _cfg.DidWin = true;
            _cfg.Save();
        }
        else
        {
            _log.Message($"Entered invalid password: '{Value}'");
            OnInvalidPasswordEntered.Invoke();
            Value = "";
            _cfg.NumberOfAttemptsFailedSoFar += 1;
            var oldExplosionTime = _explosionTime;
            _explosionTime = _cfg.ComputeActualExplosionTime();
            var explosionTimeDifference = oldExplosionTime - _explosionTime;
            _log.Message($"Subtracted time: '{explosionTimeDifference}' -> end time changed from '{oldExplosionTime}' to '{_explosionTime}'");
            _log.Flush();
            _penaltyField.text = $"- {PrintTimeSpan(explosionTimeDifference)}";
            _cfg.Save();
        }
    }
    public void RemoveLastCharacter()
    {
        if (Value.Length <= 0) return;
        Value = Value.Substring(0, Value.Length-1);
    }

    private static string PrintTimeSpan(TimeSpan s)
    {
        var bld = new StringBuilder();
        bool didPrintSomething = false;
        void entry(long item, string format = "{0:00}", string separator = " : ")
        {
            if (didPrintSomething |= item > 0)
            {
                if (bld.Length > 0) bld.Append(separator);
                bld.AppendFormat(format, item);
            }
        }
        entry(s.Days, format: "{0}");
        entry(s.Hours);
        entry(s.Minutes);
        entry(s.Seconds);


        return bld.ToString();
    }
}
