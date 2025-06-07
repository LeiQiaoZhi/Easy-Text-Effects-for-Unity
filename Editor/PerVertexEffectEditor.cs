#if UNITY_EDITOR
using EasyTextEffects.Effects;
using UnityEditor;

namespace EasyTextEffects.Editor
{
    [CustomEditor(typeof(Effect_PerVertex))]
    public class PerVertexEffectEditor : UnityEditor.Editor
    {
        private bool foldout = true;

        public override void OnInspectorGUI()
        {
            var myScript = (Effect_PerVertex)target;

            EditorDocumentation.EditorDocumentation.BeginFoldBox("Per Vertex Effect", ref foldout);

            if (foldout)
            {
                EditorGUILayout.BeginHorizontal();
                EditorDocumentation.EditorDocumentation.FileLink(
                    "Packages/com.qiaozhilei.easy-text-effects/Documentation/Documentation.md");
                EditorDocumentation.EditorDocumentation.UrlLink(
                    "https://github.com/LeiQiaoZhi/Easy-Text-Effect-for-Unity/blob/main/Documentation/Documentation.md#per-vertex");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField(
                    @"Per-vertex effects allow you to assign a different effect to each vertex of the text. This allows for more complex animations.

A simple example: you could let the top vertices move up and down while keeping the bottom vertices static.",
                    EditorStyles.wordWrappedLabel
                );
            }

            EditorDocumentation.EditorDocumentation.EndFoldBox();

            EditorGUI.BeginChangeCheck();
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectTag"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topLeftEffects"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topRightEffects"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bottomLeftEffects"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bottomRightEffects"), true);
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck()) myScript.HandleValueChanged();
        }
    }
}

#endif
