#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace FlexAnimation
{
    public enum FlexEditorTheme { Modern, Minimal, Classic }

    public class FlexBaseEditor : Editor
    {
        protected SerializedProperty modulesProp;
        
        // Editor Preview
        protected double lastEditorTime;
        protected bool isPreviewing;

        // Editor State
        protected static bool isAdvancedMode = false;
        protected static FlexEditorTheme currentTheme = FlexEditorTheme.Modern;
        protected static string copiedModuleJson;
        protected static Type copiedModuleType;
        private bool showThemeSettings = false;

        // Colors
        private static readonly Color ColorMove = new Color(0.29f, 0.64f, 0.87f);   // Blue
        private static readonly Color ColorRotate = new Color(0.90f, 0.49f, 0.13f); // Orange
        private static readonly Color ColorScale = new Color(0.61f, 0.35f, 0.71f);  // Purple
        private static readonly Color ColorFade = new Color(0.18f, 0.80f, 0.44f);   // Green
        private static readonly Color ColorUI = new Color(0.91f, 0.12f, 0.39f);     // Pink
        private static readonly Color ColorEffect = new Color(0.91f, 0.30f, 0.24f); // Red
        private static readonly Color ColorMaterial = new Color(0.1f, 0.7f, 0.7f);  // Cyan
        private static readonly Color ColorGray = new Color(0.5f, 0.5f, 0.5f);

        protected virtual void OnEnable()
        {
            modulesProp = serializedObject.FindProperty("modules");
            EditorApplication.update += OnEditorUpdate;

            // Load preferences
            currentTheme = (FlexEditorTheme)EditorPrefs.GetInt("FlexAnim_Theme", (int)FlexEditorTheme.Modern);
            isAdvancedMode = EditorPrefs.GetBool("FlexAnim_Advanced", false);
        }

        protected virtual void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            if (target != null && !Application.isPlaying && target is FlexAnimation anim)
            {
                anim.StopAndReset();
            }
        }

        private void OnEditorUpdate()
        {
            if (Application.isPlaying) return;

            var anim = target as FlexAnimation;
            if (anim == null) return;

            double currentTime = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(currentTime - lastEditorTime);
            lastEditorTime = currentTime;

            if (isPreviewing)
            {
                anim.EditorPreviewUpdate(deltaTime);
                if (!Application.isPlaying) Repaint(); 
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (isPreviewing && !Application.isPlaying)
            {
                Repaint();
            }

            // 1. Header & Quick Settings (TOP)
            DrawHeader();
            DrawEditorSettings();
            
            EditorGUILayout.Space(5);

            // 2. Main Content
            DrawContent();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual new void DrawHeader()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 32);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.15f));
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 13;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            
            GUIContent titleContent = new GUIContent(" FLEX ANIMATION PRO", EditorGUIUtility.IconContent("d_AnimationClip Icon").image);
            EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, rect.width - 40, rect.height), titleContent, titleStyle);

            // Settings Toggle (Gear Icon)
            Rect settingsRect = new Rect(rect.xMax - 30, rect.y + 4, 24, 24);
            if (GUI.Button(settingsRect, EditorGUIUtility.IconContent("d_Settings"), EditorStyles.iconButton))
            {
                showThemeSettings = !showThemeSettings;
            }
        }

        private void DrawEditorSettings()
        {
            if (!showThemeSettings) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Editor Configuration", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Close", EditorStyles.miniButton, GUILayout.Width(50))) showThemeSettings = false;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);

            EditorGUI.BeginChangeCheck();
            FlexEditorTheme newTheme = (FlexEditorTheme)EditorGUILayout.EnumPopup("UI Theme", currentTheme);
            if (EditorGUI.EndChangeCheck())
            {
                currentTheme = newTheme;
                EditorPrefs.SetInt("FlexAnim_Theme", (int)currentTheme);
            }

            EditorGUI.BeginChangeCheck();
            bool newAdvanced = EditorGUILayout.Toggle("Expert Mode", isAdvancedMode);
            if (EditorGUI.EndChangeCheck())
            {
                isAdvancedMode = newAdvanced;
                EditorPrefs.SetBool("FlexAnim_Advanced", isAdvancedMode);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        protected virtual void DrawContent()
        {
            DrawModules();
            DrawAddModuleButton();
        }

        protected void DrawModules()
        {
            if (modulesProp == null) return;

            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                DrawModule(i);
            }
        }

        protected void DrawModule(int index)
        {
            var moduleProp = modulesProp.GetArrayElementAtIndex(index);
            var moduleType = GetManagedReferenceType(moduleProp);
            if (moduleType == null) return;

            var enabledProp = moduleProp.FindPropertyRelative("enabled");
            var linkTypeProp = moduleProp.FindPropertyRelative("linkType");
            
            string title = ObjectNames.NicifyVariableName(moduleType.Name).Replace("Module", "");
            Color themeColor = GetModuleColor(moduleType.Name);
            GUIContent icon = GetModuleIcon(moduleType.Name);

            if (currentTheme == FlexEditorTheme.Modern)
                DrawModernModule(index, moduleProp, enabledProp, linkTypeProp, title, themeColor, icon);
            else if (currentTheme == FlexEditorTheme.Minimal)
                DrawMinimalModule(index, moduleProp, enabledProp, linkTypeProp, title, themeColor, icon);
            else
                DrawClassicModule(index, moduleProp, enabledProp, linkTypeProp, title, themeColor, icon);
        }

        private void DrawModernModule(int index, SerializedProperty moduleProp, SerializedProperty enabledProp, SerializedProperty linkTypeProp, string title, Color color, GUIContent icon)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Rect rect = EditorGUILayout.GetControlRect(false, 28);
            
            Rect stripRect = new Rect(rect.x - 2, rect.y - 2, 4, rect.height + 4);
            EditorGUI.DrawRect(stripRect, enabledProp.boolValue ? color : new Color(0.2f, 0.2f, 0.2f));

            Rect toggleRect = new Rect(rect.x + 8, rect.y + 6, 16, 16);
            enabledProp.boolValue = EditorGUI.Toggle(toggleRect, enabledProp.boolValue);

            Rect titleRect = new Rect(rect.x + 30, rect.y, rect.width - 120, rect.height);
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };
            if (!enabledProp.boolValue) labelStyle.normal.textColor = Color.gray;

            if (Event.current.type == EventType.MouseDown && titleRect.Contains(Event.current.mousePosition))
            {
                moduleProp.isExpanded = !moduleProp.isExpanded;
                Event.current.Use();
            }

            EditorGUI.LabelField(titleRect, new GUIContent($" {title}", icon.image), labelStyle);
            DrawLinkBadge(rect, linkTypeProp);

            Rect gearRect = new Rect(rect.xMax - 22, rect.y + 4, 20, 20);
            if (GUI.Button(gearRect, EditorGUIUtility.IconContent("_Popup"), EditorStyles.iconButton))
                ShowModuleContextMenu(index);

            if (moduleProp.isExpanded)
            {
                using (new EditorGUI.DisabledScope(!enabledProp.boolValue))
                {
                    EditorGUILayout.Space(4);
                    DrawModuleContent(moduleProp, isAdvancedMode);
                    EditorGUILayout.Space(4);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void DrawMinimalModule(int index, SerializedProperty moduleProp, SerializedProperty enabledProp, SerializedProperty linkTypeProp, string title, Color color, GUIContent icon)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 24);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), new Color(0.5f, 0.5f, 0.5f, 0.2f));

            Rect toggleRect = new Rect(rect.x, rect.y + 4, 16, 16);
            enabledProp.boolValue = EditorGUI.Toggle(toggleRect, enabledProp.boolValue);

            Rect titleRect = new Rect(rect.x + 20, rect.y, rect.width - 100, 24);
            if (GUI.Button(titleRect, new GUIContent(title), EditorStyles.label))
                moduleProp.isExpanded = !moduleProp.isExpanded;

            EditorGUI.DrawRect(new Rect(rect.xMax - 30, rect.y + 10, 4, 4), color);

            Rect gearRect = new Rect(rect.xMax - 20, rect.y + 2, 20, 20);
            if (GUI.Button(gearRect, EditorGUIUtility.IconContent("_Popup"), EditorStyles.iconButton))
                ShowModuleContextMenu(index);

            if (moduleProp.isExpanded)
            {
                using (new EditorGUI.DisabledScope(!enabledProp.boolValue))
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawModuleContent(moduleProp, isAdvancedMode);
                    EditorGUILayout.Space(5);
                }
            }
        }

        private void DrawClassicModule(int index, SerializedProperty moduleProp, SerializedProperty enabledProp, SerializedProperty linkTypeProp, string title, Color color, GUIContent icon)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            
            Rect toggleRect = new Rect(rect.x, rect.y, 16, 16);
            enabledProp.boolValue = EditorGUI.Toggle(toggleRect, enabledProp.boolValue);

            Rect titleRect = new Rect(rect.x + 20, rect.y, rect.width - 100, 20);
            moduleProp.isExpanded = EditorGUI.Foldout(titleRect, moduleProp.isExpanded, title, true);
            
            Rect gearRect = new Rect(rect.xMax - 20, rect.y, 20, 20);
            if (GUI.Button(gearRect, EditorGUIUtility.IconContent("_Popup"), EditorStyles.iconButton))
                ShowModuleContextMenu(index);

            if (moduleProp.isExpanded)
            {
                using (new EditorGUI.DisabledScope(!enabledProp.boolValue))
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawModuleContent(moduleProp, isAdvancedMode);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawLinkBadge(Rect headerRect, SerializedProperty linkTypeProp)
        {
            Rect badgeRect = new Rect(headerRect.xMax - 85, headerRect.y + 4, 55, 18);
            int linkVal = linkTypeProp.enumValueIndex;
            string linkText = linkVal == 0 ? "SEQ" : (linkVal == 1 ? "JOIN" : "INS");
            
            Color badgeColor = linkVal == 0 ? new Color(0.2f, 0.2f, 0.2f, 0.5f) : (linkVal == 1 ? new Color(0.2f, 0.6f, 0.2f, 0.8f) : new Color(0.8f, 0.6f, 0.1f, 0.8f));
            
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = badgeColor;
            
            GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniButton);
            badgeStyle.normal.textColor = Color.white;
            badgeStyle.fontSize = 9;

            if (GUI.Button(badgeRect, linkText, badgeStyle))
                linkTypeProp.enumValueIndex = (linkVal == 0) ? 1 : 0; 
            
            GUI.backgroundColor = oldColor;
        }

        private void DrawModuleContent(SerializedProperty moduleProp, bool advanced)
        {
            var iterator = moduleProp.Copy();
            var end = iterator.GetEndProperty();
            iterator.NextVisible(true);

            while (iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, end))
            {
                string name = iterator.name;
                if (name == "enabled") continue;

                // [Special Handling for MaterialModule Pipeline]
                if (moduleProp.managedReferenceFullTypename.Contains("MaterialModule"))
                {
                    var pipelineProp = moduleProp.FindPropertyRelative("pipeline");
                    var propNameProp = moduleProp.FindPropertyRelative("propertyName");
                    var propTypeProp = moduleProp.FindPropertyRelative("propertyType");

                    if (pipelineProp != null && propNameProp != null && propTypeProp != null)
                    {
                        if (pipelineProp.enumValueIndex == 0) // Standard
                        {
                            if (propTypeProp.enumValueIndex == 0) propNameProp.stringValue = "_Color";
                            else if (propTypeProp.enumValueIndex == 1) propNameProp.stringValue = "_Glossiness";
                            else if (propTypeProp.enumValueIndex == 3) propNameProp.stringValue = "_MainTex";
                        }
                        else if (pipelineProp.enumValueIndex == 1) // URP
                        {
                            if (propTypeProp.enumValueIndex == 0) propNameProp.stringValue = "_BaseColor";
                            else if (propTypeProp.enumValueIndex == 1) propNameProp.stringValue = "_Smoothness";
                            else if (propTypeProp.enumValueIndex == 3) propNameProp.stringValue = "_BaseMap";
                        }
                        
                        // Hide propertyName field if not Custom or not in Advanced mode
                        if (name == "propertyName" && pipelineProp.enumValueIndex != 2) continue;
                    }
                }
                
                if (!advanced)
                {
                    // [Expert Only Fields]
                    if (name == "linkType" || name == "ease" || name == "loop" || name == "loopCount" || name == "relative" || 
                        name == "randomSpread" || name == "space" || name == "materialIndex" || name == "persist" || name == "interval" ||
                        name == "scrambleChars" || name == "waveFrequency" || name == "waveSpeed" || 
                        name == "overlap" || name == "slideDirection" || 
                        name == "effectStrength" || name == "effectIntensity" || name == "effectColor" ||
                        name == "vibrato" || name == "randomness" || name == "fadeOut" || name == "elasticity" ||
                        name == "glitchStrength" || name == "waveAmplitude" || name == "scrambleMode") 
                        continue;
                    
                    // Also hide propertyName in Basic View unless it's Custom
                    if (name == "propertyName")
                    {
                        var pipelineProp = moduleProp.FindPropertyRelative("pipeline");
                        if (pipelineProp == null || pipelineProp.enumValueIndex != 2) continue;
                    }
                }

                if (name == "duration")
                {
                    EditorGUILayout.BeginHorizontal();
                    float originalLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 60;
                    EditorGUILayout.PropertyField(iterator, new GUIContent("Time"));
                    
                    var delayProp = moduleProp.FindPropertyRelative("delay");
                    if (delayProp != null)
                    {
                        EditorGUILayout.Space(10);
                        EditorGUIUtility.labelWidth = 40;
                        EditorGUILayout.PropertyField(delayProp, new GUIContent("Delay"));
                    }
                    EditorGUIUtility.labelWidth = originalLabelWidth;
                    EditorGUILayout.EndHorizontal();
                }
                else if (name == "delay") { continue; }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
        }

        private Color GetModuleColor(string typeName)
        {
            if (typeName.Contains("Move")) return ColorMove;
            if (typeName.Contains("Rotate")) return ColorRotate;
            if (typeName.Contains("Scale")) return ColorScale;
            if (typeName.Contains("Fade") || typeName.Contains("Color")) return ColorFade;
            if (typeName.Contains("UI")) return ColorUI;
            if (typeName.Contains("Event") || typeName.Contains("Punch") || typeName.Contains("Shake")) return ColorEffect;
            if (typeName.Contains("Material")) return ColorMaterial;
            return ColorGray;
        }

        protected GUIContent GetModuleIcon(string typeName)
        {
            string iconName = "cs Script Icon";
            if (typeName.Contains("Move")) iconName = "MoveTool";
            else if (typeName.Contains("Rotate")) iconName = "RotateTool";
            else if (typeName.Contains("Scale")) iconName = "ScaleTool";
            else if (typeName.Contains("Fade") || typeName.Contains("Color")) iconName = "PreMatCube";
            else if (typeName.Contains("UI")) iconName = "RectTransform Icon";
            else if (typeName.Contains("Punch") || typeName.Contains("Shake")) iconName = "Animation.EventMarker";
            else if (typeName.Contains("Audio")) iconName = "AudioSource Icon";
            else if (typeName.Contains("Text")) iconName = "TextAsset Icon";
            else if (typeName.Contains("Material")) iconName = "Material Icon";

            return EditorGUIUtility.IconContent(iconName);
        }

        protected void ShowModuleContextMenu(int index)
        {
            GenericMenu menu = new GenericMenu();
            var moduleProp = modulesProp.GetArrayElementAtIndex(index);
            var currentType = GetManagedReferenceType(moduleProp);

            menu.AddItem(new GUIContent("Copy Module"), false, () =>
            {
                object module = moduleProp.managedReferenceValue;
                copiedModuleJson = JsonUtility.ToJson(module);
                copiedModuleType = currentType;
            });

            if (!string.IsNullOrEmpty(copiedModuleJson) && copiedModuleType == currentType)
            {
                menu.AddItem(new GUIContent("Paste Module Value"), false, () =>
                {
                    object newModule = Activator.CreateInstance(currentType);
                    JsonUtility.FromJsonOverwrite(copiedModuleJson, newModule);
                    moduleProp.managedReferenceValue = newModule;
                    serializedObject.ApplyModifiedProperties();
                });
            }
            else
                menu.AddDisabledItem(new GUIContent("Paste Module Value"));

            menu.AddSeparator("");
            if (index > 0) menu.AddItem(new GUIContent("Move Up"), false, () => MoveModule(index, index - 1));
            else menu.AddDisabledItem(new GUIContent("Move Up"));

            if (index < modulesProp.arraySize - 1) menu.AddItem(new GUIContent("Move Down"), false, () => MoveModule(index, index + 1));
            else menu.AddDisabledItem(new GUIContent("Move Down"));

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Remove"), false, () =>
            {
                modulesProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
            });

            menu.ShowAsContext();
        }

        protected void MoveModule(int from, int to)
        {
            modulesProp.MoveArrayElement(from, to);
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawAddModuleButton()
        {
            EditorGUILayout.Space(10);
            Rect rect = EditorGUILayout.GetControlRect(false, 30);
            if (GUI.Button(rect, new GUIContent(" Add New Module", EditorGUIUtility.IconContent("d_CreateAddNew").image)))
                ShowAddModuleMenu();
        }

        protected void ShowAddModuleMenu()
        {
            GenericMenu menu = new GenericMenu();
            var moduleTypes = Assembly.GetAssembly(typeof(AnimationModule)).GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AnimationModule)));

            foreach (var type in moduleTypes)
            {
                string niceName = ObjectNames.NicifyVariableName(type.Name).Replace("Module", "");
                menu.AddItem(new GUIContent(niceName), false, () => AddModule(type));
            }
            menu.ShowAsContext();
        }

        protected void AddModule(Type type)
        {
            int index = modulesProp.arraySize;
            modulesProp.InsertArrayElementAtIndex(index);
            modulesProp.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type);
            serializedObject.ApplyModifiedProperties();
        }

        protected Type GetManagedReferenceType(SerializedProperty property)
        {
            if (property == null || string.IsNullOrEmpty(property.managedReferenceFullTypename)) return null;
            var parts = property.managedReferenceFullTypename.Split(' ');
            return Type.GetType($"{parts[1]}, {parts[0]}");
        }

        protected void StartPreview()
        {
            lastEditorTime = EditorApplication.timeSinceStartup;
            isPreviewing = true;
            if (target is FlexAnimation anim) anim.PlayAll();
        }

        protected void StopPreview()
        {
            isPreviewing = false;
            if (target is FlexAnimation anim) anim.StopAndReset();
        }
    }

    [CustomEditor(typeof(FlexAnimation))]
    public class FlexAnimationEditor : FlexBaseEditor
    {
        private SerializedProperty presetProp;
        private SerializedProperty playOnEnableProp;
        private SerializedProperty timeScaleProp;
        private SerializedProperty ignoreTimeScaleProp;
        private bool showConfig = true;

        protected override void OnEnable()
        {
            base.OnEnable();
            presetProp = serializedObject.FindProperty("preset");
            playOnEnableProp = serializedObject.FindProperty("playOnEnable");
            timeScaleProp = serializedObject.FindProperty("timeScale");
            ignoreTimeScaleProp = serializedObject.FindProperty("ignoreTimeScale");
        }

        protected override void DrawContent()
        {
            DrawPlayerControls();
            EditorGUILayout.Space(10);

            // --- Toolbar ---
            DrawToolbarTab();
            EditorGUILayout.Space(5);

            // --- Configuration Section ---
            if (isAdvancedMode)
            {
                showConfig = EditorGUILayout.Foldout(showConfig, "Core Settings", true);
                if (showConfig)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(playOnEnableProp);
                        EditorGUILayout.PropertyField(presetProp);
                        if (presetProp.objectReferenceValue != null)
                            EditorGUILayout.HelpBox("Using Preset Data. Local modules are ignored.", MessageType.Info);
                        
                        EditorGUILayout.Space(5);
                        EditorGUILayout.PropertyField(timeScaleProp);
                        EditorGUILayout.PropertyField(ignoreTimeScaleProp);
                    }
                }
            }
            else
            {
                // Simple Config
                EditorGUILayout.PropertyField(playOnEnableProp);
                EditorGUILayout.PropertyField(presetProp);
            }

            EditorGUILayout.Space(10);

            // --- Modules Section ---
            if (presetProp.objectReferenceValue == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Animation Timeline", EditorStyles.boldLabel);
                if (GUILayout.Button("All Join", EditorStyles.miniButtonLeft, GUILayout.Width(60))) SetAllLinkType(FlexLinkType.Join);
                if (GUILayout.Button("All Seq", EditorStyles.miniButtonRight, GUILayout.Width(60))) SetAllLinkType(FlexLinkType.Append);
                EditorGUILayout.EndHorizontal();
                
                DrawModules();
                DrawAddModuleButton();
            }

            if (isAdvancedMode)
            {
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPlay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnComplete"));
            }
        }

        private void DrawToolbarTab()
        {
            string[] modes = { "Basic View", "Expert View" };
            int modeIndex = isAdvancedMode ? 1 : 0;
            int newIndex = GUILayout.Toolbar(modeIndex, modes, GUILayout.Height(22));
            
            if (newIndex != modeIndex)
            {
                isAdvancedMode = (newIndex == 1);
                EditorPrefs.SetBool("FlexAnim_Advanced", isAdvancedMode);
            }
        }
        
        private void SetAllLinkType(FlexLinkType type)
        {
            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                var prop = modulesProp.GetArrayElementAtIndex(i).FindPropertyRelative("linkType");
                if (prop != null) prop.enumValueIndex = (int)type;
            }
        }

        private void DrawPlayerControls()
        {
            EditorGUILayout.BeginHorizontal();
            
            float btnWidth = (EditorGUIUtility.currentViewWidth - 40) / 3f;
            
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
            if (GUILayout.Button(new GUIContent(" Play", EditorGUIUtility.IconContent("PlayButton").image), GUILayout.Height(32), GUILayout.Width(btnWidth)))
            {
                if (Application.isPlaying) ((FlexAnimation)target).PlayAll();
                else StartPreview();
            }

            GUI.backgroundColor = new Color(1f, 1f, 0.7f);
            if (GUILayout.Button(new GUIContent(" Pause", EditorGUIUtility.IconContent("PauseButton").image), GUILayout.Height(32), GUILayout.Width(btnWidth)))
            {
                if (Application.isPlaying) ((FlexAnimation)target).PauseAll();
                else isPreviewing = false;
            }

            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
            if (GUILayout.Button(new GUIContent(" Stop", EditorGUIUtility.IconContent("PreMatQuad").image), GUILayout.Height(32), GUILayout.Width(btnWidth)))
            {
                if (Application.isPlaying) ((FlexAnimation)target).StopAndReset();
                else StopPreview();
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
    }

    [CustomEditor(typeof(FlexAnimationPreset))]
    public class FlexAnimationPresetEditor : FlexBaseEditor
    {
        protected override void DrawContent()
        {
            EditorGUILayout.LabelField("Preset Timeline", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            DrawModules();
            DrawAddModuleButton();
        }
    }
}
#endif