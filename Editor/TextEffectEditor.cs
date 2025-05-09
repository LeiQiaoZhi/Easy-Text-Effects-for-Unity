#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EasyTextEffects.Editor.EditorDocumentation.EditorDocumentation;

namespace EasyTextEffects.Editor
{
    [CustomEditor(typeof(TextEffect))]
    public class TextEffectEditor : UnityEditor.Editor
    {
        private string manualEffectName_;
        private string manualTagEffectName_;

        private bool debugButtonsVisible_;

        private bool effectStatusesVisible_;
        private bool tagStartEffectStatusesVisible_ = true;
        private bool tagManualEffectStatusesVisible_ = true;
        private bool globalStartEffectStatusesVisible_ = true;
        private bool globalManualEffectStatusesVisible_ = true;

        private bool documentationVisible_;
        private bool createEffectVisible_;
        private bool applyEffectVisible_;
        private bool controlEffectVisible_;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            Repaint();

            var myScript = (TextEffect)target;

            GUILayout.Space(10);

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.fixedHeight = 30;
            buttonStyle.fontStyle = FontStyle.Bold;

            if (GUILayout.Button("REFRESH", buttonStyle))
            {
                if (myScript.text != null)
                {
                    myScript.text.ForceMeshUpdate();
                    myScript.UpdateStyleInfos();
                }
            }

            BeginFoldBox("Debug Buttons", ref debugButtonsVisible_, IconType.Tool);
            if (debugButtonsVisible_)
            {
                GUILayout.BeginHorizontal();
                manualTagEffectName_ = EditorGUILayout.TextField("Manual Tag Effect Name", manualTagEffectName_);
                if (GUILayout.Button("START"))
                {
                    myScript.StartManualTagEffect(manualTagEffectName_);
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                manualEffectName_ = EditorGUILayout.TextField("Manual Global Effect Name", manualEffectName_);
                if (GUILayout.Button("START"))
                {
                    myScript.StartManualEffect(manualEffectName_);
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("START Tag Manual Effects", buttonStyle))
                {
                    myScript.StartManualTagEffects();
                }

                if (GUILayout.Button("STOP", buttonStyle))
                {
                    myScript.StopManualTagEffects();
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("START Global Manual Effects", buttonStyle))
                {
                    myScript.StartManualEffects();
                }

                if (GUILayout.Button("STOP", buttonStyle))
                {
                    myScript.StopManualEffects();
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("STOP All Effects", buttonStyle))
                {
                    myScript.StopAllEffects();
                }

                if (GUILayout.Button("STOP OnStart Effects", buttonStyle))
                {
                    myScript.StopOnStartEffects();
                }

                GUILayout.EndHorizontal();
            }

            EndFoldBox();

            BeginFoldBox("Effect Statuses", ref effectStatusesVisible_, IconType.Tool);
            if (effectStatusesVisible_)
            {
                var foldoutHeaderStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold
                };
                EditorGUI.indentLevel++; // Increase indent level
                tagStartEffectStatusesVisible_ =
                    EditorGUILayout.Foldout(tagStartEffectStatusesVisible_, "Tag OnStart Effects", true,
                        foldoutHeaderStyle);
                if (tagStartEffectStatusesVisible_)
                {
                    var statuses =
                        myScript.QueryEffectStatuses(TextEffectType.Tag, TextEffectEntry.TriggerWhen.OnStart);
                    DrawStatuses(statuses);
                }
                EditorGUILayout.Space(2);

                tagManualEffectStatusesVisible_ =
                    EditorGUILayout.Foldout(tagManualEffectStatusesVisible_, "Tag Manual Effects", true,
                        foldoutHeaderStyle);
                if (tagManualEffectStatusesVisible_)
                {
                    var statuses = myScript.QueryEffectStatuses(TextEffectType.Tag, TextEffectEntry.TriggerWhen.Manual);
                    DrawStatuses(statuses);
                }
                EditorGUILayout.Space(2);

                globalStartEffectStatusesVisible_ =
                    EditorGUILayout.Foldout(globalStartEffectStatusesVisible_, "Global OnStart Effects", true,
                        foldoutHeaderStyle);
                if (globalStartEffectStatusesVisible_)
                {
                    var statuses =
                        myScript.QueryEffectStatuses(TextEffectType.Global, TextEffectEntry.TriggerWhen.OnStart);
                    DrawStatuses(statuses);
                }
                EditorGUILayout.Space(2);

                globalManualEffectStatusesVisible_ =
                    EditorGUILayout.Foldout(globalManualEffectStatusesVisible_, "Global Manual Effects", true,
                        foldoutHeaderStyle);
                if (globalManualEffectStatusesVisible_)
                {
                    var statuses =
                        myScript.QueryEffectStatuses(TextEffectType.Global, TextEffectEntry.TriggerWhen.Manual);
                    DrawStatuses(statuses);
                }

                EditorGUI.indentLevel--;
            }

