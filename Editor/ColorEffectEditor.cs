using EasyTextEffects.Effects;
using UnityEditor;

namespace EasyTextEffects.Editor
{
    [CustomEditor(typeof(Effect_Color))]
    public class ColorEffectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var myScript = (Effect_Color)target;
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck()) myScript.HandleValueChanged();
        }
    }
}