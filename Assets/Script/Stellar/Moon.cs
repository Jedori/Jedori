using UnityEngine;

public class Moon : MonoBehaviour
{
    [Header("Orbital Parameters")]
    [SerializeField] private float distance = 0.3f;  // StarSpawner의 distance와 비슷한 범위로 조정
    [SerializeField] private float orbitalPeriod = 27.32f;  // 일 단위
    [SerializeField] private float orbitalInclination = 5.145f;  // 도 단위
    [SerializeField] private float axialTilt = 6.687f;  // 도 단위
    [SerializeField] private float eccentricity = 0.0549f;  // 달 궤도의 이심률
    [SerializeField] private float perigeeLongitude = 318.15f;  // 근지점 경도 (도)
    [SerializeField] private float ascendingNodeLongitude = 125.08f;  // 승교점 경도 (도)

    [Header("Moon Appearance")]
    [SerializeField] private Material moonMaterial;  // 달의 기본 머티리얼
    [SerializeField] private float moonSize = 1f;    // 달의 크기
    [SerializeField] private float maxEmissionIntensity = 1f;  // 최대 발광 강도
    [SerializeField] private float minEmissionIntensity = 0.1f;  // 최소 발광 강도
    [SerializeField] private float dayEmissionIntensity = 0.05f;  // 낮 시간대 발광 강도

    private float currentAngle;
    private float phase;  // 0-1 사이의 값 (0: 새달, 0.5: 보름달)
    private StarSpawner starSpawner;
    private float meanAnomaly;
    private float trueAnomaly;
    private float radius;
    private Vector3 starSpawnerCenter;  // StarSpawner의 중심점
    private MeshRenderer moonRenderer;
    private Camera mainCamera;  // 메인 카메라 참조

    private void Start()
    {
        // StarSpawner 찾기
        starSpawner = FindObjectOfType<StarSpawner>();
        if (starSpawner == null)
        {
            Debug.LogError("Scene에 StarSpawner가 없습니다!");
        }
        else
        {
            // StarSpawner의 중심점 저장
            starSpawnerCenter = starSpawner.transform.position;
        }

        // 메인 카메라 찾기
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다!");
        }

        // MeshRenderer 컴포넌트 확인
        moonRenderer = GetComponent<MeshRenderer>();
        if (moonRenderer == null)
        {
            Debug.LogError("Moon 오브젝트에 MeshRenderer가 없습니다!");
        }
        else
        {
            // 달의 크기 설정 (x와 z는 같게, y는 얇게)
            transform.localScale = new Vector3(moonSize, 0.1f, moonSize);
        }