            EndFoldBox();

            BeginFoldBox("Creating Effects", ref createEffectVisible_);
            if (createEffectVisible_)
            {
                EditorGUILayout.BeginHorizontal();
                FileLink(
                    "Packages/com.qiaozhilei.easy-text-effects/Documentation/Documentation.md");
                UrlLink(
                    "https://github.com/LeiQiaoZhi/Easy-Text-Effect-for-Unity/blob/main/Documentation/Documentation.md#creating-effects");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(
                    @"Effects are ScriptableObjects that can be created in the project view. Right-click and select `Create/Easy Text Effect/[Text Effect Type]`. Since effects are assets, they can be shared between multiple `TextEffect` components, and changes to the effect will be reflected in all components.",
                    EditorStyles.wordWrappedLabel
                );
            }

            EndFoldBox();

            BeginFoldBox("Applying Effects", ref applyEffectVisible_);
            if (applyEffectVisible_)
            {
                EditorGUILayout.BeginHorizontal();
                FileLink(
                    "Packages/com.qiaozhilei.easy-text-effects/Documentation/Documentation.md");
                UrlLink(
                    "https://github.com/LeiQiaoZhi/Easy-Text-Effect-for-Unity/blob/main/Documentation/Documentation.md#applying-effects");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(
                    @"There are 2 effect lists:
1. Tag Effects: Effects that are applied to the text based on rich text tags.
2. Global Effects: Effects that are applied to every character in the text.

Global effects are very easy to apply, just add an element to the list and drag the effect to the Effect field.
- The option overrideTagEffects determines whether a global effect override tag effects or not.

Tag effects are applied by adding a rich text tag to the text. The format is <link=effectName>text</link>. The effectName should match the Effect Name of the effect.
- When adding multiple tag effects, the format is <link=effectName1+effectName2>text</link>. Don't include ""+"" in effect names for this reason.",
                    EditorStyles.wordWrappedLabel
                );
            }

            EndFoldBox();

            BeginFoldBox("Controlling Effects", ref controlEffectVisible_);
            if (controlEffectVisible_)
            {
                EditorGUILayout.BeginHorizontal();
                FileLink(
                    "Packages/com.qiaozhilei.easy-text-effects/Documentation/Documentation.md");
                UrlLink(
                    "https://github.com/LeiQiaoZhi/Easy-Text-Effect-for-Unity/blob/main/Documentation/Documentation.md#controlling-effects");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(
                    @"Every element of an effect list has a `Trigger When` field, which determines when the effect is triggered. 
- `On Start`: The effect will start when the text is enabled.
- `Manual`: The effect will start only when a script tells it to.
   - `StartAllManualEffects()`: start all manual effects in the global list.
   - `StartManualEffects(string effectName)`: start the manual effect with the given name in the global list.
   - `StartManualTagEffects()`: start all manual effects in the tag list.
   - `StartManualTagEffects(string effectName)`: start the manual effect with the given name in the tag list.

There are some debug buttons to help you test manual effects in the editor.",
                    EditorStyles.wordWrappedLabel
                );
            }

            EndFoldBox();
        }

        private void DrawStatuses(List<TextEffectStatus> _statuses)
        {
            var boldTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };
            foreach (TextEffectStatus status in _statuses)
            {
                var started = status.Started ? "Started" : "Not Started";
                var isComplete = status.IsComplete ? "Complete" : "Not Complete";
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(status.Tag, boldTextStyle, GUILayout.Width(100));
                EditorGUILayout.LabelField(started, GUILayout.Width(100));
                EditorGUILayout.LabelField(isComplete, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#endif