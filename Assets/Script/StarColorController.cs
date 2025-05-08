using UnityEngine;


public class StarColorController : MonoBehaviour
{
    private Material mat;
    public string sp_type;
    public char color_type;


    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();

        mat = renderer.material;
        
        color_type = (sp_type[0] == 'k') ? sp_type[1] : sp_type[0];
        if (color_type == 'W')
            color_type = 'O';


        switch (color_type)
        {
            case 'O':   // 청색
                mat.SetColor("_CellColor", new Color32(155, 176, 255, 255));
                mat.SetColor("_BaseColor", new Color32(155, 176, 255, 255));
                break;
            case 'B':   // 청백색
                mat.SetColor("_CellColor", new Color32(162, 200, 255, 255));
                mat.SetColor("_BaseColor", new Color32(162, 200, 255, 255));
                break;
            case 'A':   // 백색
                mat.SetColor("_CellColor", new Color32(219, 233, 255, 255));
                mat.SetColor("_BaseColor", new Color32(219, 233, 255, 255));
                break;
            case 'F':   // 황백색
                mat.SetColor("_CellColor", new Color32(255, 250, 243, 255));
                mat.SetColor("_BaseColor", new Color32(255, 250, 243, 255));
                break;
            case 'G':   // 황색
                mat.SetColor("_CellColor", new Color32(255, 245, 236, 255));
                mat.SetColor("_BaseColor", new Color32(255, 245, 236, 255));
                break;
            case 'K':   // 주황색
                mat.SetColor("_CellColor", new Color32(255, 210, 161, 255));
                mat.SetColor("_BaseColor", new Color32(255, 210, 161, 255));
                break;
            case 'M':   // 적색
                mat.SetColor("_CellColor", new Color32(255, 204, 111, 255));
                mat.SetColor("_BaseColor", new Color32(255, 204, 111, 255));
                break;
            default:    // 검정색(unknown)
                mat.SetColor("_CellColor", new Color32(0, 0, 0, 255));
                mat.SetColor("_BaseColor", new Color32(0, 0, 0, 255));
                break;
        }

    }
}

