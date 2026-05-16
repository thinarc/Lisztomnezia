using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.HUD
{
    public class StartOptions : MonoBehaviour
    {
        public async UniTask CheckOptions()
        {
            await UniTask.NextFrame();
        }
    }
}