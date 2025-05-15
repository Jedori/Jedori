using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CelestialControlPanel : MonoBehaviour
{
    [Header("Observer Settings (Sliders)")]
    [SerializeField] private Slider _latitudeSlider;
    [SerializeField] private TMP_Text _latitudeValueText; // 슬라이더 값 표시용
    [SerializeField] private Slider _longitudeSlider;
    [SerializeField] private TMP_Text _longitudeValueText; // 슬라이더 값 표시용
    [SerializeField] private Slider _altitudeSlider;
    [SerializeField] private TMP_Text _altitudeValueText; // 슬라이더 값 표시용

    [Header("Date Settings (InputFields)")]
    [SerializeField] private TMP_InputField _yearInput;
    [SerializeField] private TMP_InputField _monthInput;
    [SerializeField] private TMP_InputField _dayInput;

    [Header("Time Setting (Slider)")]
    [SerializeField] private Slider _timeSlider;
    [SerializeField] private TMP_Text _timeValueText;   // 슬라이더 값 표시용

    // 현재 값들
    private float _latitude;
    private float _longitude;
    private float _altitude;
    private int _year;
    private int _month;
    private int _day;
    private float _time;
    private int _hour;
    private int _minute;
    private int _second;

    // 외부 구독용 콜백
    public Action<float, float, float> OnObserverChanged;
    public Action<int, int, int> OnDateChanged;
    public Action<float> OnTimeChanged;

    void Awake()
    {
        // 초기값
        _latitude = _latitudeSlider.value;
        _longitude = _longitudeSlider.value;
        _altitude = _altitudeSlider.value;
        int.TryParse(_yearInput.text, out _year);
        int.TryParse(_monthInput.text, out _month);
        int.TryParse(_dayInput.text, out _day);
        _time = _timeSlider.value;

        // 리스너 등록
        _latitudeSlider.onValueChanged.AddListener(OnLatitudeChanged);
        _longitudeSlider.onValueChanged.AddListener(OnLongitudeChanged);
        _altitudeSlider.onValueChanged.AddListener(OnAltitudeChanged);

        _yearInput.onEndEdit.AddListener(OnYearEdited);
        _monthInput.onEndEdit.AddListener(OnMonthEdited);
        _dayInput.onEndEdit.AddListener(OnDayEdited);

        _timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);

        // UI 갱신
        RefreshAll();
    }

    void OnDestroy()
    {
        // 리스너 해제
        _latitudeSlider.onValueChanged.RemoveListener(OnLatitudeChanged);
        _longitudeSlider.onValueChanged.RemoveListener(OnLongitudeChanged);
        _altitudeSlider.onValueChanged.RemoveListener(OnAltitudeChanged);

        _yearInput.onEndEdit.RemoveListener(OnYearEdited);
        _monthInput.onEndEdit.RemoveListener(OnMonthEdited);
        _dayInput.onEndEdit.RemoveListener(OnDayEdited);

        _timeSlider.onValueChanged.RemoveListener(OnTimeSliderChanged);
    }

    // --- Observer 슬라이더 콜백 ---
    private void OnLatitudeChanged(float v)
    {
        _latitude = v;
        _latitudeValueText.text = v.ToString("F2"); 
        OnObserverChanged?.Invoke(_latitude, _longitude, _altitude);
    }
    private void OnLongitudeChanged(float v)
    {
        _longitude = v;
        _longitudeValueText.text = v.ToString("F2");
        // 경도는 -180 ~ 180 범위로 제한
        OnObserverChanged?.Invoke(_latitude, _longitude, _altitude);
    }
    private void OnAltitudeChanged(float v)
    {
        _altitude = v;
        _altitudeValueText.text = v.ToString("F2");
        OnObserverChanged?.Invoke(_latitude, _longitude, _altitude);
    }

    // --- Date 입력 필드 콜백 ---
    private void OnYearEdited(string text)
    {
        if (int.TryParse(text, out var y))
        {
            _year = y;
            OnDateChanged?.Invoke(_year, _month, _day);
        }
        else
        {
            _yearInput.text = _year.ToString();
        }
    }
    private void OnMonthEdited(string text)
    {
        if (int.TryParse(text, out var m) && m >= 1 && m <= 12)
        {
            _month = m;
            OnDateChanged?.Invoke(_year, _month, _day);
        }
        else
        {
            _monthInput.text = _month.ToString();
        }
    }
    private void OnDayEdited(string text)
    {
        if (int.TryParse(text, out var d) && d >= 1 && d <= 31)
        {
            _day = d;
            OnDateChanged?.Invoke(_year, _month, _day);
        }
        else
        {
            _dayInput.text = _day.ToString();
        }
    }

    // --- Time 슬라이더 콜백 ---
    private void OnTimeSliderChanged(float v)
    {
        _time = v;
        int hour = Mathf.FloorToInt(_time / 3600f);
        int minute = Mathf.FloorToInt((_time - hour * 3600f) / 60f);
        int second = Mathf.FloorToInt(_time - hour * 3600f - minute * 60f);
        _timeValueText.text = string.Format("{0:D2}H :{1:D2}M :{2:D2}S", hour, minute, second);
        OnTimeChanged?.Invoke(_time);
    }

    // 외부에서 초기값 세팅할 때 사용
    public void RefreshAll()
    {
        // 슬라이더 갱신
        _latitudeSlider.value = _latitude;
        _longitudeSlider.value = _longitude;
        _altitudeSlider.value = _altitude;
        _timeSlider.value = _time;

        // 텍스트 갱신
        _yearInput.text = _year.ToString();
        _monthInput.text = _month.ToString();
        _dayInput.text = _day.ToString();
        _timeValueText.text = _time.ToString("F2");
    }
}