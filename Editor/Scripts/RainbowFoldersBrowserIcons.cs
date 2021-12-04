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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Borodar.RainbowFolders.Editor.Settings;

using UnityEditor;
using UnityEditor.VersionControl;

using UnityEngine;

#if UNITY_2020_1_OR_NEWER
using ProjectWindowItemCallback = System.Action<string, UnityEngine.Rect, System.Action>;
#else 
using ProjectWindowItemCallback = UnityEditor.EditorApplication.ProjectWindowItemCallback;
#endif


namespace Borodar.RainbowFolders.Editor
{
    /*
    * This script allows you to set custom icons for folders in project browser.
    * Recommended icon sizes - small: 16x16 px, large: 64x64 px;
    */

    [InitializeOnLoad]
    public class RainbowFoldersBrowserIcons
    {
        private const float LARGE_ICON_SIZE = 64f;

        private static Func<bool> _isCollabEnabled;
        private static Func<bool> _isVcsEnabled;
        private static ProjectWindowItemCallback _drawCollabOverlay;
        private static ProjectWindowItemCallback _drawVcsOverlay;
        private static bool _multiSelection;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        static RainbowFoldersBrowserIcons()
        {
            EditorApplication.projectWindowItemOnGUI += ReplaceFolderIcon;
            EditorApplication.projectWindowItemOnGUI += DrawEditIcon;
            EditorApplication.projectWindowItemOnGUI += ShowWelcomeWindow;

            var assembly = typeof(EditorApplication).Assembly;
            InitCollabDelegates(assembly);
            InitVcsDelegates(assembly);
        }

        //---------------------------------------------------------------------
        // Delegates
        //---------------------------------------------------------------------

        private static bool Enabled()
        {
            return RainbowFoldersPreferences.Enabled;
        }

        private static void ReplaceFolderIcon(string guid, Rect rect)
        {
            if (!Enabled())
                return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(path)) return;

            var isSmall = IsIconSmall(ref rect);

            var setting = RainbowFoldersSettings.Instance;
            if (setting == null) return;

            //var folderByPath = ;
            //var folder = folderByPath?.Copy();
            //return;
            var folder = setting.GetFolderByPath(path, true);
            if (folder == null && setting.UseDefault)
            {
                //DrawCustomFolder(guid, rect, setting.DefaultFolder, isSmall);
            }
            else if (folder != null)
            {
                DrawCustomFolder(guid, rect, folder, isSmall);
            }
        }

        private static void DrawEditIcon(string guid, Rect rect)
        {
            if (!Enabled())
                return;

            if ((Event.current.modifiers & RainbowFoldersPreferences.ModifierKey) == EventModifiers.None)
            {
                _multiSelection = false;
                return;
            }

            var isSmall = IsIconSmall(ref rect);
            var isMouseOver = rect.Contains(Event.current.mousePosition);
            _multiSelection = (IsSelected(guid)) ? isMouseOver || _multiSelection : !isMouseOver && _multiSelection;

            // if mouse is not over current folder icon or selected group
            if (!isMouseOver && (!IsSelected(guid) || !_multiSelection)) return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(path)) return;

