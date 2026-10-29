using System;
using System.Collections.Generic;
using _Project.Scripts.Effects;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Project.Scripts.Sense
{
    public class SenseManager : MonoBehaviour
    {
        public static SenseManager Instance { get; private set; }
        public bool HasVision => HasReward(RabbitType.Vision);
        public bool HasHearing => HasReward(RabbitType.Hearing);
        public bool HasSmell => HasReward(RabbitType.Smell);
        public IReadOnlyCollection<RabbitType> UnlockedAbilities => _rewards;
        public event Action<RabbitType> RewardGranted;
        private readonly HashSet<RabbitType> _rewards = new HashSet<RabbitType>();

        private Volume _visionVolume;
        private VolumeProfile _profile;
        private Tween _recovery;
        private float _atmosphere;

        private void Awake()
        {
            Instance = this;
            // Weight zero preserves the existing scene look until vision is earned.
            var foggedVision = new GameObject("Restored vision");
            foggedVision.transform.SetParent(transform, false);
            _visionVolume = foggedVision.AddComponent<Volume>();
            _visionVolume.isGlobal = true;
            _visionVolume.priority = 100f;
            _visionVolume.weight = 1f;
            _profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _visionVolume.sharedProfile = _profile;
            var color = _profile.Add<ColorAdjustments>();
            color.postExposure.Override(0f);
            color.saturation.Override(0f);
            color.contrast.Override(0f);
            color.colorFilter.Override(Color.white);
            color.hueShift.Override(0f);
            var balance = _profile.Add<WhiteBalance>();
            balance.temperature.Override(0f);
            balance.tint.Override(0f);
            var focus = _profile.Add<DepthOfField>();
            focus.mode.Override(DepthOfFieldMode.Off);
            var vignette = _profile.Add<Vignette>();
            vignette.intensity.Override(0.2f);
            vignette.smoothness.Override(0.8f);
            ApplyAtmosphere(0f);
            var camera = Camera.main;
            if (camera != null && camera.TryGetComponent<UniversalAdditionalCameraData>(out var data))
            {
                data.renderPostProcessing = true;
                data.volumeLayerMask |= 1 << foggedVision.layer;
            }
        }

        public bool HasReward(RabbitType type) => _rewards.Contains(type);

        public bool GrantReward(RabbitType type)
        {
            if (!Enum.IsDefined(typeof(RabbitType), type) || !_rewards.Add(type)) return false;
            _recovery?.Kill();
            var from = _atmosphere;
            var target = (HasVision ? 0.5f : 0f) + (HasHearing ? 0.25f : 0f) + (HasSmell ? 0.25f : 0f);
            _recovery = DOTween.To(() => from, value => { from=value; ApplyAtmosphere(value); }, target, 1.5f)
                .SetEase(Ease.InOutSine).SetLink(gameObject);
            RewardGranted?.Invoke(type);
            return true;
        }

        private void ApplyAtmosphere(float stage)
        {
            _atmosphere = stage;
            _profile.TryGet<ColorAdjustments>(out var color);
            color.postExposure.Override(Mathf.Lerp(-1.25f, -0.15f, stage));
            color.saturation.Override(Mathf.Lerp(-80f, -10f, stage));
            color.contrast.Override(Mathf.Lerp(24f, 8f, stage));
            color.colorFilter.Override(Color.Lerp(new Color(0.65f,0.73f,0.69f), new Color(0.96f,1f,0.94f), stage));
            _profile.TryGet<WhiteBalance>(out var balance);
            balance.temperature.Override(Mathf.Lerp(-12f, 1f, stage));
            _profile.TryGet<Vignette>(out var vignette);
            vignette.intensity.Override(Mathf.Lerp(0.48f,0.22f,stage));
            OutlineMover.SetWorldRecovery(stage);
        }

        [ContextMenu("Debug/Reset rabbit abilities")]
        public void ResetRabbitAbilities()
        {
            _recovery?.Kill();
            _rewards.Clear();
            if (_profile != null) ApplyAtmosphere(0f);
        }

        private void OnDestroy()
        {
            _recovery?.Kill();
            if (Instance == this) Instance = null;
            if (_profile == null) return;
            foreach (var component in _profile.components) Destroy(component);
            Destroy(_profile);
        }
    }
}
