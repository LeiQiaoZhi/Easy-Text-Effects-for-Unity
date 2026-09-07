#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EasyTextEffects.Editor.EditorDocumentation
{
    public static class EditorDocumentation
    {
        public static bool show = true;

        // Cache styles to prevent per-frame allocations
        private static GUIStyle _headingStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _clickToExpandStyle;
        private static GUIStyle _inlineButtonStyle;

        private static void InitStyles()
        {
            if (_headingStyle == null)
            {
                _headingStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft
                };
            }
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.BoldAndItalic,
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft
                };
            }
            if (_clickToExpandStyle == null)
            {
                _clickToExpandStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Italic,
                    fontSize = 12,
                    alignment = TextAnchor.MiddleRight
                };
            }
            if (_inlineButtonStyle == null)
            {
                var iconDimension = EditorGUIUtility.singleLineHeight;
                _inlineButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fixedHeight = iconDimension,
                    fixedWidth = iconDimension,
                    padding = new RectOffset(2, 2, 2, 2)
                };
            }
        }

        public enum IconType
        {
            Help,
            Info,
            Warn,
            Error,
            Text,
            Move,
            Rotate,
            Scale,
            Search,
            Tool
        }

        public static string GetIconName(IconType _iconType)
        {
            return _iconType switch
            {
                // IconType.Help => "d__Help",
                IconType.Help => "UnityEditor.ConsoleWindow",
                IconType.Info => "d_console.infoicon",
                IconType.Warn => "d_console.warnicon",
                IconType.Error => "d_console.erroricon",
                IconType.Text => "d_TextAsset Icon",
                IconType.Move => "d_MoveTool",
                IconType.Rotate => "d_RotateTool",
                IconType.Scale => "d_ScaleTool",
                IconType.Search => "d_ViewToolOrbit",
                IconType.Tool => "CustomTool",
                _ => "d__Help"
            };
        }
        
        public static GUIStyle HeadingStyle()
        {
            InitStyles();
            return _headingStyle;
        }
        public static GUIStyle TitleStyle()
        {
            InitStyles();
            return _titleStyle;
        }

        public static void BeginFoldBox(string _title, ref bool _foldoutState, IconType _icon = IconType.Help)
        {
            if (!show) return;
            InitStyles();
            EditorGUILayout.BeginVertical("HelpBox");
            // Create a horizontal layout for the foldout header
            EditorGUILayout.BeginHorizontal();
            // icon
            GUIContent icon = EditorGUIUtility.IconContent(GetIconName(_icon));
            GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
            GUILayout.Label(_title, _titleStyle); // Use cached style
            // Right-aligned label for "(Click to expand)"
            GUILayout.FlexibleSpace();
            GUILayout.Label(_foldoutState ? "(Click to collapse)" : "(Click to expand)", _clickToExpandStyle); // Use cached style


            EditorGUILayout.EndHorizontal();

            // Check for mouse click to toggle foldout state
            Rect headerRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition))
            {
                _foldoutState = !_foldoutState; // Toggle foldout state
                Event.current.Use();
            }

            if (_foldoutState)
            {
                EditorGUILayout.Space(2);
            }
        }

        public static void EndFoldBox()
        {
            if (!show) return;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private static string _errorMessage = string.Empty;

        public static void UrlLink(string _url, string _label = "Online Documentation")
        {
            if (!show) return;
            Texture iconImage = EditorGUIUtility.IconContent(GetIconName(IconType.Info)).image;

            if (GUILayout.Button(new GUIContent(_label, iconImage), GUILayout.ExpandWidth(false),
                    GUILayout.ExpandHeight(false), GUILayout.Height(20)))
            {
                System.Diagnostics.Process.Start(_url);
            }
        }

        public static void FileLink(string _path, string _label = "Local Documentation")
        {
            if (!show) return;
            Texture iconImage = EditorGUIUtility.IconContent(GetIconName(IconType.Info)).image;

            if (GUILayout.Button(new GUIContent(_label, iconImage), GUILayout.ExpandWidth(false),
                    GUILayout.ExpandHeight(false), GUILayout.Height(20)))
            {
                if (System.IO.File.Exists(_path))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(_path);
                    if (asset != null)
                    {
                        EditorGUIUtility.PingObject(asset);
                    }
                    else
                    {
                        _errorMessage = $"Asset not found: {_path}";
                    }
                }
                else
                {
                    _errorMessage = $"File not found: {_path}";
                }
            }

            if (_errorMessage != string.Empty)
            {
                // EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);
                Debug.LogError(_errorMessage);
            }
        }

                public static void InlineFoldBox(Rect _position, SerializedProperty _property, GUIContent _label, ref bool _foldoutState)
        {
            if (!show)
            {
                EditorGUI.PropertyField(_position, _property, _label, true);
                return;
            }
            InitStyles();
            GUIContent icon = EditorGUIUtility.IconContent(GetIconName(IconType.Help));
            var iconDimension = EditorGUIUtility.singleLineHeight;
            var rect = new Rect(_position.x, _position.y, iconDimension, iconDimension);
            if (GUI.Button(rect, icon, _inlineButtonStyle)) // Use cached style
            {
                _foldoutState = !_foldoutState; // Toggle foldout state
                Event.current.Use();
            }
            
            var propertyWidth = _position.width - iconDimension; // Adjust width after foldout
            var propertyRect = new Rect(_position.x + iconDimension, _position.y, propertyWidth, _position.height);
            EditorGUI.PropertyField(propertyRect, _property, _label, true);
        }
    }
}
#endif