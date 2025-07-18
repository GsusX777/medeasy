<!-- „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17 -->

---
description: How to use the workflows
---

## Wie Sie die Workflows einsetzen:

### **1. In Windsurf UI:**
```
Cmd/Ctrl + Shift + P → "Run Workflow" → Workflow auswählen → Parameter eingeben
```

### **2. Im Chat/Terminal:**
```
/workflow test-security feature=Anonymization
/workflow prepare-release version=1.0.0
/workflow new-feature-complete featureName=PrescriptionManagement
```

### **3. Wann welchen Workflow nutzen:**

#### **test-security**
- **Wann:** Nachdem Sie ein sicherheitskritisches Feature implementiert haben
- **Beispiele:**
  - Nach Implementierung der Anonymisierung
  - Nach Erstellung des Audit-Systems
  - Nach Integration der Verschlüsselung
  - Bei jedem Feature mit Patientendaten

#### **prepare-release**
- **Wann:** Vor jedem Release (Major, Minor, Patch)
- **Beispiele:**
  - MVP Release: `/workflow prepare-release version=1.0.0`
  - Bugfix Release: `/workflow prepare-release version=1.0.1`
  - Feature Release: `/workflow prepare-release version=1.1.0`

#### **new-feature-complete**
- **Wann:** Bei jeder neuen Hauptfunktion
- **Beispiele:**
  - `/workflow new-feature-complete featureName=VideoConsultation`
  - `/workflow new-feature-complete featureName=LabResultsIntegration`
  - `/workflow new-feature-complete featureName=MedicationManagement`

### **4. Workflow-Ablauf:**
1. Sie starten den Workflow mit Parametern
2. Windsurf führt jeden Step nacheinander aus
3. Sie können bei jedem Step:
   - Die vorgeschlagene Lösung akzeptieren
   - Modifikationen anfragen
   - Zum nächsten Step springen
4. Am Ende haben Sie eine vollständige Implementierung

### **5. Praktisches Beispiel:**
```
Sie: /workflow new-feature-complete featureName=Prescription

Windsurf Step 1: "Ich erstelle folgende API Endpoints für Prescription:
- POST /api/prescriptions
- GET /api/prescriptions/{id}
- PUT /api/prescriptions/{id}
[Code...]"

Sie: "Sieht gut aus, weiter"

Windsurf Step 2: "Ich erstelle die Prescription Entity mit Verschlüsselung:
[Code...]"

... und so weiter durch alle Steps
```

Die Workflows sind Ihre "Autopilot"-Funktion für komplexe Aufgaben!