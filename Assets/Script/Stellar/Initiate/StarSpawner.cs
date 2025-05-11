using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 힙 번호로 별을 생성하고 별자리 선을 그리는 스크립트입니다.
/// 별 데이터는 JSON 파일에서 로드됩니다.
/// </summary>
public class StarSpawner : MonoBehaviour
{   
    [Header("Star Data")]
    [SerializeField] TextAsset lineJsonFile;
    [Tooltip("별 데이터가 존재하는 JSonFile입니다.")]

    [Header("Star Prefab")]
    [SerializeField] GameObject starPrefab;
    [Tooltip("별을 생성할 프리팹입니다.")]
    [SerializeField] Material lineMaterial;
    [Tooltip("별자리 선을 그릴 재질입니다.")]
    [SerializeField] float lineWidth = 0.05f;
    [Tooltip("별자리 선의 두께입니다. 기본값 0.05f")]
    [SerializeField] float starScale = 10f;
    [Tooltip("별의 크기입니다. 기본값 10f")]

    [Header("Spawner Settings")]
    [SerializeField] StarDrawMode drawMode = StarDrawMode.SameDistance;
    [Tooltip("별을 그릴 모드입니다. SameDistance는 모든 별을 같은 거리로 그립니다. ActualStarDistance는 실제 거리를 사용합니다.")]
    [SerializeField] float distance = 20f;
    [Tooltip("별이 천구 상의 위치하는 거리입니다. 기본값 20f")]

    [Header("Observer Settings")]
    [SerializeField] float observerLatitude = 37.5665f;  // 서울 위도
    [Tooltip("관측자의 위도 (도)")]
    [SerializeField] float observerLongitude = 126.9780f;  // 서울 경도
    [Tooltip("관측자의 경도 (도)")]
    [SerializeField] float observerAltitude = 0f;  // 해발고도 (미터)
    [Tooltip("관측자의 해발고도 (미터)")]

    [Header("Time Settings")]
    [SerializeField] float year = 2024f;
    [Tooltip("관측 연도")]
    [SerializeField] float month = 1f;
    [Tooltip("관측 월")]
    [SerializeField] float day = 1f;
    [Tooltip("관측 일")]
    [SerializeField] float hour = 0f;
    [Tooltip("관측 시간 (0-23)")]
    [SerializeField] float minute = 0f;
    [Tooltip("관측 분 (0-59)")]
    [SerializeField] float second = 0f;
    [Tooltip("관측 초 (0-59)")]
    [SerializeField] float timeZone = 9f;  // 한국 시간대
    [Tooltip("시간대 (UTC 기준)")]

    private Dictionary<int, GameObject> hipToStar = new();
    private Dictionary<int, StarData> starDataDict = new();
    private List<GameObject> constellationLines = new();  // 별자리 선 오브젝트 저장용
    private float julianDate;

    private void Start()
    {
        // 별 생성
        LoadStarsFromJson();

        // 별자리 선 그리기
        if (lineJsonFile != null)
        {
            DrawConstellationLines();
        }
    }

    private void Update()
    {
        // 시간이 지남에 따라 별의 위치 업데이트
        UpdateStarPositions();
        
        // 별자리 선 업데이트
        UpdateConstellationLines();
    }

    private void UpdateStarPositions()
    {
        // 율리우스 날짜 계산
        CalculateJulianDate();

        foreach (var star in hipToStar)
        {
            // 별의 현재 위치 계산
            Vector3 newPosition = CalculateStarPosition(star.Value);
            star.Value.transform.position = newPosition;
        }
    }

    private void UpdateConstellationLines()
    {
        // 기존 별자리 선 제거
        foreach (var line in constellationLines)
        {
            if (line != null)
            {
                Destroy(line);
            }
        }
        constellationLines.Clear();

        // 새로운 별자리 선 그리기
        DrawConstellationLines();
    }

