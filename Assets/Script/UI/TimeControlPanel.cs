using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeControlPanel : MonoBehaviour
{
    [Header("Start Time Setting")]
    [SerializeField] private TMP_InputField _StartHourInput;
    [SerializeField] private TMP_InputField _StartMinuteInput;
    [SerializeField] private TMP_InputField _StartSecondInput;

    [Header("End Time Setting")]
    [SerializeField] private TMP_InputField _EndHourInput;
    [SerializeField] private TMP_InputField _EndMinuteInput;
    [SerializeField] private TMP_InputField _EndSecondInput;

    [Header("Button")]
    [SerializeField] private Button _Startbutton;

    private int _startHour;
    private int _startMinute;
    private int _startSecond;

    private int _EndHour;
    private int _EndMinute;
    private int _EndSecond;

    public event Action<int, int, int, int, int, int> OnTimeChanged;

    void Awake()
    {
        int.TryParse(_StartHourInput.text, out _startHour);
        int.TryParse(_StartMinuteInput.text, out _startMinute);
        int.TryParse(_StartSecondInput.text, out _startSecond);

        int.TryParse(_EndHourInput.text, out _EndHour);
        int.TryParse(_EndMinuteInput.text, out _EndMinute);
        int.TryParse(_EndSecondInput.text, out _EndSecond);

        _StartHourInput.onValueChanged.AddListener(_ => NotifyStartChanged());
        _StartMinuteInput.onValueChanged.AddListener(_ => NotifyStartChanged());
        _StartSecondInput.onValueChanged.AddListener(_ => NotifyStartChanged());

        // End Time
        _EndHourInput.onValueChanged.AddListener(_ => NotifyEndChanged());
        _EndMinuteInput.onValueChanged.AddListener(_ => NotifyEndChanged());
        _EndSecondInput.onValueChanged.AddListener(_ => NotifyEndChanged());

        _Startbutton.onClick.AddListener(() =>
            OnButtonPressed(_startHour, _startMinute, _startSecond, _EndHour, _EndMinute, _EndSecond)
        );
    }


    private void OnDisable()
    {
        // Start Time
        _StartHourInput.onValueChanged.RemoveAllListeners();
        _StartMinuteInput.onValueChanged.RemoveAllListeners();
        _StartSecondInput.onValueChanged.RemoveAllListeners();

        // End Time
        _EndHourInput.onValueChanged.RemoveAllListeners();
        _EndMinuteInput.onValueChanged.RemoveAllListeners();
        _EndSecondInput.onValueChanged.RemoveAllListeners();
    }

    private void NotifyStartChanged()
    {
        int.TryParse(_StartHourInput.text, out _startHour);
        int.TryParse(_StartMinuteInput.text, out _startMinute);
        int.TryParse(_StartSecondInput.text, out _startSecond);
    }

    private void NotifyEndChanged()
    {
        int.TryParse(_EndHourInput.text, out _EndHour);
        int.TryParse(_EndMinuteInput.text, out _EndMinute);
        int.TryParse(_EndSecondInput.text, out _EndSecond);
    }

    public void OnButtonPressed(int srtH, int srtM, int srtS, int endH, int endM, int endS)
    {
        OnTimeChanged?.Invoke(srtH, srtM, srtS, endH, endM, endS);
    } 
}
