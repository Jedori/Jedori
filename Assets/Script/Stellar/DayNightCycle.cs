using UnityEngine;
using System;

public class DayNightCycle : MonoBehaviour
{
    [Header("Observer Settings")]
    [SerializeField] private float observerLatitude;   // 위도 (deg)
    [SerializeField] private float observerLongitude;  // 경도 (deg)
    [SerializeField] private int year;
    [SerializeField] private int month;
    [SerializeField] private int day;
    [SerializeField, Range(0,23)] private int hour;
    [SerializeField, Range(0,59)] private int minute;
    [SerializeField, Range(0,59)] private int second;


    [Header("Sun & Lighting")]
    [SerializeField] private Light sun;
    [SerializeField] private Gradient skyColor;
    [SerializeField] private Gradient equatorColor;
    [SerializeField] private Gradient sunColor;

    [Header("Time Control")]
    [SerializeField] private float timeScale = 1f;     // 1 = real time, >1 = 빠르게

    private TimeManager timeManager;
    private StarSpawner starSpawner;
    private float julianDate;


    private void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
        starSpawner = FindObjectOfType<StarSpawner>();
    }

    private void Update()
    {
        observerLatitude = starSpawner.GetObserverLatitude();
        observerLongitude = starSpawner.GetObserverLongitude();

        DateTime currentDateTime = timeManager.GetCurrentDateTime();
        year = currentDateTime.Year;
        month = currentDateTime.Month;
        day = currentDateTime.Day;
        hour = currentDateTime.Hour;
        minute = currentDateTime.Minute;
        second = currentDateTime.Second;

        julianDate = timeManager.GetJulianDate();
    }

}
