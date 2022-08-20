using System;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RequestForMirror.Editor
{
    public abstract class PackageSettingsWindow<TSettings> : EditorWindow where TSettings : SettingsAsset
    {
        protected static TSettings Settings;
        private UnityEditor.Editor _settingsEditor;

        private static Type TraceCallingType()
        {
            var stackTrace = new StackTrace();
            var thisType = stackTrace.GetFrame(0).GetMethod().DeclaringType;
            
            for (var i = 1; i < stackTrace.FrameCount; i++)
            {
                var declaringType = stackTrace.GetFrame(i).GetMethod().DeclaringType;
                if (declaringType != thisType) return declaringType;
            }

            return thisType;
        }
        
        protected static void ShowWindow()
        {
            Settings = EditorUtils.LoadSettings<TSettings>();
            var window = GetWindow(TraceCallingType(), false, Settings.GetEditorWindowTitle());
            window.minSize = new Vector2(420, 300);
        }
        
        /// <summary>
        /// Creates Editor for Settings ScriptableObject
        /// so changes are saved to asset file as soon as they made.
        /// </summary>
        private void CreateCachedSettingsEditor()
        {
            if (Settings != null && _settingsEditor != null) return;
            Settings = EditorUtils.LoadSettings<TSettings>();
            _settingsEditor = UnityEditor.Editor.CreateEditor(Settings);
        }

        protected virtual void OnGUI()
        {
            CreateCachedSettingsEditor();
        }

        protected void BeginSection(string heading, bool addDivider=false)
        {
            EditorGUILayout.BeginVertical(new GUIStyle("ObjectPickerBackground"));
            if (addDivider) EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField(heading, new GUIStyle("BoldLabel"));
            EditorGUI.indentLevel++;
            ChangeLabelWidth(250);
        }
        
        protected void EndSection()
        {
            RestoreLabelWidth();
            EditorGUI.indentLevel--;
            GUILayout.Space(20);

            EditorGUILayout.EndVertical();
        }

        protected void Checkbox(string text, ref bool value)
        {
            value = EditorGUILayout.Toggle(text, value);
        }

        protected void InputField(string text, ref string value)
        {
            EditorGUILayout.LabelField(text);
            EditorGUI.indentLevel++;
            value = EditorGUILayout.TextField(value);
            EditorGUI.indentLevel--;
            GUILayout.Space(5);
        }

        protected void InputField(string text)
        {
            var empty = "";
            InputField(text, ref empty);
        }

        private const int HorizontalButtonsMargin = 10;

        protected void HorizontalButtons(params Button[] buttons)
        {
            GUILayout.Space(HorizontalButtonsMargin);
            // Horizontally centered
            using (var l = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                foreach (var button in buttons)
                {
                    button.Construct();
                    GUILayout.Space(5);
                }
                GUILayout.FlexibleSpace();
            }
            //GUILayout.Space(HorizontalButtonsMargin);
        }

        protected void CallToAction(string heading, params Button[] buttons)
        {
            GUILayout.Space(-25);
            EditorGUILayout.BeginVertical(new GUIStyle("NotificationBackground"));
            GUILayout.Space(-20);
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.LabelField(heading, new GUIStyle("PR Label"));
            EditorGUI.indentLevel += 2;
            
            GUILayout.Space(-HorizontalButtonsMargin);
            GUILayout.Space(5);
            HorizontalButtons(buttons);
            GUILayout.Space(-5);
            //GUILayout.Space(-HorizontalButtonsMargin);

            GUILayout.Space(-20);
            EditorGUILayout.EndVertical();
            GUILayout.Space(-25);
        }
        
        private float _memorizedLabelWidth = -1;
        
        private void ChangeLabelWidth(float newWidth)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_memorizedLabelWidth != -1) return;
            _memorizedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = newWidth;
        }

        private void RestoreLabelWidth()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_memorizedLabelWidth == -1) return;
            EditorGUIUtility.labelWidth = _memorizedLabelWidth;
            _memorizedLabelWidth = -1;
        }
    }

    public class Button
    {
        private readonly string _innerText;
        private readonly Action _onClick;

        public Button(string innerText, Action onClick=null)
        {
            _innerText = innerText;
            _onClick = onClick;
        }

        public void Construct()
        {
            if (GUILayout.Button(_innerText, new GUIStyle("ToolbarButton"), GUILayout.Width(120)))
            {
                _onClick?.Invoke();
            }
        }
    }
}