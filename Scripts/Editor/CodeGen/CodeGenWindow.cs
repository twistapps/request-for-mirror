using UnityEditor;
using UnityEngine;

namespace RequestForMirror.Editor.CodeGen
{
    internal class CodeGenWindow : EditorWindow
    {
        private float _oldLabelWidth = -1;
        private CodeGenSettings _settings;

        private UnityEditor.Editor _settingsEditor;

        private void OnGUI()
        {
            CreateCachedSettingsEditor();
            EditorGUILayout.BeginVertical(new GUIStyle("ObjectPickerBackground"));
            EditorGUILayout.LabelField("CodeGen settings", new GUIStyle("BoldLabel"));

            ChangeLabelWidth(250);
            EditorGUI.indentLevel++;
            _settings.autoGenerateOnCompile =
                EditorGUILayout.Toggle("Auto Generate Scripts On Compile", _settings.autoGenerateOnCompile);
            _settings.debugMode = EditorGUILayout.Toggle("Debug Mode", _settings.debugMode);
            RestoreLabelWidth();


            EditorGUI.indentLevel--;
            GUILayout.Space(20);

            EditorGUILayout.EndVertical();

            GUILayout.Space(-25);
            EditorGUILayout.BeginVertical(new GUIStyle("NotificationBackground"));

            GUILayout.Space(-20);
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.LabelField($"Generate all files marked with {nameof(IMarkedForCodeGen)}:",
                new GUIStyle("PR Label"));
            EditorGUI.indentLevel += 2;
            using (var l = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Generate CS", new GUIStyle("ToolbarButton"), GUILayout.Width(120)))
                    //todo: catch exceptions and show dialog with result ('ok' or 'got errors')
                    CodeGen.GenerateScripts(true);
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(-20);

            EditorGUILayout.EndVertical();
            GUILayout.Space(-25);


            EditorGUILayout.BeginVertical(new GUIStyle("ObjectPickerBackground"));
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Create codegen-supported script:", new GUIStyle("BoldLabel"));

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Classname");
            EditorGUI.indentLevel++;
            EditorGUILayout.TextField("CodeGenSample");
            EditorGUI.indentLevel--;

            GUILayout.Space(5);

            EditorGUILayout.LabelField("SomeOtherField");
            EditorGUI.indentLevel++;
            EditorGUILayout.TextField("SomeOtherDefaultText ");
            EditorGUI.indentLevel--;


            GUILayout.Space(15);

            // Horizontally centered
            using (var l = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Button("Create", new GUIStyle("ToolbarButton"), GUILayout.Width(120));
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(10);


            EditorGUILayout.EndVertical();
        }

        [MenuItem("Tools/MyCodeGen/CodeGenMenu")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(CodeGenWindow));
            window.minSize = new Vector2(460, 300);
        }

        //creates editor to track changes made to _settings so they are saved to asset as soon as, for example, checkbox is marked.
        private void CreateCachedSettingsEditor()
        {
            if (_settings != null && _settingsEditor != null) return;
            _settings = CodeGen.LoadSettingsAsset();
            _settingsEditor = UnityEditor.Editor.CreateEditor(_settings);
        }

        private void ChangeLabelWidth(float newWidth)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_oldLabelWidth != -1) return;
            _oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = newWidth;
        }

        private void RestoreLabelWidth()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_oldLabelWidth == -1) return;
            EditorGUIUtility.labelWidth = _oldLabelWidth;
            _oldLabelWidth = -1;
        }
    }
}