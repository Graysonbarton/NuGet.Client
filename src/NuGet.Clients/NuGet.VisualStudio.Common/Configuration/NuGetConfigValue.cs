// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.VisualStudio.Common.Configuration
{
    public class NuGetConfigValue<T>
    {
        private T _value;

        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                IsInitialized = true;
                IsDefault = false;
            }
        }

        public bool IsDefault { get; private set; }

        public bool IsInitialized { get; private set; }

        public NuGetConfigValue(T value, bool isDefault)
        {
            _value = value;
            IsDefault = isDefault;
        }

        #region Equality operators
        public static bool operator ==(NuGetConfigValue<T> left, NuGetConfigValue<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NuGetConfigValue<T> left, NuGetConfigValue<T> right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj is NuGetConfigValue<T> other)
            {
                return Equals(Value, other.Value);
            }

            throw new System.NotImplementedException();
        }
        #endregion

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
