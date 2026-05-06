using _Code.Scripts.VFX;
using _Project.Scripts.Dialog;
using _Project.Scripts.Managers;
using _Project.Scripts.osu_;
using DG.Tweening;
using EvolveGames;
using UnityEngine;

namespace _Project.Scripts.Rabbit
{
    public class OsuEntity : MonoBehaviour
    {
        private SpriteRenderer _sprite;
        private PlayerController _player;

        private DialogBox _dialog;

        public int lvl = 1;

        private void Start()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _sprite.DOFade(0, 0);
            
            _dialog = GetComponentInChildren<DialogBox>();
            
            var col = GetComponent<BoxCollider>();
            var size = col.size;
            size.z = Random.Range(5.6f, 7f);
            col.size = size;
        }

        private async void ShowView()
        {
            _player.canMove = false;
            ParticleSystem_FogAvoidPlayer.Instances.ForEach(p => p.NewView(transform));
            
            await _sprite.DOFade(1, 0.4f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
            _player.targetView = true;
            
            _dialog.TriggerMessage(transform);
            await _dialog.TypeWait();
            await StarterOsu.LoadAndRunGame(lvl);
            HideView();
        }

        private async void HideView()
        {
            _player.canMove = true;
            SoundManager.Instance.MusicLoop.KillNoise();
            ParticleSystem_FogAvoidPlayer.Instances.ForEach(p => p.ReturnView());
            await _sprite.DOFade(0, 0.2f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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