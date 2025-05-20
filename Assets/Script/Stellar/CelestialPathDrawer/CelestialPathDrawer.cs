using System.Collections.Generic;
using UnityEngine;

public class CelestialPathDrawer : MonoBehaviour
{
    [Header("Star Data")]
    [SerializeField] TextAsset starJsonFile; // 별 데이터 JSON 파일
    [Tooltip("별 데이터가 있는 JSON 파일")]

    [Header("Observer Settings")]
    [SerializeField] float observerLatitude = 37.5665f; // 관측자 위도 (서울)
    [Tooltip("관측자의 위도 (도)")]
    [SerializeField] float observerLongitude = 126.9780f; // 관측자 경도 (서울)
    [Tooltip("관측자의 경도 (도)")]

    [Header("Time Settings")]
    [SerializeField] float startHour = 0f;
    [Tooltip("관측 시작 시간 (0-23)")]
    [SerializeField] float durationHours = 3f;
    [Tooltip("관측 시간 (시간)")]
    [SerializeField] float year = 2024f;
    [Tooltip("관측 연도")]
    [SerializeField] float month = 1f;
    [Tooltip("관측 월")]
    [SerializeField] float day = 1f;
    [Tooltip("관측 일")]

    [Header("Arc Settings")]
    [SerializeField] float arcRadius = 20f;
    [Tooltip("호의 반지름")]
    [SerializeField] Material arcMaterial;
    [Tooltip("호의 재질")]
    [SerializeField] float arcWidth = 0.05f;
    [Tooltip("호의 두께")]
    [SerializeField] int arcSegments = 30;
    [Tooltip("호의 분할 수")]

    private Dictionary<string, List<Vector3>> starTrajectoryPoints = new Dictionary<string, List<Vector3>>();
    private Dictionary<string, StarData> starDataDict = new Dictionary<string, StarData>();
    private const float EARTH_ROTATION_PERIOD = 23.934472f; // 지구 자전 주기 (시간)

