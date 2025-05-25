using UnityEngine;

public class TimeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeControlPanel controlPanel;
    [SerializeField] private Trajectory trajectoryController;
    [SerializeField] private StarSpawner starSpawner;

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
        TimeManager.Instance.SetTime(sh, sm, ss);
    }

    private void OnDurationChanged(float duration)
    {
        trajectoryController.SetDurationHour(duration);
        starSpawner.DrawTrajectoriesOnce();
    }

}
