using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.osu_.Hit
{
    public class HitCompensateRot : MonoBehaviour
    {
        [SerializeField] private RectTransform[] compensate;

        public void Compensate()
        {
            compensate.ForEach(rect => rect.rotation = Quaternion.Euler(0, 0, 0));
        }
    }
}