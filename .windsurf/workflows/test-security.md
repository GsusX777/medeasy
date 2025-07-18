<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

---
description: Ensures 100% test coverage for security-critical features
---

name: "Test Security Critical Feature"
description: "Ensures 100% test coverage for security-critical features"
version: "1.0"
inputs:
  - name: feature
    description: "Name of the security-critical feature to test"
    required: true
    
steps:
  - name: "Identify security paths"
    prompt: "List all security-critical code paths in {{feature}} that need testing"
    
  - name: "Create authentication tests"
    prompt: "Create unit tests for {{feature}} that verify authentication is required"
    
  - name: "Create authorization tests"
    prompt: "Create tests that verify proper role-based access control for {{feature}}"
    
  - name: "Test encryption"
    prompt: "Create tests verifying that all sensitive data in {{feature}} is encrypted with SQLCipher"
    
  - name: "Test audit trail"
    prompt: "Create tests ensuring all operations in {{feature}} are logged in the audit trail"
    
  - name: "Test anonymization"
    prompt: "If {{feature}} handles patient data, create tests for anonymization with edge cases"
    
  - name: "Test error scenarios"
    prompt: "Create tests for security failure scenarios (invalid input, injection attempts, etc.)"
    
  - name: "Verify coverage"
    prompt: "Run code coverage for {{feature}} and verify 100% coverage on security paths"
    
  - name: "Create integration tests"
    prompt: "Create integration tests for {{feature}} including security aspects"
    
  - name: "Document security tests"
    prompt: "Update test documentation explaining the security test scenarios for {{feature}}"