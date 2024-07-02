using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PasswordField : MonoBehaviour
{
    string _value = "";
    public string Value { get => _value; private set => _passwordField.text = _passwordPlaceholderChar.RepeatString((_value = value).Length); } 

    [SerializeField] int _maxLength;
    [SerializeField] string _password => _cfg.Password;
    [SerializeField] string _passwordPlaceholderChar = "H";

    [SerializeField] UnityEvent OnLengthLimitExceeded;
    [SerializeField] UnityEvent OnValidPasswordEntered;
    [SerializeField] UnityEvent OnInvalidPasswordEntered;

    [SerializeField] TMP_Text _passwordField;
    [SerializeField] TMP_Text _countdownField;

    Config _cfg;
    DateTime _explosionTime;
    private void Awake()
    {
        _cfg = Config.Load();
        _explosionTime = _cfg.ComputeActualExplosionTime();
    }
    private void Update()
    {
        var timeRemaining = _explosionTime - DateTime.Now;
        _countdownField.text = $"{timeRemaining}\nexplosionTime: {_explosionTime}\n Attempts: {_cfg.NumberOfAttemptsFailedSoFar}";
    }
    public void AddCharacter(char toAdd)
    {
        if (Value.Length >= _maxLength) OnLengthLimitExceeded?.Invoke();
        else
        {
            Value += toAdd;
        }
    }
    public void AddText(string toAdd)
    {
        if (Value.Length >= _maxLength) OnLengthLimitExceeded?.Invoke();
        else
        {
            Value += toAdd;
        }
    }

    public void SubmitValue()
    {
        if (string.IsNullOrEmpty(Value)) return;

        if(Value == _password)
        {
            OnValidPasswordEntered.Invoke();
        }
        else
        {
            OnInvalidPasswordEntered.Invoke();
            Value = "";
            _cfg.NumberOfAttemptsFailedSoFar += 1;
            _explosionTime = _cfg.ComputeActualExplosionTime();
            _cfg.Save();
        }
    }
    public void RemoveLastCharacter()
    {
        if (Value.Length <= 0) return;
        Value = Value.Substring(0, Value.Length-1);
    }
}
