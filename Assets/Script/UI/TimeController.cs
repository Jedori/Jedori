using UnityEngine;

public class TimeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeControlPanel controlPanel;
    [SerializeField] private TimeManager timeManager;
    
    void OnEnable()
    {
        controlPanel.OnTimeChanged += OnTimeChanged;
    }

    void OnDisable()
    {
        controlPanel.OnTimeChanged -= OnTimeChanged;
    }

    private void OnTimeChanged(int sh, int sm, int ss, int eh, int em, int es)
    {
        timeManager.SetTime(sh, sm, ss);
        //timeManager.SetStartTime(sh, sm, ss);
        //timeManager.SetEndTime(eh, em, es);
    }

}
