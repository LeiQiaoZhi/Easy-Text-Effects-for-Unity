using System.Collections.Generic;
using System.Linq;
using EasyTextEffects.Editor.MyBoxCopy.Attributes;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using EasyTextEffects.Effects;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;
using static EasyTextEffects.TextEffectEntry.TriggerWhen;

namespace EasyTextEffects
{
    [ExecuteAlways]
    public class TextEffect : MonoBehaviour
    {
        public TMP_Text text;
        [Space(5)] public bool usePreset;

        [ConditionalField(nameof(usePreset), false)]
        public TagEffectsPreset preset;

        public List<TextEffectEntry> tagEffects;

        [Space(5)] [FormerlySerializedAs("effectsList")]
        public List<GlobalTextEffectEntry> globalEffects;

        [Space(5)] [Range(1, 120)] public int updatesPerSecond = 30;
        
        private static readonly List<TextEffectEntry> EmptyEffectEntryList = new();
        private static readonly List<GlobalTextEffectEntry> EmptyGlobalEffectEntryList = new();
        private readonly HashSet<TextEffectInstance> monitoredEffects = new ();
        
        private List<TextEffectEntry> allTagEffects_;
        private List<TextEffectEntry> onStartTagEffects_;
        private List<TextEffectEntry> manualTagEffects_;
        private List<GlobalTextEffectEntry> onStartEffects_;
        private List<GlobalTextEffectEntry> manualEffects_;
        private List<TextEffectInstance> entryEffectsCopied_;

        public void UpdateStyleInfos()
        {
            if (text == null || text.textInfo == null)
                return;
            TMP_TextInfo textInfo = text.textInfo;

            var styles = textInfo.linkInfo;
            var linkCount = textInfo.linkCount;

            CopyGlobalEffects(textInfo);
            AddTagEffects(styles, linkCount);

            StartOnStartEffects();
        }

        private void CopyGlobalEffects(TMP_TextInfo textInfo)
        {
            onStartEffects_ = new List<GlobalTextEffectEntry>();
            manualEffects_ = new List<GlobalTextEffectEntry>();

            if (globalEffects == null)
            {
                return;
            }

            globalEffects.ForEach(_entry =>
            {
                if (_entry.effect == null)
                    return;
                var effectEntry = new GlobalTextEffectEntry();
                effectEntry.effect = _entry.effect.Instantiate();
                effectEntry.effect.startCharIndex = 0;
                effectEntry.effect.charLength = textInfo.characterCount;
                effectEntry.overrideTagEffects = _entry.overrideTagEffects;
                effectEntry.onEffectCompleted = _entry.onEffectCompleted;
                if (_entry.triggerWhen == OnStart)
                    onStartEffects_.Add(effectEntry);
                else
                    manualEffects_.Add(effectEntry);
            });
        }

        private void AddTagEffects(TMP_LinkInfo[] styles, int linkCount)
        {
            onStartTagEffects_ = new List<TextEffectEntry>();
            manualTagEffects_ = new List<TextEffectEntry>();

            if (tagEffects == null)
            {
                return;
            }

            allTagEffects_ = new List<TextEffectEntry>(tagEffects);
            if (usePreset && preset != null)
                allTagEffects_.AddRange(preset.tagEffects);


            for (var i = 0; i < linkCount; i++)
            {
                TMP_LinkInfo style = styles[i];
                if (style.GetLinkID() == string.Empty)
                    continue;

                // copy effects
                var effectTemplates = GetTagEffectsByName(style.GetLinkID());
                foreach (TextEffectEntry entry in effectTemplates)
                {
                    if (entry.effect == null)
                        continue;
                    // note: make sure onEffectCompleted events get copied 
                    TextEffectEntry entryCopy = entry.GetCopy(
                        style.linkTextfirstCharacterIndex,
                        style.linkTextLength);
                    if (entry.triggerWhen == OnStart)
                        onStartTagEffects_.Add(entryCopy);
                    else
                        manualTagEffects_.Add(entryCopy);
                }
            }
        }

