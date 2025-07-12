# Material Search Guide - BauteilPlugin

## Übersicht

Das BauteilPlugin wurde mit einer neuen **intelligenten Materialsuche** ausgestattet. Statt komplizierter Dropdown-Menüs können Sie jetzt einfach den Materialnamen direkt in die Textfelder eingeben.

## Neue Funktionalität

### 1. Suchbare Materialfelder

**Anstatt Dropdown-Menüs** haben Sie jetzt **Textfelder** für:
- **Material**: Eingabe des Materialnamens
- **Grain Direction**: Eingabe von "X" oder "Y"
- **Edge Type**: Eingabe von "Top", "Bottom", "Front", "Back", "Left", "Right"
- **Processing Type**: Eingabe von "Raw", "EdgeBanded", "Solid", etc.

### 2. Automatische Materialerkennung

Das Plugin enthält eine **umfassende Materialdatenbank** mit über 30 Materialien:

#### Holzmaterialien:
- Birke Multiplex
- Buche Massiv
- Eiche Massiv
- Fichte Leimholz
- Kiefer Massiv
- Bambus Platte

#### Plattenwerkstoffe:
- Spanplatte roh
- Spanplatte melaminbeschichtet (weiß, grau, schwarz)
- MDF roh
- MDF lackiert (weiß, schwarz)
- OSB Platte

#### Metallmaterialien:
- Aluminium (1mm, 2mm)
- Stahl verzinkt (1mm, 2mm)
- Edelstahl (1mm, 2mm)

#### Kunststoffe:
- Acrylglas (3mm, 6mm)
- Polycarbonat (4mm, 6mm)

#### Glas:
- Sicherheitsglas (4mm, 6mm, 8mm)

#### Verbundwerkstoffe:
- Carbon
- Glasfaser

#### Sonstige:
- Kork Platte
- Linoleum

### 3. Intelligente Suchfunktion

**Wie es funktioniert:**

1. **Exakte Übereinstimmung**: Geben Sie den genauen Materialnamen ein (z.B. "Birke Multiplex")
2. **Keyword-Suche**: Das Plugin sucht auch nach Stichwörtern:
   - "spanplatte" → findet alle Spanplatten
   - "edelstahl" → findet Edelstahl-Varianten
   - "transparent" → findet Glas und Kunststoffe
3. **Automatische Eigenschaften**: Bei gefundenen Materialien werden automatisch gesetzt:
   - Materialtyp
   - Standarddichte
   - Standarddicke

### 4. Suchvorschläge

**Wenn ein Material nicht gefunden wird:**
- Das Plugin zeigt **Vorschläge** in der Rhino-Konsole an
- Sie können **eigene Materialnamen** eingeben
- Diese werden als **benutzerdefinierte Materialien** gespeichert

## Anwendungsbeispiele

### Material Layer bearbeiten:

1. **Klicken Sie in das "Material"-Feld**
2. **Geben Sie ein:**
   - `spanplatte` → Findet alle Spanplatten
   - `mdf weiß` → Findet "MDF lackiert weiß"
   - `birke` → Findet "Birke Multiplex"
3. **Das Plugin setzt automatisch:**
   - Materialtyp
   - Standarddichte
   - Weitere Eigenschaften

### Grain Direction eingeben:

- `X` für X-Richtung
- `Y` für Y-Richtung

### Edge Type eingeben:

- `Top` oder `Oben`
- `Bottom` oder `Unten`
- `Front` oder `Vorne`
- `Back` oder `Hinten`
- `Left` oder `Links`
- `Right` oder `Rechts`

### Processing Type eingeben:

- `Raw` oder `Roh`
- `EdgeBanded` oder `Bekantet`
- `Solid` oder `Massiv`
- `Rounded` oder `Gerundet`
- `Chamfered` oder `Gefast`

## Materialdatenbank erweitern

Die Materialdatenbank wird in der Datei `Materials.json` gespeichert:

**Speicherort:**
- `%USERPROFILE%\Documents\BauteilPlugin\Materials.json`

**Struktur:**
```json
{
  "materials": [
    {
      "name": "Ihr Material",
      "type": "Wood",
      "density": 650,
      "thickness": 18,
      "keywords": ["keyword1", "keyword2", "keyword3"]
    }
  ]
}
```

**Materialtypen:**
- `Wood` - Holz
- `Plywood` - Sperrholz
- `MDF` - MDF
- `Chipboard` - Spanplatte
- `Metal` - Metall
- `Plastic` - Kunststoff
- `Glass` - Glas
- `Composite` - Verbundwerkstoff
- `Other` - Sonstige

## Tipps für optimale Nutzung

1. **Verwenden Sie Stichwörter**: Geben Sie nur Teile des Materialnamens ein
2. **Konsole beachten**: Schauen Sie in die Rhino-Konsole für Vorschläge
3. **Deutsch und Englisch**: Beide Sprachen werden unterstützt
4. **Eigene Materialien**: Einfach den gewünschten Namen eingeben

## Vorteile der neuen Lösung

✅ **Keine Dropdown-Abstürze mehr**
✅ **Schnelle Eingabe** durch Tippen
✅ **Intelligente Suche** mit Vorschlägen
✅ **Umfangreiche Datenbank** mit 30+ Materialien
✅ **Erweiterbar** durch eigene Materials.json
✅ **Mehrsprachig** (Deutsch/Englisch)
✅ **Automatische Eigenschaften** werden gesetzt

## Fehlerbehebung

**Problem**: Material wird nicht gefunden
- **Lösung**: Schauen Sie in die Rhino-Konsole für Vorschläge

**Problem**: Eigenschaften werden nicht gesetzt
- **Lösung**: Verwenden Sie die exakten Materialnamen aus der Datenbank

**Problem**: Grain Direction funktioniert nicht
- **Lösung**: Verwenden Sie nur "X" oder "Y"

**Problem**: Materials.json fehlt
- **Lösung**: Das Plugin erstellt automatisch eine Standard-Datenbank

---

**Diese neue Lösung ist viel stabiler und benutzerfreundlicher als die vorherigen Dropdown-Menüs!** 