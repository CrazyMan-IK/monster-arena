using System;
using MonsterArena.Extensions;
using UnityEngine;

public class PlayTimeLogger : MonoBehaviour
{
    private const string AllPlayTimeKey = "AllPlayTime";

    private const int TimeAfterChangeIntervalInMinutes = 10;
    private const int FirstIntervalInMinutes = 1;
    private const int SecondIntervalInMinutes = 5;

    private const float DelayBetweenAddIntervalInSeconds = 1;

    private TimeSpan _time;
    private float _elapsedTime;

    private string _allPlayTime
    {
        get { return PlayerPrefs.GetString(AllPlayTimeKey, TimeSpan.Zero.ToString()); }
        set { PlayerPrefs.SetString(AllPlayTimeKey, value); }
    }

    private void Start()
    {
        _time = TimeSpan.Parse(_allPlayTime);
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime < DelayBetweenAddIntervalInSeconds)
            return;

        int interval = _time.TotalMinutes >= TimeAfterChangeIntervalInMinutes ? SecondIntervalInMinutes : FirstIntervalInMinutes;
        double endMinutes = interval * ((int)_time.TotalMinutes / interval) + interval;

        _time = _time.Add(TimeSpan.FromSeconds(DelayBetweenAddIntervalInSeconds));
        _allPlayTime = _time.ToString();

        _elapsedTime = 0;

        if (_time.TotalMinutes >= endMinutes)
            AnalyticsExtensions.LogTime((int)_time.TotalMinutes);
    }
}
