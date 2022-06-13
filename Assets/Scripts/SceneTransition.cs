using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace MonsterArena
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private Image _fadeImage = null;

        public void Load(SceneReference scene, bool unloadCurrent = true, Action<Scene, GameObject[]> onLoaded = null, Action onPreLoad = null)
        {
            LoadAsync(scene, unloadCurrent, onLoaded, onPreLoad);
        }

        public YieldInstruction LoadAsync(SceneReference scene, bool unloadCurrent = true, Action<Scene, GameObject[]> onLoaded = null, Action onPreLoad = null)
        {
            return StartCoroutine(LoadScene(scene, unloadCurrent, onLoaded, onPreLoad));
        }

        public void Load(string path, bool unloadCurrent = true, Action<Scene, GameObject[]> onLoaded = null, Action onPreLoad = null)
        {
            LoadAsync(path, unloadCurrent, onLoaded, onPreLoad);
        }

        public YieldInstruction LoadAsync(string path, bool unloadCurrent = true, Action<Scene, GameObject[]> onLoaded = null, Action onPreLoad = null)
        {
            return StartCoroutine(LoadScene(path, unloadCurrent, onLoaded, onPreLoad));
        }

        public void ReloadCurrent(bool unloadCurrent = true, Action<Scene, GameObject[]> onLoaded = null, Action onPreLoad = null)
        {
            ReloadCurrentAsync(unloadCurrent, onLoaded, onPreLoad);
        }

        public YieldInstruction ReloadCurrentAsync(bool unloadCurrent = true, Action<Scene, GameObject[]> onLoaded = null, Action onPreLoad = null)
        {
            return StartCoroutine(LoadScene(SceneManager.GetActiveScene().path, unloadCurrent, onLoaded, onPreLoad));
        }

        private IEnumerator LoadScene(SceneReference scene, bool unloadCurrent = true, Action<Scene, GameObject[]> onLoaded = null, Action onPreLoad = null)
        {
            return LoadScene(scene.ScenePath, unloadCurrent, onLoaded, onPreLoad);
        }

        private IEnumerator LoadScene(string path, bool unloadCurrent = true, Action<Scene, GameObject[]> onLoaded = null, Action onPreLoad = null)
        {
            _fadeImage.raycastTarget = true;
            _fadeImage.maskable = true;
            yield return _fadeImage.DOFade(1, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();

            Camera.main.gameObject.SetActive(false);

            var currentScene = SceneManager.GetActiveScene();
            if (unloadCurrent)
            {
                foreach (var root in currentScene.GetRootGameObjects().Where(x => x.transform != transform))
                {
                    root.SetActive(false);
                }
            }

            yield return null;

            var targetScene = SceneManager.LoadScene(path, new LoadSceneParameters(LoadSceneMode.Additive));
            while (!targetScene.isLoaded)
            {
                yield return null;
            }

            var rootGOs = targetScene.GetRootGameObjects();
            /*var level = rootGOs.Select(x => x.GetComponent<Level>()).First(x => x != null);
            level.Initialize(levelNum);*/
            onLoaded?.Invoke(targetScene, rootGOs);

            yield return _fadeImage.DOFade(0, 0.5f).SetEase(Ease.InQuad).WaitForCompletion();
            _fadeImage.raycastTarget = false;
            _fadeImage.maskable = false;

            SceneManager.SetActiveScene(targetScene);
            if (unloadCurrent)
            {
                yield return SceneManager.UnloadSceneAsync(currentScene);
            }
        }
    }
}
