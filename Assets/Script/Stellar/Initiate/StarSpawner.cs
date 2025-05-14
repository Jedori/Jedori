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
        switch (drawMode)
        {
            case StarDrawMode.SameDistance:
                star.distance_parsec = distance;
                break;
            case StarDrawMode.ActualStarDistance:
                // 실제 거리 사용
                break;
            default:
                Debug.LogError("알 수 없는 별 그리기 모드입니다.");
                return;
        }

        Vector3 position = EquatorialToCartesian(star.ra, star.dec, star.distance_parsec);
        GameObject starObject = Instantiate(starPrefab, position, Quaternion.identity);
    
        starObject.name = star.main_id;

        float scale = Mathf.Clamp(10f / (star.V + 2f), 0.1f, 2f);
        starObject.transform.localScale = Vector3.one * scale;

        Color color = getColorFromSpectralTypes(star.sp_type);
        Renderer renderer = starObject.GetComponent<Renderer>();
        renderer.material.SetColor("_BaseColor", color);
        renderer.material.SetColor("_CellColor", color);


        /*
        Add star instance to dictionary
        */
        if (!hipToStar.ContainsKey(star.hip))
        {
            hipToStar.Add(star.hip, starObject);
        }
    }

    Vector3 EquatorialToCartesian(float raDeg, float decDeg, float distance)
    {
        distance *= 3.262f; // 파섹 → 광년
        float raRad = raDeg * Mathf.Deg2Rad;
        float decRad = decDeg * Mathf.Deg2Rad;

        float x = distance * Mathf.Cos(decRad) * Mathf.Cos(raRad);
        float y = distance * Mathf.Cos(decRad) * Mathf.Sin(raRad);
        float z = distance * Mathf.Sin(decRad);

        return new Vector3(x, y, z);
    }

    Color getColorFromSpectralTypes(string sp_type)
    {
        if (string.IsNullOrWhiteSpace(sp_type) || !sptToRgb.ContainsKey(sp_type[0]))
            sp_type = "G2V";  // default


        int p = 0;

        /*
        분광형 클래스 계산
        */
        if (sp_type[p] == 'k') p++;  // kA, kB 등으로 시작하는 sp_type 처리
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
        if (!sptToRgb[sp_class].ContainsKey(key))
        {
            Debug.LogError($"{sp_class}{key} is not mapped...");
            return new Color(0f, 0f, 0f);  // if key is not found...
        }

        var rgb = sptToRgb[sp_class][key];
        return new Color(rgb[0], rgb[1], rgb[2]);
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