        private List<TextEffectEntry> GetTagEffectsByName(string _effectName)
        {
            var results = new List<TextEffectEntry>();
            var effectNames = _effectName.Split('+');

            foreach (var effectName in effectNames)
            {
                var findAll = allTagEffects_.FindAll(_entry => _entry.effect?.effectTag == effectName);
                if (findAll.Count >= 1)
                    results.Add(findAll[0]);
#if UNITY_EDITOR
                if (findAll.Count == 0)
                    Debug.LogWarning("Effect not found: " + effectName);

                if (findAll.Count > 1)
                {
                    Debug.LogWarning("Multiple effects found: " + effectName);
                    // Debug.Log("Effects: " + string.Join(", ", findAll.Select(_entry => _entry.effect.name).ToArray()));
                }
#endif
            }

            return results;
        }

        public void Refresh()
        {
            if (text == null)
                return;
            text.ForceMeshUpdate();
            UpdateStyleInfos();
        }

        private void Reset()
        {
            text = GetComponent<TMP_Text>();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            ListenForEffectChanges();
            Refresh();
#endif
        }

        private void ListenForEffectChanges()
        {
            var effects = (tagEffects ?? EmptyEffectEntryList)
                          .Concat(globalEffects ?? EmptyGlobalEffectEntryList)
                          .Where(entry => entry.effect)
                          .Select(entry => entry.effect)
                          .ToHashSet();

            foreach (var effect in effects.Where(effect => monitoredEffects.Add(effect)))
                effect.OnValueChanged += Refresh;
            
            monitoredEffects.RemoveWhere(effect =>
            {
                if (effects.Contains(effect)) return false;
                effect.OnValueChanged -= Refresh;
                return true;
            });
        }

        private void StopListeningForEffectChanges()
        {
            monitoredEffects.ForEach(x => x.OnValueChanged -= Refresh);
            monitoredEffects.Clear();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += Update;
#endif
            ListenForEffectChanges();
            Refresh();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= Update;
#endif
            StopListeningForEffectChanges();
        }

        private float nextUpdateTime_ = 0;

        public void Update()
        {
            if (!text)
                return;

            // updates should be independent of time scale
            var time = TimeUtil.GetTime(TimeUtil.TimeType.UnscaledTime); 
            if (time < nextUpdateTime_)
                return;
            nextUpdateTime_ = time + 1f / updatesPerSecond;

            text.ForceMeshUpdate();
            TMP_TextInfo textInfo = text.textInfo;

            for (var i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                var capturedI = i;
                onStartEffects_.Where(_entry => !_entry.overrideTagEffects)
                    .ForEach(_entry => _entry.effect.ApplyEffect(textInfo, capturedI, 0, 3));
                manualEffects_.Where(_entry => !_entry.overrideTagEffects).ForEach(_entry =>
                    _entry.effect.ApplyEffect(textInfo, capturedI, 0, 3));

                onStartTagEffects_.ForEach(_entry => _entry.effect.ApplyEffect(textInfo, capturedI, 0, 3));
                manualTagEffects_.ForEach(_entry => _entry.effect.ApplyEffect(textInfo, capturedI, 0, 3));

                onStartEffects_.Where(_entry => _entry.overrideTagEffects).ForEach(_entry =>
                    _entry.effect.ApplyEffect(textInfo, capturedI, 0, 3));
                manualEffects_.Where(_entry => _entry.overrideTagEffects).ForEach(_entry =>
                    _entry.effect.ApplyEffect(textInfo, capturedI, 0, 3));
            }

            // apply changes and update mesh
            for (var i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];

                meshInfo.mesh.colors32 = meshInfo.colors32;
                meshInfo.mesh.vertices = meshInfo.vertices;

                text.UpdateGeometry(meshInfo.mesh, i);
            }
        }

        #region API

        public void StopAllEffects()
        {
            onStartEffects_.ForEach(_entry => _entry.effect.StopEffect());
            manualEffects_.ForEach(_entry => _entry.effect.StopEffect());
            onStartTagEffects_.ForEach(_entry => _entry.effect.StopEffect());
            manualTagEffects_.ForEach(_entry => _entry.effect.StopEffect());
        }

        public void StartOnStartEffects()
        {
            onStartEffects_.ForEach(_entry => _entry.StartEffect());
            onStartTagEffects_.ForEach(_entry => _entry.StartEffect());
            nextUpdateTime_ = 0; // immediately update
        }

