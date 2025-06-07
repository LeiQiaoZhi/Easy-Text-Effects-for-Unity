using EasyTextEffects.Editor.EditorDocumentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static EasyTextEffects.Editor.EditorDocumentation.FoldBoxAttribute;

namespace EasyTextEffects
{
    public abstract class TextEffect_Base : ScriptableObject
    {
        [FormerlySerializedAs("effectName")]
        [FoldBox("Effect Tag Explained",
            new[]
            {
                "Used for 'Tag Effects' -- effects only applied to the text based on rich text tags.\n\nThe format is <link=effectTag>text<link>\n\n To add multiple effects, the format is <link=effectTag1+effectTag2>text</link>. Don't include \"+\" in effect tags for this reason.",
            }, new[] { ContentType.Text })]
        public string effectTag;

        [HideInInspector] public int startCharIndex;
        [HideInInspector] public int charLength;

        public event System.Action OnValueChanged;
        public void HandleValueChanged() => OnValueChanged?.Invoke();

        private void OnEnable()
        {
            #if UNITY_EDITOR
            Undo.undoRedoPerformed += HandleValueChanged;
            #endif
        }

        private void OnDisable()
        {
            #if UNITY_EDITOR
            Undo.undoRedoPerformed -= HandleValueChanged;
            #endif
        }

        public abstract void ApplyEffect(TMP_TextInfo _textInfo, int _charIndex, int _startVertex = 0,
            int _endVertex = 3);

        protected Vector3 CharCenter(TMP_CharacterInfo _charInfo, Vector3[] _verts)
        {
            var charBegin = _charInfo.vertexIndex;
            var charEnd = charBegin + 2;
            Vector3 center = (_verts[charBegin] + _verts[charEnd]) / 2;
            return center;
        }
    }
}