🏗️ WAS IST EIN "BUILD"?
Build = Übersetzen von Code in ausführbare Programme

Dein C#-Code ist wie ein Rezept 📝
Der Build-Prozess ist wie das Kochen nach dem Rezept 👨‍🍳
Das Ergebnis ist ein fertiges Programm (.exe, .dll) 🍽️
🏛️ CLEAN ARCHITECTURE - DIE SCHICHTEN
MedEasy hat 4 Schichten (wie ein Hochhaus):

🏢 MedEasy.API          (4. Stock - REST Endpoints)
🏢 MedEasy.Application  (3. Stock - Business Logic)  
🏢 MedEasy.Infrastructure (2. Stock - Datenbank, gRPC)
🏢 MedEasy.Domain       (1. Stock - Grundlagen)
🔗 ABHÄNGIGKEITEN (WER BRAUCHT WEN?)
Abhängigkeiten gehen NACH UNTEN:

API braucht → Application + Infrastructure + Domain
Application braucht → Domain (nur!)
Infrastructure braucht → Domain + Application
Domain braucht → NIEMANDEN (reine Medizinlogik)
🛠️ WARUM VERSCHIEDENE BUILDS?
1. Domain Build (Fundament)
bash
# Selten nötig, da keine externen Abhängigkeiten
dotnet build MedEasy.Domain
Wann: Nur bei Änderungen an Patient, Session, Transcript
Warum: Ist das Fundament - muss stabil sein
2. Infrastructure Build (Technik)
bash
# Das machen wir gerade!
dotnet build MedEasy.Infrastructure
Wann: Bei Datenbank, gRPC, Verschlüsselung
Warum: Hier sind die technischen Probleme (Protobuf, SQLCipher)
3. Application Build (Business Logic)
bash
dotnet build MedEasy.Application
Wann: Bei DTOs, Services, Interfaces
Warum: Die Geschäftslogik für Medizin
4. API Build (Alles zusammen)
bash
dotnet build MedEasy.API
Wann: Final Test - funktioniert alles zusammen?
Warum: Das ist das echte Programm, das läuft
🎯 WARUM BUILDEN WIR INFRASTRUCTURE?
Aktuell haben wir ein gRPC-Problem:

Protobuf-Dateien werden generiert (automatisch)
Infrastructure enthält den gRPC-Client
Fehler entstehen bei der Code-Generierung
Deshalb builden wir Infrastructure isoliert
🚀 DER NORMALE WORKFLOW
In der Praxis:

bash
# 1. Einzelne Schicht testen
dotnet build MedEasy.Infrastructure

# 2. Alles zusammen testen  
dotnet build MedEasy.API

# 3. Programm starten
dotnet run --project MedEasy.API
🔧 UNSER AKTUELLER STAND
Problem: gRPC-Client wird nicht korrekt generiert Lösung: Infrastructure isoliert builden → Fehler finden → API builden

Nächste Schritte:

✅ Infrastructure Build (fast fertig)
🔄 API Build (alles zusammen)
🚀 Backend starten
🧪 Python AI Service testen
🎉 End-to-End Test