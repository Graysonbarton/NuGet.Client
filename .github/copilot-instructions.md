# Copilot Instructions – NuGet.Client

These guidelines help GitHub Copilot generate suggestions that are consistent with NuGet.Client's standards and existing codebase.

## 1. Coding Style Guidelines

- Use the following coding guidelines: https://github.com/NuGet/NuGet.Client/blob/dev/docs/coding-guidelines.md

## 2. Reuse of Test Utilities
When writing tests, always check for existing helpers before introducing new logic.

- **Utility Directory**:
  - Copilot should search recursively through all sub folders under `test/TestUtilities/`.

- **Avoid Duplication**:
  - Reuse existing test infrastructure such as setup helpers, package creation utilities, mock server implementations, and common assertion logic.