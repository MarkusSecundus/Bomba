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
    [SerializeField] string _password => cfg.Password;
    [SerializeField] string _passwordPlaceholderChar = "H";

    [SerializeField] UnityEvent OnLengthLimitExceeded;
    [SerializeField] UnityEvent OnValidPasswordEntered;
    [SerializeField] UnityEvent OnInvalidPasswordEntered;

    [SerializeField] TMP_Text _passwordField;
    [SerializeField] TMP_Text _countdownField;

    Config cfg;
    private void Awake()
    {
        cfg = Config.Load();
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
        if(Value == _password)
        {
            OnValidPasswordEntered.Invoke();
        }
        else
        {
            OnInvalidPasswordEntered.Invoke();
        }
    }
    public void RemoveLastCharacter()
    {
        if (Value.Length <= 0) return;
        Value = Value.Substring(0, Value.Length-1);
    }
}
