using System.Collections.Generic;
using System.Linq;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using TMPro;
using UnityEngine;

namespace EasyTextEffects.Effects
{
    [CreateAssetMenu(fileName = "PerVertex", menuName = "Easy Text Effects/5. Per Vertex", order = 5)]
    public class Effect_PerVertex : TextEffectInstance
    {
        private static readonly List<TextEffectInstance> EmptyEffectInstanceList = new();
        private readonly HashSet<TextEffectInstance> monitoredEffects = new();
        
        [Space(10)] public List<TextEffectInstance> topLeftEffects = new List<TextEffectInstance>();
        [Space(10)] public List<TextEffectInstance> topRightEffects = new List<TextEffectInstance>();
        [Space(10)] public List<TextEffectInstance> bottomLeftEffects = new List<TextEffectInstance>();
        [Space(10)] public List<TextEffectInstance> bottomRightEffects = new List<TextEffectInstance>();

        private void OnEnable()
        {
            ListenForEffectChanges();
        }
        
        private void OnValidate()
        {
            if (topLeftEffects.Contains(this))
            {
                Debug.LogError("Per Vertex effect can't contain itself");
                topLeftEffects.Remove(this);
            }

            if (topRightEffects.Contains(this))
            {
                Debug.LogError("Per Vertex effect can't contain itself");
                topRightEffects.Remove(this);
            }

            if (bottomLeftEffects.Contains(this))
            {
                Debug.LogError("Per Vertex effect can't contain itself");
                bottomLeftEffects.Remove(this);
            }

            if (bottomRightEffects.Contains(this))
            {
                Debug.LogError("Per Vertex effect can't contain itself");
                bottomRightEffects.Remove(this);
            }
            
            ListenForEffectChanges();
        }

        private void OnDisable()
        {
            StopListeningForEffectChanges();
        }

        public override void ApplyEffect(TMP_TextInfo _textInfo, int _charIndex, int _startVertex = 0,
            int _endVertex = 3)
        {
            if (!CheckCanApplyEffect(_charIndex)) return;

            for (int i = 0; i < topLeftEffects.Count; i++) topLeftEffects[i]?.ApplyEffect(_textInfo, _charIndex, 1, 1);
            for (int i = 0; i < topRightEffects.Count; i++) topRightEffects[i]?.ApplyEffect(_textInfo, _charIndex, 2, 2);
            for (int i = 0; i < bottomLeftEffects.Count; i++) bottomLeftEffects[i]?.ApplyEffect(_textInfo, _charIndex, 0, 0);
            for (int i = 0; i < bottomRightEffects.Count; i++) bottomRightEffects[i]?.ApplyEffect(_textInfo, _charIndex, 3, 3);
        }

        public override void StartEffect(TextEffectEntry entry)
        {
            base.StartEffect(entry);
            var allEffects = new List<List<TextEffectInstance>>
            {
                topLeftEffects,
                topRightEffects,
                bottomLeftEffects,
                bottomRightEffects
            };
            
            foreach (var effects in allEffects)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    if (effects[i] == null) continue;
                    effects[i].startCharIndex = startCharIndex;
                    effects[i].charLength = charLength;
                    effects[i].StartEffect(entry);
                }
            }
        }

        public override bool IsComplete
        {
            get
            {
                if (HasCompletedEffect(topLeftEffects)) return true;
                if (HasCompletedEffect(topRightEffects)) return true;
                if (HasCompletedEffect(bottomLeftEffects)) return true;
                if (HasCompletedEffect(bottomRightEffects)) return true;
                return false;
            }
        }
        // Helper for IsComplete
        private bool HasCompletedEffect(List<TextEffectInstance> effects)
        {
            if (effects == null) return false;
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null && effects[i].IsComplete)
                    return true;
            }
            return false;
        }

        public override void StopEffect()
        {
            base.StopEffect();

            for (int i = 0; i < topLeftEffects.Count; i++) topLeftEffects[i]?.StopEffect();
            for (int i = 0; i < topRightEffects.Count; i++) topRightEffects[i]?.StopEffect();
            for (int i = 0; i < bottomLeftEffects.Count; i++) bottomLeftEffects[i]?.StopEffect();
            for (int i = 0; i < bottomRightEffects.Count; i++) bottomRightEffects[i]?.StopEffect();
        }

        public override TextEffectInstance Instantiate()
        {
            Effect_PerVertex instance = Instantiate(this);
            instance.topLeftEffects = topLeftEffects.Select(_effect => _effect?.Instantiate()).ToList();
            instance.topRightEffects = topRightEffects.Select(_effect => _effect?.Instantiate()).ToList();
            instance.bottomLeftEffects = bottomLeftEffects.Select(_effect => _effect?.Instantiate()).ToList();
            instance.bottomRightEffects = bottomRightEffects.Select(_effect => _effect?.Instantiate()).ToList();
            return instance;
        }
        
        private void ListenForEffectChanges()
        {
            HashSet<TextEffectInstance> effectsSet = new HashSet<TextEffectInstance>();
            AddEffectsToSet(topLeftEffects, effectsSet);
            AddEffectsToSet(topRightEffects, effectsSet);
            AddEffectsToSet(bottomLeftEffects, effectsSet);
            AddEffectsToSet(bottomRightEffects, effectsSet);

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
        // Helper for ListenForEffectChanges
        private void AddEffectsToSet(List<TextEffectInstance> list, HashSet<TextEffectInstance> set)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                    set.Add(list[i]);
            }
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