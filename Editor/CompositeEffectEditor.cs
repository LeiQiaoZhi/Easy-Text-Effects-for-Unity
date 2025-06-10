#if UNITY_EDITOR
using EasyTextEffects.Effects;
using UnityEditor;

namespace EasyTextEffects.Editor
{
    [CustomEditor(typeof(Effect_Composite))]
    public class CompositeEffectEditor : UnityEditor.Editor
    {
        private bool foldout = true;

        public override void OnInspectorGUI()
        {
            var myScript = (Effect_Composite)target;

            EditorDocumentation.EditorDocumentation.BeginFoldBox("Composite Effect", ref foldout);

            if (foldout)
            {
                EditorGUILayout.BeginHorizontal();
                EditorDocumentation.EditorDocumentation.FileLink(
                    "Packages/com.qiaozhilei.easy-text-effects/Documentation/Documentation.md");
                EditorDocumentation.EditorDocumentation.UrlLink("https://github.com/LeiQiaoZhi/Easy-Text-Effect-for-Unity/blob/main/Documentation/Documentation.md#composite");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField(
                    @"This is useful for combining multiple effects into one.

This is for organizational purposes only, and does not have any properties of its own. It is the same as adding multiple effects to the same list.

This can be useful if there is a common set of effects that you want to apply to multiple texts. For example, you can create a composite entry animation that contains a fade in and a move up effect, and apply it to multiple texts.",
                    EditorStyles.wordWrappedLabel
                );
            }

            EditorDocumentation.EditorDocumentation.EndFoldBox();

            EditorGUI.BeginChangeCheck();
            
            // draw the list of effects 
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectTag"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"), true);
            serializedObject.ApplyModifiedProperties();
            
            if (EditorGUI.EndChangeCheck()) myScript.HandleValueChanged();
        }
    }
}
#endif