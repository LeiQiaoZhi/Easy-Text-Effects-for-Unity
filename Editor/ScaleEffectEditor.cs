using EasyTextEffects.Effects;
using UnityEditor;

namespace EasyTextEffects.Editor
{
    [CustomEditor(typeof(Effect_Scale))]
    public class ScaleEffectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var myScript = (Effect_Scale)target;
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck()) myScript.HandleValueChanged();
        }
    }
}