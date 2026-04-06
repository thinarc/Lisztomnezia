using UnityEngine;

namespace _Project.Scripts.Diary
{
    public class DiaryZone : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Diary")) DiaryController.Instance.FillDraw();
        }
    }
}