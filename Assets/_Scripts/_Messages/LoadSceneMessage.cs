using UnityEngine.SceneManagement;

namespace _Scripts
{
    public readonly struct LoadSceneMessage
    {
        public string SceneName { get; }
        public LoadSceneMode LoadMode { get; }

        public LoadSceneMessage(string sceneName, LoadSceneMode loadMode)
        {
            SceneName = sceneName;
            LoadMode = loadMode;
        }
    }
}