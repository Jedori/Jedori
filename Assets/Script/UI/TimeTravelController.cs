using UnityEngine;

public class TimeTravelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeTravelPanel timeTravelPanel;
    [SerializeField] private StarSpawner starSpawner;

    void OnEnable()
    {
        timeTravelPanel.OnTimeLoaded += OnTimeLoaded;
    }
    void OnDisable()
    {
        timeTravelPanel.OnTimeLoaded -= OnTimeLoaded;
    }

    private void OnTimeLoaded(int startHour, int startMinute, int startSecond, int endHour, int endMinute, int endSecond)
    {
        // 시간 여행 로직 구현 일단 처음 시간으로 세팅
        starSpawner.SetObserverHMSTime(startHour, startMinute, startSecond);
        
        //나머지 시간을 흐르도록하는 표현을 구현하는 로직을 따로 구현 하면 작업가능
    }
}


