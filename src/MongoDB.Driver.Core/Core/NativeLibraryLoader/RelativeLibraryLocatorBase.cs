﻿/* Copyright 2019-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.IO;
using System.Reflection;
using MongoDB.Shared;

namespace MongoDB.Driver.Core.NativeLibraryLoader
{
    internal abstract class RelativeLibraryLocatorBase : ILibraryLocator
    {
        // public properties
        public virtual bool IsX32ModeSupported => false;

        // public methods
        public virtual string GetBaseAssemblyUri() => typeof(RelativeLibraryLocatorBase).GetTypeInfo().Assembly.CodeBase;

        public string GetLibraryAbsolutePath(OperatingSystemPlatform currentPlatform)
        {
            var relativePath = GetLibraryRelativePath(currentPlatform);
            return GetAbsolutePath(relativePath);
        }

        public virtual string GetLibraryBaseAssemblyPath()
        {
            var baseAssemblyPathUri = GetBaseAssemblyUri();
            var uri = new Uri(baseAssemblyPathUri);
            return Uri.UnescapeDataString(uri.AbsolutePath);
        }

        public virtual string GetLibraryBasePath()
        {
            var absoluteAssemblyPath = GetLibraryBaseAssemblyPath();
            return Path.GetDirectoryName(absoluteAssemblyPath);
        }

        public abstract string GetLibraryRelativePath(OperatingSystemPlatform currentPlatform);

        // private methods
        private string GetAbsolutePath(string relativePath)
        {
            var libraryBasePath = GetLibraryBasePath();
            var libraryName = Path.GetFileName(relativePath);

            var absolutePathsToCheck = new[]
            {
                Path.Combine(libraryBasePath, libraryName),  // look in the current assembly folder
                Path.Combine(libraryBasePath, @"..\..\", relativePath),
                Path.Combine(libraryBasePath, relativePath)
            };

            foreach (var absolutePath in absolutePathsToCheck)
            {
                if (File.Exists(absolutePath))
                {
                    return absolutePath;
                }
            }

            throw new FileNotFoundException($"Could not find library {libraryName}. Checked {string.Join(";", absolutePathsToCheck)}.");
        }
    }
}
