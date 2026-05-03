using _Code.Scripts.VFX;
using _Project.Scripts.Dialog;
using _Project.Scripts.Diary;
using _Project.Scripts.Managers;
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

        private DialogBox _dialog;

        private void Start()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _sprite.DOFade(0, 0);
            
            _dialog = GetComponentInChildren<DialogBox>();
        }

        private async void ShowView()
        {
            _player.canMove = false;
            ParticleSystem_FogAvoidPlayer.Instances.ForEach(p => p.NewView(transform));
            
            await _sprite.DOFade(1, 0.4f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
            _player.targetView = true;
            
            _dialog.TriggerMessage(transform);
            _dialog.world = false;
            
            await DiaryController.Instance.EnterDiary();
            _dialog.TriggerMessage(transform);
            HideView();
        }

        private async void HideView()
        {
            _player.canMove = true;
            SoundManager.Instance.MusicLoop.KillNoise();
            ParticleSystem_FogAvoidPlayer.Instances.ForEach(p => p.ReturnView());
            await _sprite.DOFade(0, 0.2f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
            _player.targetView = false;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _player = other.GetComponent<PlayerController>();
            ShowView();
        }
    }
}