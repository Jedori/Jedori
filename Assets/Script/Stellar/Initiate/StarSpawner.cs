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
    [SerializeField] TextAsset dummyStarsJsonFile;
    [Tooltip("더미 별 데이터가 존재하는 JSonFile입니다.")]

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

    [Header("Display Settings")]
    [SerializeField] bool showDummyStars = false;
    [Tooltip("더미 별을 표시할지 여부입니다.")]
    [SerializeField] bool showConstellationLines = true;
    [Tooltip("별자리 선을 표시할지 여부입니다.")]

    private Dictionary<char, Dictionary<int,List<float>>> sptToRgb = new Dictionary<char, Dictionary<int,List<float>>>
    {
        {   
            'M', new Dictionary<int,List<float>> {
                    { 19, new List<float> { 1.0f, 0.491f, 0.144f } },
                    { 18, new List<float> { 1.0f, 0.518f, 0.179f } },
                    { 16, new List<float> { 1.0f, 0.542f, 0.202f } },
                    { 15, new List<float> { 1.0f, 0.607f, 0.255f } },
                    { 13, new List<float> { 1.0f, 0.648f, 0.286f } },
                    { 12, new List<float> { 1.0f, 0.644f, 0.285f } },
                    { 11, new List<float> { 1.0f, 0.641f, 0.289f } },
                    { 9, new List<float> { 1.0f, 0.638f, 0.293f } },
                    { 8, new List<float> { 1.0f, 0.638f, 0.3f } },
                    { 7, new List<float> { 1.0f, 0.638f, 0.308f } },
                    { 6, new List<float> { 1.0f, 0.638f, 0.315f } },
                    { 5, new List<float> { 1.0f, 0.637f, 0.322f } },
                    { 4, new List<float> { 1.0f, 0.635f, 0.327f } },
                    { 2, new List<float> { 1.0f, 0.637f, 0.34f } },
                    { 1, new List<float> { 1.0f, 0.635f, 0.346f } },
                    { 0, new List<float> { 1.0f, 0.636f, 0.354f } },
                }
        },
        {   
            'K', new Dictionary<int,List<float>> {
                    { 16, new List<float> { 1.0f, 0.641f, 0.369f } },
                    { 14, new List<float> { 1.0f, 0.65f, 0.389f } },
                    { 13, new List<float> { 1.0f, 0.662f, 0.411f } },
                    { 11, new List<float> { 1.0f, 0.677f, 0.439f } },
                    { 10, new List<float> { 1.0f, 0.696f, 0.47f } },
                    { 9, new List<float> { 1.0f, 0.717f, 0.501f } },
                    { 8, new List<float> { 1.0f, 0.739f, 0.533f } },
                    { 7, new List<float> { 1.0f, 0.761f, 0.565f } },
                    { 6, new List<float> { 1.0f, 0.781f, 0.595f } },
                    { 5, new List<float> { 1.0f, 0.821f, 0.657f } },
                    { 3, new List<float> { 1.0f, 0.84f, 0.691f } },
                    { 2, new List<float> { 1.0f, 0.857f, 0.722f } },
                    { 0, new List<float> { 1.0f, 0.872f, 0.753f } },
                }
        },
        {
            'G', new Dictionary<int,List<float>> {
                    { 18, new List<float> { 1.0f, 0.886f, 0.783f } },
                    { 16, new List<float> { 1.0f, 0.898f, 0.813f } },
                    { 12, new List<float> { 1.0f, 0.91f, 0.845f } },
                    { 8, new List<float> { 1.0f, 0.922f, 0.878f } },
                    { 4, new List<float> { 1.0f, 0.931f, 0.905f } },
                    { 2, new List<float> { 1.0f, 0.94f, 0.931f } },
                }
        },
        {
            'F', new Dictionary<int,List<float>> {
                    { 19, new List<float> { 1.0f, 0.951f, 0.967f } },
                    { 18, new List<float> { 1.0f, 0.96f, 0.998f } },
                    { 16, new List<float> { 0.955f, 0.931f, 1.0f } },
                    { 12, new List<float> { 0.922f, 0.908f, 1.0f } },
                    { 10, new List<float> { 0.869f, 0.871f, 1.0f } },
                    { 8, new List<float> { 0.844f, 0.855f, 1.0f } },
                    { 6, new List<float> { 0.823f, 0.84f, 1.0f } },
                    { 4, new List<float> { 0.782f, 0.812f, 1.0f } },
                    { 2, new List<float> { 0.763f, 0.799f, 1.0f } },
                    { 0, new List<float> { 0.725f, 0.773f, 1.0f } },
                }
        },
        {   
            'A', new Dictionary<int,List<float>> {
                    { 18, new List<float> { 0.692f, 0.75f, 1.0f } },
                    { 16, new List<float> { 0.674f, 0.738f, 1.0f } },
                    { 14, new List<float> { 0.636f, 0.712f, 1.0f } },
                    { 12, new List<float> { 0.606f, 0.69f, 1.0f } },
                    { 8, new List<float> { 0.556f, 0.652f, 1.0f } },
                    { 6, new List<float> { 0.546f, 0.645f, 1.0f } },
                    { 4, new List<float> { 0.531f, 0.634f, 1.0f } },
                    { 2, new List<float> { 0.508f, 0.616f, 1.0f } },
                    { 0, new List<float> { 0.483f, 0.595f, 1.0f } },
                }
        },
        {   
            'B', new Dictionary<int,List<float>> {
                    { 19, new List<float> { 0.472f, 0.586f, 1.0f } },
                    { 18, new List<float> { 0.456f, 0.572f, 1.0f } },
                    { 16, new List<float> { 0.446f, 0.562f, 1.0f } },
                    { 14, new List<float> { 0.429f, 0.547f, 1.0f } },
                    { 12, new List<float> { 0.421f, 0.541f, 1.0f } },
                    { 10, new List<float> { 0.414f, 0.536f, 1.0f } },
                    { 6, new List<float> { 0.408f, 0.532f, 1.0f } },
                    { 5, new List<float> { 0.398f, 0.524f, 1.0f } },
                    { 4, new List<float> { 0.39f, 0.517f, 1.0f } },
                    { 3, new List<float> { 0.381f, 0.509f, 1.0f } },
                    { 2, new List<float> { 0.373f, 0.503f, 1.0f } },
                    { 1, new List<float> { 0.368f, 0.498f, 1.0f } },
                }
        },
        {   
            'O', new Dictionary<int,List<float>> {
                    { 19, new List<float> { 0.361f, 0.491f, 1.0f } },
                    { 16, new List<float> { 0.357f, 0.487f, 1.0f } },
                    { 12, new List<float> { 0.359f, 0.487f, 1.0f } },
                    { 10, new List<float> { 0.358f, 0.486f, 1.0f } },
                    { 8, new List<float> { 0.357f, 0.485f, 1.0f } },
                    { 6, new List<float> { 0.357f, 0.485f, 1.0f } },
                    { 4, new List<float> { 0.359f, 0.487f, 1.0f } },
                    { 2, new List<float> { 0.361f, 0.489f, 1.0f } },
                }
        },
    };

    private Dictionary<int, GameObject> hipToStar = new();
    private Dictionary<int, StarData> starDataDict = new();
    private Dictionary<int, GameObject> dummyStarObjects = new();  // 더미 별 오브젝트 저장용
    private List<GameObject> constellationLines = new();  // 별자리 선 오브젝트 저장용
    private float julianDate;
    private float previousDistance;  // 이전 distance 값 저장용
    private bool previousShowDummyStars;
    private bool previousShowConstellationLines;

    private void Start()
    {
        previousDistance = distance;  // 초기 distance 값 저장
        previousShowDummyStars = showDummyStars;
        previousShowConstellationLines = showConstellationLines;
        
        // 별 생성
        LoadStarsFromJson();

        // 별자리 선 그리기
        if (lineJsonFile != null && showConstellationLines)
        {
            DrawConstellationLines();
        }
    }

    private void OnValidate()
    {
        // 더미 별 표시 상태가 변경된 경우
        if (previousShowDummyStars != showDummyStars)
        {
            SetDummyStarsVisibility(showDummyStars);
            previousShowDummyStars = showDummyStars;
        }

        // 별자리 선 표시 상태가 변경된 경우
        if (previousShowConstellationLines != showConstellationLines)
        {
            SetConstellationLinesVisibility(showConstellationLines);
            previousShowConstellationLines = showConstellationLines;
        }
    }

    private void Update()
    {
        if (TimeManager.Instance == null) return;

        // 시간이 지남에 따라 별의 위치 업데이트
        UpdateStarPositions();

        // 별자리 선 위치 업데이트 (showConstellationLines가 true일 때만)
        if (showConstellationLines)
        {
            UpdateConstellationLinePositions();
        }
    }

    private void UpdateConstellationLinePositions()
    {
        foreach (var line in constellationLines)
        {
            if (line != null)  // null 체크만 수행
            {
                LineRenderer lr = line.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    // 별자리 선의 이름에서 HIP 번호 배열 추출
                    string[] nameParts = line.name.Split('_');
                    if (nameParts.Length >= 2)
                    {
                        string[] hipNumbers = nameParts[1].Split(',');
                        for (int i = 0; i < lr.positionCount && i < hipNumbers.Length; i++)
                        {
                            if (int.TryParse(hipNumbers[i], out int hip))
                            {
                                if (hipToStar.TryGetValue(hip, out GameObject starObj))
                                {
                                    lr.SetPosition(i, starObj.transform.position);
                                }
                            }
                        }
                    }
                }
            }
        }

        // 별자리 선 업데이트
        UpdateConstellationLines();
    }

    public void UpdateStarPositions()
    {
        // 율리우스 날짜는 TimeManager에서 가져옴
        julianDate = TimeManager.Instance.GetJulianDate();

        // distance 값이 변경되었는지 확인
        if (previousDistance != distance)
        {
            // distance 값이 변경되면 모든 별의 거리를 업데이트
            foreach (var starData in starDataDict.Values)
            {
                if (drawMode == StarDrawMode.SameDistance)
                {
                    starData.distance_parsec = distance;
                }
            }
            previousDistance = distance;
        }

        // Simbad 별 위치 업데이트
        foreach (var star in hipToStar)
        {
            Vector3 newPosition = CalculateStarPosition(star.Value);
            star.Value.transform.position = newPosition;
        }

        // 더미 별 위치 업데이트
        foreach (var star in dummyStarObjects)
        {
            Vector3 newPosition = CalculateStarPosition(star.Value);
            star.Value.transform.position = newPosition;
        }
    }

    public void UpdateConstellationLines()
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

        // 더미 별인 경우
        if (hip == -1)
        {
            foreach (var kvp in dummyStarObjects)
            {
                if (kvp.Value == starObject)
                {
                    hip = kvp.Key;
                    break;
                }
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

        // 고도와 방위각을 3D 좌표로 변환
        return AltAzToCartesian(altitude, azimuth, starData.distance_parsec);
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
        // distance는 파섹 단위로 유지
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
        // Simbad 별 데이터 로드
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

        // 더미 별 데이터 로드
        if (dummyStarsJsonFile != null)
        {
            Debug.Log($"Loading dummy stars from: {dummyStarsJsonFile.name}");
            string[] dummyLines = dummyStarsJsonFile.text.Split('\n');
            int count = 0;
            foreach (string line in dummyLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (count >= 1000) break;  // 1000개로 제한

                try
                {
                    StarData star = JsonUtility.FromJson<StarData>(line);
                    if (star != null)
                    {
                        CreateDummyStar(star);
                        count++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing dummy star data: {e.Message}\nLine: {line}");
                }
            }
            Debug.Log($"Created {count} dummy stars");
        }
        else
        {
            Debug.LogWarning("dummyStarsJsonFile is not assigned!");
        }
    }

    /// <summary>
    /// ra, dec, 거리 정보를 사용하여 별을 생성합니다.
    /// 별의 크기는 V 등급에 따라 조정됩니다.
    /// </summary>
    /// <param name="star"></param>
    void CreateStar(StarData star)
    {
        // 별 데이터 복사본 생성
        StarData starCopy = new StarData
        {
            hip = star.hip,
            main_id = star.main_id,
            ra = star.ra,
            dec = star.dec,
            V = star.V,
            sp_type = star.sp_type,
            distance_parsec = drawMode == StarDrawMode.SameDistance ? distance : star.distance_parsec
        };

        // 별 데이터 저장
        starDataDict[star.hip] = starCopy;

        // 초기 위치 계산 및 별 생성
        Vector3 position = AltAzToCartesian(0, 0, starCopy.distance_parsec);
        GameObject starObject = Instantiate(starPrefab, position, Quaternion.identity);
        starObject.name = starCopy.main_id;

        float scale = Mathf.Clamp(starScale / (starCopy.V + 2f), 0.1f, 2f);
        starObject.transform.localScale = Vector3.one * scale;

        if (!hipToStar.ContainsKey(starCopy.hip))
        {
            hipToStar.Add(starCopy.hip, starObject);
        }
    }

    /// <summary>
    /// 더미 별을 생성합니다.
    /// </summary>
    void CreateDummyStar(StarData star)
    {
        // Simbad 별과 HIP 번호가 겹치지 않도록 조정
        int dummyHip = star.hip + 1000000;  // 더미 별의 HIP 번호를 1000000 이상으로 설정

        // 별 데이터 복사본 생성
        StarData starCopy = new StarData
        {
            hip = dummyHip,  // 수정된 HIP 번호 사용
            main_id = star.main_id,
            ra = star.ra,
            dec = star.dec,
            V = star.V,
            sp_type = star.sp_type,
            distance_parsec = drawMode == StarDrawMode.SameDistance ? distance : star.distance_parsec
        };

        // 별 데이터 저장
        starDataDict[dummyHip] = starCopy;  // 수정된 HIP 번호로 저장

        // 초기 위치 계산 및 별 생성
        Vector3 position = AltAzToCartesian(0, 0, starCopy.distance_parsec);
        GameObject starObject = Instantiate(starPrefab, position, Quaternion.identity);
        starObject.name = "Dummy_" + starCopy.main_id;

        float scale = Mathf.Clamp(starScale / (starCopy.V + 2f), 0.1f, 2f);
        starObject.transform.localScale = Vector3.one * scale;

        Color color = getColorFromSpectralTypes(star.sp_type);
        Renderer renderer = starObject.GetComponent<Renderer>();
        renderer.material.SetColor("_BaseColor", color);
        renderer.material.SetColor("_CellColor", color);

        // 더미 별 오브젝트 저장
        dummyStarObjects[dummyHip] = starObject;  // 수정된 HIP 번호로 저장
        
        // 초기 상태 설정
        starObject.SetActive(showDummyStars);
    }

    Color getColorFromSpectralTypes(string sp_type)
    {
        if (string.IsNullOrWhiteSpace(sp_type) || !sptToRgb.ContainsKey(sp_type[0]))
            sp_type = "G2V";  // default


        int p = 0;

        /*
        분광형 클래스 계산
        */
        if (!sptToRgb.ContainsKey(sp_type[p])) p++;  // kA, kB, gG 등으로 시작하는 sp_type 처리
        char sp_class = sp_type[p++];
        if (!sptToRgb.ContainsKey(sp_class)) sp_type = "G2V";  // 7가지 분광형에 포함되지 않는 경우

        /*
        분광형 값 계산 -> 0.5단위 처리하기 위해 2배 곱한 값을 딕셔너리 key로 사용
        */
        int key = (sp_type[p++] - '0') * 2;
        if (p < sp_type.Length && sp_type[p] == '.')
            key += 1;

        if (!sptToRgb[sp_class].ContainsKey(key))  // try to find the closest key
        {
            int a = key - 1;
            int b = key + 1;

            while (0 <= a || b <= 20)
            {
                if (sptToRgb[sp_class].ContainsKey(a)) {
                    key = a;
                    break;
                }
                if (sptToRgb[sp_class].ContainsKey(b)) {
                    key = b;
                    break;
                }
                a--; b++;
            }
        }
        
        /*
        분광형에 따른 색상 반환 
        */
        var rgb = sptToRgb[sp_class][key];
        Color result = new Color(rgb[0], rgb[1], rgb[2]);
        return EnhanceContrast(result, 1.5f, 2.0f);
    }

    Color EnhanceContrast(Color color, float saturationBoost, float brightnessScale)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);

        s = Mathf.Clamp01(s * saturationBoost);  // 채도 조절
        v = Mathf.Clamp01(v * brightnessScale);  // 밝기 조절

        return Color.HSVToRGB(h, s, v);
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

        int lineIndex = 0;
        foreach (var group in data.lines)
        {
            // HIP 번호들을 쉼표로 구분된 문자열로 변환
            string hipNumbers = string.Join(",", group.points);
            
            // 시작점과 끝점의 HIP 번호를 가져와서 별의 이름을 찾음
            string startStarName = "Unknown";
            string endStarName = "Unknown";
            
            if (group.points.Length >= 2)
            {
                int startHip = group.points[0];
                int endHip = group.points[group.points.Length - 1];
                
                if (hipToStar.TryGetValue(startHip, out GameObject startStar))
                {
                    startStarName = startStar.name;
                }
                if (hipToStar.TryGetValue(endHip, out GameObject endStar))
                {
                    endStarName = endStar.name;
                }
            }
            
            // 별자리 선의 이름을 "ConstellationLine_[시작별이름]_to_[끝별이름]_[인덱스]" 형식으로 생성
            GameObject lineObj = new GameObject($"ConstellationLine_{startStarName}_to_{endStarName}_{lineIndex}");
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
            lineObj.SetActive(showConstellationLines);  // 초기 상태 설정
            lineIndex++;
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

    // 관측자 위치 설정 메서드
    public void SetObserverLocation(float latitude, float longitude, float altitude)
    {
        observerLatitude = latitude;
        observerLongitude = longitude;
        observerAltitude = altitude;
    }

    // 관측자 시간 설정 메서드
    public void SetObserverHMSTime(float hour, float minute, float second)
    {
        if (TimeManager.Instance != null)
        {
            // TimeManager의 시간을 업데이트
            TimeManager.Instance.SetTimeScale(0);  // 시간 자동 진행 중지
            TimeManager.Instance.SetTime(hour, minute, second);
        }
    }

    // 관측자 날짜 설정 메서드
    public void SetObserverYMDTime(float year, float month, float day)
    {
        if (TimeManager.Instance != null)
        {
            // TimeManager의 날짜를 업데이트
            TimeManager.Instance.SetTimeScale(0);  // 시간 자동 진행 중지
            TimeManager.Instance.SetDate(year, month, day);
        }
    }

    // 지구-천체 거리 반환 메서드
    public float GetEarthToStarDistance()
    {
        return distance;
    }

    // 더미 별 표시 여부를 설정하는 public 메서드
    public void SetDummyStarsVisibility(bool visible)
    {
        showDummyStars = visible;
        foreach (var star in dummyStarObjects.Values)
        {
            if (star != null)
            {
                star.SetActive(visible);
            }
        }
        Debug.Log($"Dummy stars visibility set to: {visible}");
    }

    // 별자리 선 표시 여부를 설정하는 public 메서드
    public void SetConstellationLinesVisibility(bool visible)
    {
        showConstellationLines = visible;
        foreach (var line in constellationLines)
        {
            if (line != null)
            {
                line.SetActive(visible);
            }
        }
        Debug.Log($"Constellation lines visibility set to: {visible}");
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