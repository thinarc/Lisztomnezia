using _Project.Scripts.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.HUD
{
    public class StartOptions : MonoBehaviour
    {
        public async UniTask CheckOptions() => await UniTask.Delay(0);
        // {
            // var soundManager = new SoundManager();
            // soundManager.Initialize();
            //
            // await UniTask.NextFrame();
            //
            // var langManager = new LangManager();
            // langManager.Initialize(0);
        // }
    }
}