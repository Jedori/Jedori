using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

public class Trajectory : MonoBehaviour
{
    [Header("Star Data")]
    [SerializeField] TextAsset starJsonFile;
    [Tooltip("별 데이터가 있는 JSON 파일")]

    [Header("천구 중심 좌표")]
    [SerializeField] Transform StarSpawnerZeroCoodination;
    [Tooltip("모든 별과 별자리 선의 기준이 될 Transform입니다. 이 Transform의 위치를 중심으로 별이 생성됩니다.")]

    [Header("Observer Settings")]
    [SerializeField] float observerLatitude = 37.5665f;
    [Tooltip("관측자의 위도 (도)")]
    [SerializeField] float observerLongitude = 126.9780f;
    [Tooltip("관측자의 경도 (도)")]

    [Header("Time Settings")]
    [SerializeField] float startHourOffset = 0f;  // 현재 시간으로부터 몇 시간 전부터 시작할지
    [Tooltip("현재 시간으로부터 몇 시간 전부터 궤적을 그릴지 설정 (음수: 과거, 양수: 미래)")]
    [SerializeField] float durationHours = 3f;    // 궤적을 그릴 총 시간
    [Tooltip("궤적을 그릴 총 시간 (시간)")]

    [Header("Arc Settings")]
    [SerializeField] Material arcMaterial;
    [Tooltip("호의 재질")]
    [SerializeField] float arcWidth = 0.05f;
    [Tooltip("호의 두께")]
    [SerializeField] int arcSegments = 30;
    [Tooltip("호의 분할 수")]

    private Dictionary<string, List<Vector3>> starTrajectoryPoints = new Dictionary<string, List<Vector3>>();
    private Dictionary<string, StarData> starDataDict = new Dictionary<string, StarData>();
    private List<GameObject> trajectoryObjects = new List<GameObject>();  // 궤적 오브젝트들을 저장할 리스트
    private const float EARTH_ROTATION_PERIOD = 23.934472f; // 지구 자전 주기 (시간)
    private float julianDate;
    private StarSpawner starSpawner;  // StarSpawner 참조

    void Start()
    {
        LoadStarDataFromJson();
        starSpawner = FindObjectOfType<StarSpawner>();  // StarSpawner 찾기
    }

    public void SetObserverAndTime(float lat, float lon, float y, float m, float d, float h, float min, float sec)
    {
        observerLatitude = lat;
        observerLongitude = lon;

        // TimeManager가 있으면 TimeManager의 시간을 사용
        if (TimeManager.Instance != null)
        {
            julianDate = TimeManager.Instance.GetJulianDate();
        }
        else
        {
            CalculateJulianDate(y, m, d, h, min, sec);
        }
    }

    public void CalculateAndDrawTrajectories()
    {
        if (TimeManager.Instance == null)
        {
            Debug.LogError("TimeManager instance not found!");
            return;
        }

        if (starSpawner == null)
        {
            Debug.LogError("StarSpawner not found!");
            return;
        }

        // 현재 시간으로 업데이트
        julianDate = TimeManager.Instance.GetJulianDate();

        ClearExistingTrajectories();
        CalculateStarTrajectories();
        DrawStarTrajectories();
    }

