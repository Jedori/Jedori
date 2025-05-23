using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float year = 2024f;
    [SerializeField] private float month = 1f;
    [SerializeField] private float day = 1f;
    [SerializeField] private float hour = 0f;
    [SerializeField] private float minute = 0f;
    [SerializeField] private float second = 0f;
    [SerializeField] private float timeZone = 9f;  // 한국 시간대
    

    [Header("Time Scale")]
    [SerializeField] private float timeScale = 1f;  // 시간 진행 속도 (1 = 실시간)

    private DateTime currentDateTime;
    private float julianDate;

    private float previousYear;
    private float previousMonth;
    private float previousDay;
    private float previousHour;
    private float previousMinute;
    private float previousSecond;

    private Material _skyboxMaterial;


    // 싱글톤 패턴
    public static TimeManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        _skyboxMaterial = RenderSettings.skybox;
        if (_skyboxMaterial == null)
        {
            Debug.LogError("스카이박스 매터리얼을 발견하지 못했습니다.");
            return;
        }

        


        // 초기값 저장
        previousYear = year;
        previousMonth = month;
        previousDay = day;
        previousHour = hour;
        previousMinute = minute;
        previousSecond = second;
    }

    private void Start()
    {
        UpdateDateTime();
    }

    private void Update()
    {
        // 시간 자동 진행
        if (timeScale != 0)
        {
            second += Time.deltaTime * timeScale;
            UpdateTime();
        }

        // 시간이 변경되었는지 확인
        if (HasTimeChanged())
        {
            // 시간이 변경되면 즉시 모든 천체 업데이트
            ForceUpdateAllCelestialBodies();
            
            // 현재 값 저장
            SaveCurrentTimeValues();
        }

        // 시간 업데이트
        UpdateDateTime();
        CalculateJulianDate();
    }

    private bool HasTimeChanged()
    {
        return previousYear != year ||
               previousMonth != month ||
               previousDay != day ||
               previousHour != hour ||
               previousMinute != minute ||
               previousSecond != second;
    }

    private void SaveCurrentTimeValues()
    {
        previousYear = year;
        previousMonth = month;
        previousDay = day;
        previousHour = hour;
        previousMinute = minute;
        previousSecond = second;
    }

    private void ForceUpdateAllCelestialBodies()
    {
        // Sun 업데이트
        Sun sun = FindObjectOfType<Sun>();
        if (sun != null)
        {
            sun.UpdateSunPosition(GetJulianDate());
            sun.UpdateSunRotation();
        }

        // Moon 업데이트
        Moon moon = FindObjectOfType<Moon>();
        if (moon != null)
        {
            moon.UpdateMoonPosition(GetJulianDate());
            moon.UpdateMoonRotation();
            moon.UpdateMoonPhase(GetJulianDate());
        }

        // StarSpawner 업데이트
        StarSpawner starSpawner = FindObjectOfType<StarSpawner>();
        if (starSpawner != null)
        {
            starSpawner.UpdateStarPositions();
            starSpawner.UpdateConstellationLines();
        }
    }

    private void UpdateTime()
    {
        if (second >= 60f)
        {
            minute += Mathf.Floor(second / 60f);
            second %= 60f;
        }
        if (minute >= 60f)
        {
            hour += Mathf.Floor(minute / 60f);
            minute %= 60f;
        }
        if (hour >= 24f)
        {
            day += Mathf.Floor(hour / 24f);
            hour %= 24f;
        }

        // 월별 일수 처리
        int daysInMonth = GetDaysInMonth(year, month);
        if (day > daysInMonth)
        {
            month += Mathf.Floor((day - 1) / daysInMonth);
            day = ((day - 1) % daysInMonth) + 1;
        }
        if (month > 12f)
        {
            year += Mathf.Floor((month - 1) / 12f);
            month = ((month - 1) % 12f) + 1;
        }
    }

    private int GetDaysInMonth(float year, float month)
    {
        int[] daysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        int monthIndex = Mathf.FloorToInt(month) - 1;
        
        // 윤년 처리
        if (monthIndex == 1 && IsLeapYear(year))
        {
            return 29;
        }
        
        return daysInMonth[monthIndex];
    }

    private bool IsLeapYear(float year)
    {
        int y = Mathf.FloorToInt(year);
        return (y % 4 == 0 && y % 100 != 0) || (y % 400 == 0);
    }

    private void UpdateDateTime()
    {
        currentDateTime = new DateTime(
            (int)year,
            (int)month,
            (int)day,
            (int)hour,
            (int)minute,
            (int)second
        );
    }

    private void CalculateJulianDate()
    {
        // 율리우스 날짜 계산
        float y = year;
        float m = month;
        if (m <= 2)
        {
            y -= 1;
            m += 12;
        }

        float a = Mathf.Floor(y / 100);
        float b = 2 - a + Mathf.Floor(a / 4);

        julianDate = Mathf.Floor(365.25f * (y + 4716)) + Mathf.Floor(30.6001f * (m + 1)) + day + b - 1524.5f;
        
        // 시간 추가
        float time = hour + minute / 60f + second / 3600f;
        julianDate += time / 24f;
    }

    // Getter 메서드들
    public DateTime GetCurrentDateTime() => currentDateTime;
    public float GetJulianDate() => julianDate;
    public float GetTimeScale() => timeScale;
    public float GetTimeZone() => timeZone;

    public float GetElapsedSeconds()
    {
        return hour + minute / 60 + second / 3600;
    }


    // Setter 메서드들
    public void SetTimeScale(float scale) => timeScale = scale;
    public void SetTimeZone(float zone) => timeZone = zone;

    // 시간 설정 메서드
    public void SetTime(float newHour, float newMinute, float newSecond)
    {
        hour = newHour;
        minute = newMinute;
        second = newSecond;
        ForceUpdateAllCelestialBodies();
    }

    // 날짜 설정 메서드
    public void SetDate(float newYear, float newMonth, float newDay)
    {
        year = newYear;
        month = newMonth;
        day = newDay;
        ForceUpdateAllCelestialBodies();
    }
}