//// Copyright (c) .NET Foundation. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//#nullable enable

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.VisualStudio.Utilities.UnifiedSettings;
//using NuGet.Configuration;
//using NuGet.VisualStudio;

//namespace NuGet.PackageManagement.VisualStudio.Options
//{
//    [Guid("9519DBF7-7CB3-4C32-BE1B-1A510FC9CE77")]
//    public class PackageSourcesExternalSettingsProviderService : IExternalSettingsProvider, IExternalArrayItemCommandsProvider
//    {
//        private const string MonikerConfigurationFiles = "packageSources";

//        private readonly ISettings? _settings;
//        private readonly VSSettings? _vsSettings;

//        public event EventHandler<ExternalSettingsChangedEventArgs> SettingValuesChanged;
//        public event EventHandler<EnumSettingChoicesChangedEventArgs> EnumSettingChoicesChanged;
//        public event EventHandler<DynamicMessageTextChangedEventArgs> DynamicMessageTextChanged;
//        public event EventHandler ErrorConditionResolved;

//        public Task<IReadOnlyList<IArrayItemCommand>> GetArrayItemCommandsAsync(string arraySettingMoniker, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ExternalSettingOperationResult<IReadOnlyList<EnumChoice>>> GetEnumChoicesAsync(string enumSettingMoniker, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<string> GetMessageTextAsync(string messageId, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ExternalSettingOperationResult<T>> GetValueAsync<T>(string moniker, CancellationToken cancellationToken) where T : notnull
//        {
//            throw new NotImplementedException();
//        }

//        public Task OpenBackingStoreAsync(CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ExternalSettingOperationResult> SetValueAsync<T>(string moniker, T value, CancellationToken cancellationToken) where T : notnull
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
