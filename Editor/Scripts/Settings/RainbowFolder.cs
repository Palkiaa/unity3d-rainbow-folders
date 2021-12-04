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

using UnityEngine;

namespace Borodar.RainbowFolders.Editor.Settings
{
    [Serializable]
    public class BasicRainbowFolder
    {
        public List<BaseRainbowFolder> IconLayers;

        public BasicRainbowFolder()
        {
            IconLayers = new List<BaseRainbowFolder>();
        }

        public void Load(BasicRainbowFolder basicRainbowFolder)
        {
            IconLayers.Clear();
            foreach (var item in basicRainbowFolder.IconLayers)
            {
                IconLayers.Add(item.Copy());
            }
        }

        public BasicRainbowFolder Copy()
        {
            return new BasicRainbowFolder()
            {
                IconLayers = IconLayers.Select(s => s.Copy()).ToList()
            };
        }
    }

    [Serializable]
    public class RainbowFolder : BasicRainbowFolder// : BaseRainbowFolder
    {
        public KeyType Type;
        public string Name;
        public string[] Keys => Name.Split(';');
        public bool IsRecursive;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public RainbowFolder(RainbowFolder value)
        {
            Type = value.Type;
            Name = value.Name;
            IsRecursive = value.IsRecursive;
            IconLayers = new List<BaseRainbowFolder>();
            foreach (var item in value.IconLayers)
            {
                IconLayers.Add(item.Copy());
            }
        }

        public RainbowFolder(KeyType type, string key)
        {
            Type = type;
            Name = key;
            IconLayers = new List<BaseRainbowFolder>();
        }

        public RainbowFolder(KeyType type, string key, RainbowFolder baseFolder)
        {
            Type = type;
            Name = key;
            IsRecursive = baseFolder.IsRecursive;
            IconLayers = new List<BaseRainbowFolder>();
            Load(baseFolder);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void CopyFrom(RainbowFolder target)
        {
            Type = target.Type;
            Name = target.Name;
            IsRecursive = target.IsRecursive;
            Load(target);
        }

        public bool HasAtLeastOneIcon()
        {
            return IconLayers.Any(s => s.Icon != null);// || !string.IsNullOrEmpty(UnityResourceId);
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum KeyType
        {
            Name,
            Path
        }
    }
}