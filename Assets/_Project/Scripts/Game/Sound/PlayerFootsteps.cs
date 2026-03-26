using EvolveGames;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace _Project.Scripts.Sound
{
    public class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        
        private EventInstance _footsteps;

        void Start()
        {
            _footsteps = RuntimeManager.CreateInstance("event:/Player/Footsteps");
            _footsteps.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            _footsteps.start();
            _footsteps.setPaused(true);
        }

        private void Update()
        {
            bool isMoving = playerController.Velocity > 0.01f;

            if (isMoving)
            {
                PLAYBACK_STATE state;
                _footsteps.getPlaybackState(out state);

                if (state is PLAYBACK_STATE.STOPPED or PLAYBACK_STATE.STOPPING)
                {
                    _footsteps = RuntimeManager.CreateInstance("event:/Player/Footsteps");
                    _footsteps.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
                    _footsteps.start();
                }
            }
            else
            {
                _footsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
}