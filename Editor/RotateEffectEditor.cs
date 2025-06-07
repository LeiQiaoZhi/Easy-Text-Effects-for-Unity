using EasyTextEffects.Effects;
using UnityEditor;

namespace EasyTextEffects.Editor
{
    [CustomEditor(typeof(Effect_Rotate))]
    public class RotateEffectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var myScript = (Effect_Rotate)target;
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) myScript.HandleValueChanged();
        }
    }
}