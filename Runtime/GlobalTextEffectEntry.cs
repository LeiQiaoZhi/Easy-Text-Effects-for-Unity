using EasyTextEffects.Effects;

using UnityEngine;
using UnityEngine.Events;
using TMPro;

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
        
        
        public TextEffectEntry GetCopy(TMP_LinkInfo style)
        {
            var entryCopy = new TextEffectEntry();
            entryCopy.effect = effect.Instantiate();
            entryCopy.effect.startCharIndex = style.linkTextfirstCharacterIndex;
            entryCopy.effect.charLength = style.linkTextLength;
            entryCopy.triggerWhen = triggerWhen;
            entryCopy.onEffectCompleted = onEffectCompleted;
            
            return entryCopy;
        }

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