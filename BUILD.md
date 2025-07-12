# Build-Anleitung - Professional Bauteil-Plugin

## Voraussetzungen

- **Visual Studio 2022** oder **Visual Studio Code** mit C# Extension
- **.NET Framework 4.8** oder höher
- **Rhino 8** (oder Rhino 7 mit entsprechender Anpassung der NuGet-Pakete)
- **NuGet Package Manager**

## Build-Schritte

### 1. Projekt vorbereiten

```bash
# Navigiere zum Projektverzeichnis
cd PartPluginforRhino

# Stelle sicher, dass alle NuGet-Pakete installiert sind
dotnet restore
```

### 2. Kompilieren

#### Mit Visual Studio:
1. Öffne `BauteilPlugin.csproj` in Visual Studio
2. Wähle Build-Konfiguration (Debug oder Release)
3. Baue das Projekt: `Build` → `Build Solution` (Strg+Shift+B)

#### Mit Kommandozeile:
```bash
# Debug-Build
dotnet build --configuration Debug

# Release-Build  
dotnet build --configuration Release
```

### 3. Installation

Das Plugin wird automatisch durch das PostBuild-Event in das Rhino-Plugin-Verzeichnis kopiert:
```
%APPDATA%\McNeel\Rhinoceros\packages\8.0\BauteilPlugin
```

### 4. Rhino-Plugin laden

1. Starte Rhino
2. Führe den Befehl `PluginManager` aus
3. Klicke auf `Install...`
4. Wähle die `BauteilPlugin.dll` aus dem Build-Verzeichnis
5. Alternativ: Das Plugin wird automatisch geladen, wenn es im packages-Verzeichnis liegt

## Verfügbare Befehle

Nach der Installation sind folgende Befehle verfügbar:

- `BauteilEditor` - Öffnet/schließt das Bauteil-Editor-Panel
- `CreateBauteil` / `BauteilErstellen` - Erstellt Bauteil aus ausgewählter Geometrie
- `ApplyBauteil` / `BauteilAnwenden` - Wendet Bauteil-Eigenschaften auf Geometrie an

## Entwicklung

### Projektstruktur

```
PartPluginforRhino/
├── BauteilPlugin.cs           # Haupt-Plugin-Klasse
├── Models/                    # Datenmodelle
│   ├── Bauteil.cs            # Bauteil-Klasse
│   ├── Material.cs           # Material-Klasse
│   ├── MaterialSchicht.cs    # Materialschicht-Klasse
│   └── Kantenbild.cs         # Kantenbild-Klasse
├── UI/                       # Benutzeroberfläche
│   └── BauteilEditorPanel.cs # Hauptpanel
├── Commands/                 # Rhino-Befehle
│   └── BauteilEditorCommand.cs
└── BauteilPlugin.csproj      # Projektdatei
```

### Debug-Modus

1. Setze Breakpoints in Visual Studio
2. Starte Rhino über Visual Studio Debug (F5)
3. Verwende das Plugin in Rhino

### Erweiterungen

Das Plugin ist modular aufgebaut und kann leicht erweitert werden:

- **Neue Materialtypen**: Erweitere `MaterialType` enum
- **Neue Kantentypen**: Erweitere `EdgeProcessingType` enum
- **Neue UI-Komponenten**: Füge neue Panels in `UI/` hinzu
- **Neue Befehle**: Füge neue Commands in `Commands/` hinzu

## Problembehandlung

### Häufige Probleme

1. **Plugin wird nicht geladen**
   - Überprüfe Rhino-Version (muss Rhino 8 sein)
   - Überprüfe .NET Framework Version
   - Schaue in Rhino-Befehlszeile nach Fehlermeldungen

2. **Kompilierungsfehler**
   - Stelle sicher, dass alle NuGet-Pakete installiert sind
   - Überprüfe .NET Framework Target (net48)

3. **UI wird nicht angezeigt**
   - Stelle sicher, dass Eto.Forms korrekt referenziert ist
   - Überprüfe Panel-Registrierung in `OnLoad()`

### Logs

Plugin-Logs werden in der Rhino-Befehlszeile angezeigt. Verwende:
```csharp
RhinoApp.WriteLine("Debug-Nachricht");
```

## Deployment

Für die Verteilung:

1. Kompiliere im Release-Modus
2. Kopiere `BauteilPlugin.dll` und alle abhängigen Dateien
3. Erstelle Installer oder Zip-Archiv
4. Dokumentiere Installation für Endbenutzer

## Support

Bei Problemen:
1. Überprüfe diese Anleitung
2. Schaue in die Rhino-Befehlszeile nach Fehlermeldungen
3. Überprüfe Plugin-Logs
4. Kontaktiere den Entwickler mit detaillierter Fehlerbeschreibung 