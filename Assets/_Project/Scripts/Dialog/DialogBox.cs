using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.Dialog
{
    public class DialogBox : MonoBehaviour
    {
        [SerializeField, TextArea(3, 6)] private string messageField;
        private Queue<string> _message = new ();
        
        [SerializeField, Space(5)] private bool onTrigger;
        public bool world = true;

        private void Start()
        {
            _message.Clear();
            var messages = messageField.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
            messages.ForEach(m => _message.Enqueue(m));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && onTrigger) TriggerMessage();
        }

        public void TriggerMessage(Transform pos = null, bool downY = false)
        {
            var message = _message.Dequeue() ?? "no message";

            if (!world) DialogController.Instances[0].ShowMessage(message);
            else DialogController.Instances[1].ShowMessage(message, pos, downY);
        }

        public void UpdateLocale(string newMessage)
        {
            messageField = newMessage;
            
            _message.Clear();
            var messages = messageField.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
            messages.ForEach(m => _message.Enqueue(m));
        }
    }
}