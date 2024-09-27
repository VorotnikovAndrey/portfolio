using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Defong.Utils
{
    public enum Scenes
    {
        Popups,
        Menu
    }

    public static class SceneLoader
    {
        private static Action _callback;

        public static IEnumerable<Scene> GetLoadedScenes()
        {
            var totalCount = SceneManager.sceneCount;

            for (var i = 0; i < totalCount; i++)
                yield return SceneManager.GetSceneAt(i);
        }

        public static bool IsSceneLoaded(Scenes type)
        {
            return IsSceneLoaded(type.ToString());
        }

        public static bool IsSceneLoaded(string sceneName)
        {
            return GetLoadedScenes().Any(x => x.name == sceneName);
        }

        public static AsyncOperation LoadSceneAsync(Scenes sceneType, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            return SceneManager.LoadSceneAsync(sceneType.ToString(), mode);
        }

        public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            return SceneManager.LoadSceneAsync(sceneName, mode);
        }

        public static void LoadSceneAsync(Scenes sceneType, Action callback, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            LoadSceneAsync(sceneType.ToString(), callback, mode);
        }

        public static void LoadSceneAsync(string sceneName, Action callback, LoadSceneMode mode = LoadSceneMode.Additive)
        {
           var async = SceneManager.LoadSceneAsync(sceneName, mode);
           async.completed += OnCompleted;
           _callback = callback;
        }

        public static AsyncOperation UnloadSceneAsync(Scenes sceneType)
        {
            return (IsSceneLoaded(sceneType)) ? UnloadSceneAsync(sceneType.ToString()) : null;
        }

        public static AsyncOperation UnloadSceneAsync(string sceneName)
        {
            return (IsSceneLoaded(sceneName)) ? SceneManager.UnloadSceneAsync(sceneName) : null;
        }

        public static void UnloadSceneAsync(Scenes sceneType, Action callback)
        {
            if (IsSceneLoaded(sceneType)) 
                UnloadSceneAsync(sceneType.ToString(), callback);
        }

        public static void UnloadSceneAsync(string sceneName, Action callback)
        {
            var async = SceneManager.UnloadSceneAsync(sceneName);
            async.completed += OnCompleted;
            _callback = callback;
        }

        public static void SetActiveScene(Scenes sceneType)
        {
            SetActiveScene(sceneType.ToString());
        }

        public static void SetActiveScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
                SetActiveScene(scene);
        }

        public static void SetActiveScene(Scene scene)
        {
            SceneManager.SetActiveScene(scene);
        }

        private static void OnCompleted(AsyncOperation async)
        {
            async.completed -= OnCompleted;
            _callback?.Invoke();
            _callback = null;
        }
    }

}
