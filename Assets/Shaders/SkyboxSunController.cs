using UnityEngine;

public class SkyboxSunController : MonoBehaviour
{
    [SerializeField] private Light _directionalLight;
    Material _skyboxMaterial;

    private GameObject obj;
    void Awake()
    {
        obj = this.gameObject;
        _skyboxMaterial = obj.GetComponent<Renderer>().sharedMaterial;
        if (_skyboxMaterial == null)
        {
            Debug.LogError("스카이박스 메터리얼이 없습니다..");
            return;
        }

    }
    
    void Update()
    {
        ConnectSkyboxAndDirectionLight();
    }

    public void ConnectSkyboxAndDirectionLight()
    {
        float _CurrentTime = _skyboxMaterial.GetFloat("_CurrentTime");
        _directionalLight.transform.rotation = Quaternion.Euler(_CurrentTime * 360f, 0f, 0f);
        _directionalLight.intensity = Mathf.Clamp01(1f - _CurrentTime);
    }
    
    
}
