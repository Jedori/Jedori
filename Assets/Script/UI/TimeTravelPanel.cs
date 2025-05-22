using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeTravelPanel : MonoBehaviour
{
    [Header("Time Travel Settings")]
    [SerializeField] private TMP_InputField StartHourInput;
    [SerializeField] private TMP_InputField StartMinuteInput;
    [SerializeField] private TMP_InputField StartSecondInput;

    [SerializeField] private TMP_InputField EndHourInput;
    [SerializeField] private TMP_InputField EndMinuteInput;
    [SerializeField] private TMP_InputField EndSecondInput;

    [SerializeField] private Button _OperateButton;


    private int _StartHour;
    private int _StartMinute;
    private int _StartSecond;

    private int _EndHour;
    private int _EndMinute;
    private int _EndSecond;

    public Action<int, int, int, int, int, int> OnTimeLoaded;

    void Awake()
    {
        // 초기값
        int.TryParse(StartHourInput.text, out _StartHour);
        int.TryParse(StartMinuteInput.text, out _StartMinute);
        int.TryParse(StartSecondInput.text, out _StartSecond);

        int.TryParse(EndHourInput.text, out _EndHour);
        int.TryParse(EndMinuteInput.text, out _EndMinute);
        int.TryParse(EndSecondInput.text, out _EndSecond);

        // 리스너 등록
        StartHourInput.onValueChanged.AddListener(OnStartHourEdited);
        StartMinuteInput.onValueChanged.AddListener(OnStartMinuteEdited);
        StartSecondInput.onValueChanged.AddListener(OnStartSecondEdited);

        EndHourInput.onValueChanged.AddListener(OnEndHourEdited);
        EndMinuteInput.onValueChanged.AddListener(OnEndMinuteEdited);
        EndSecondInput.onValueChanged.AddListener(OnEndSecondEdited);

        _OperateButton.onClick.AddListener( OnOperateButtonClicked);
    }

    void OnDestroy()
    {
        StartHourInput.onValueChanged.RemoveListener(OnStartHourEdited);
        StartMinuteInput.onValueChanged.RemoveListener(OnStartMinuteEdited);
        StartSecondInput.onValueChanged.RemoveListener(OnStartSecondEdited);

        EndHourInput.onValueChanged.RemoveListener(OnEndHourEdited);
        EndMinuteInput.onValueChanged.RemoveListener(OnEndMinuteEdited);
        EndSecondInput.onValueChanged.RemoveListener(OnEndSecondEdited);
    }

    private void OnStartHourEdited(string value)
    {
        int.TryParse(value, out _StartHour);
    }

    private void OnStartMinuteEdited(string value)
    {
        int.TryParse(value, out _StartMinute);
    }

    private void OnStartSecondEdited(string value)
    {
        int.TryParse(value, out _StartSecond);
    }

    private void OnEndHourEdited(string value)
    {
        int.TryParse(value, out _EndHour);
    }
    private void OnEndMinuteEdited(string value)
    {
        int.TryParse(value, out _EndMinute);
    }
    private void OnEndSecondEdited(string value)
    {
        int.TryParse(value, out _EndSecond);
    }

    public void OnOperateButtonClicked()
    {
        // 버튼 클릭 시 시간 변경 이벤트 발생
        OnTimeLoaded?.Invoke(_StartHour, _StartMinute, _StartSecond, _EndHour, _EndMinute, _EndSecond);
    }
}
