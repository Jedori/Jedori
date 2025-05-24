using UnityEngine;

public class InteractionMenuController : MonoBehaviour
{

    [SerializeField] private Camera _mainCamera;

    [SerializeField] private InteractionMenu _interactionMenu;
    [SerializeField] private StarSpawner _starSpawner;


    [SerializeField] private GameObject _starControlPanel;
    [SerializeField] private GameObject _screenCapturePanel;
    [SerializeField] private GameObject _timeControlPanel;

    [SerializeField] private float distance = 1f;
    [SerializeField] private float _horizontalSpacing = 0.5f;

    private void OnEnable()
    {
        _interactionMenu.OnStarControlRequested += ShowStarControlPanel;
        _interactionMenu.OnConstellationActivationRequested += SwitchConstellation;
        _interactionMenu.OnScreenCaptureRequested += ShowScreenCapturePanel;
        _interactionMenu.OnTimeControlRequested += ShowTimeControlPanel;
    }
    private void OnDisable()
    {
        _interactionMenu.OnStarControlRequested -= ShowStarControlPanel;
        _interactionMenu.OnConstellationActivationRequested -= SwitchConstellation;
        _interactionMenu.OnScreenCaptureRequested -= ShowScreenCapturePanel;
        _interactionMenu.OnTimeControlRequested -= ShowTimeControlPanel;
    }

    private void ShowStarControlPanel()
    {
        _starControlPanel.SetActive(!_starControlPanel.activeSelf);

        if (_starControlPanel.activeSelf)
            PositionPanel(_starControlPanel.transform, -1);

    }
    private void SwitchConstellation()
    {
        _starSpawner.SetConstellationLinesVisibility(!_starSpawner.ShowConstellationLines);
    }

    private void ShowScreenCapturePanel()
    {
        _screenCapturePanel.SetActive(!_screenCapturePanel.activeSelf);

        if (_screenCapturePanel.activeSelf)
            PositionPanel(_screenCapturePanel.transform, 1);
    }
    private void ShowTimeControlPanel()
    {
        _timeControlPanel.SetActive(!_timeControlPanel.activeSelf);

        if (_timeControlPanel.activeSelf)
            PositionPanel(_timeControlPanel.transform, -2);
    }

    private void PositionPanel(Transform panel, int index)
    {
        var camT = _mainCamera.transform;

        // 1) 카메라 y축 회전만 반영한 전방벡터
        Vector3 flatForward = Vector3.ProjectOnPlane(camT.forward, Vector3.up).normalized;
        // 2) 카메라 기준 오른쪽 벡터
        Vector3 flatRight = Vector3.Cross(flatForward, Vector3.up).normalized;

        // 3) 기준 위치: 카메라 앞 distance 만큼
        Vector3 basePos = camT.position + flatForward * distance;
        
        float xOffset = index * _horizontalSpacing ;

        Vector3 worldPos = basePos + flatRight * xOffset;
        panel.position = worldPos;
    }
}
