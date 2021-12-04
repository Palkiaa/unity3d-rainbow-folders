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

namespace Borodar.RainbowFolders
{
    [Serializable]
    public class BaseRainbowFolder
    {
        public bool Tint = false;
        public Color Color = Color.white;
        public bool Background = false;
        public Color BackgroundColor = Color.clear;
        public Rect Rect;
        public string UnityResourceId;
        public Texture2D Icon;
        public ScaleMode ScaleMode;
        public bool AutoCrop;
        public Rect CropRect = new Rect(0, 0, 1, 1);

        public BaseRainbowFolder Copy()
        {
            return new BaseRainbowFolder()
            {
                Tint = Tint,
                Color = Color,
                Background = Background,
                BackgroundColor = BackgroundColor,
                Rect = Rect,
                CropRect = CropRect,
                UnityResourceId = UnityResourceId,
                Icon = Icon,
            };
        }

        public void LoadFolder(BaseRainbowFolder baseFolder)
        {
            Tint = baseFolder.Tint;
            Color = baseFolder.Color;
            Background = baseFolder.Background;
            BackgroundColor = baseFolder.BackgroundColor;
            Rect = baseFolder.Rect;
            CropRect = baseFolder.CropRect;
            UnityResourceId = baseFolder.UnityResourceId;
            Icon = baseFolder.Icon;
        }

        public void Reset()
        {
            Rect = new Rect(0, 0, 1, 1);
            CropRect = new Rect(0, 0, 1, 1);
            Tint = false;
            Background = false;
            BackgroundColor = Color.clear;
            Color = Color.white;
            UnityResourceId = string.Empty;
            Icon = null;
        }
    }
}