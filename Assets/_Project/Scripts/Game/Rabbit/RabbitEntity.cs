using _Code.Scripts.VFX;
using _Project.Scripts.Dialog;
using _Project.Scripts.Diary;
using _Project.Scripts.Effects;
using _Project.Scripts.Managers;
using _Project.Scripts.Sense;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EvolveGames;
using UnityEngine;

namespace _Project.Scripts.Rabbit
{
    public class RabbitEntity : MonoBehaviour
    {
        [SerializeField] private RabbitType rewardType = RabbitType.Vision;

        [SerializeField] private Material burnMaterial;
        [SerializeField] private Material absorptionMaterial;
        private MaterialPropertyBlock _burnProperties;
        private float _burnProgress;
        private float _emittedBurnProgress;
        private bool _catchFading;
        private Tween _catchFade;
        private static readonly int BurnProgressId = Shader.PropertyToID("_BurnProgress");
        private static readonly int SpriteBoundsId = Shader.PropertyToID("_SpriteBounds");

        private SpriteRenderer _sprite;
        private PlayerController _player;

        private DialogBox _dialog;
        private bool _collecting;
        private bool _collected;

        private void Start()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _sprite.DOFade(0, 0);
            if (burnMaterial != null)
            {
                _sprite.sharedMaterial = burnMaterial;
                _burnProperties = new MaterialPropertyBlock();
                UpdateBurnProgress(0f);
            }
            
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
            
            await DiaryController.Instance.EnterDiary(UpdateBurnProgress, rewardType);
            if (this == null) return;
            if (_catchFade != null && _catchFade.IsActive())
                await _catchFade.AsyncWaitForCompletion();
            if (this == null) return;
            SenseManager.Instance?.GrantReward(rewardType);
            _collected = true;
            _dialog.TriggerMessage(transform);
            HideView();
        }

        private void UpdateBurnProgress(float progress)
        {
            if (_burnProperties == null || _sprite == null) return;
            // Pausing drawing must not grow the burned interior back.
            // The hand grabs a mostly intact rabbit, never an empty silhouette.
            var burn = 0.32f * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
            _burnProgress = Mathf.Max(_burnProgress, burn);
            if (_collecting && absorptionMaterial != null && _burnProgress - _emittedBurnProgress >= 0.015f)
            {
                RabbitAbsorption.Play(_sprite, Camera.main, absorptionMaterial,
                    _emittedBurnProgress, _burnProgress).Forget();
                _emittedBurnProgress = _burnProgress;
            }
            _sprite.GetPropertyBlock(_burnProperties);
            _burnProperties.SetFloat(BurnProgressId, _burnProgress);
            var bounds = _sprite.sprite.bounds;
            _burnProperties.SetVector(SpriteBoundsId, new Vector4(bounds.center.x,
                bounds.center.y, bounds.size.x, bounds.size.y));
            _sprite.SetPropertyBlock(_burnProperties);
            if (progress >= 1f && !_catchFading)
            {
                _catchFading = true;
                _sprite.DOKill();
                _catchFade = _sprite.DOFade(0, 0.2f).SetDelay(0.75f).SetEase(Ease.OutQuad).SetLink(gameObject);
            }
        }

        private async void HideView()
        {
            _player.canMove = true;
            SoundManager.Instance.MusicLoop.KillNoise();
            ParticleSystem_FogAvoidPlayer.Instances.ForEach(p => p.ReturnView());
            if (!_catchFading)
                await _sprite.DOFade(0, 0.2f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
            if (this != null) _player.targetView = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collecting || _collected || !other.CompareTag("Player")) return;
            _collecting = true;
            _player = other.GetComponent<PlayerController>();
            ShowView();
        }
    }
}
