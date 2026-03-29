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
        private float _speed;

        void Start()
        {
            _footsteps = RuntimeManager.CreateInstance("event:/Player/Footsteps");
            _footsteps.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            _footsteps.start();
            _footsteps.setPaused(true);
        }

        private void Update()
        {
            var targetWalk = playerController.Velocity / playerController.MaxSpeed;
            if (playerController.IsRunning) targetWalk += 1;
            
            bool isMoving = playerController.Velocity > 0.01f;
            if (!isMoving) _speed = Mathf.MoveTowards(_speed, 0, Time.deltaTime * 4);
            else _speed = Mathf.MoveTowards(_speed, targetWalk + 1, Time.deltaTime * 4);

            if (isMoving && playerController.canMove)
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
            
            _footsteps.setParameterByName("Speed", _speed);
        }
    }
}