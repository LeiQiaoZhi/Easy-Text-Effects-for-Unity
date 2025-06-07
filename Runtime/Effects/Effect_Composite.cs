using System.Collections.Generic;
using System.Linq;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using TMPro;
using UnityEngine;

namespace EasyTextEffects.Effects
{
    [CreateAssetMenu(fileName = "Composite", menuName = "Easy Text Effects/6. Composite",
                     order = 6)]
    public class Effect_Composite : TextEffectInstance
    {
        private HashSet<TextEffectInstance> monitoredEffects = new();
        
        [Space(10)] public List<TextEffectInstance> effects = new List<TextEffectInstance>();

        private void OnEnable()
        {
            ListenForEffectChanges();
        }
        
        private void OnValidate()
        {
            if (effects.Contains(this))
            {
                Debug.LogError("Composite effect can't contain itself");
                effects.Remove(this);
            }

            ListenForEffectChanges();
        }

        private void OnDisable()
        {
            ListenForEffectChanges();
        }

        public override void ApplyEffect(TMP_TextInfo _textInfo, int _charIndex,
                                         int _startVertex = 0,
                                         int _endVertex = 3)
        {
            if (!CheckCanApplyEffect(_charIndex)) return;

            foreach (TextEffectInstance effect in effects)
            {
                if (!effect) continue;
                effect.ApplyEffect(_textInfo, _charIndex, _startVertex, _endVertex);
            }
        }

        public override void StartEffect(TextEffectEntry entry)
        {
            base.StartEffect(entry);

            foreach (TextEffectInstance effect in effects)
            {
                if (!effect) continue;
                effect.startCharIndex = startCharIndex;
                effect.charLength = charLength;
                // side effect: any child effect that finishes will invoke OnEffectComplete
                effect.StartEffect(entry);
            }
        }

        public override void StopEffect()
        {
            base.StopEffect();

            foreach (TextEffectInstance effect in effects)
            {
                if (!effect) continue;
                effect.StopEffect();
            }
        }

        public override bool IsComplete =>
            effects.Any(_effect => _effect != null && _effect.IsComplete);

        public override TextEffectInstance Instantiate()
        {
            Effect_Composite instance = Instantiate(this);
            instance.effects = new List<TextEffectInstance>();
            foreach (TextEffectInstance effect in effects)
            {
                if (!effect) continue;
                instance.effects.Add(effect.Instantiate());
            }

            return instance;
        }
        
        private void ListenForEffectChanges()
        {
            if (effects.IsNullOrEmpty())
            {
                StopListeningForEffectChanges();
                return;
            }

            var effectsSet = effects.Where(effect => effect).ToHashSet();
    
            foreach (var effect in effectsSet.Where(effect => monitoredEffects.Add(effect)))
                effect.OnValueChanged += HandleValueChanged;

            monitoredEffects.RemoveWhere(effect =>
            {
                if (effectsSet.Contains(effect)) return false;
                effect.OnValueChanged -= HandleValueChanged;
                return true;
            });
        }

        private void StopListeningForEffectChanges()
        {
            monitoredEffects.ForEach(x => x.OnValueChanged -= HandleValueChanged);
            monitoredEffects.Clear();
        }
    }
}