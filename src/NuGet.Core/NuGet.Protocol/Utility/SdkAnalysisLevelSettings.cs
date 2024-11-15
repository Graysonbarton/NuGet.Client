// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Protocol.Utility
{
    /// <summary>
    /// Utility class for managing SDK analysis level settings.
    /// </summary>
    public static class SdkAnalysisLevelSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether new errors and warnings are enabled for the current SDK analysis level.
        /// The default value is true
        /// </summary>
        public static bool EnableNewErrorsAndWarnings = true;
    }
}
