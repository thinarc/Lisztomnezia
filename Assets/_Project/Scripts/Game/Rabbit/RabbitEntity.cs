using _Project.Scripts.Sense;
using EvolveGames;
using UnityEngine;

namespace _Project.Scripts.Rabbit
{
    public class RabbitEntity : MonoBehaviour
    {
        public SenseType rewardType;
        public float rewardValue = 0.3f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerController>().canMove = false;
                print(transform.name);
            }
        }
    }
}