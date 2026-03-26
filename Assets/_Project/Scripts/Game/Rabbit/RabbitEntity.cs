using _Code.Scripts.VFX;
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

        private void Start()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();

            _sprite.DOFade(0, 0);
        }

        private void ShowView(PlayerController player)
        {
            player.canMove = false;
            ParticleSystem_FogAvoidPlayer.Instances.ForEach(p => p.NewView(transform));
            
            _sprite.DOFade(1, 0.4f).SetEase(Ease.OutQuad);
            player.targetView = true;
        }

        private void HideView(PlayerController player)
        {
            player.canMove = true;
            ParticleSystem_FogAvoidPlayer.Instances.ForEach(p => p.ReturnView());
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) ShowView(other.GetComponent<PlayerController>());
        }
    }
}