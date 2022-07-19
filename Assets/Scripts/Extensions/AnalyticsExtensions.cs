using System;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;

namespace MonsterArena.Extensions
{
    public static class AnalyticsExtensions
    {
        private static YandexAppMetricaUserProfile _userProfile = new YandexAppMetricaUserProfile();

        private static int SoftSpents
        {
            get => PlayerPrefs.GetInt(Constants.SoftSpentsKey, 0);
            set => PlayerPrefs.SetInt(Constants.SoftSpentsKey, value);
        }

        public static void SendGameStartEvent(int sessionsCount)
        {
            var props = new Dictionary<string, object>() { { "count", sessionsCount } };

            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "game_start", props);
            AppMetrica.Instance.ReportEvent("game_start", props);

            var now = DateTime.Now;
            if (!PlayerPrefs.HasKey(Constants.RegistrationDayKey))
            {
                PlayerPrefs.SetString(Constants.RegistrationDayKey, now.ToString("dd.MM.yy"));
            }
            var cached = DateTime.ParseExact(PlayerPrefs.GetString(Constants.RegistrationDayKey, now.ToString("dd.MM.yy")), "dd.MM.yy", null);

            _userProfile.Apply(YandexAppMetricaAttribute.CustomNumber("session_count").WithValue(sessionsCount));
            _userProfile.Apply(YandexAppMetricaAttribute.CustomString("reg_day").WithValueIfUndefined(now.ToString("dd.MM.yy")));
            _userProfile.Apply(YandexAppMetricaAttribute.CustomNumber("days_in_game").WithValue((now.Date - cached).TotalDays));
            AppMetrica.Instance.ReportUserProfile(_userProfile);
        }

        public static void SendLevelStartEvent(int levelNumber)
        {
            var props = new Dictionary<string, object>() { { "level", levelNumber } };

            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "level_start", props);
            AppMetrica.Instance.ReportEvent("level_start", props);
        }

        public static void SendSoftSpentEvent(string name, int price)
        {
            var props = new Dictionary<string, object>() { { "type", "Upgrade" }, { "name", name }, { "amount", price }, { "count", ++SoftSpents } };

            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "soft_spent", props);
            AppMetrica.Instance.ReportEvent("soft_spent", props);
        }

        public static void UpdateUserSoft(int currentValue)
        {
            _userProfile.Apply(YandexAppMetricaAttribute.CustomNumber("current_soft").WithValue(currentValue));
            AppMetrica.Instance.ReportUserProfile(_userProfile);
        }

        public static void LogTime(int timeTotalMinutes)
        {
            var properties = new Dictionary<string, object>()
            {
                {"elapsed_minutes", timeTotalMinutes}
            };

            AppMetrica.Instance.ReportEvent("play_time", properties);
        }
    }
}
