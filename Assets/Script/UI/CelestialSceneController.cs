using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class CelestialSceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CelestialControlPanel controlPanel;
    [SerializeField] private StarSpawner starSpawner;

    void OnEnable()
    {
        // Observer(위도·경도·고도) 변경 시
        controlPanel.OnObserverChanged += OnObserverChanged;
        // 날짜(Y/M/D) 변경 시
        controlPanel.OnDateChanged += OnDateChanged;
        // 시간(Time 슬라이더) 변경 시
        controlPanel.OnTimeChanged += OnTimeChanged;
    }

    void OnDisable()
    {
        controlPanel.OnObserverChanged -= OnObserverChanged;
        controlPanel.OnDateChanged -= OnDateChanged;
        controlPanel.OnTimeChanged -= OnTimeChanged;
    }

    // 콜백 구현부
    private void OnObserverChanged(float lat, float lon, float alt)
    {
        starSpawner.SetObserverLocation(lat, lon, alt);
    }

    private void OnDateChanged(int y, int m, int d)
    {
        // 기존 시간(시분초·타임존)은 유지하고 날짜만 변경
        starSpawner.SetObserverYMDTime(
            y, m, d
        );
    }

    private void OnTimeChanged(float t)
    {
        // Slider 값 t 가 “0~86400초”를 나타낸다고 가정
        int hour = Mathf.FloorToInt(t / 3600f);
        int minute = Mathf.FloorToInt((t - hour * 3600f) / 60f);
        int second = Mathf.FloorToInt(t - hour * 3600f - minute * 60f);
        // 초는 버림
        starSpawner.SetObserverHMSTime(
            hour, minute, second
        );
    }
}
