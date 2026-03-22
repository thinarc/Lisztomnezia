using UnityEngine;

namespace _Project.Scripts.Dialog
{
    public class DialogBox : MonoBehaviour
    {
        [SerializeField] private string message;

        private void OnTriggerEnter(Collider other)
        {
            if (message == null) message = "no message";
            DialogController.Instance.ShowMessage(message);
        }

        public void UpdateLocale(string newMessage) => message = newMessage;
    }
}