    public void ClearExistingTrajectories()
    {
        // 저장된 모든 궤적 오브젝트 제거
        foreach (var obj in trajectoryObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        trajectoryObjects.Clear();
        starTrajectoryPoints.Clear();
    }

    void LoadStarDataFromJson()
    {
        if (starJsonFile == null)
        {
            Debug.LogError("Star JSON file is not assigned!");
            return;
        }

        string[] jsonLines = starJsonFile.text.Split('\n');
        foreach (string line in jsonLines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            StarData star = JsonUtility.FromJson<StarData>(line);
            starDataDict[star.main_id] = star;
        }
    }

    void CalculateStarTrajectories()
    {
        if (TimeManager.Instance == null || starSpawner == null)
        {
            Debug.LogError("TimeManager or StarSpawner instance not found!");
            return;
        }

        foreach (var starData in starDataDict.Values)
        {
            List<Vector3> trajectoryPoints = new List<Vector3>();
            float distance = starSpawner.GetEarthToStarDistance();

            // 시작 시간과 종료 시간 계산
            float startTime = startHourOffset;
            float endTime = startTime + durationHours;

            for (int i = 0; i <= arcSegments; i++)
            {
                // 시간 비율 계산 (0~1 사이의 값)
                float timeRatio = (float)i / arcSegments;
                // 실제 시간 계산 (시작 시간부터 종료 시간까지)
                float currentTime = Mathf.Lerp(startTime, endTime, timeRatio);

                // 현재 시간에 대한 별의 위치 계산
                var (altitude, azimuth) = StarPositionCalculator.VerifyStarPosition(
                    starData.ra,
                    starData.dec,
                    TimeManager.Instance.GetCurrentDateTime().AddHours(currentTime),
                    observerLatitude,
                    observerLongitude,
                    TimeManager.Instance.GetTimeZone()
                );

                // 고도와 방위각을 3D 좌표로 변환
                Vector3 position = AltAzToCartesian((float)altitude, (float)azimuth, distance);
                trajectoryPoints.Add(position + (StarSpawnerZeroCoodination != null ? StarSpawnerZeroCoodination.position : Vector3.zero));
            }

            starTrajectoryPoints[starData.main_id] = trajectoryPoints;
        }
    }

    Vector3 CalculateObserverPosition(StarData starData, float distance)
    {
        float hourAngle = CalculateHourAngle(starData.ra);
        float altitude = CalculateAltitude(starData.dec, hourAngle);
        float azimuth = CalculateAzimuth(starData.dec, hourAngle);
        return AltAzToCartesian(altitude, azimuth, distance);
    }

    private float CalculateHourAngle(float ra)
    {
        if (TimeManager.Instance == null) return 0f;

        float t = (TimeManager.Instance.GetJulianDate() - 2451545.0f) / 36525.0f;
        float gst = 100.46061837f + 36000.770053608f * t + 0.000387933f * t * t - t * t * t / 38710000f;
        float lst = gst + observerLongitude;
        float hourAngle = lst - ra;
        hourAngle = (hourAngle + 360f) % 360f;
        return hourAngle;
    }

    void DrawStarTrajectories()
    {
        foreach (var starName in starTrajectoryPoints.Keys)
        {
            List<Vector3> points = starTrajectoryPoints[starName];
            StarData starData = starDataDict[starName];

            GameObject trajectoryObject = new GameObject("Trajectory_" + starName);
            trajectoryObject.transform.SetParent(transform);  // Trajectory 컴포넌트의 자식으로 설정
            LineRenderer lineRenderer = trajectoryObject.AddComponent<LineRenderer>();
            lineRenderer.material = arcMaterial;
            lineRenderer.widthMultiplier = GetWidthByMagnitude(starData.V);
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());

            trajectoryObjects.Add(trajectoryObject);  // 생성된 궤적 오브젝트를 리스트에 추가
        }
    }

    private void CalculateJulianDate(float y, float m, float d, float h, float min, float sec)
    {
        if (m <= 2)
        {
            y -= 1;
            m += 12;
        }

        float a = Mathf.Floor(y / 100);
        float b = 2 - a + Mathf.Floor(a / 4);

        julianDate = Mathf.Floor(365.25f * (y + 4716)) + Mathf.Floor(30.6001f * (m + 1)) + d + b - 1524.5f;
        float time = h + min / 60f + sec / 3600f;
        julianDate += time / 24f;
    }

    private float CalculateAltitude(float dec, float hourAngle)
    {
        float sinAlt = Mathf.Sin(dec * Mathf.Deg2Rad) * Mathf.Sin(observerLatitude * Mathf.Deg2Rad) +
                      Mathf.Cos(dec * Mathf.Deg2Rad) * Mathf.Cos(observerLatitude * Mathf.Deg2Rad) * Mathf.Cos(hourAngle * Mathf.Deg2Rad);
        return Mathf.Asin(sinAlt) * Mathf.Rad2Deg;
    }

    private float CalculateAzimuth(float dec, float hourAngle)
    {
        float altitude = CalculateAltitude(dec, hourAngle);
        float cosAz = (Mathf.Sin(dec * Mathf.Deg2Rad) - Mathf.Sin(observerLatitude * Mathf.Deg2Rad) * Mathf.Sin(altitude * Mathf.Deg2Rad)) /
                     (Mathf.Cos(observerLatitude * Mathf.Deg2Rad) * Mathf.Cos(altitude * Mathf.Deg2Rad));

        float sinAz = -Mathf.Sin(hourAngle * Mathf.Deg2Rad) * Mathf.Cos(dec * Mathf.Deg2Rad) /
                     Mathf.Cos(altitude * Mathf.Deg2Rad);

        float azimuth = Mathf.Atan2(sinAz, cosAz) * Mathf.Rad2Deg;
        return (azimuth + 360f) % 360f;
    }

    private Vector3 AltAzToCartesian(float altitude, float azimuth, float radius)
    {
        float altRad = altitude * Mathf.Deg2Rad;
        float azRad = azimuth * Mathf.Deg2Rad;

        float x = radius * Mathf.Cos(altRad) * Mathf.Sin(azRad);
        float y = radius * Mathf.Sin(altRad);
        float z = radius * Mathf.Cos(altRad) * Mathf.Cos(azRad);

        return new Vector3(x, y, z);
    }

    float GetWidthByMagnitude(float V)
    {
        float minMag = -1f;
        float maxMag = 6f;

        float minWidth = 0.03f;
        float maxWidth = 0.12f;

        float t = Mathf.InverseLerp(maxMag, minMag, V);
        return Mathf.Lerp(minWidth, maxWidth, t);
    }

    // 궤적이 존재하는지 확인하는 메서드
    public bool HasTrajectories()
    {
        return trajectoryObjects.Count > 0;
    }

    public void SetDurationHour(float duration)
    {
        durationHours = duration;
    }
} 