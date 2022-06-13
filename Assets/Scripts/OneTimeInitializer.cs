using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using MonsterArena.Extensions;

namespace MonsterArena
{
    [DefaultExecutionOrder(-9999)]
    public class OneTimeInitializer : MonoBehaviour
    {
        private static OneTimeInitializer _instance = null;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            /*foreach (var loadable in FindObjectsOfType<MonoBehaviour>().OfType<ISceneLoadable>())
            {
                loadable.SceneLoaded();
            }*/

#if UNITY_EDITOR
            Application.targetFrameRate = 60;
#else
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
#endif
            var sessionsCount = PlayerPrefs.GetInt(Constants.SessionsCountKey, 0);
            sessionsCount++;
            PlayerPrefs.SetInt(Constants.SessionsCountKey, sessionsCount);
        }

        private void Start()
        {
            GameAnalytics.Initialize();

            AnalyticsExtensions.SendGameStartEvent(PlayerPrefs.GetInt(Constants.SessionsCountKey, 0));

            var level = FindObjectOfType<Level>();
            if (level != null)
            {
                level.GameStarted();
            }
        }
    }
}
