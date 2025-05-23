using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Light sun;
    [SerializeField, Range(0, 24)] private float timeOfDay;
    [SerializeField] private float sunRotateSpeed;

    [Header("LightingPreset")]
    [SerializeField] private Gradient skyColor;
    [SerializeField] private Gradient equatorColor;
    [SerializeField] private Gradient sunColor;


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


    private void Update()
    {
        timeOfDay = _skyboxMaterial.GetFloat("_CurrentTime") * 24;
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
        float timeFraction = timeOfDay / 24;
        RenderSettings.ambientEquatorColor = equatorColor.Evaluate(timeFraction);
        RenderSettings.ambientSkyColor = skyColor.Evaluate(timeFraction);
        sun.color = sunColor.Evaluate(timeFraction);
    }
}