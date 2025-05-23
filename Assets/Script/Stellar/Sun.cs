using UnityEngine;

public class Sun : MonoBehaviour
{
    [Header("Orbital Parameters")]
    [SerializeField] private float distance = 0.5f;  // StarSpawner의 distance와 비슷한 범위로 조정
    [SerializeField] private float orbitalPeriod = 365.25f;  // 일 단위
    [SerializeField] private float axialTilt = 23.5f;  // 도 단위

    [Header("Light Settings")]
    [SerializeField] private float lightIntensity = 1.0f;
    [SerializeField] private Color lightColor = Color.white;

    private Light sunLight;
    private StarSpawner starSpawner;

    private void Start()
    {
        // Directional Light 컴포넌트 가져오기
        sunLight = GetComponent<Light>();
        if (sunLight == null)
        {
            Debug.LogError("Sun GameObject에 Light 컴포넌트가 없습니다!");
            return;
        }

        // StarSpawner 찾기
        starSpawner = FindObjectOfType<StarSpawner>();
        if (starSpawner == null)
        {
            Debug.LogError("Scene에 StarSpawner가 없습니다!");
            return;
        }

        // Light 설정
        sunLight.intensity = lightIntensity;
        sunLight.color = lightColor;
    }

    private void Update()
    {
        if (TimeManager.Instance == null || starSpawner == null) return;

        // 현재 시간 가져오기
        float julianDate = TimeManager.Instance.GetJulianDate();

        // 태양 위치 업데이트
        UpdateSunPosition(julianDate);

        // 태양 회전 업데이트
        UpdateSunRotation();

        // Directional Light 방향 업데이트
        UpdateLightDirection();
    }

    public void UpdateSunPosition(float julianDate)
    {
        // 태양의 평균 각도 계산 (0-360도)
        float meanAnomaly = (julianDate % orbitalPeriod) / orbitalPeriod * 360f;

        // 태양의 위치 계산 (원형 궤도 가정)
        float x = Mathf.Cos(meanAnomaly * Mathf.Deg2Rad);
        float z = Mathf.Sin(meanAnomaly * Mathf.Deg2Rad);

        // StarSpawner의 지구-천체 거리와 동일하게 설정
        float scale = starSpawner.GetEarthToStarDistance();
        transform.position = new Vector3(x * scale, 0, z * scale);
    }

    public void UpdateSunRotation()
    {
        // 태양의 자전축 기울기 적용
        transform.rotation = Quaternion.Euler(axialTilt, 0, 0);
    }

    private void UpdateLightDirection()
    {
        if (sunLight != null)
        {
            // 태양의 위치를 기준으로 빛의 방향 설정
            sunLight.transform.forward = -transform.position.normalized;
        }
    }
} 