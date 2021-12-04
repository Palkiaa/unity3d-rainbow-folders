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

using System;
using UnityEngine;
using UnityEditor;
using KeyType = Borodar.RainbowFolders.Editor.Settings.RainbowFolder.KeyType;
using System.Linq;
using System.Collections.Generic;

namespace Borodar.RainbowFolders.Editor.Settings
{
    //[CustomPropertyDrawer(typeof(RainbowFolder))]
    public class RainbowFolderDrawer : PropertyDrawer
    {
        //private static readonly Dictionary<int, float> heights;
        //static RainbowFolderDrawer()
        //{
        //    heights = new Dictionary<int, float>();
        //}

        private const float PADDING = 8f;
        private const float SPACING = 1f;
        private const float LINE_HEIGHT = 16f;
        private const float LABELS_WIDTH = 100f;
        private const float PREVIEW_SIZE_SMALL = 16f;
        private const float PREVIEW_SIZE_LARGE = 96f;

        float totalHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var originalPosition = position;

            totalHeight = 0f;
            /*
            var folderKey = property.FindPropertyRelative(nameof(RainbowFolder.Key));
            var folderKeyType = property.FindPropertyRelative(nameof(RainbowFolder.Type));
            var folderRecursive = property.FindPropertyRelative(nameof(RainbowFolder.IsRecursive));
            var folderColor = property.FindPropertyRelative(nameof(RainbowFolder.Color));
            var rect = property.FindPropertyRelative(nameof(RainbowFolder.Rect));
            var icon = property.FindPropertyRelative(nameof(RainbowFolder.Icon));*/

            // Labels
            position.y += PADDING;
            position.width = LABELS_WIDTH - PADDING;
            position.height = LINE_HEIGHT;

            var propNames = typeof(RainbowFolder).GetFields();
            for (int i = 0; i < propNames.Length; i++)
            {
                var item = propNames[i];
                try
                {
                    var prop = property.FindPropertyRelative(item.Name);
                    float height = LINE_HEIGHT;

                    position.x = originalPosition.x;
                    EditorGUI.LabelField(position, item.Name);

                    position.x += LABELS_WIDTH;
                    position.width = originalPosition.width - LABELS_WIDTH;
                    if (item.FieldType.IsEnum)
                    {
                        height += LINE_HEIGHT;
                        position.height = height;

                        var typeSelected = (Enum)Enum.GetValues(item.FieldType).GetValue(prop.enumValueIndex);
                        prop.enumValueIndex = (int)(object)EditorGUI.EnumPopup(position, typeSelected);
                    }
                    else
                    {
                        height = EditorGUI.GetPropertyHeight(prop);
                        position.height = height;
                        EditorGUI.PropertyField(position, prop, GUIContent.none);
                    }

                    position.y += height + SPACING;
                    totalHeight += height + SPACING;
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat("Failed to draw the property '{0}' for {1}\r\n{2}", item.Name, nameof(RainbowFolder), ex);
                }
            }
            totalHeight += SPACING;
            /*
            var typeSelected = (KeyType)Enum.GetValues(typeof(KeyType)).GetValue(folderKeyType.enumValueIndex);
            folderKeyType.enumValueIndex = (int)(KeyType)EditorGUI.EnumPopup(position, typeSelected);

            position.y += LINE_HEIGHT + SPACING;
            EditorGUI.LabelField(position, "Recursive");

            position.y += LINE_HEIGHT + SPACING;
            EditorGUI.LabelField(position, "Color");

            position.y += LINE_HEIGHT + SPACING;
            EditorGUI.LabelField(position, "Rect");

            position.y += LINE_HEIGHT + LINE_HEIGHT + LINE_HEIGHT + SPACING;
            EditorGUI.LabelField(position, "Icon");

            // Values

            position.x += LABELS_WIDTH;
            position.y = originalPosition.y + PADDING;
            position.width = originalPosition.width - LABELS_WIDTH;
            EditorGUI.PropertyField(position, folderKey, GUIContent.none);

            position.width = originalPosition.width - LABELS_WIDTH - PREVIEW_SIZE_LARGE - PADDING;
            position.y += LINE_HEIGHT + (EditorGUIUtility.isProSkin ? 0f : SPACING);
            EditorGUI.PropertyField(position, folderRecursive, GUIContent.none);
            
            position.y += LINE_HEIGHT + SPACING + (EditorGUIUtility.isProSkin ? SPACING : 0f);
            EditorGUI.PropertyField(position, folderColor, GUIContent.none);

            position.y += LINE_HEIGHT + SPACING + (EditorGUIUtility.isProSkin ? SPACING : 0f);
            EditorGUI.PropertyField(position, rect, GUIContent.none);

            position.y += LINE_HEIGHT + LINE_HEIGHT + LINE_HEIGHT + SPACING;
            EditorGUI.PropertyField(position, icon, GUIContent.none);


            // Preview
            */
            /*
            position.x += position.width + PADDING;
            position.y = originalPosition.y + LINE_HEIGHT + SPACING + 5f;
            position.width = position.height = PREVIEW_SIZE_LARGE;
            GUI.DrawTexture(position, (Texture2D)icon.objectReferenceValue ?? RainbowFoldersEditorUtility.GetDefaultFolderIcon());

            position.y += PREVIEW_SIZE_LARGE - PREVIEW_SIZE_SMALL - 4f;
            position.width = position.height = PREVIEW_SIZE_SMALL;
            GUI.DrawTexture(position, (Texture2D)icon.objectReferenceValue ?? RainbowFoldersEditorUtility.GetDefaultFolderIcon());
            */
            //heights[GetId(property)] = 1000f ;// totalHeight + PADDING;
        }

        public int GetId(SerializedProperty prop)
        {
            return prop.GetHashCode();// ..GetInstanceID();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return totalHeight;
            //if (heights != null && heights.TryGetValue(GetId(property), out var value))
            //{
            //    return value;
            //}
            //return PREVIEW_SIZE_LARGE + LINE_HEIGHT + 4f;
        }
    }
}