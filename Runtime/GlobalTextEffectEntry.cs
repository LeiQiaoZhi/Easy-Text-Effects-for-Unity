using EasyTextEffects.Effects;

using UnityEngine;
using UnityEngine.Events;

namespace EasyTextEffects
{
    [System.Serializable]
    public class TextEffectEntry
    {
        public enum TriggerWhen
        {
            OnStart,
            Manual,
        }

        public TriggerWhen triggerWhen;
        public TextEffectInstance effect;

        public UnityEvent onEffectCompleted = new UnityEvent();

        public void StartEffect()
        {
            effect.StartEffect(this);
        }

        internal void InvokeCompleted()
        {
            onEffectCompleted?.Invoke();
		}

    }

    [System.Serializable]
    public class GlobalTextEffectEntry : TextEffectEntry
    {
        public bool overrideTagEffects;
    }
}