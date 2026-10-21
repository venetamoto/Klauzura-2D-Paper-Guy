using System;
using UnityEngine;
using UnityEngine.Events;

namespace PaperGuy.Animations
{
    /// <summary>
    /// Dává se na objekt s Animatorem. V klipech přidejte Animation Event s funkcí
    /// <c>Fire</c> a textovým parametrem (např. "Footstep", "EatBite", "Land").
    /// Kód se přihlásí k <see cref="EventFired"/>, designér k UnityEvent v inspektoru.
    /// </summary>
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
