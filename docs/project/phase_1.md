### **Phase 1: Projekt-Grundstruktur mit Auto-Docs (Tag 4-5)**

#### 1.1 Repository-Struktur
- [ ] Ordnerstruktur anlegen:
  ```
  medeasy/
  ├── .windsurf/          # AI-Kontext & Regeln
  │   ├── context/        # Auto-generierte Kontexte
  │   ├── templates/      # Prompt-Templates
  │   └── hooks/          # Auto-Doc Hooks
  ├── docs/               # IMMER AKTUELL
  │   ├── CURRENT_STATE.md
  │   ├── QUICK_START.md
  │   └── architecture/
  ├── src/
  └── tests/              # Nur kritische Tests
  ```

#### 1.2 Git & CI Setup
- [ ] .gitignore mit Windsurf-Caches
- [ ] Pre-commit: Docs-Update
- [ ] GitHub Actions:
  - [ ] Build-Check
  - [ ] Kritische Tests
  - [ ] Doc-Generation

#### 1.3 Basis-Konfiguration
- [ ] Monorepo-Setup (alle Sprachen)
- [ ] Gemeinsame Config-Files
- [ ] Environment-Variables
- [ ] **Windsurf-Workspace** Config:
  ```json
  {
    "ai.contextPaths": [
      "docs/CURRENT_STATE.md",
      "docs/DB_SCHEMA.md"
    ],
    "ai.autoUpdateDocs": true
  }
  ```

#### 1.4 Fallback-Dokumentation
- [ ] `docs/FALLBACK_STRATEGIES.md`:
  - Anonymisierung (80% Threshold)
  - KI-Provider-Chain (OpenAI→Claude→Gemini)
  - Audio-Quality-Degradation
  - Offline-Mode