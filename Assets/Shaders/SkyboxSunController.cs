using UnityEngine;

public class SkyboxSunController : MonoBehaviour
{
    [SerializeField] private Light sunlight;

    private GameObject obj;
    Material _skyboxMaterial;


    private void Awake()
    {
        obj = this.gameObject;
        _skyboxMaterial = obj.GetComponent<Renderer>().sharedMaterial;

        if (_skyboxMaterial == null)
        {
            Debug.LogError("스카이박스 매터리얼이 없습니다.");
            return;
        }

        _skyboxMaterial.SetFloat("_CurrentTime", 0f);  // timeOfDay와의 동기화를 위한 초기화 작업
    }


    private void Update()
    {
        ConnectSkyboxAndDirectionLight();
    }


    private void ConnectSkyboxAndDirectionLight()
    {
        float _CurrentTime = _skyboxMaterial.GetFloat("_CurrentTime");
        sunlight.transform.rotation = Quaternion.Euler(_CurrentTime * 360f, 0f, 0f);
        sunlight.intensity = Mathf.Clamp01(Mathf.Sin(_CurrentTime * Mathf.PI));
    }


    public void SetCurrentTime(float ct)
    {
        _skyboxMaterial.SetFloat("_CurrentTime", ct);
    }
}