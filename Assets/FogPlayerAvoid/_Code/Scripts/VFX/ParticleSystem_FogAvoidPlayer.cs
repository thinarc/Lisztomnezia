using UnityEngine;

namespace _Code.Scripts.VFX
{
    public class ParticleSystem_FogAvoidPlayer : MonoBehaviour
    {
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

        public void Init(Transform playerTransform)
        {
            player = playerTransform;
        }
        
        void Start()
        {
            particleRenderer = GetComponent<ParticleSystemRenderer>();
            propBlock = new MaterialPropertyBlock();
            UpdateShaderParameters();
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