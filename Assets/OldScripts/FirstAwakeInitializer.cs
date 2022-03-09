using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterArena
{

    [DefaultExecutionOrder(-9999)]
    public class FirstAwakeInitializer : MonoBehaviour
    {
        private const string _SessionsCountKey = "_sessionsCount";

        private static FirstAwakeInitializer _instance = null;

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
            FindObjectOfType<Level>().GameStarted();

            Application.targetFrameRate = 60;
            var sessionsCount = PlayerPrefs.GetInt(_SessionsCountKey, 0);
            sessionsCount++;
            PlayerPrefs.SetInt(_SessionsCountKey, sessionsCount);
        }
    }
}
