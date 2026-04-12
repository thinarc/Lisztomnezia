using UnityEngine;

namespace _Project.Scripts.Diary
{
    public class DiaryZone : MonoBehaviour
    {
        public bool secondTrigger;
        
        private void OnTriggerStay(Collider other)
        {
            if (secondTrigger) return;
            if (other.CompareTag("Diary")) DiaryController.Instance.FillDraw();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Diary")) DiaryController.Instance.EnterDraw();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Diary")) return;
            
            if (!secondTrigger) DiaryController.Instance.DropDraw();
            else DiaryController.Instance.DropDrawZone();
        }
    }
}