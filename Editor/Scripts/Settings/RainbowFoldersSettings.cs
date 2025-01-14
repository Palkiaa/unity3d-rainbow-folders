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
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEditor;
using KeyType = Borodar.RainbowFolders.Editor.Settings.RainbowFolder.KeyType;

namespace Borodar.RainbowFolders.Editor.Settings
{
    [HelpURL(AssetInfo.HELP_URL)]
    [CreateAssetMenu(fileName = "RainbowFoldersSettings", menuName = "RainbowFolders/Settings")]
    public class RainbowFoldersSettings : ScriptableObject
    {
        private static string RELATIVE_PATH => $"{RainbowFoldersPreferences.Path}/RainbowFoldersSettings.asset";

        public bool UseDefault;
        public BasicRainbowFolder DefaultFolder;
        public List<RainbowFolder> Folders;

        public RainbowFoldersSettings()
        {
            DefaultFolder = new BasicRainbowFolder();
            Folders = new List<RainbowFolder>();
        }

        //---------------------------------------------------------------------
        // Instance
        //---------------------------------------------------------------------

        private static RainbowFoldersSettings _instance;

        [SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
        public static RainbowFoldersSettings Instance
        {
            get
            {
                if (_instance == null)
#if RAINBOW_FOLDERS_DEVEL
                        _instance = AssetDatabase.LoadAssetAtPath<RainbowFoldersSettings>(DEVEL_PATH);
#else
                    _instance = RainbowFoldersEditorUtility.LoadSetting<RainbowFoldersSettings>(RELATIVE_PATH);
#endif

                return _instance;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        /// <summary>  
        /// Searches for a folder config that has the same type and key values.
        /// Returns the first occurrence within the settings, if found; null otherwise.
        /// </summary>  
        public RainbowFolder GetFolder(RainbowFolder match)
        {
            if (IsNullOrEmpty(Folders) || match == null) return null;
            return Folders.Find(x => x.Type == match.Type && x.Name == match.Name);
        }

        /// <summary>
        /// Searches for a folder config that should be applied for the specified path (regardless of
        /// the key type). Returns the last occurrence within the settings, if found; null otherwise.
        /// </summary>  
        public RainbowFolder GetFolderByPath(string folderPath, bool allowRecursive = false)
        {


            if (IsNullOrEmpty(Folders)) return null;

            for (var index = Folders.Count - 1; index >= 0; index--)
            {
                var folder = Folders[index];
                switch (folder.Type)
                {
                    case KeyType.Name:
                        var folderName = Path.GetFileName(folderPath);
                        if (allowRecursive && folder.IsRecursive)
                        {
                            if (folder.Keys.Any(key => folderPath.Contains(string.Format("/{0}", key)))) return folder;
                        }
                        else
                        {
                            if (folder.Keys.Any(key => key.Equals(folderName))) return folder;
                        }
                        break;
                    case KeyType.Path:
                        if (allowRecursive && folder.IsRecursive)
                        {
                            if (folder.Keys.Any(key => folderPath.StartsWith(key))) return folder;
                        }
                        else
                        {
                            if (folder.Keys.Any(key => key.Equals(folderPath))) return folder;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }

        /// <summary>  
        /// Searches for a folder config that has the same type and key, and updates
        /// its other fields with provided value, if found; creates new folder config otherwise.
        /// </summary>  
        public void UpdateFolder(RainbowFolder match, RainbowFolder value)
        {
            Undo.RecordObject(this, "Modify Rainbow Folder Settings");

            var existingFolder = GetFolder(match);
            if (existingFolder != null)
            {
                if (value.HasAtLeastOneIcon())
                {
                    existingFolder.CopyFrom(value);
                }
                else
                {
                    RemoveAll(match);
                }
            }
            else
            {
                if (value.HasAtLeastOneIcon()) AddFolder(value);
            }

            EditorUtility.SetDirty(this);
        }

        public void AddFolder(RainbowFolder value)
        {
            Folders.Add(new RainbowFolder(value));
        }

        public void RemoveAll(RainbowFolder match)
        {
            if (match == null) return;
            Undo.RecordObject(this, "Modify Rainbow Folder Settings");
            Folders.RemoveAll(x => x.Type == match.Type && x.Name == match.Name);
            EditorUtility.SetDirty(this);
        }

        public void RemoveAllByPath(string path)
        {
            var match = GetFolderByPath(path);
            RemoveAll(match);
        }

        public void ChangeFolderIcons(RainbowFolder value)
        {
            Undo.RecordObject(this, "Modify Rainbow Folder Settings");

            var folder = Folders.SingleOrDefault(x => x.Type == value.Type && x.Name == value.Name);
            if (folder == null)
            {
                AddFolder(new RainbowFolder(value));
            }
            else
            {
                //folder.LoadFolder(value);
            }

            EditorUtility.SetDirty(this);
        }

        public void ChangeFolderIconsByPath(string path, BaseRainbowFolder baseFolder)
        {
            //ChangeFolderIcons(new RainbowFolder(KeyType.Path, path, baseFolder));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static bool IsNullOrEmpty(ICollection collection)
        {
            return collection == null || (collection.Count == 0);
        }
    }
}