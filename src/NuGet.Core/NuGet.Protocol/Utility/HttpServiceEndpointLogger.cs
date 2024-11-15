// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Protocol.Utility
{
    /// <summary>
    /// Provides logging functionality for HTTP service endpoints.
    /// </summary>
    public static class HttpServiceEndpointLogger
    {
        /// <summary>
        /// Delegate for logging messages related to HTTP service endpoints.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public delegate void HttpServiceEndpointLogDelegate(string message);

        private static HttpServiceEndpointLogDelegate DefaultLogDelegate =
            message => throw new Plugins.ProtocolException(message);

        /// <summary>
        /// Gets or sets the delegate used for logging HTTP service endpoint messages.
        /// If not set, it defaults to throwing a <see cref="Plugins.ProtocolException"/>.
        /// </summary>
        public static HttpServiceEndpointLogDelegate LogDelegate { get; set; } = DefaultLogDelegate;

        /// <summary>
        /// Logs a message using the configured <see cref="LogDelegate"/>.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Log(string message)
        {
            LogDelegate(message);
        }
    }
}