    private void CalculateJulianDate()
    {
        // 율리우스 날짜 계산 공식
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

    private Vector3 CalculateStarPosition(GameObject starObject)
    {
        // 별의 HIP 번호 찾기
        int hip = -1;
        foreach (var kvp in hipToStar)
        {
            if (kvp.Value == starObject)
            {
                hip = kvp.Key;
                break;
            }
        }

        if (hip == -1 || !starDataDict.ContainsKey(hip))
        {
            Debug.LogWarning($"별 데이터를 찾을 수 없습니다: {starObject.name}");
            return starObject.transform.position;
        }

        StarData starData = starDataDict[hip];

        // 시각각 계산
        float hourAngle = CalculateHourAngle(starData.ra);
        
        // 고도와 방위각 계산
        float altitude = CalculateAltitude(starData.dec, hourAngle);
        float azimuth = CalculateAzimuth(starData.dec, hourAngle);

        // 거리 계산 (파섹 → 광년 변환 및 최소 거리 설정)
        float distanceInLightYears = starData.distance_parsec * 3.262f;  // 파섹 → 광년
        if (drawMode == StarDrawMode.SameDistance)
        {
            distanceInLightYears = distance;  // SameDistance 모드에서는 고정 거리 사용
        }
        else if (drawMode == StarDrawMode.ActualStarDistance)
        {
            distanceInLightYears = starData.distance_parsec * 3.262f;  // ActualStarDistance 모드에서는 실제 거리 사용
        }
        else
        {
            distanceInLightYears = 20f;
        }
        float scaledDistance = Mathf.Max(20f, distanceInLightYears);  // 최소 거리 20f

        // 고도와 방위각을 3D 좌표로 변환
        return AltAzToCartesian(altitude, azimuth, scaledDistance);
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

    private float CalculateAltitude(float dec, float hourAngle)
    {
        // 고도 계산 공식
        float sinAlt = Mathf.Sin(dec * Mathf.Deg2Rad) * Mathf.Sin(observerLatitude * Mathf.Deg2Rad) +
                      Mathf.Cos(dec * Mathf.Deg2Rad) * Mathf.Cos(observerLatitude * Mathf.Deg2Rad) * Mathf.Cos(hourAngle * Mathf.Deg2Rad);
        
        return Mathf.Asin(sinAlt) * Mathf.Rad2Deg;
    }

    private float CalculateAzimuth(float dec, float hourAngle)
    {
        // 방위각 계산 공식
        float cosAz = (Mathf.Sin(dec * Mathf.Deg2Rad) - Mathf.Sin(observerLatitude * Mathf.Deg2Rad) * Mathf.Sin(CalculateAltitude(dec, hourAngle) * Mathf.Deg2Rad)) /
                     (Mathf.Cos(observerLatitude * Mathf.Deg2Rad) * Mathf.Cos(CalculateAltitude(dec, hourAngle) * Mathf.Deg2Rad));
        
        float sinAz = -Mathf.Sin(hourAngle * Mathf.Deg2Rad) * Mathf.Cos(dec * Mathf.Deg2Rad) /
                     Mathf.Cos(CalculateAltitude(dec, hourAngle) * Mathf.Deg2Rad);
        
        float azimuth = Mathf.Atan2(sinAz, cosAz) * Mathf.Rad2Deg;
        
        // 0-360 범위로 정규화
        return (azimuth + 360f) % 360f;
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

    /// <summary>
    /// JSON 파일에서 별 데이터를 로드하고 별을 생성합니다.
    /// </summary>
    void LoadStarsFromJson()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("simbad_results");
        if (jsonText == null)
        {
            Debug.LogError("'simbad_results' 리소스를 찾을 수 없습니다.");
            return;
        }

        string[] jsonLines = jsonText.text.Split('\n');
        foreach (string line in jsonLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            StarData star = JsonUtility.FromJson<StarData>(line);
            CreateStar(star);
        }
    }

    /// <summary>
    /// ra, dec, 거리 정보를 사용하여 별을 생성합니다.
    /// 별의 크기는 V 등급에 따라 조정됩니다.
    /// </summary>
    /// <param name="star"></param>
    void CreateStar(StarData star)
    {
        // 별 데이터 저장
        starDataDict[star.hip] = star;

        // 거리 계산 (파섹 → 광년 변환 및 최소 거리 설정)
        float distanceInLightYears;  // 파섹 → 광년
        if (drawMode == StarDrawMode.SameDistance)
        {
            distanceInLightYears = distance;  // SameDistance 모드에서는 고정 거리 사용
        }
        else if (drawMode == StarDrawMode.ActualStarDistance)
        {
            distanceInLightYears = star.distance_parsec * 3.262f;  // ActualStarDistance 모드에서는 실제 거리 사용
        }
        else
        {
            distanceInLightYears = 20f;
        }
        distanceInLightYears = Mathf.Max(20f, distanceInLightYears);  // 최소 거리 20f
        // 초기 위치 계산 및 별 생성
        Vector3 position = EquatorialToCartesian(star.ra, star.dec, distanceInLightYears);
        GameObject starObject = Instantiate(starPrefab, position, Quaternion.identity);
        starObject.name = star.main_id;

        float scale = Mathf.Clamp(starScale / (star.V + 2f), 0.1f, 2f);
        starObject.transform.localScale = Vector3.one * scale;

        if (!hipToStar.ContainsKey(star.hip))
        {
            hipToStar.Add(star.hip, starObject);
        }
    }

    Vector3 EquatorialToCartesian(float raDeg, float decDeg, float distance)
    {
        float raRad = raDeg * Mathf.Deg2Rad;
        float decRad = decDeg * Mathf.Deg2Rad;

        float x = distance * Mathf.Cos(decRad) * Mathf.Cos(raRad);
        float y = distance * Mathf.Sin(decRad);
        float z = distance* Mathf.Cos(decRad) * Mathf.Sin(raRad);

        return new Vector3(x, y, z);
    }

    void DrawConstellationLines()
    {
        if (lineJsonFile == null)
        {
            Debug.LogError("lineJsonFile 비어 있음!");
            return;
        }

        string raw = lineJsonFile.text.Trim();
        ConstellationData data = JsonUtility.FromJson<ConstellationData>(raw);

        foreach (var group in data.lines)
        {
            GameObject lineObj = new GameObject("ConstellationLine");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            lr.positionCount = group.points.Length;
            lr.widthMultiplier = lineWidth;
            lr.material = lineMaterial;
            lr.useWorldSpace = true;

            for (int i = 0; i < group.points.Length; i++)
            {
                int hip = group.points[i];
                if (hipToStar.TryGetValue(hip, out GameObject starObj))
                {
                    lr.SetPosition(i, starObj.transform.position);
                }
                else
                {
                    Debug.LogWarning($"HIP {hip} not found in star dictionary");
                }
            }

            constellationLines.Add(lineObj);  // 별자리 선 오브젝트 저장
        }
    }

    [System.Serializable]
    public class LineGroup
    {
        public int[] points;
    }

    [System.Serializable]
    public class ConstellationData
    {
        public LineGroup[] lines;
    }
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

public enum StarDrawMode
{
    SameDistance,
    ActualStarDistance
}