    void Start()
    {
        LoadStarDataFromJson();
        CalculateStarTrajectories();
        DrawStarTrajectories();
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
        foreach (var starData in starDataDict.Values)
        {
            List<Vector3> trajectoryPoints = new List<Vector3>();
            Vector3 initialPosition = CalculateObserverPosition(starData, startHour, starData.ra);

            Vector3 rotationAxis;
            if (observerLatitude >= 0) // 북반구
            {
                rotationAxis = new Vector3(
                    0f,
                    Mathf.Cos(observerLatitude * Mathf.Deg2Rad),
                    -Mathf.Sin(observerLatitude * Mathf.Deg2Rad) // z축 부호 반전
                );
            }
            else // 남반구
            {
                rotationAxis = new Vector3(
                    0f,
                    Mathf.Cos(Mathf.Abs(observerLatitude) * Mathf.Deg2Rad),
                    Mathf.Sin(Mathf.Abs(observerLatitude) * Mathf.Deg2Rad) // z축 부호 반전
                );
            }

            for (int i = 0; i <= arcSegments; i++)
            {
                float timeRatio = (durationHours / arcSegments) * i / EARTH_ROTATION_PERIOD;
                float azimuthChange = 360f * timeRatio;

                // 회전 변환 생성 (회전축 기준 회전)
                Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.AngleAxis(azimuthChange, rotationAxis));
                Vector3 rotatedPosition = rotationMatrix.MultiplyPoint(initialPosition);

                trajectoryPoints.Add(rotatedPosition);
            }

            starTrajectoryPoints[starData.main_id] = trajectoryPoints;
        }
    }


    Vector3 CalculateObserverPosition(StarData starData, float hour, float ra)
    {
        // 초기 위치 계산 (시각각 사용)
        float julianDate = CalculateJulianDate(year, month, day, hour, 0f, 0f);
        float hourAngle = CalculateHourAngle(ra, julianDate, observerLongitude); // 시각각 계산
        float altitude = CalculateAltitude(starData.dec, 0f, observerLatitude);
        float azimuth = CalculateAzimuth(starData.dec, hourAngle, observerLatitude, altitude);
        return AltAzToCartesian(altitude, azimuth, arcRadius);
    }
    float CalculateHourAngle(float ra, float julianDate, float observerLongitude)
    {
        // 1. 그리니치 항성시 (GST) 계산
        float t = (julianDate - 2451545.0f) / 36525.0f;
        float gst =
            280.46061837f +
            360.98564736629f * (julianDate - 2451545f) +
            0.000387933f * t * t -
            t * t * t / 38710000f;
        gst = gst % 360f;
        if (gst < 0)
           gst += 360f;

        // 2. 관측지 항성시 (LST) 계산
        float lst = gst + observerLongitude;
        lst = lst % 360f;
        if (lst < 0)
            lst += 360f;

        // 3. 시각각 (HA) 계산
        float hourAngle = lst - ra * 15f; // RA를 도 단위로 변환
        hourAngle = hourAngle % 360f;
        if (hourAngle > 180f)
            hourAngle -= 360f;
        if (hourAngle < -180f)
            hourAngle += 360f;

        return hourAngle;
    }

    void DrawStarTrajectories()
    {
        foreach (var starName in starTrajectoryPoints.Keys)
        {
            List<Vector3> points = starTrajectoryPoints[starName];

            GameObject trajectoryObject = new GameObject("Trajectory_" + starName);
            LineRenderer lineRenderer = trajectoryObject.AddComponent<LineRenderer>();
            lineRenderer.material = arcMaterial;
            lineRenderer.widthMultiplier = arcWidth;
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }

    float CalculateJulianDate(float year, float month, float day, float hour, float minute, float second)
    {
        float y = year;
        float m = month;
        if (m <= 2)
        {
            y -= 1;
            m += 12;
        }

        float a = Mathf.Floor(y / 100);
        float b = 2 - a + Mathf.Floor(y / 4);

        float jd = Mathf.Floor(365.25f * (y + 4716)) + Mathf.Floor(30.6001f * (m + 1)) + day + b - 1524.5f;

        // 시간 추가
        float time = hour + minute / 60f + second / 3600f;
        jd += time / 24f;

        return jd;
    }

    float CalculateAltitude(float dec, float hourAngle, float observerLatitude)
    {
        float sinAlt =
            Mathf.Sin(dec * Mathf.Deg2Rad) *
            Mathf.Sin(observerLatitude * Mathf.Deg2Rad) +
            Mathf.Cos(dec * Mathf.Deg2Rad) *
            Mathf.Cos(observerLatitude * Mathf.Deg2Rad) *
            Mathf.Cos(hourAngle * Mathf.Deg2Rad);

        return Mathf.Asin(sinAlt) * Mathf.Rad2Deg;
    }

    float CalculateAzimuth(
        float dec,
        float hourAngle,
        float observerLatitude,
        float altitude
    )
    {
        float sinAz =
            Mathf.Sin(hourAngle * Mathf.Deg2Rad) *
            Mathf.Cos(dec * Mathf.Deg2Rad) /
            Mathf.Cos(altitude * Mathf.Deg2Rad);
        float cosAz =
            (
                Mathf.Sin(observerLatitude * Mathf.Deg2Rad) *
                Mathf.Sin(dec * Mathf.Deg2Rad) -
                Mathf.Cos(observerLatitude * Mathf.Deg2Rad) *
                Mathf.Cos(dec * Mathf.Deg2Rad) *
                Mathf.Cos(hourAngle * Mathf.Deg2Rad)
            ) /
            Mathf.Cos(altitude * Mathf.Deg2Rad);

        float azimuth = Mathf.Atan2(sinAz, cosAz) * Mathf.Rad2Deg;
        azimuth = (azimuth + 360f) % 360f;
        return azimuth;
    }

    Vector3 AltAzToCartesian(float altitude, float azimuth, float distance)
    {
        float altRad = altitude * Mathf.Deg2Rad;
        float azRad = azimuth * Mathf.Deg2Rad;

        float x = distance * Mathf.Cos(altRad) * Mathf.Sin(azRad);
        float y = distance * Mathf.Sin(altRad);
        float z = distance * Mathf.Cos(altRad) * Mathf.Cos(azRad);

        return new Vector3(x, y, z);
    }

    [System.Serializable]
    public class StarData
    {
        public int hip;
        public string main_id;
        public float ra;
        public float dec;
        public float V;
        public string sp_type;
        public float distance_parsec;
    }
}