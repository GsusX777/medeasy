---
description: Implements a complete feature from backend to frontend
---

name: "New Feature End-to-End"
description: "Implements a complete feature from backend to frontend"
version: "1.0"
inputs:
  - name: featureName
    description: "Name of the feature (e.g., PrescriptionManagement)"
    required: true
    
steps:
  - name: "Design API"
    prompt: "Design REST API endpoints for {{featureName}} following existing patterns in docs/api/API_REFERENCE.md"
    
  - name: "Create domain entities"
    prompt: "Create domain entities for {{featureName}} in MedEasy.Domain with proper encryption"
    
  - name: "Implement CQRS"
    prompt: "Create commands and queries for {{featureName}} using MediatR pattern"
    
  - name: "Create repository"
    prompt: "Implement repository pattern for {{featureName}} with SQLCipher encryption"
    
  - name: "Add audit trail"
    prompt: "Integrate audit logging for all {{featureName}} operations"
    
  - name: "Implement API endpoints"
    prompt: "Create API controllers for {{featureName}} with JWT authentication"
    
  - name: "Add gRPC service if needed"
    prompt: "If {{featureName}} needs AI analysis, create Python gRPC service"
    
  - name: "Create Svelte components"
    prompt: "Build frontend components for {{featureName}} in Svelte with Tailwind"
    
  - name: "Connect frontend to API"
    prompt: "Integrate Svelte components with backend API using proper error handling"
    
  - name: "Add anonymization"
    prompt: "If {{featureName}} handles patient data, integrate anonymization pipeline"
    
  - name: "Create unit tests"
    prompt: "Write comprehensive unit tests for {{featureName}} (aim for >80% coverage)"
    
  - name: "Create integration tests"
    prompt: "Write integration tests for complete {{featureName}} workflow"
    
  - name: "Update all documentation"
    prompt: "Update all relevant documentation: API, database schema, user guide"
    
  - name: "Security review"
    prompt: "Perform security review of {{featureName}}: encryption, authentication, audit trail"