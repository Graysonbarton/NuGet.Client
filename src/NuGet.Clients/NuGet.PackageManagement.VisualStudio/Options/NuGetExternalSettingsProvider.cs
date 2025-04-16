// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Utilities.UnifiedSettings;
using NuGet.Configuration;
using NuGet.VisualStudio;

namespace NuGet.PackageManagement.VisualStudio.Options
{
    public abstract class NuGetExternalSettingsProvider : IExternalSettingsProvider, ISuspendableExternalSettingsProvider
    {
        protected readonly ISettings? _settings;
        private readonly VSSettings? _vsSettings;

        protected NuGetExternalSettingsProvider()
        {
            var componentModel = NuGetUIThreadHelper.JoinableTaskFactory.Run(ServiceLocator.GetComponentModelAsync);
            _settings = componentModel.GetService<ISettings>();
            _vsSettings = _settings as VSSettings;
            AddVsSettingsChangedEventHandler();
            Debug.Assert(_settings != null);
        }

        public event EventHandler<ExternalSettingsChangedEventArgs>? SettingValuesChanged;

        // Events are typically unused, so an empty add and remove accessor block is used to avoid a CS0067 analyzer warning.
        public virtual event EventHandler<EnumSettingChoicesChangedEventArgs> EnumSettingChoicesChanged { add { } remove { } }
        public virtual event EventHandler<DynamicMessageTextChangedEventArgs> DynamicMessageTextChanged { add { } remove { } }
        public virtual event EventHandler ErrorConditionResolved { add { } remove { } }

        public abstract Task<ExternalSettingOperationResult<IReadOnlyList<EnumChoice>>> GetEnumChoicesAsync(string enumSettingMoniker, CancellationToken cancellationToken);
        public abstract Task<ExternalSettingOperationResult<T>> GetValueAsync<T>(string moniker, CancellationToken cancellationToken) where T : notnull;
        public abstract Task<ExternalSettingOperationResult> SetValueAsync<T>(string moniker, T value, CancellationToken cancellationToken) where T : notnull;

        protected virtual void VsSettings_SettingsChanged(object sender, EventArgs e)
        {
            SettingValuesChanged?.Invoke(this, ExternalSettingsChangedEventArgs.SomeOrAll);
        }

        public virtual Task<string> GetMessageTextAsync(string messageId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task OpenBackingStoreAsync(CancellationToken cancellationToken)
        {
            var optionsPageActivator = ServiceLocator.GetComponentModelService<IOptionsPageActivator>();
            optionsPageActivator.ActivatePage(OptionsPage.ConfigurationFiles, closeCallback: null);
            return Task.CompletedTask;
        }

        public Task SuspendAsync(CancellationToken cancellationToken)
        {
            // Stop listening for changes to NuGet.Config.
            if (_vsSettings != null)
            {
                _vsSettings.SettingsChanged -= VsSettings_SettingsChanged;
            }
            return Task.CompletedTask;
        }

        public Task ResumeAsync(CancellationToken cancellationToken)
        {
            AddVsSettingsChangedEventHandler();
            VsSettings_SettingsChanged(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private void AddVsSettingsChangedEventHandler()
        {
            if (_vsSettings != null)
            {
                _vsSettings.SettingsChanged += VsSettings_SettingsChanged;
            }
        }
        public static ExternalSettingOperationResult<T> CreateSettingErrorResult<T>(string errorMessage)
        {
            var failure = new ExternalSettingOperationResult<T>.Failure(
                errorMessage,
                scope: ExternalSettingsErrorScope.SingleSettingOnly,
                isTransient: true);

            return failure;
        }

        public static Task<ExternalSettingOperationResult<T>> ConvertValueOrThrow<T>(object input) where T : notnull
        {
            if (input is T value)
            {
                return Task.FromResult(ExternalSettingOperationResult.SuccessResult(value));
            }
            else
            {
                // Shouldn't happen as these are types we declared in registration.json.
                throw new IncompatibleSettingTypeException(input.GetType().Name, typeof(T).Name);
            }
        }
    }
}
