using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        public bool person;

        private bool _type;

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

        public async void TriggerMessage(Transform pos = null, bool downY = false)
        {
            var message = _message.Dequeue() ?? "no message";

            _type = true;
            if (!world)
            {
                if (person) DialogController.Instances[0].personCall = true;
                await DialogController.Instances[0].ShowMessage(message);
            }
            else await DialogController.Instances[1].ShowMessage(message, pos, downY);
            _type = false;
        }

        public async UniTask TypeWait() => await UniTask.WaitUntil(() => !_type);

        public void UpdateLocale(string newMessage)
        {
            messageField = newMessage;
            
            _message.Clear();
            var messages = messageField.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
            messages.ForEach(m => _message.Enqueue(m));
        }
    }
}