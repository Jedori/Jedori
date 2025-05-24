using UnityEngine;

public class Moon : MonoBehaviour
{
    [Header("Orbital Parameters")]
    [SerializeField] private float distance = 0.3f;  // StarSpawner의 distance와 비슷한 범위로 조정
    [SerializeField] private float orbitalPeriod = 27.32f;  // 일 단위
    [SerializeField] private float orbitalInclination = 5.145f;  // 도 단위
    [SerializeField] private float axialTilt = 6.687f;  // 도 단위

    private float currentAngle;
    private float phase;  // 0-1 사이의 값 (0: 새달, 0.5: 보름달)
    private StarSpawner starSpawner;

    private void Start()
    {
        // StarSpawner 찾기
        starSpawner = FindFirstObjectByType<StarSpawner>();
        if (starSpawner == null)
        {
            Debug.LogError("Scene에 StarSpawner가 없습니다!");
        }
    }

    private void Update()
    {
        if (TimeManager.Instance == null || starSpawner == null) return;

        // 현재 시간 가져오기
        float julianDate = TimeManager.Instance.GetJulianDate();

        // 달의 위치 업데이트
        UpdateMoonPosition(julianDate);

        // 달의 회전 업데이트
        UpdateMoonRotation();

        // 달의 위상 업데이트
        UpdateMoonPhase(julianDate);
    }

    public void UpdateMoonPosition(float julianDate)
    {
        // 달의 평균 각도 계산 (0-360도)
        float meanAnomaly = (julianDate % orbitalPeriod) / orbitalPeriod * 360f;

        // 달의 위치 계산 (원형 궤도 가정)
        float x = Mathf.Cos(meanAnomaly * Mathf.Deg2Rad);
        float z = Mathf.Sin(meanAnomaly * Mathf.Deg2Rad);

        // 궤도 경사각 적용
        float y = Mathf.Sin(meanAnomaly * Mathf.Deg2Rad) * Mathf.Sin(orbitalInclination * Mathf.Deg2Rad);

        // StarSpawner의 지구-천체 거리와 동일하게 설정
        float scale = starSpawner.GetEarthToStarDistance();
        transform.position = new Vector3(x * scale, y * scale, z * scale);
    }

    public void UpdateMoonRotation()
    {
        // 달의 자전축 기울기 적용
        transform.rotation = Quaternion.Euler(axialTilt, 0, 0);
    }

    public void UpdateMoonPhase(float julianDate)
    {
        // 달의 위상 계산 (0-1)
        phase = (julianDate % orbitalPeriod) / orbitalPeriod;

        // TODO: 달의 머티리얼을 위상에 따라 업데이트
        // 현재는 단순히 위상값만 계산
    }

    public float GetMoonPhase()
    {
        return phase;
    }
} 