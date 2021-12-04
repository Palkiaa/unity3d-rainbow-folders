﻿/*
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using System;

using UnityEditor;

using UnityEngine;

namespace Borodar.RainbowFolders.Editor
{
    public class RainbowFoldersPreferences
    {
        private const string HOME_ENABLED_PREF_KEY = "Borodar.RainbowFolders.Enabled.";
        private const bool HOME_ENABLED_DEFAULT = false;
        private const string HOME_ENABLED_HINT = "Toggle to enable and disable \"Rainbow Folders\".";

        private const string HOME_FOLDER_PREF_KEY = "Borodar.RainbowFolders.Path.";
        private const string HOME_FOLDER_DEFAULT = "Editor/Setting";
        private const string HOME_FOLDER_HINT = "Where \"Rainbow Folders\" saves your settings.";

        private const string MOD_KEY_PREF_KEY = "Borodar.RainbowFolders.EditMod.";
        private const EventModifiers MOD_KEY_DEFAULT = EventModifiers.Alt;
        private const string MOD_KEY_HINT = "Modifier key that is used to show configuration dialogue when clicking on a folder icon.";

        private static readonly EditorPrefsBool ENABLE_KEY_PREF;

        private static readonly EditorPrefsString PATH_KEY_PREF;

        private static readonly EditorPrefsModifierKey MODIFIER_KEY_PREF;

        public static bool Enabled;

        public static string Path;

        public static EventModifiers ModifierKey;

        static RainbowFoldersPreferences()
        {
            var enableLabel = new GUIContent("Enabled", HOME_ENABLED_HINT);
            ENABLE_KEY_PREF = new EditorPrefsBool(HOME_ENABLED_PREF_KEY + ProjectName, enableLabel, HOME_ENABLED_DEFAULT);
            Enabled = ENABLE_KEY_PREF.Value;

            var pathLabel = new GUIContent("Settings location", HOME_FOLDER_HINT);
            PATH_KEY_PREF = new EditorPrefsString(HOME_FOLDER_PREF_KEY + ProjectName, pathLabel, HOME_FOLDER_DEFAULT);
            Path = PATH_KEY_PREF.Value;

            var modifierLabel = new GUIContent("Modifier Key", MOD_KEY_HINT);
            MODIFIER_KEY_PREF = new EditorPrefsModifierKey(MOD_KEY_PREF_KEY + ProjectName, modifierLabel, MOD_KEY_DEFAULT);
            ModifierKey = MODIFIER_KEY_PREF.Value;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        [PreferenceItem("Rainbow Folders")]
        public static void EditorPreferences()
        {
            EditorGUILayout.Separator();
            ENABLE_KEY_PREF.Draw();
            Enabled = ENABLE_KEY_PREF.Value;

            PATH_KEY_PREF.Draw();
            Path = PATH_KEY_PREF.Value;

            MODIFIER_KEY_PREF.Draw();
            ModifierKey = MODIFIER_KEY_PREF.Value;

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Version " + AssetInfo.VERSION, EditorStyles.centeredGreyMiniLabel);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static string ProjectName
        {
            get
            {
                var s = Application.dataPath.Split('/');
                var p = s[s.Length - 2];
                return p;
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public abstract class EditorPrefsItem<T>
        {
            public string Key;
            public GUIContent Label;
            public T DefaultValue;

            protected EditorPrefsItem(string key, GUIContent label, T defaultValue)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException("key");
                }

                Key = key;
                Label = label;
                DefaultValue = defaultValue;
            }

            public abstract T Value { get; set; }
            public abstract void Draw();

            public static implicit operator T(EditorPrefsItem<T> s)
            {
                return s.Value;
            }
        }

        private class EditorPrefsString : EditorPrefsItem<string>
        {
            public EditorPrefsString(string key, GUIContent label, string defaultValue)
                : base(key, label, defaultValue) { }

            public override string Value
            {
                get { return EditorPrefs.GetString(Key, DefaultValue); }
                set { EditorPrefs.SetString(Key, value); }
            }

            public override void Draw()
            {
                EditorGUIUtility.labelWidth = 100f;
                Value = EditorGUILayout.TextField(Label, Value);
            }
        }

        private class EditorPrefsModifierKey : EditorPrefsItem<EventModifiers>
        {

            public EditorPrefsModifierKey(string key, GUIContent label, EventModifiers defaultValue)
                : base(key, label, defaultValue) { }

            public override EventModifiers Value
            {
                get
                {
                    var index = EditorPrefs.GetInt(Key, (int)DefaultValue);
                    return (Enum.IsDefined(typeof(EventModifiers), index)) ? (EventModifiers)index : DefaultValue;
                }
                set
                {
                    EditorPrefs.SetInt(Key, (int)value);
                }
            }

            public override void Draw()
            {
                Value = (EventModifiers)EditorGUILayout.EnumPopup(Label, Value);
            }
        }

        private class EditorPrefsBool : EditorPrefsItem<bool>
        {
            public EditorPrefsBool(string key, GUIContent label, bool defaultValue)
                : base(key, label, defaultValue) { }

            public override bool Value
            {
                get
                {
                    return EditorPrefs.GetBool(Key, DefaultValue);
                }
                set
                {
                    EditorPrefs.SetBool(Key, value);
                }
            }

            public override void Draw()
            {
                Value = EditorGUILayout.Toggle(Label, Value);
            }
        }
    }
}