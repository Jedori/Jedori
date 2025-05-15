using System;
using UnityEngine.UI;
using UnityEngine;

public class InteractionMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _starContorlButton;
    [SerializeField] private Button _constellationActivationButton;
    [SerializeField] private Button _screenCaptureButton;
    [SerializeField] private Button _TimeControlButton;


    public event Action OnStarControlRequested;
    public event Action OnConstellationActivationRequested;
    public event Action OnScreenCaptureRequested;
    public event Action OnTimeControlRequested;
    private void Awake()
    {
        _starContorlButton.onClick.AddListener(() => OnStarControlRequested?.Invoke());
        _constellationActivationButton.onClick.AddListener(() => OnConstellationActivationRequested?.Invoke());
        _screenCaptureButton.onClick.AddListener(() => OnScreenCaptureRequested?.Invoke());
        _TimeControlButton.onClick.AddListener(() => OnTimeControlRequested?.Invoke());
    }

    private void OnDestroy()
    {
        _starContorlButton.onClick.RemoveAllListeners();
        _constellationActivationButton.onClick.RemoveAllListeners();
        _screenCaptureButton.onClick.RemoveAllListeners();
        _TimeControlButton.onClick.RemoveAllListeners();
    }
}
