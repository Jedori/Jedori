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

 
    private Dictionary<int, GameObject> hipToStar = new();
    private Dictionary<int, string> hipToSptype = new();

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

        Renderer renderer = starObject.GetComponent<Renderer>();
        string sp_type = string.IsNullOrWhiteSpace(star.sp_type) ? "  " : star.sp_type;
        char color_type = (sp_type[0] == 'k') ? sp_type[1] : sp_type[0];  // kO, kB 등으로 시작하는 sp_type 처리
        if (color_type == 'W') color_type = 'O';
        switch (color_type)
        {
        case 'O':   // 청색
            renderer.material.SetColor("_BaseColor", new Color32(0, 0, 255, 255));
            renderer.material.SetColor("_CellColor", new Color32(0, 0, 255, 255));
            break;
        case 'B':   // 청백색
            renderer.material.SetColor("_BaseColor", new Color32(135, 206, 235, 255));
            renderer.material.SetColor("_CellColor", new Color32(135, 206, 235, 255));
            break;
        case 'A':   // 백색
            renderer.material.SetColor("_BaseColor", new Color32(255, 255, 255, 255));
            renderer.material.SetColor("_CellColor", new Color32(255, 255, 255, 255));
            break;
        case 'F':   // 황백색
            renderer.material.SetColor("_BaseColor", new Color32(250, 250, 210, 255));
            renderer.material.SetColor("_CellColor", new Color32(250, 250, 210, 255));
            break;
        case 'G':   // 황색
            renderer.material.SetColor("_BaseColor", new Color32(255, 255, 0, 255));
            renderer.material.SetColor("_CellColor", new Color32(255, 255, 0, 255));
            break;
        case 'K':   // 주황색
            renderer.material.SetColor("_BaseColor", new Color32(255, 165, 0, 255));
            renderer.material.SetColor("_CellColor", new Color32(255, 165, 0, 255));
            break;
        case 'M':   // 적색
            renderer.material.SetColor("_BaseColor", new Color32(255, 0, 0, 255));
            renderer.material.SetColor("_CellColor", new Color32(255, 0, 0, 255));
            break;
        default:    // 검정색(unknown)
            renderer.material.SetColor("_BaseColor", new Color32(0, 0, 0, 255));
            renderer.material.SetColor("_CellColor", new Color32(0, 0, 0, 255));
            break;
        }


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