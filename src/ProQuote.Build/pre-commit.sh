#!/bin/sh

set -e

# Ensure we are at the repository root
REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
cd "$REPO_ROOT"

echo "Running dotnet format --verify-no-changes..."
if ! dotnet format --verify-no-changes; then
  echo ""
  echo "dotnet format reported issues."
  echo "Please run 'dotnet format' locally to fix them, stage the changes, and try committing again."
  exit 1
fi

echo "Running dotnet test..."
if ! dotnet test ProQuote.sln --no-build; then
  echo ""
  echo "dotnet test failed. Please fix the failing tests before committing."
  exit 1
fi

echo "Pre-commit checks (lint + tests) passed."
exit 0