        public void StopOnStartEffects()
        {
            onStartEffects_.ForEach(_entry => _entry.effect.StopEffect());
            onStartTagEffects_.ForEach(_entry => _entry.effect.StopEffect());
        }

        public void StartManualEffects()
        {
            manualEffects_.ForEach(_entry => _entry.StartEffect());
        }

        public void StopManualEffects()
        {
            manualEffects_.ForEach(_entry => _entry.effect.StopEffect());
        }

        public void StartManualTagEffects()
        {
            manualTagEffects_.ForEach(_entry => _entry.StartEffect());
        }

        public void StopManualTagEffects()
        {
            manualTagEffects_.ForEach(_entry => _entry.effect.StopEffect());
        }

        public GlobalTextEffectEntry StartManualEffect(string _effectName)
        {
            GlobalTextEffectEntry effectEntry = manualEffects_.Find(_entry => _entry.effect.effectTag == _effectName);
            if (effectEntry != null)
            {
                effectEntry.StartEffect();
                return effectEntry;
            }
            Debug.LogWarning($"Effect {_effectName} not found. Available effects: {string.Join(", ", manualEffects_.Select(_entry => _entry.effect.effectTag).ToList())}");
            return null;
        }

        public TextEffectEntry StartManualTagEffect(string _effectName)
        {
            TextEffectEntry effectEntry = manualTagEffects_.Find(_entry => _entry.effect.effectTag == _effectName);
            if (effectEntry != null)
            {
                effectEntry.StartEffect();
                return effectEntry;
            }
            Debug.LogWarning($"Effect {_effectName} not found. Available effects: {string.Join(", ", manualEffects_.Select(_entry => _entry.effect.effectTag).ToList())}");
            return null;
        }

        public List<TextEffectStatus> QueryEffectStatuses(TextEffectType _effectType,
            TextEffectEntry.TriggerWhen _triggerWhen)
        {
            IReadOnlyList<TextEffectEntry> effectsList = _effectType == TextEffectType.Global
                ? _triggerWhen == OnStart
                    ? onStartEffects_
                    : manualEffects_
                : _triggerWhen == OnStart
                    ? onStartTagEffects_
                    : manualTagEffects_;
            if (effectsList == null)
                return new List<TextEffectStatus>();
            return effectsList.Select(_entry => new TextEffectStatus
            {
                Tag = _entry.effect.effectTag,
                Started = _entry.effect.started,
                IsComplete = _entry.effect.IsComplete
            }).ToList();
        }

        public List<TextEffectStatus> QueryEffectStatusesByTag(TextEffectType _effectType,
            TextEffectEntry.TriggerWhen _triggerWhen, string _tag)
        {
            IReadOnlyList<TextEffectEntry> effectsList = _effectType == TextEffectType.Global
                ? _triggerWhen == OnStart
                    ? onStartEffects_
                    : manualEffects_
                : _triggerWhen == OnStart
                    ? onStartTagEffects_
                    : manualTagEffects_;
            if (effectsList == null)
                return new List<TextEffectStatus>();
            return effectsList
                .Where(_entry => _entry.effect.effectTag == _tag)
                .Select(_entry => new TextEffectStatus
                {
                    Tag = _entry.effect.effectTag,
                    Started = _entry.effect.started,
                    IsComplete = _entry.effect.IsComplete
                }).ToList();
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (text == null || text.textInfo == null)
                return;
            Gizmos.color = Color.green;
            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = text.textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                Vector3 botLeft = text.transform.TransformPoint(charInfo.bottomLeft);
                Vector3 topRight = text.transform.TransformPoint(charInfo.topRight);

                var verts = text.textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                var first = verts[charInfo.vertexIndex + 0];
                var last = verts[charInfo.vertexIndex + 2];

                first = text.transform.TransformPoint(first);
                last = text.transform.TransformPoint(last);

                Gizmos.color = Color.red;
                // Gizmos.DrawWireSphere(first, 10);
                // Gizmos.DrawWireSphere(last,     10);

                // Gizmos.DrawWireCube((botLeft + topRight) / 2, topRight - botLeft);
            }
        }
#endif
    }
}