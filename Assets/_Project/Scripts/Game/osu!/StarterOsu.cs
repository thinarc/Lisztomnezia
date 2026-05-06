using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.osu_
{
    public static class StarterOsu
    {
        public static async UniTask LoadAndRunGame(int level)
        {
            SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);
            await OsuContainer.WaitInit();
            await OsuController.Instance.RunGame(level);
            SceneManager.UnloadSceneAsync(3);
            OsuContainer.Initialized = false;
        }
    }
}