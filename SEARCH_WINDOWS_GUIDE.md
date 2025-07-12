# Search Windows Guide - BauteilPlugin

## Neue Such-Fenster Funktionalität ✨

Das BauteilPlugin wurde jetzt mit **eleganten Such-Fenstern** erweitert, die neben den Material- und Edge-Tabellen erscheinen und eine **AutoComplete-Funktionalität** bieten!

## Funktionen im Überblick

### 🔍 Material Search Window
- **Öffnen**: Klick auf "🔍 Material" Button in der Material Layers Sektion
- **Live-Suche**: Echtzeit-Filterung während der Eingabe
- **30+ Materialien**: Umfassende Datenbank mit intelligenter Suche
- **AutoComplete**: Sofortige Vorschläge beim Tippen
- **Mehrsprachig**: Deutsch und Englisch unterstützt

### 🔍 Edge Search Window
- **Öffnen**: Klick auf "🔍 Edge" Button in der Edge Configuration Sektion
- **Zwei Modi**: 
  - Edge Type Search (Top, Bottom, Front, Back, Left, Right)
  - Processing Type Search (Raw, EdgeBanded, Solid, Rounded, etc.)
- **Intelligente Suche**: Keyword-basierte Filterung
- **Sofortige Anwendung**: Direkte Übernahme in die Tabelle

## Bedienung

### Material Search Window

1. **Öffnen**:
   - Material Layer in der Tabelle auswählen
   - Klick auf "🔍 Material" Button
   - Such-Fenster öffnet sich neben der Tabelle

2. **Suchen**:
   - Materialname oder Keyword eingeben (z.B. "spanplatte", "edelstahl", "mdf")
   - Live-Filterung der Vorschläge
   - Pfeiltasten für Navigation in der Liste

3. **Auswählen**:
   - **Doppelklick** auf Material
   - **Enter-Taste** drücken
   - **"Auswählen"** Button klicken
   - Material wird automatisch in die ausgewählte Schicht übernommen

### Edge Search Window

1. **Öffnen**:
   - Edge in der Kanten-Tabelle auswählen
   - Klick auf "🔍 Edge" Button
   - Auswahl-Dialog erscheint

2. **Typ auswählen**:
   - **"Edge Type"** für Kantenposition (Top, Bottom, etc.)
   - **"Processing Type"** für Bearbeitung (Raw, Banded, etc.)

3. **Suchen und Auswählen**:
   - Suchbegriff eingeben (z.B. "top", "raw", "banded")
   - Gewünschte Option auswählen
   - Automatische Übernahme in die Kanten-Tabelle

## Tastaturkürzel

### Allgemeine Navigation:
- **Tab**: Zwischen Suchfeld und Liste wechseln
- **↑/↓**: Navigation in der Vorschlagsliste
- **Enter**: Aktuell ausgewähltes Element übernehmen
- **Esc**: Such-Fenster schließen

### Spezielle Funktionen:
- **Doppelklick**: Sofortige Auswahl
- **Auto-Fokus**: Suchfeld erhält automatisch den Fokus

## Such-Beispiele

### Material Search:
```
Eingabe: "span"     → Findet: Spanplatte roh, Spanplatte melamin...
Eingabe: "edelstahl"→ Findet: Edelstahl 1mm, Edelstahl 2mm
Eingabe: "transparent"→ Findet: Acrylglas, Sicherheitsglas, Polycarbonat
Eingabe: "holz"     → Findet: Buche Massiv, Eiche Massiv, Bambus...
```

### Edge Type Search:
```
Eingabe: "top"      → Findet: Top / Oben
Eingabe: "bottom"   → Findet: Bottom / Unten
Eingabe: "oben"     → Findet: Top / Oben
```

### Processing Type Search:
```
Eingabe: "raw"      → Findet: Raw / Roh
Eingabe: "band"     → Findet: Edge Banded / Bekantet
Eingabe: "roh"      → Findet: Raw / Roh
```

## Automatische Funktionen

### Material-Auswahl:
- ✅ **Materialname** wird übernommen
- ✅ **Materialtyp** wird automatisch gesetzt
- ✅ **Standarddichte** wird übernommen
- ✅ **Standarddicke** wird als Vorschlag gesetzt
- ✅ **Material-Eigenschaften** werden aktualisiert

### Edge-Auswahl:
- ✅ **Edge Type** oder **Processing Type** wird gesetzt
- ✅ **Standard-Eigenschaften** werden angewendet
- ✅ **Display-Text** wird aktualisiert
- ✅ **Berechnung** wird aktualisiert

## Fenster-Positionierung

### Intelligente Platzierung:
- ✅ **Neben der Haupttabelle** positioniert
- ✅ **Automatische Bildschirm-Erkennung**
- ✅ **Collision-Detection** - springt zur anderen Seite wenn nötig
- ✅ **Immer sichtbar** - immer auf dem Bildschirm
- ✅ **Topmost** - bleibt über anderen Fenstern

## Technische Details

### .NET 7 Kompatibilität:
- ✅ Kompiliert mit **net7.0-windows**
- ✅ Optimiert für **Rhino 8**
- ✅ Kompatibel mit **modernen .NET Features**
- ✅ **Async/Await** ready

### Performance:
- ✅ **Live-Search** ohne Verzögerung
- ✅ **Intelligente Filterung** mit Prioritäten
- ✅ **Memory-efficient** - nur benötigte Daten laden
- ✅ **Schnelle Anzeige** auch bei großen Datenbanken

### Erweiterte Features:
- ✅ **Keyword-basierte Suche**
- ✅ **Mehrsprachige Unterstützung**
- ✅ **Custom Materials** - eigene Materialien eingeben
- ✅ **Auto-Complete** mit sofortigen Vorschlägen

## Vorteile der neuen Lösung

### Für Material-Auswahl:
✅ **Keine Dropdown-Abstürze** mehr
✅ **30+ vordefinierte Materialien**
✅ **Intelligente Keyword-Suche**
✅ **Schnelle Eingabe** durch Tippen
✅ **Live-Vorschläge** während der Eingabe
✅ **Automatische Eigenschaften** werden gesetzt

### Für Edge-Konfiguration:
✅ **Übersichtliche Auswahl** aller Optionen
✅ **Keine Verwechslungen** mehr
✅ **Mehrsprachige Labels**
✅ **Sofortige Anwendung**

### Allgemeine Verbesserungen:
✅ **Moderne UI** mit flotierenden Fenstern
✅ **Bessere UX** durch AutoComplete
✅ **Stabile Performance** - keine Abstürze
✅ **Erweiterbar** - neue Materialien hinzufügbar

## Nächste Schritte

1. **Plugin laden**: `bin\Release\net7.0-windows\BauteilPlugin.rhp`
2. **Objekt auswählen** in Rhino
3. **Properties Panel öffnen** (F3)
4. **Bauteil-Tab** auswählen
5. **Such-Buttons ausprobieren** 🔍

---

**Diese neue Such-Funktionalität macht die Material- und Edge-Auswahl viel effizienter und benutzerfreundlicher!** 🚀 