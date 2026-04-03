using _Code.Scripts.VFX;
using _Project.Scripts.Managers;
using _Project.Scripts.osu_;
using _Project.Scripts.Sense;
using DG.Tweening;
using EvolveGames;
using UnityEngine;

namespace _Project.Scripts.Rabbit
{
    public class RabbitEntity : MonoBehaviour
    {
        [SerializeField] private SenseType rewardType;
        [SerializeField] private float rewardValue = 0.3f;

        private SpriteRenderer _sprite;
        private PlayerController _player;

        public int lvl = 1;

        private void Start()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();

            _sprite.DOFade(0, 0);
            // OsuController.Instance.RunGame(1);
        }

        private async void ShowView()
        {
            _player.canMove = false;
            ParticleSystem_FogAvoidPlayer.Instances.ForEach(p => p.NewView(transform));
            
            await _sprite.DOFade(1, 0.4f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
            _player.targetView = true;
            
            await OsuController.Instance.RunGame(lvl);
            HideView();
        }

        private async void HideView()
        {
            _player.canMove = true;
            SoundManager.Instance.MusicLoop.KillNoise();
            ParticleSystem_FogAvoidPlayer.Instances.ForEach(p => p.ReturnView());
            await _sprite.DOFade(0, 0.2f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            _player = other.GetComponent<PlayerController>();
            if (other.CompareTag("Player")) ShowView();
        }
    }
}