using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Light sun;

    [Header("LightingPreset")]
    [SerializeField] private Gradient skyColor;
    [SerializeField] private Gradient equatorColor;
    [SerializeField] private Gradient sunColor;

    [Header("Settings")]
    [SerializeField, Range(0, 24)] private float timeOfDay;

    private Material _skyboxMaterial;


    private void Awake()
    {
        _skyboxMaterial = RenderSettings.skybox;
        if (_skyboxMaterial == null)
        {
            Debug.LogError("스카이박스 매터리얼을 발견하지 못했습니다.");
            return;
        }

        timeOfDay = 0f;  // CurrentTime과의 동기화를 위한 초기화 작업
    }


    private void Start()
    {
        UpdateSunRotation();
        UpdateLighting();
    }


    private void Update()
    {
        if (timeOfDay > 24)
            timeOfDay -= 24;

        UpdateSunRotation();
        UpdateLighting();
    }


    private void UpdateSunRotation()
    {
        float sunRotation = Mathf.Lerp(-90, 270, timeOfDay / 24);
        sun.transform.rotation = Quaternion.Euler(sunRotation, sun.transform.rotation.y, sun.transform.rotation.z);
    }

    private void UpdateLighting()
    {
        float dayProgressed = timeOfDay / 24;
        RenderSettings.ambientEquatorColor = equatorColor.Evaluate(dayProgressed);
        RenderSettings.ambientSkyColor = skyColor.Evaluate(dayProgressed);
        sun.color = sunColor.Evaluate(dayProgressed);
    }


    public void SetTimeOfDay(float tod, bool isDirectional)
    {
        sun.type = isDirectional ? LightType.Directional : LightType.Point;
        timeOfDay = tod;
    }
}
