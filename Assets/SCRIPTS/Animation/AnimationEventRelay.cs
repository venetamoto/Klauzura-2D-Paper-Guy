using System;
using UnityEngine;
using UnityEngine.Events;

namespace PaperGuy.Animations
{
    public class AnimationEventRelay : MonoBehaviour
    {
        [Serializable]
        public struct NamedEvent
        {
            public string name;
            public UnityEvent response;
        }

        [SerializeField] private NamedEvent[] events = Array.Empty<NamedEvent>();

        public event Action<string> EventFired;

        // Volá se z Animation Eventu.
        public void Fire(string eventName)
        {
            EventFired?.Invoke(eventName);
            foreach (NamedEvent e in events)
                if (e.name == eventName) e.response?.Invoke();
        }
    }
}