            var editIcon = RainbowFoldersEditorUtility.GetEditFolderIcon(isSmall);
            DrawCustomIcon(guid, rect, editIcon, isSmall);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                ShowPopupWindow(rect, path);
            }

            EditorApplication.RepaintProjectWindow();
        }

        private static void ShowWelcomeWindow(string guid, Rect rect)
        {
            if (!Enabled())
                return;

            if (EditorPrefs.GetBool(RainbowFoldersWelcome.PREF_KEY))
            {
                // ReSharper disable once DelegateSubtraction
                EditorApplication.projectWindowItemOnGUI -= ShowWelcomeWindow;
                return;
            }

            RainbowFoldersWelcome.ShowWindow();
            EditorPrefs.SetBool(RainbowFoldersWelcome.PREF_KEY, true);

        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void InitVcsDelegates(Assembly assembly)
        {
            try
            {
                _isVcsEnabled = () => Provider.isActive;

                var vcsHookType = assembly.GetType("UnityEditorInternal.VersionControl.ProjectHooks");
                var vcsHook = vcsHookType.GetMethod("OnProjectWindowItem", BindingFlags.Static | BindingFlags.Public);
                _drawVcsOverlay = (ProjectWindowItemCallback)Delegate.CreateDelegate(typeof(ProjectWindowItemCallback), vcsHook);
            }
            catch (SystemException ex)
            {
                if (!(ex is NullReferenceException) && !(ex is ArgumentNullException)) throw;
                _isVcsEnabled = () => false;

#if RAINBOW_FOLDERS_DEVEL
                    Debug.LogException(ex);
#endif
            }
        }

        private static void InitCollabDelegates(Assembly assembly)
        {
            try
            {
                var collabAccessType = assembly.GetType("UnityEditor.Web.CollabAccess");
                var collabAccessInstance = collabAccessType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
                var collabAccessMethod = collabAccessInstance.GetType().GetMethod("IsServiceEnabled", BindingFlags.Instance | BindingFlags.Public);
                _isCollabEnabled = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), collabAccessInstance, collabAccessMethod);

                var collabHookType = assembly.GetType("UnityEditor.Collaboration.CollabProjectHook");
                var collabHook = collabHookType.GetMethod("OnProjectWindowItemIconOverlay", BindingFlags.Static | BindingFlags.Public);
                _drawCollabOverlay = (ProjectWindowItemCallback)Delegate.CreateDelegate(typeof(ProjectWindowItemCallback), collabHook);
            }
            catch (SystemException ex)
            {
                if (!(ex is NullReferenceException) && !(ex is ArgumentNullException)) throw;
                _isCollabEnabled = () => false;

#if RAINBOW_FOLDERS_DEVEL
                    Debug.LogException(ex);
#endif
            }
        }

        private static void ShowPopupWindow(Rect rect, string path)
        {
            var window = RainbowFoldersPopup.GetDraggableWindow();
            var position = GUIUtility.GUIToScreenPoint(rect.position + new Vector2(0, rect.height + 2));

            if (_multiSelection)
            {
                // ReSharper disable once RedundantTypeArgumentsOfMethod
                var paths = Selection.assetGUIDs
                    .Select<string, string>(AssetDatabase.GUIDToAssetPath)
                    .Where(AssetDatabase.IsValidFolder).ToList();

                var index = paths.IndexOf(path);
                window.ShowWithParams(position, paths, index);
            }
            else
            {
                window.ShowWithParams(position, new List<string> { path }, 0);
            }
        }

        private static void DrawCustomFolder(string guid, Rect rect, BasicRainbowFolder rainbowFolder, bool isSmall)
        {

            //if (new UnityEditor.UI.NavigationDrawer().)
            //{
            //
            //}

            foreach (var folder in rainbowFolder.IconLayers)
            {
                var displayRect = rect;

                displayRect.x += rect.width * folder.Rect.x;
                displayRect.y += rect.height * folder.Rect.y;

                displayRect.width = rect.width * folder.Rect.width;
                displayRect.height = rect.height * folder.Rect.height;

                var icon = folder.Icon;
                bool unityIcon = false;
                if (icon == null && !string.IsNullOrWhiteSpace(folder.UnityResourceId))
                {
                    unityIcon = true;
                    icon = (Texture2D)EditorGUIUtility.IconContent(folder.UnityResourceId)?.image;
                }

                if (icon == null)
                {
                    return;
                }

                if (folder.Background)
                {
                    EditorGUI.DrawRect(displayRect, folder.BackgroundColor);
                }

                if (folder.AutoCrop || folder.CropRect.position != Vector2.zero || folder.CropRect.size != Vector2.one)
                {
                    if (unityIcon || !icon.isReadable)
                    {
                        Texture2D copy;
                        if (!ReadableCopy.TryGetValue(icon, out copy))
                        {
                            copy = new Texture2D(icon.width, icon.height, icon.format, icon.mipmapCount, false);
                            copy.LoadRawTextureData(icon.GetRawTextureData());
                            ReadableCopy[icon] = copy;
                        }
                        if (copy != null)
                        {
                            icon = copy;
                        }
                        
                    }

                    Color[] colors;
                    int width;
                    int height;
                    if (folder.AutoCrop)
                    {
                        var cropArea = CroppedArea(icon);
                        width = cropArea.Right - cropArea.Left;
                        height = cropArea.Bottom - cropArea.Top;

                        colors = icon.GetPixels(cropArea.Left, cropArea.Top, width, height);
                    }
                    else
                    {
                        int x = (int)(icon.width * folder.CropRect.x);
                        int y = (int)(icon.height * folder.CropRect.y);
                        width = (int)(icon.width * folder.CropRect.width);
                        height = (int)(icon.height * folder.CropRect.height);

                        colors = icon.GetPixels(x, y, width, height);
                    }

                    
                    icon = new Texture2D(width, height);
                    icon.SetPixels(colors);
                    icon.Apply();
                }
                
                if (folder.Tint)
                {
                    DrawCustomIcon(guid, displayRect, icon, isSmall, folder.Color, folder.ScaleMode);
                }
                else
                {
                    DrawCustomIcon(guid, displayRect, icon, isSmall, scaleMode: folder.ScaleMode);
                }
            }
        }

        public class RectInt
        {
            public int Top;
            public int Bottom;
            public int Left;
            public int Right;
        }

        public class Texture2DComparer : IEqualityComparer<Texture2D>
        {
            public bool Equals(Texture2D x, Texture2D y)
            {
                return x.imageContentsHash == y.imageContentsHash;
            }

            public int GetHashCode(Texture2D obj)
            {
                return obj.imageContentsHash.GetHashCode();
            }
        }

        private static Dictionary<Texture2D, Texture2D> ReadableCopy = new Dictionary<Texture2D, Texture2D>(new Texture2DComparer());
        private static Dictionary<Texture2D, RectInt> CroppedRects = new Dictionary<Texture2D, RectInt>(new Texture2DComparer());
        private static RectInt CroppedArea(Texture2D texture2D)
        {
            var result = new RectInt()
            {
                Top = 0,
                Bottom = texture2D.height,
                Left = 0,
                Right = texture2D.width
            };

            if (CroppedRects.TryGetValue(texture2D, out var cropped))
            {
                return cropped;
            }

            bool foundColor = false;
            for (int Xi = 0; Xi < texture2D.width; Xi++)
            {
                for (int Yi = 0; Yi < texture2D.height; Yi++)
                    if (IsClearPixel(texture2D, Xi, Yi, out foundColor))
                        break;

                if (foundColor)
                {
                    result.Left = Xi;
                    break;
                }
            }

            foundColor = false;
            for (int Xi = texture2D.width - 1; Xi >= 0; Xi--)
            {
                for (int Yi = 0; Yi < texture2D.height; Yi++)
                    if (IsClearPixel(texture2D, Xi, Yi, out foundColor))
                        break;

                if (foundColor)
                {
                    result.Right = Xi;
                    break;
                }
            }

            foundColor = false;
            for (int Yi = 0; Yi < texture2D.height; Yi++)
            {
                for (int Xi = 0; Xi < texture2D.width; Xi++)
                    if (IsClearPixel(texture2D, Xi, Yi, out foundColor))
                        break;

                if (foundColor)
                {
                    result.Top = Yi;
                    break;
                }
            }

            foundColor = false;
            for (int Yi = texture2D.height - 1; Yi >= 0; Yi--)
            {
                for (int Xi = 0; Xi < texture2D.width; Xi++)
                    if (IsClearPixel(texture2D, Xi, Yi, out foundColor))
                        break;

                if (foundColor)
                {
                    result.Bottom = Yi;
                    break;
                }
            }

            CroppedRects[texture2D] = result;
            return result;
        }

        private static bool IsClearPixel(Texture2D texture2D, int x, int y, out bool clear)
        {
            var color = texture2D.GetPixel(x, y);
            clear = color != Color.clear;
            return clear;
        }

        private static void DrawCustomIcon(string guid, Rect rect, Texture texture, bool isSmall, Color? color = null, ScaleMode scaleMode = ScaleMode.StretchToFill)
        {
            if (isSmall)
            {
                Vector2 scale = Vector2.one * 1.5f;
                rect.position -= scale;
                rect.size += scale * 2;
            }


            if (_isCollabEnabled())
            {
                var background = RainbowFoldersEditorUtility.GetCollabBackground(isSmall, EditorGUIUtility.isProSkin);
                GUI.DrawTexture(rect, background);
                GUI.DrawTexture(rect, texture);

#if UNITY_2020_1_OR_NEWER
                _drawCollabOverlay(guid, rect, null);
#else 
                _drawCollabOverlay(guid, rect);
#endif
            }
            else if (_isVcsEnabled())
            {
                var iconRect = (!isSmall) ? rect : new Rect(rect.x + 7, rect.y, rect.width, rect.height);
                GUI.DrawTexture(iconRect, texture);
#if UNITY_2020_1_OR_NEWER
                _drawCollabOverlay(guid, rect, null);
#else 
                _drawCollabOverlay(guid, rect);
#endif
            }
            else
            {
                if (color != null)
                {
                    GUI.DrawTexture(rect, texture, scaleMode, true, 0, color.Value, 0f, 0f);
                }
                else
                {
                    GUI.DrawTexture(rect, texture, scaleMode);
                }
            }
        }

        private static bool IsIconSmall(ref Rect rect)
        {
            var isSmall = rect.width > rect.height;

            if (isSmall)
                rect.width = rect.height;
            else
                rect.height = rect.width;

            return isSmall;
        }

        private static bool IsTreeView(Rect rect)
        {
            return (rect.x - 16) % 14 == 0;
        }

        private static bool IsSelected(string guid)
        {
            return Selection.assetGUIDs.Any() && Selection.assetGUIDs[0] == guid;
        }
    }
}
