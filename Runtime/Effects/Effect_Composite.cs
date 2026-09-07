using System.Collections.Generic;
using System.Linq;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using TMPro;
using UnityEngine;

namespace EasyTextEffects.Effects
{
    [CreateAssetMenu(fileName = "Composite", menuName = "Easy Text Effects/6. Composite", order = 6)]
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

        public override void ApplyEffect(TMP_TextInfo _textInfo, int _charIndex, int _startVertex = 0, int _endVertex = 3)
        {
            if (!CheckCanApplyEffect(_charIndex)) return;
            
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null)
                    effects[i].ApplyEffect(_textInfo, _charIndex, _startVertex, _endVertex);
            }
        }

        public override void StartEffect(TextEffectEntry entry)
        {
            base.StartEffect(entry);

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null) continue;
                effects[i].startCharIndex = startCharIndex;
                effects[i].charLength = charLength;
                // side effect: any child effect that finishes will invoke OnEffectComplete
                effects[i].StartEffect(entry);
            }
        }

        public override void StopEffect()
        {
            base.StopEffect();

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null)
                    effects[i].StopEffect();
            }
        }

        public override bool IsComplete
        {
            get
            {
                if (effects == null) return false;
                for (int i = 0; i < effects.Count; i++)
                {
                    if (effects[i] != null && effects[i].IsComplete)
                        return true;
                }
                return false;
            }
        }

        public override TextEffectInstance Instantiate()
        {
            Effect_Composite instance = Instantiate(this);
            instance.effects = new List<TextEffectInstance>();
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null) continue;
                instance.effects.Add(effects[i].Instantiate());
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

            HashSet<TextEffectInstance> effectsSet = new HashSet<TextEffectInstance>();
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null)
                    effectsSet.Add(effects[i]);
            }

            foreach (var effect in effectsSet)
            {
                if (monitoredEffects.Add(effect))
                    effect.OnValueChanged += HandleValueChanged;
            }

            monitoredEffects.RemoveWhere(effect =>
            {
                if (effectsSet.Contains(effect)) return false;
                effect.OnValueChanged -= HandleValueChanged;
                return true;
            });
        }

        private void StopListeningForEffectChanges()
        {
            foreach (var effect in monitoredEffects)
            {
                effect.OnValueChanged -= HandleValueChanged;
            }
            monitoredEffects.Clear();
        }
    }
}