Rhino Professional Bauteil-Plugin



Projektziel



Erstellung eines Rhino-Plugins zur professionellen Definition und Verwaltung von Werkstücken mit komplexen Materialschichten, erweiterten Materialeigenschaften, Kantenbildern und intuitiver Benutzeroberfläche. Ziel ist ein robustes und flexibles Datenmodell, das eine spätere Integration mit ERP-Systemen (z.B. Borm) und CNC-Maschinen ermöglicht.



Technologie-Stack



RhinoCommon (C#, .NET)



Eto.Forms für UI-Erstellung (Dockable Panels)



CustomUserData / UserDictionary zur Speicherung der Basisinformationen



Optional externe Datenhaltung via SQLite/JSON zur Erweiterung komplexer Metadaten



Entwicklungsphasen (Initial)



Phase 1: Core-Datenmodell



Erstelle eine professionelle Klassenstruktur in RhinoCommon zur Definition von Bauteilen:



Bauteilklasse:



ID, Name



Liste von Schichten



Liste von Kantenbildern



Schichtklasse:



Schichtname



Material (Name, Typ)



Dicke (mm)



Laufrichtung (X/Y/Ausrichtung frei definierbar)



Dichte (kg/m³)



Lackmenge (ml/m², optional)



Kantenbildklasse:



Kante (oben/unten/vorne/hinten)



Bearbeitungstyp (roh, bekantet, massiv, etc.)



Implementiere diese Klassen so, dass sie direkt in Rhino-Objekten (CustomUserData/UserDictionary) gespeichert werden.



Phase 2: Professionelle Benutzeroberfläche (UI)



Erstelle ein Dockable Panel mit Eto.Forms:



Bauteileditor:



Bauteil hinzufügen, bearbeiten, löschen



Schichten dynamisch hinzufügen, editieren, sortieren



Kantenbilder intuitiv definieren (Dropdown, visuelle Auswahl)



UI Inspiration:



Nutze die Klarheit und intuitive Bedienung des Unreal Engine Inspectors als Vorbild



Struktur und Logik ähnlich wie VisualARQ (Eigenschaften in Reitern)



Phase 3: Visualisierung \& Berechnungen



Visuelle Darstellung der definierten Schichten in Rhino:



Farbige Schichten im Schnitt (pro Materialtyp)



Automatische Berechnung:



Gesamtgewicht basierend auf Dichte und Volumen der Schichten



Lackverbrauch basierend auf Flächenberechnung



Technische Anforderungen



Code sauber strukturieren nach Best Practices:



Modular und wartbar



Umfangreiche und klar verständliche Kommentare



Effizientes Datenmodell:



Schneller Zugriff, optimale Performance auch bei vielen Objekten



Ergebnis-Dokumentation (durch AI Coding Agent)



Entwicklungsprozess detailliert dokumentieren



Dokumentation intelligenter Lösungsansätze und angewandter Best Practices



Übersicht über implementierte Funktionen mit Beispielen



Empfehlungen zur Erweiterbarkeit und zukünftigen Integration (ERP \& CNC)



Nächste Schritte (zukünftige Phasen)



Integration ERP-Schnittstelle (Borm)



CNC-Exportformate (Maestro/XCS)



Erweiterung der UI nach Feedback



Erweiterte Datenbankintegration (optional)



Zielergebnis Phase 1



Ein funktionales, skalierbares Plugin mit professionellem Datenmodell, intuitiver Bedienoberfläche und zuverlässigen Berechnungen, als Grundlage für die zukünftigen Erweiterungen und Integrationen.





