using _Project.Scripts.Managers;
using UnityEngine;

namespace _Project.Scripts.Sound
{
    public class PoolPhrases : MonoBehaviour
    {
        // private EventInstance _beginning1;
        
        private void Start() 
        {
            SoundManager.Instance.InjectPhrases(this);
            // _beginning1 = RuntimeManager.CreateInstance("event:/Phrases/Beggining 1");
        }
    }
}