        // 달 머티리얼 확인
        if (moonMaterial == null)
        {
            Debug.LogError("달 머티리얼이 할당되지 않았습니다!");
        }
        else
        {
            moonRenderer.material = moonMaterial;
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

    private float SolveKeplerEquation(float M, float e)
    {
        // 초기 추정값 설정
        float E = M;
        if (e > 0.8f) E = Mathf.PI;  // 이심률이 큰 경우 더 나은 초기값 사용

        float delta = 0.0f;
        float tolerance = 1e-12f;  // 더 높은 정확도
        int maxIterations = 50;    // 더 많은 반복 허용
        int iterations = 0;

        do
        {
            float sinE = Mathf.Sin(E);
            float cosE = Mathf.Cos(E);
            
            // 뉴턴-랩슨 방법으로 해 구하기
            float f = E - e * sinE - M;
            float fPrime = 1 - e * cosE;
            
            // 수치적 안정성을 위한 보호
            if (Mathf.Abs(fPrime) < 1e-10f)
            {
                Debug.LogWarning("Kepler equation solver: fPrime too small");
                break;
            }

            delta = f / fPrime;
            E -= delta;
            
            iterations++;
            
            // 수렴 검사
            if (iterations >= maxIterations)
            {
                Debug.LogWarning($"Kepler equation solver: Max iterations reached. Residual: {Mathf.Abs(delta)}");
                break;
            }
        } while (Mathf.Abs(delta) > tolerance);

        return E;
    }

    public void UpdateMoonPosition(float julianDate)
    {
        // J2000.0 기준 세기 계산
        float T = (julianDate - 2451545.0f) / 36525.0f;

        // 평균 근점이각 계산 (Meeus 알고리즘)
        meanAnomaly = 134.9633964f + 477198.8675055f * T + 0.0087414f * T * T;
        meanAnomaly = meanAnomaly % 360f;
        if (meanAnomaly < 0) meanAnomaly += 360f;

        // 이심이각 계산 (개선된 Kepler 방정식 해결)
        float E = SolveKeplerEquation(meanAnomaly * Mathf.Deg2Rad, eccentricity) * Mathf.Rad2Deg;

        // 진근점이각 계산
        trueAnomaly = 2f * Mathf.Atan(Mathf.Sqrt((1 + eccentricity) / (1 - eccentricity)) *
                                    Mathf.Tan(E * Mathf.Deg2Rad / 2f)) * Mathf.Rad2Deg;

        // 궤도 반경 계산 (StarSpawner의 거리와 비슷하게 조정)
        float a = starSpawner.GetEarthToStarDistance();  // StarSpawner의 거리를 장반경으로 사용
        radius = a * (1 - eccentricity * eccentricity) /
                (1 + eccentricity * Mathf.Cos(trueAnomaly * Mathf.Deg2Rad));

        // 관측자의 위도와 경도 가져오기
        float observerLatitude = starSpawner.GetObserverLatitude();
        float observerLongitude = starSpawner.GetObserverLongitude();

        // 달의 적경과 적위 계산
        float moonRA = CalculateMoonRA(T, trueAnomaly);
        float moonDec = CalculateMoonDec(T, trueAnomaly);

        // 관측자의 위치에서 본 달의 고도와 방위각 계산
        var (altitude, azimuth) = StarPositionCalculator.VerifyStarPosition(
            moonRA,
            moonDec,
            TimeManager.Instance.GetCurrentDateTime(),
            observerLatitude,
            observerLongitude,
            TimeManager.Instance.GetTimeZone()
        );

        // 고도와 방위각을 3D 좌표로 변환
        Vector3 moonPosition = AltAzToCartesian((float)altitude, (float)azimuth, radius);

        // StarSpawner의 중심점을 기준으로 위치 설정
        transform.position = starSpawnerCenter + moonPosition;
    }

    private float CalculateMoonRA(float T, float trueAnomaly)
    {
        // 달의 적경 계산 (Meeus 알고리즘)
        float L = 218.3164477f + 481267.88123421f * T - 0.0015786f * T * T;
        float M = 134.9633964f + 477198.8675055f * T + 0.0087414f * T * T;
        float F = 93.2720950f + 483202.0175233f * T - 0.0036539f * T * T;

        // 달의 적경 계산
        float ra = L + 6.288f * Mathf.Sin(M * Mathf.Deg2Rad) +
                   1.274f * Mathf.Sin((2 * F - M) * Mathf.Deg2Rad) +
                   0.658f * Mathf.Sin(2 * F * Mathf.Deg2Rad) +
                   0.214f * Mathf.Sin(2 * M * Mathf.Deg2Rad);

        return ra % 360f;
    }

    private float CalculateMoonDec(float T, float trueAnomaly)
    {
        // 달의 적위 계산 (Meeus 알고리즘)
        float L = 218.3164477f + 481267.88123421f * T - 0.0015786f * T * T;
        float M = 134.9633964f + 477198.8675055f * T + 0.0087414f * T * T;
        float F = 93.2720950f + 483202.0175233f * T - 0.0036539f * T * T;

        // 달의 적위 계산
        float dec = 5.128f * Mathf.Sin(F * Mathf.Deg2Rad) +
                   0.280f * Mathf.Sin((M + F) * Mathf.Deg2Rad) +
                   0.277f * Mathf.Sin((M - F) * Mathf.Deg2Rad);

        return dec;
    }

    private Vector3 AltAzToCartesian(float altitude, float azimuth, float distance)
    {
        // 고도와 방위각을 라디안으로 변환
        float altRad = altitude * Mathf.Deg2Rad;
        float azRad = azimuth * Mathf.Deg2Rad;

        // 구면 좌표를 카테시안 좌표로 변환
        float x = distance * Mathf.Cos(altRad) * Mathf.Sin(azRad);
        float y = distance * Mathf.Sin(altRad);
        float z = distance * Mathf.Cos(altRad) * Mathf.Cos(azRad);

        return new Vector3(x, y, z);
    }

    public void UpdateMoonRotation()
    {
        // 달이 항상 천구 중심을 향하도록 회전
        Vector3 directionToCenter = starSpawnerCenter - transform.position;
        if (directionToCenter != Vector3.zero)
        {
            // 기본적으로 천구 중심을 향하도록 회전
            Quaternion baseRotation = Quaternion.LookRotation(directionToCenter);
            // 얇은 면이 천구 중심을 향하도록 X축으로 90도 회전
            Quaternion tiltRotation = Quaternion.Euler(90, 0, 0);
            transform.rotation = baseRotation * tiltRotation;
        }
    }

    public void UpdateMoonPhase(float julianDate)
    {
        // J2000.0 기준 세기 계산
        float T = (julianDate - 2451545.0f) / 36525.0f;

        // 달의 위상 계산 (0-1)
        float meanElongation = 297.8501921f + 445267.1114034f * T - 0.0018819f * T * T;
        meanElongation = meanElongation % 360f;
        if (meanElongation < 0) meanElongation += 360f;

        phase = (meanElongation / 360f + 0.5f) % 1f;

        // 위상에 따른 머티리얼 업데이트
        if (moonRenderer != null && moonMaterial != null)
        {
            // 현재 시간 가져오기
            float currentHour = TimeManager.Instance.GetCurrentDateTime().Hour;
            float currentMinute = TimeManager.Instance.GetCurrentDateTime().Minute;
            float timeOfDay = currentHour + currentMinute / 60f;

            // 낮/밤 시간대 판단 (6시~18시를 낮으로 가정)
            bool isDay = timeOfDay >= 6f && timeOfDay < 18f;

            // 위상에 따른 기본 발광 강도 계산
            float phaseEmission = Mathf.Abs(phase - 0.5f) * 2f;  // 0.5(보름달)에서 가장 밝게

            // 시간대에 따른 발광 강도 조정
            float finalEmission;
            if (isDay)
            {
                // 낮 시간대: 최소 발광 강도 사용
                finalEmission = dayEmissionIntensity;
            }
            else
            {
                // 밤 시간대: 위상에 따른 발광 강도 사용
                finalEmission = Mathf.Lerp(minEmissionIntensity, maxEmissionIntensity, phaseEmission);
            }

            // 발광 강도 적용
            moonMaterial.SetFloat("_EmissionIntensity", finalEmission);
        }
    }

    public float GetMoonPhase()
    {
        return phase;
    }
} 