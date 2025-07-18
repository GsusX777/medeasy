<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

---
description: Complete checklist for releasing a new version
---

name: "Prepare MedEasy Release"
description: "Complete checklist for releasing a new version"
version: "1.0"
inputs:
  - name: version
    description: "Version number (e.g., 1.0.0)"
    required: true
    
steps:
  - name: "Run all tests"
    prompt: "Run complete test suite and verify all tests pass, especially security tests"
    
  - name: "Check code coverage"
    prompt: "Verify code coverage meets requirements: 100% for security features, >80% overall"
    
  - name: "Search for TODOs"
    prompt: "Search codebase for TODO, FIXME, HACK comments and list them"
    
  - name: "Verify documentation"
    prompt: "Check that all documentation is up-to-date: API, database schema, architecture"
    
  - name: "Update version numbers"
    prompt: "Update version to {{version}} in all project files (package.json, .csproj, etc.)"
    
  - name: "Generate changelog"
    prompt: "Generate CHANGELOG.md from git commits since last release"
    
  - name: "Create release notes"
    prompt: "Create release notes in German for version {{version}} highlighting key features and fixes"
    
  - name: "Security audit"
    prompt: "Run security audit: check for hardcoded credentials, unencrypted data, missing audit logs"
    
  - name: "List breaking changes"
    prompt: "Identify and document any breaking changes in this release"
    
  - name: "Update dependencies"
    prompt: "Check for security updates in dependencies and update if needed"
    
  - name: "Build release artifacts"
    prompt: "Build release version and verify all components compile without warnings"
    
  - name: "Create release branch"
    prompt: "Create release branch 'release/{{version}}' and push to repository"