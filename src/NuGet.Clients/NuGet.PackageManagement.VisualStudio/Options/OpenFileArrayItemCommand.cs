// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities.UnifiedSettings;
using Resx = NuGet.PackageManagement.VisualStudio.Strings;

namespace NuGet.PackageManagement.VisualStudio.Options
{
    internal class OpenFileArrayItemCommand : IArrayItemCommand
    {
        public const string FILE_PATH = "filePath";

        public string Title => Resx.VSOptions_Button_Open;

        public string Description => "";

        public int DefaultActionPriority => 1;

        public void Invoke(IDictionary<string, object> arrayItemContent)
        {
            var path = arrayItemContent[FILE_PATH] as string;
            VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, path);
        }

        public Task<bool> IsEnabledAsync(IDictionary<string, object> arrayItemContent, CancellationToken cancellationToken)
        {
            if (arrayItemContent != null && arrayItemContent.ContainsKey(FILE_PATH))
            {
                if (arrayItemContent[FILE_PATH] is string path)
                {
                    return Task.FromResult(!string.IsNullOrWhiteSpace(path) && File.Exists(path));
                }
            }

            return Task.FromResult(false);
        }
    }
}
