// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xunit;
using NuGet.Protocol.Events;
using NuGet.Configuration;
using NuGet.Protocol.Plugins;

namespace NuGet.Protocol.Tests
{
    public class ServiceIndexResourceV3Tests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            var serviceIndex = CreateServiceIndex();
            var expectedJson = serviceIndex.ToString();
            var expectedRequestTime = DateTime.UtcNow;
            var resource = new ServiceIndexResourceV3(serviceIndex, expectedRequestTime);

            Assert.Equal(expectedJson, resource.Json);
            Assert.Equal(expectedRequestTime, resource.RequestTime);
            Assert.Equal(1, resource.Entries.Count);
            Assert.Equal("a", resource.Entries[0].Type);
            Assert.Equal("http://unit.test/b", resource.Entries[0].Uri.ToString());
        }

        [Fact]
        public void GetServiceEntries_InvokesDiagnosticEventForSourceResources()
        {
            // Arrange
            int eventInvokeCount = 0;
            List<ProtocolDiagnosticServiceIndexEntryEvent> capturedEvents = new List<ProtocolDiagnosticServiceIndexEntryEvent>();

            ProtocolDiagnostics.ServiceIndexEntryEvent += (pdEvent) =>
            {
                eventInvokeCount++;
                capturedEvents.Add(pdEvent);
            };

            var source = $"https://test/index.json";
            var content = CreateServiceIndexWithFourResourceTypesTwoHTTP();
            var packageSource = new Configuration.PackageSource(source);
            packageSource.AllowInsecureConnections = true;

            var expectedRequestTime = DateTime.UtcNow;
            var resource = new ServiceIndexResourceV3(content, expectedRequestTime, packageSource);

            // Act
            var result = resource.GetServiceEntries(ServiceTypes.SearchQueryService);

            // Assert
            int httpResourceCapture = 0;

            foreach (var serviceIndexEvent in capturedEvents)
            {
                Assert.Equal(serviceIndexEvent.Source, source);
                httpResourceCapture += serviceIndexEvent.HttpsSourceHasHttpResource ? 1 : 0;
            }

            Assert.Equal(2, httpResourceCapture);
            Assert.Equal(2, eventInvokeCount);
        }

        private static JObject CreateServiceIndexWithFourResourceTypesTwoHTTP()
        {
            var obj = new JObject
            {
                { "version", "3.1.0-beta" },
                { "resources", new JArray
                    {
                        new JObject
                        {
                            { "@type", "SearchQueryService/Versioned" },
                            { "@id", "http://tempuri.org/A/5.0.0/2" },
                            { "clientVersion", "5.0.0" },
                        },
                        new JObject
                        {
                            { "@type", "SearchQueryService/Versioned" },
                            { "@id", "http://tempuri.org/A/5.0.0/1" },
                            { "clientVersion", "5.0.0" },
                        },
                        new JObject
                        {
                            { "@type", "SearchQueryService/Versioned" },
                            { "@id", "https://test" },
                            { "clientVersion", "4.0.0" },
                        },
                        new JObject
                        {
                            { "@type", "SearchQueryService/Versioned" },
                            { "@id", "https://test" },
                            { "clientVersion", "5.0.0" },
                        },
                    }
                }
            };

            return obj;
        }

        [Fact]
        public void GetServiceEntries_WithHttpResourceEndPoint_ThrowsException()
        {
            // Arrange
            var serviceIndex = CreateServiceIndexWithHttpResources();
            PackageSource source = new PackageSource("https://test");
            var resource = new ServiceIndexResourceV3(serviceIndex, DateTime.Now, source);
            Protocol.Utility.SdkAnalysisLevelUtility.EnableNewErrorsAndWarnings = true;

            // Act & Assert
            ProtocolException exception;
            exception = Assert.Throws<ProtocolException>(() => resource.GetServiceEntries("SearchQueryService"));
            Assert.Contains("non-HTTPS", exception.Message);
            exception = Assert.Throws<ProtocolException>(() => resource.GetServiceEntries("RegistrationsBaseUrl"));
            Assert.Contains("non-HTTPS", exception.Message);
            exception = Assert.Throws<ProtocolException>(() => resource.GetServiceEntries("LegacyGallery"));
            Assert.Contains("non-HTTPS", exception.Message);
        }

        [Fact]
        public void GetServiceEntries_WithHttpResourceEndPointAndUnsupportedSdkAnalysisLevel_DoesNotThrowException()
        {
            // Arrange
            var serviceIndex = CreateServiceIndexWithHttpResources();
            PackageSource source = new PackageSource("https://test");
            var resource = new ServiceIndexResourceV3(serviceIndex, DateTime.Now, source);
            Protocol.Utility.SdkAnalysisLevelUtility.EnableNewErrorsAndWarnings = false;

            // Act & Assert
            Assert.Equal(1, resource.GetServiceEntries("SearchQueryService").Count);
            Assert.Equal(1, resource.GetServiceEntries("RegistrationsBaseUrl").Count);
            Assert.Equal(1, resource.GetServiceEntries("LegacyGallery").Count);
        }

        [Fact]
        public void GetServiceEntries_WithHttpResourceEndPointAndAllowInsecureConnections_Succeeds()
        {
            // Arrange
            var serviceIndex = CreateServiceIndexWithHttpResources();
            PackageSource source = new PackageSource("https://unit.test");
            source.AllowInsecureConnections = true;
            var resource = new ServiceIndexResourceV3(serviceIndex, DateTime.Now, source);

            // Act
            var searchRec = resource.GetServiceEntries("SearchQueryService").FirstOrDefault().Uri.ToString();
            var regRec = resource.GetServiceEntries("RegistrationsBaseUrl").FirstOrDefault().Uri.ToString();
            var legacyRec = resource.GetServiceEntries("LegacyGallery").FirstOrDefault().Uri.ToString();

            // Assert
            Assert.Equal(searchRec, "http://search/");
            Assert.Equal(regRec, "http://reg/");
            Assert.Equal(legacyRec, "http://legacy/");
        }

        [Fact]
        public void GetServiceEntries_RequestsHttpsResourceInServiceIndexContainingOtherHttpResourcesWithoutAllowInsecureConnections_Succeeds()
        {
            // Arrange
            var serviceIndex = CreateServiceIndexWithHttpResources();
            var resource = new ServiceIndexResourceV3(serviceIndex, DateTime.Now);

            // Act
            var vulnRec = resource.GetServiceEntries("VulnerabilityInfo/6.7.0").FirstOrDefault().Uri.ToString();

            // Assert
            Assert.Equal(vulnRec, "https://vulnerability/");
        }

        private static JObject CreateServiceIndex()
        {
            return new JObject
            {
                { "version", "1.2.3" },
                { "resources", new JArray
                    {
                        new JObject
                        {
                            { "@type", "a" },
                            { "@id", "http://unit.test/b" }
                        }
                    }
                }
            };
        }

        private static JObject CreateServiceIndexWithHttpResources()
        {
            return new JObject
            {
                { "version", "1.2.3" },
                { "resources", new JArray
                    {
                        new JObject
                        {
                            { "@type", "SearchQueryService" },
                            { "@id", "http://search" }
                        },
                        new JObject
                        {
                            { "@type", "RegistrationsBaseUrl" },
                            { "@id", "http://reg" }
                        },
                        new JObject
                        {
                            { "@type", "LegacyGallery" },
                            { "@id", "http://legacy" }
                        },
                        new JObject
                        {
                            { "@type", "VulnerabilityInfo/6.7.0" },
                            { "@id", "https://vulnerability" }
                        }
                    }
                }
            };
        }
    }
}
