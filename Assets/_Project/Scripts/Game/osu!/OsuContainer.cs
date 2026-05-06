using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.osu_
{
    public class OsuContainer : MonoBehaviour
    {
        [SerializeField] private OsuController controller;
        
        public static bool Initialized;
        public static async UniTask WaitInit() => await UniTask.WaitUntil(() => Initialized);

        private void Awake()
        {
            controller.Initialize();
            Initialized = true;
        }
    }
}