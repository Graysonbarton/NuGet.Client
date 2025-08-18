// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using NuGet.Common;

namespace NuGet.PackageManagement.VisualStudio.Migrate
{
    internal class UpgradeLogger : IDisposable
    {
        //private const string DescriptionString = "Description";
        private const string ExcludedPackagesString = "ExcludedPackages";
        private const string IncludedPackagesString = "IncludedPackages";
        private const string IssuesString = "Issues";

        //private const string IssueString = "Issue";
        private const string NameString = "Name";
        //private const string VersionString = "Version";
        private const string NuGetUpgradeReportString = "NuGetUpgradeReport";
        //private const string PackageString = "Package";
        private const string ProjectsString = "Projects";
        private const string ProjectString = "Project";
        private const string PropertiesString = "Properties";
        private const string PropertyString = "Property";
        private const string ValueString = "Value";
        private const string BackupPathString = "BackupPath";

        private const string XsltManifestResourceName = "NuGet.PackageManagement.VisualStudio.Migrate.UpgradeReport.xslt";

        private readonly ConcurrentDictionary<string, XmlElement> _projectElements = new ConcurrentDictionary<string, XmlElement>();

        private readonly XmlDocument _xmlDocument;

        private readonly XmlElement _projectsElement;
        private readonly XmlElement _propertiesElement;

        private readonly string _htmlFilePath;

        internal UpgradeLogger(string reportName, string solutionReportPath)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                throw new ArgumentException(Strings.Argument_Cannot_Be_Null_Or_Empty, nameof(reportName));
            }

            _xmlDocument = new XmlDocument { PreserveWhitespace = true };
            _xmlDocument.LoadXml($"<?xml version='1.0' encoding='UTF-16'?>\r\n<{NuGetUpgradeReportString}>\r\n</{NuGetUpgradeReportString}>");

            var upgradeReportElement = _xmlDocument.DocumentElement;
            Debug.Assert(upgradeReportElement != null, "_upgradeReportElement != null");

            upgradeReportElement.SetAttribute(NameString, reportName);

            if (string.IsNullOrEmpty(solutionReportPath) || !Directory.Exists(solutionReportPath))
            {
                //TODO: UpgradeLogger_BackupPathMustBeValid
                throw new ArgumentException("Path not found.", nameof(solutionReportPath));
            }
            upgradeReportElement.SetAttribute(BackupPathString, solutionReportPath);

            _htmlFilePath = $@"{solutionReportPath}\NuGetUpgradeLog.html";

            _propertiesElement = _xmlDocument.CreateElement(PropertiesString);
            upgradeReportElement.AppendChild(_propertiesElement);

            _projectsElement = _xmlDocument.CreateElement(ProjectsString);
            upgradeReportElement.AppendChild(_projectsElement);
        }

        internal void SetProperty(string propertyName, string propertyValue)
        {
            var propertyElement = _xmlDocument.CreateElement(PropertyString);
            propertyElement.SetAttribute(NameString, propertyName);
            propertyElement.SetAttribute(ValueString, propertyValue);
            _propertiesElement.AppendChild(propertyElement);
        }


        internal void RegisterProject(string projectName, IList<PackagingLogMessage> issues, bool included)
        {
            //var upgradeReportElement = _xmlDocument.DocumentElement;
            //packageElement.SetAttribute(VersionString, version);

            var packagesElement = GetProjectElement(projectName).SelectSingleNode(included ? IncludedPackagesString : ExcludedPackagesString);
            Debug.Assert(packagesElement != null, "packagesElement != null");

            if (issues.Count > 0)
            {
                var issuesElement = GetProjectElement(projectName).SelectSingleNode(IssuesString);
                Debug.Assert(issuesElement != null, "issuesElement != null");
                //issuesElement.AppendChild(issuePackageElement);

                //foreach (var issue in issues)
                //{
                //    var issueElement = _xmlDocument.CreateElement(IssueString);
                //    issueElement.SetAttribute(DescriptionString, issue.Message);

                //    issuePackageElement.AppendChild(issueElement);
                //}
            }
        }

        internal string GetHtmlFilePath()
        {
            return _htmlFilePath;
        }

        internal void Flush()
        {

            using (var xsltStream = typeof(UpgradeLogger).Assembly.GetManifestResourceStream(XsltManifestResourceName))
            {
                Debug.Assert(xsltStream != null, $"Resource {XsltManifestResourceName} could not be loaded.");

                using (var xmlReader = XmlReader.Create(xsltStream))
                using (var writer = new XmlTextWriter(_htmlFilePath, null))
                {
                    var transform = new XslCompiledTransform();
                    transform.Load(xmlReader);
                    transform.Transform(_xmlDocument, writer);
                }
            }
        }

        private XmlElement GetProjectElement(string projectName)
        {
            return _projectElements.GetOrAdd(projectName, name =>
            {
                var projectElement = _xmlDocument.CreateElement(ProjectString);
                projectElement.SetAttribute("Name", projectName);
                projectElement.AppendChild(_xmlDocument.CreateElement(IssuesString));
                projectElement.AppendChild(_xmlDocument.CreateElement(IncludedPackagesString));
                projectElement.AppendChild(_xmlDocument.CreateElement(ExcludedPackagesString));
                _projectsElement.AppendChild(projectElement);
                return projectElement;
            });
        }

        public void Dispose()
        {
            Flush();
        }

        internal enum ErrorLevel
        {
            Information,
            Warning,
            Error
        }
    }
}
