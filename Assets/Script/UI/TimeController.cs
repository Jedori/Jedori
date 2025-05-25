using UnityEngine;

public class TimeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeControlPanel controlPanel;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private Trajectory trajectoryController;

    void OnEnable()
    {
        controlPanel.OnTimeChanged += OnTimeChanged;
        controlPanel.OnDurtionChanged += OnDurationChanged;
    }

    void OnDisable()
    {
        controlPanel.OnTimeChanged -= OnTimeChanged;
        controlPanel.OnDurtionChanged -= OnDurationChanged;
    }

    private void OnTimeChanged(int sh, int sm, int ss)
    {
        timeManager.SetTime(sh, sm, ss);
    }
    private void OnDurationChanged(float duration)
    {
        trajectoryController.SetDurationHour(duration);
    }

}
