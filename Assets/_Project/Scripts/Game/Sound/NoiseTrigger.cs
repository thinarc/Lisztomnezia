using _Project.Scripts.Managers;
using UnityEngine;

namespace _Project.Scripts.Sound
{
    public class NoiseTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) SoundManager.Instance.MusicLoop.CallNoise();
        }
    }
}