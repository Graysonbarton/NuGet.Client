# Copilot Instructions – NuGet.Client

These guidelines help GitHub Copilot generate suggestions that are consistent with NuGet.Client's standards and existing codebase.

## 1. Coding Style Guidelines

- Use the following coding guidelines: https://github.com/NuGet/NuGet.Client/blob/dev/docs/coding-guidelines.md

## 2. Test writing and Review/Suggestion Guidelines

When reviewing or suggesting improvements for test methods, Copilot should:

- **Encourage Reusability**:
  - Recommend abstracting repetitive test setup logic into reusable helper methods or utilities.
  - Suggest leveraging existing utilities in `test/TestUtilities/` for file operations, project creation, and command execution.
  - Folders to look into for reusable utilities:
    - test/TestUtilities/Test.Utility/Commands
    - test/TestUtilities/Test.Utility/DependencyResolver
    - test/TestUtilities/Test.Utility/MockResponses
    - test/TestUtilities/Test.Utility/PackageManagement
    - test/TestUtilities/Test.Utility/PlatformXunitAttributes
    - test/TestUtilities/Test.Utility/ProjectManagement
    - test/TestUtilities/Test.Utility/Protocol
    - test/TestUtilities/Test.Utility/Signing
    - test/TestUtilities/Test.Utility/SimpleTestSetup
    - test/TestUtilities/Test.Utility/SourceRepository
    - test/TestUtilities/Test.Utility/Telemetry
    - test/TestUtilities/Test.Utility/Threading
