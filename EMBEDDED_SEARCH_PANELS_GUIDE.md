# Embedded Search Panels Guide - BauteilPlugin

## ✅ **Neue Panel-basierte Suchfunktion**

Das BauteilPlugin verwendet jetzt **eingebettete Such-Panels** anstatt Modal-Dialoge. Diese Panels werden direkt in die Rhino-Eigenschaftenseite integriert und verhalten sich wie native Rhino-Komponenten.

---

## **🔍 Material Search Panel**

### **Verwendung:**
1. **Bauteil-Eigenschaftenseite** öffnen (rechte Maustaste auf Objekt → Eigenschaften)
2. **Material Layer** auswählen (Zeile in der Schichten-Tabelle anklicken)
3. **"🔍 Material" Button** klicken
4. **Such-Panel** öffnet sich unterhalb der Tabelle
5. **Material eingeben** (z.B. "spanplatte", "mdf", "edelstahl")
6. **Aus der Liste auswählen** (Doppelklick oder "Anwenden" Button)
7. **Panel schließt sich automatisch** und Material wird angewendet

### **Features:**
- ✅ **Live-Suche** während der Eingabe
- ✅ **Intelligente Suche** nach Namen, Typ und Schlüsselwörtern
- ✅ **Tastatur-Navigation** (Pfeiltasten, Enter, Escape)
- ✅ **Automatische Auswahl** des ersten Ergebnisses
- ✅ **Custom Materials** werden automatisch erstellt wenn nicht gefunden
- ✅ **Sofortige Anwendung** der Material-Eigenschaften

---

## **🔍 Edge Search Panel**

### **Verwendung:**
1. **Bauteil-Eigenschaftenseite** öffnen
2. **Kante auswählen** (Zeile in der Kanten-Tabelle anklicken)
3. **"🔍 Edge" Button** klicken
4. **Such-Panel** öffnet sich unterhalb der Tabelle
5. **Suchtyp wählen:**
   - **Kantentyp**: Oben, Unten, Links, Rechts, Vorne, Hinten
   - **Bearbeitungstyp**: Roh, Bekantet, Massiv, Gerundet, etc.
6. **Begriff eingeben** (z.B. "top", "bottom", "roh", "bekantet")
7. **Aus der Liste auswählen** (Doppelklick oder "Anwenden")
8. **Panel schließt sich automatisch** und Kante wird aktualisiert

### **Features:**
- ✅ **Dual-Mode Suche** (Kantentyp + Bearbeitungstyp in einem Panel)
- ✅ **Mehrsprachige Suche** (Deutsch/Englisch)
- ✅ **Radio-Button Auswahl** für Suchtyp
- ✅ **Dynamische Platzhalter** je nach Suchtyp
- ✅ **Sofortige Anwendung** der Standard-Eigenschaften

---

## **🎮 Benutzerfreundlichkeit**

### **Panel-Verhalten:**
- **Einblenden/Ausblenden**: Klick auf Such-Button öffnet/schließt Panel
- **Automatisches Schließen**: Panel schließt sich nach erfolgreicher Auswahl
- **Gegenseitiges Ausblenden**: Nur ein Panel ist gleichzeitig sichtbar
- **Button-Text ändert sich**: "🔍 Material" → "🔍 Hide" wenn Panel offen

### **Tastatur-Shortcuts:**
- **Enter**: Auswahl anwenden
- **Escape**: Panel schließen
- **Pfeiltasten**: Navigation in der Ergebnisliste
- **Tab**: Zwischen Feldern wechseln

### **Visual Feedback:**
- **Status-Labels**: Zeigen Anzahl der gefundenen Ergebnisse
- **Auto-Auswahl**: Erstes Ergebnis wird automatisch markiert
- **Deaktivierte Buttons**: "Anwenden" nur aktiv wenn Auswahl vorhanden

---

## **🚀 Vorteile der neuen Lösung**

### **Nativ integriert:**
- ✅ **Kein separates Fenster** das "verschwinden" kann
- ✅ **Direkt in Rhino-Eigenschaftenseite** eingebettet
- ✅ **Konsistente Benutzerführung** mit anderen Rhino-Panels

### **Bessere Performance:**
- ✅ **Keine Modal-Dialoge** die das Interface blockieren
- ✅ **Sofortiges Feedback** ohne Fenster-Wechsel
- ✅ **Weniger Klicks** für häufige Aufgaben

### **Erweiterte Funktionalität:**
- ✅ **Kontextbezogene Suche** (aktuelles Material als Startwert)
- ✅ **Intelligente Vorschläge** basierend auf Eingabe
- ✅ **Flexible Suchtypen** (Edge Panel mit Radio-Buttons)

---

## **🔧 Technische Details**

### **Neue Klassen:**
- `MaterialSearchPanel`: Eingebettetes Panel für Material-Suche
- `EdgeSearchPanel`: Eingebettetes Panel für Kanten-Suche
- `EdgeSearchResultEventArgs`: Event-Argumente für Kanten-Auswahl

### **Gelöschte Klassen:**
- `MaterialSearchWindow`: Ersetzt durch MaterialSearchPanel
- `EdgeSearchWindow`: Ersetzt durch EdgeSearchPanel

### **Integration:**
- Panels werden als **versteckte Zeilen** in die Tabellen-Layouts eingefügt
- **Event-Handler** für Auswahl und Abbruch implementiert
- **Automatische Panel-Verwaltung** (nur ein Panel gleichzeitig sichtbar)

---

## **🎯 Nächste Schritte**

1. **Rhino neu starten** um das neue Plugin zu laden
2. **Bauteil-Objekt erstellen** oder auswählen
3. **Eigenschaftenseite öffnen** und neue Such-Panels testen
4. **Feedback geben** über Benutzerfreundlichkeit und Performance

Das neue System sollte sich **deutlich nativer und intuitiver** anfühlen als die vorherigen Modal-Dialoge! 