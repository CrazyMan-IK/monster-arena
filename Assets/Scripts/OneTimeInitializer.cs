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
        private const string _SessionsCountKey = "_sessionsCount";

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

            Application.targetFrameRate = 60;
            var sessionsCount = PlayerPrefs.GetInt(_SessionsCountKey, 0);
            sessionsCount++;
            PlayerPrefs.SetInt(_SessionsCountKey, sessionsCount);
        }

        private void Start()
        {
            GameAnalytics.Initialize();

            AnalyticsExtensions.SendGameStartEvent(PlayerPrefs.GetInt(_SessionsCountKey, 0));

            var level = FindObjectOfType<Level>();
            if (level != null)
            {
                level.GameStarted();
            }
        }
    }
}
