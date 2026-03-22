using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Code.Scripts.VFX
{
    public class ParticleSystem_FogAvoidPlayer : MonoBehaviour
    {
        public static List<ParticleSystem_FogAvoidPlayer> Instances = new List<ParticleSystem_FogAvoidPlayer>();
        
        public Transform player;
        public float minDistance = 2f;
        public float maxDistance = 10f;
        public float updateInterval = 0.1f; // Обновлять позицию не каждый кадр для оптимизации
    
        private ParticleSystemRenderer particleRenderer;
        private MaterialPropertyBlock propBlock;
        private float nextUpdateTime;
    
        private readonly int playerPosId = Shader.PropertyToID("_PlayerPosition");
        private readonly int minDistId = Shader.PropertyToID("_MinDistance");
        private readonly int maxDistId = Shader.PropertyToID("_MaxDistance");
        
        Transform _player;
        float _minDistance;
        float _maxDistance;
        private bool updatedView;

        public void Init(Transform playerTransform)
        {
            player = playerTransform;
            _player = player;
            _minDistance = minDistance;
            _maxDistance = maxDistance;
            Instances.Add(this);
        }

        public async void NewView(Transform viewer, float min = 1f, float max = 20f)
        {
            player = viewer;
            updatedView = true;
            await LerpMinDistance();
            await LerpMaxDistance();
            
            async UniTask LerpMinDistance()
            {
                while (Mathf.Abs(minDistance - min) > 0.01f)
                {
                    minDistance = Mathf.MoveTowards(minDistance, min, Time.deltaTime * 70f);
                    await UniTask.Yield();
                }
                // minDistance = min;
            }
            async UniTask LerpMaxDistance()
            {
                while(Mathf.Abs(maxDistance - max) > 0.01f)
                {
                    maxDistance = Mathf.MoveTowards(maxDistance, max, Time.deltaTime * 40f);
                    await UniTask.Yield();
                }
                // maxDistance = max;
            }
        }

        public async void ReturnView()
        {
            if (!updatedView) return;
            player = _player;
            updatedView = false;
            await LerpMinDistance();
            await LerpMaxDistance();
            
            async UniTask LerpMinDistance()
            {
                while (Mathf.Abs(minDistance - _minDistance) > 0.01f)
                {
                    minDistance = Mathf.MoveTowards(minDistance, _minDistance, Time.deltaTime * 40f);
                    await UniTask.Yield();
                }
                // minDistance = min;
            }
            async UniTask LerpMaxDistance()
            {
                while(Mathf.Abs(maxDistance - _maxDistance) > 0.01f)
                {
                    maxDistance = Mathf.MoveTowards(maxDistance, _maxDistance, Time.deltaTime * 70f);
                    await UniTask.Yield();
                }
                // maxDistance = max;
            }
            
        }
        
        void Start()
        {
            particleRenderer = GetComponent<ParticleSystemRenderer>();
            propBlock = new MaterialPropertyBlock();
            UpdateShaderParameters();
            Init(player);
        }
    
        void Update()
        {
            if (Time.time >= nextUpdateTime)
            {
                UpdateShaderParameters();
                nextUpdateTime = Time.time + updateInterval;
            }
        }
    
        void UpdateShaderParameters()
        {
            if (player == null || particleRenderer == null)
                return;
            
            particleRenderer.GetPropertyBlock(propBlock);
            propBlock.SetVector(playerPosId, player.position);
            propBlock.SetFloat(minDistId, minDistance);
            propBlock.SetFloat(maxDistId, maxDistance);
            particleRenderer.SetPropertyBlock(propBlock);
        }
    }
}