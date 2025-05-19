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
    [SerializeField] float observerLongitude = 126.9780f; // 관측자 경도 (도)")]
    [Tooltip("관측자의 경도 (도)")]

    [Header("Time Settings")]
    [SerializeField] float durationHours = 3f;
    [Tooltip("관측 시간 (시간)")]
    [SerializeField] float year = 2024f;
    [Tooltip("관측 연도")]
    [SerializeField] float month = 1f;
    [Tooltip("관측 월")]
    [SerializeField] float day = 1f;
    [Tooltip("관측 일")]
    [SerializeField] float hour = 0f;
    [Tooltip("관측 시")]
    [SerializeField] float minute = 0f;
    [Tooltip("관측 분")]
    [SerializeField] float second = 0f;
    [Tooltip("관측 초")]

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
    private float julianDate;

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
            
            Vector3 initialPosition = CalculateObserverPosition(starData, hour, starData.ra);

            float latRad = observerLatitude * Mathf.Deg2Rad;

            Vector3 rotationAxis = new Vector3(
                Mathf.Cos(latRad),
                -Mathf.Sin(latRad),
                0f
            );

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
        CalculateJulianDate(year, month, day, hour, minute, second);
        float hourAngle = CalculateHourAngle(ra);
        float altitude = CalculateAltitude(starData.dec, hourAngle, observerLatitude);
        float azimuth = CalculateAzimuth(starData.dec, hourAngle, observerLatitude, altitude);
        return AltAzToCartesian(altitude, azimuth, arcRadius); // arcRadius 사용
    }

    private float CalculateHourAngle(float ra)
    {
        // 그리니치 항성시(GST) 계산
        float t = (julianDate - 2451545.0f) / 36525.0f;
        float gst = 100.46061837f + 36000.770053608f * t + 0.000387933f * t * t - t * t * t / 38710000f;

        // 관측자의 항성시(LST) 계산
        float lst = gst + observerLongitude;

        // 시각각 계산
        float hourAngle = lst - ra;

        // 0-360 범위로 정규화
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
            LineRenderer lineRenderer = trajectoryObject.AddComponent<LineRenderer>();
            lineRenderer.material = arcMaterial;
            lineRenderer.widthMultiplier = GetWidthByMagnitude(starData.V); // 밝기 기반 굵기
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }


    private void CalculateJulianDate(float y, float m, float d, float h, float min, float sec)
    {
        // 율리우스 날짜 계산 공식
        if (m <= 2)
        {
            y -= 1;
            m += 12;
        }

        float a = Mathf.Floor(y / 100);
        float b = 2 - a + Mathf.Floor(a / 4);

        julianDate = Mathf.Floor(365.25f * (y + 4716)) + Mathf.Floor(30.6001f * (m + 1)) + d + b - 1524.5f;

        // 시간 추가
        float time = h + min / 60f + sec / 3600f;
        julianDate += time / 24f;
    }

    private float CalculateAltitude(float dec, float hourAngle, float obsLat)
    {
        // 고도 계산 공식
        float sinAlt = Mathf.Sin(dec * Mathf.Deg2Rad) * Mathf.Sin(obsLat * Mathf.Deg2Rad) +
                      Mathf.Cos(dec * Mathf.Deg2Rad) * Mathf.Cos(obsLat * Mathf.Deg2Rad) * Mathf.Cos(hourAngle * Mathf.Deg2Rad);

        return Mathf.Asin(sinAlt) * Mathf.Rad2Deg;
    }

    private float CalculateAzimuth(float dec, float hourAngle, float obsLat, float alt)
    {
        // 방위각 계산 공식
        float cosAz = (Mathf.Sin(dec * Mathf.Deg2Rad) - Mathf.Sin(obsLat * Mathf.Deg2Rad) * Mathf.Sin(alt * Mathf.Deg2Rad)) /
                     (Mathf.Cos(obsLat * Mathf.Deg2Rad) * Mathf.Cos(alt * Mathf.Deg2Rad));

        float sinAz = -Mathf.Sin(hourAngle * Mathf.Deg2Rad) * Mathf.Cos(dec * Mathf.Deg2Rad) /
                     Mathf.Cos(alt * Mathf.Deg2Rad);

        float azimuth = Mathf.Atan2(sinAz, cosAz) * Mathf.Rad2Deg;

        // 0-360 범위로 정규화
        return (azimuth + 360f) % 360f;
    }

    private Vector3 AltAzToCartesian(float altitude, float azimuth, float radius)
    {
        // 고도와 방위각을 라디안으로 변환
        float altRad = altitude * Mathf.Deg2Rad;
        float azRad = azimuth * Mathf.Deg2Rad;

        // 구면 좌표를 카테시안 좌표로 변환 (고정된 반지름 사용)
        float x = radius * Mathf.Cos(altRad) * Mathf.Sin(azRad);
        float y = radius * Mathf.Sin(altRad);
        float z = radius * Mathf.Cos(altRad) * Mathf.Cos(azRad);

        return new Vector3(x, y, z);
    }

    float GetWidthByMagnitude(float V)
    {
        float minMag = -1f;  // 가장 밝은 별 (예: 시리우스 등)
        float maxMag = 6f;   // 맨눈에 보이는 한계 등급

        float minWidth = 0.03f;
        float maxWidth = 0.12f;

        // 등급을 0~1로 정규화
        float t = Mathf.InverseLerp(maxMag, minMag, V);
        return Mathf.Lerp(minWidth, maxWidth, t);
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