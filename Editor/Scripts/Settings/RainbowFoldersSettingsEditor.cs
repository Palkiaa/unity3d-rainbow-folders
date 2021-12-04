/*
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

using Borodar.ReorderableList;
using UnityEditor;

using UnityEngine;

namespace Borodar.RainbowFolders.Editor.Settings
{
    [CustomEditor(typeof (RainbowFoldersSettings))]
    public class RainbowFoldersSettingsEditor : UnityEditor.Editor
    {
        private string PROP_NAME_DEFAULTFOLDER => nameof(RainbowFoldersSettings.DefaultFolder);
        private string PROP_NAME_FOLDERS => nameof(RainbowFoldersSettings.Folders);

        private SerializedProperty _defaultFolderProperty;
        private SerializedProperty _foldersProperty;

        protected void OnEnable()
        {
            _defaultFolderProperty = serializedObject.FindProperty(PROP_NAME_DEFAULTFOLDER);
            _foldersProperty = serializedObject.FindProperty(PROP_NAME_FOLDERS);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            RainbowFoldersSettings settings = target as RainbowFoldersSettings;
            if (GUILayout.Button("Reset all color modifiers"))
            {
                foreach (var layer in settings.DefaultFolder.IconLayers)
                {
                    layer.Color = Color.white;
                }

                foreach (var folder in settings.Folders)
                {
                    foreach (var layer in folder.IconLayers)
                    {
                        layer.Color = Color.white;
                    }
                }
            }
            base.OnInspectorGUI();
            //GUILayout.Label("Default folder settings");
            //EditorGUILayout.PropertyField(_defaultFolderProperty);

            //ReorderableListGUI.Title("Rainbow Folders");
            //ReorderableListGUI.ListField(_foldersProperty);
            serializedObject.ApplyModifiedProperties();
        }

    }
}