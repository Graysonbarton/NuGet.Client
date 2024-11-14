// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Protocol.Utility
{
    public static class LogHttpServiceEndPoint
    {
        public delegate void LogHttpServiceEndPointDelegate(string message);

        private static LogHttpServiceEndPointDelegate LogHttpServiceEntry;

        public static LogHttpServiceEndPointDelegate HttpServiceEndPointLoggerDelegate
        {
            get
            {
                if (LogHttpServiceEntry == null)
                {
                    LogHttpServiceEntry = (message) => { throw new Plugins.ProtocolException(message); };
                }

                return LogHttpServiceEntry;
            }
            set
            {
                LogHttpServiceEntry = value;
            }
        }

        public static void Log(string message)
        {
            LogHttpServiceEndPoint.HttpServiceEndPointLoggerDelegate(message);
        }
    }
}
