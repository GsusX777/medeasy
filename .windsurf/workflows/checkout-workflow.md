<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

---
description: Hier ist der **exakte Workflow** für deine Windsurf + GitHub Entwicklung mit aktivierten Branch Protection Rules
---

# Windsurf + GitHub Workflow für MedEasy

Hier ist der **exakte Workflow** für deine Windsurf + GitHub Entwicklung mit aktivierten Branch Protection Rules:

## 🔄 Standard Development Workflow

### **1. Feature Branch erstellen (Windsurf)**

```bash
# In Windsurf Terminal oder über UI
git checkout -b feature/anonymization-engine
# oder
git checkout -b bugfix/whisper-memory-leak
# oder  
git checkout -b docs/api-documentation
```

**Naming Convention:**
- `feature/` - Neue Features
- `bugfix/` - Bugfixes
- `docs/` - Dokumentation
- `refactor/` - Code-Refactoring
- `test/` - Tests

### **2. Code entwickeln mit Windsurf AI**

```bash
# Windsurf Prompt
windsurf: "Implement anonymization engine following Clean Architecture, 
ensure 100% PII detection for Swiss medical data"

# Code wird generiert, du passt an, testest lokal
```

### **3. Lokale Tests ausführen**

```bash
# Backend Tests
dotnet test tests/unit/backend/
dotnet test tests/anonymization/ --collect:"XPlat Code Coverage"

# Frontend Tests  
cd src/frontend && npm test

# Python AI Tests
cd src/ai-services && pytest tests/ -v
```

### **4. Commit in Feature Branch**

```bash
# In Windsurf - entweder Terminal oder Git UI
git add .
git commit -m "feat: implement PII anonymization engine

- Add Swiss-specific name detection
- Implement confidence scoring
- Add audit trail logging  
- 100% test coverage for critical paths

Refs: #123"
```

### **5. Feature Branch zu GitHub pushen**

```bash
# Ersten Push vom Feature Branch
git push -u origin feature/anonymization-engine

# Weitere Pushes
git push
```

## 🔀 Pull Request Workflow

### **6. Pull Request erstellen**

**Option A: Direkt in Windsurf**
- Windsurf zeigt automatisch "Create Pull Request" an
- Klick darauf öffnet GitHub PR-Dialog

**Option B: Auf GitHub**
- Gehe zu deinem Repository
- GitHub zeigt automatisch "Compare & pull request" Button
- Klick darauf

### **7. Pull Request Template**

```markdown
## 🎯 Änderungen
- [x] PII Anonymization Engine implementiert
- [x] Swiss-specific patterns hinzugefügt
- [x] Audit Trail integriert
- [x] Tests mit 100% Coverage

## 🧪 Testing
- [ ] Unit Tests: ✅ Alle grün
- [ ] Integration Tests: ✅ Bestanden  
- [ ] Anonymization Tests: ✅ 100% Coverage
- [ ] Manual Testing: ✅ Getestet mit Testdaten

## 🔒 Security Check
- [ ] Keine Patientendaten in Code
- [ ] Anonymisierung nicht deaktivierbar
- [ ] Audit Trail vollständig
- [ ] Encryption korrekt implementiert

## 📋 Checklist
- [ ] Clean Architecture befolgt
- [ ] Medizinische Begriffe verwendet (DDD)
- [ ] Dokumentation aktualisiert
- [ ] Breaking Changes dokumentiert
```

## ⚙️ Branch Protection in Action

### **Was passiert mit deinen Rules:**

1. **"Require a pull request before merging"**
   - ✅ Du **kannst NICHT** direkt in `main` pushen
   - ✅ Du **musst** Feature Branch → PR → Merge verwenden

2. **"Do not allow bypassing"** 
   - ✅ Auch du als Owner kannst die Rules nicht umgehen

3. **"Do not allow force pushes"**
   - ✅ `git push --force` wird blockiert

4. **"Do not allow deletion"**
   - ✅ `main` Branch kann nicht gelöscht werden

### **8. Code Review & Merge**

**Automatic Checks (wenn CI/CD läuft):**
```yaml
✅ Build successful
✅ Tests passed  
✅ Security scan clean
✅ No merge conflicts
```

**Review Process:**
- Du reviewst deinen eigenen Code (Einzelentwickler)
- Merge über GitHub **"Merge pull request"** Button
- **Option wählen:**
  - **"Create a merge commit"** - Empfohlen für Features
  - **"Squash and merge"** - Für kleine Bugfixes
  - **"Rebase and merge"** - Für saubere Historie

### **9. Cleanup nach Merge**

```bash
# Nach erfolgreichem Merge
git checkout main
git pull origin main
git branch -d feature/anonymization-engine  # Lokal löschen
git push origin --delete feature/anonymization-engine  # Remote löschen
```

## 🚀 Windsurf-spezifische Optimierungen

### **Auto-Documentation Update**

```bash
# Nach jedem Merge automatisch
windsurf: "Update CURRENT_STATE.md with completed anonymization feature"
windsurf: "Update API_REFERENCE.md with new endpoints"
windsurf: "Update dependency graph"
```

### **AI-Assisted Branch Naming**

```bash
windsurf: "Suggest branch name for implementing Swiss German transcription support"
# Antwort: feature/swiss-german-transcription
```

### **Pre-Commit AI Review**

```bash
windsurf: "Review this code for Clean Architecture compliance and security issues before commit"
```

## ⚠️ Troubleshooting

### **Problem: Branch Protection blockiert Push**

```bash
# Falsch - wird blockiert
git push origin main

# Richtig - verwende Feature Branch
git checkout -b hotfix/critical-bug
git push origin hotfix/critical-bug
# Dann PR erstellen
```

### **Problem: Merge Conflicts**

```bash
# Main in Feature Branch mergen
git checkout feature/my-feature
git merge main
# Conflicts lösen
git commit -m "resolve merge conflicts"
git push
```

### **Problem: CI/CD fehlt**

Deine Branch Protection funktioniert, aber **ohne CI/CD werden keine Status Checks ausgeführt**. Du solltest hinzufügen:

```yaml
# .github/workflows/pr-checks.yml
name: PR Checks
on:
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - name: Run Tests
        run: dotnet test
      - name: Security Scan
        run: echo "Security scan placeholder"
```

## 📋 Täglicher Windsurf Workflow

### **Morgens**
```bash
windsurf: "Update CURRENT_STATE.md with yesterday's progress"
git checkout main && git pull  # Latest changes
```

### **Feature entwickeln**
```bash
git checkout -b feature/new-thing
windsurf: "Implement X following our Clean Architecture"
# Entwickeln, testen, commiten
git push -u origin feature/new-thing
```

### **Abends**
```bash
# PR erstellen über Windsurf oder GitHub
# Nach Merge:
windsurf: "Update documentation with today's changes"
```

## ✅ Quick Reference

| Aktion | Kommando |
|--------|----------|
| Feature Branch | `git checkout -b feature/name` |
| Push Branch | `git push -u origin feature/name` |
| PR erstellen | Windsurf UI oder GitHub |
| Nach Merge | `git checkout main && git pull` |
| Branch cleanup | `git branch -d feature/name` |

**Wichtig:** Mit deinen Branch Protection Rules **musst** du immer über PRs arbeiten. Direktes Pushen in `main` ist blockiert! 🔒