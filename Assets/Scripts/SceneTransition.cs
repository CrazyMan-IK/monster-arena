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
        [SerializeField] private SceneReference _current = null;
        [SerializeField] private Image _fadeImage = null;

        public void Load(SceneReference scene, Action<GameObject[]> onLoaded = null)
        {
            LoadAsync(scene, onLoaded);
        }

        public YieldInstruction LoadAsync(SceneReference scene, Action<GameObject[]> onLoaded = null)
        {
            return StartCoroutine(LoadScene(scene, onLoaded));
        }

        public void ReloadCurrent(Action<GameObject[]> onLoaded = null)
        {
            LoadAsync(_current, onLoaded);
        }

        public YieldInstruction ReloadCurrentAsync(Action<GameObject[]> onLoaded = null)
        {
            return LoadAsync(_current, onLoaded);
        }

        private IEnumerator LoadScene(SceneReference scene, Action<GameObject[]> onLoaded = null)
        {
            _fadeImage.raycastTarget = true;
            _fadeImage.maskable = true;
            yield return _fadeImage.DOFade(1, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();

            Camera.main.gameObject.SetActive(false);

            var currentScene = SceneManager.GetActiveScene();
            foreach (var root in currentScene.GetRootGameObjects().Where(x => x.transform != transform))
            {
                root.SetActive(false);
            }

            yield return null;

            var targetScene = SceneManager.LoadScene(scene, new LoadSceneParameters(LoadSceneMode.Additive));
            while (!targetScene.isLoaded)
            {
                yield return null;
            }

            var rootGOs = targetScene.GetRootGameObjects();
            /*var level = rootGOs.Select(x => x.GetComponent<Level>()).First(x => x != null);
            level.Initialize(levelNum);*/
            onLoaded?.Invoke(rootGOs);

            yield return _fadeImage.DOFade(0, 0.5f).SetEase(Ease.InQuad).WaitForCompletion();
            _fadeImage.raycastTarget = false;
            _fadeImage.maskable = false;

            SceneManager.SetActiveScene(targetScene);
            yield return SceneManager.UnloadSceneAsync(currentScene);
        }
    }
}
