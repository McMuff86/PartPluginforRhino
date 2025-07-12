using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;
using BauteilPlugin.Models;
using BauteilMaterial = BauteilPlugin.Models.Material;

namespace BauteilPlugin.UI
{
    /// <summary>
    /// Property page that appears in Rhino's Properties panel for objects with Bauteil data
    /// </summary>
    public class BauteilPropertyPage : ObjectPropertiesPage
    {
        private Bauteil _bauteil;
        private RhinoObject _rhinoObject;
        private Splitter _mainSplitter;
        private bool _isUpdating = false; // Prevent recursive updates

        // UI Controls - now editable
        private TextBox _nameTextBox;
        private TextBox _descriptionTextBox;
        private Label _totalThicknessLabel;
        private Label _totalWeightLabel;
        private Label _paintConsumptionLabel;
        private GridView _schichtenGridView;
        private GridView _kantenGridView;
        private Button _addSchichtButton;
        private Button _removeSchichtButton;
        private Button _editSchichtButton;
        private Button _addKanteButton;
        private Button _removeKanteButton;
        private Button _editKanteButton;
        private Button _removeButton;

        public override string EnglishPageTitle => "Bauteil";
        public override string LocalPageTitle => "Bauteil";

        public override object PageControl => _mainSplitter;

        public BauteilPropertyPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Create main splitter for resizable sections
            _mainSplitter = new Splitter
            {
                Orientation = Orientation.Vertical,
                Position = 200, // Smaller initial position for top panel
                FixedPanel = SplitterFixedPanel.None // Allow both panels to be resized
            };

            // Top panel for basic info and calculations
            var topPanel = CreateTopPanel();
            _mainSplitter.Panel1 = topPanel;

            // Bottom panel for layers and edges
            var bottomPanel = CreateBottomPanel();
            _mainSplitter.Panel2 = bottomPanel;
        }

        private Control CreateTopPanel()
        {
            var mainPanel = new Panel();
            var mainLayout = new TableLayout();
            mainLayout.Spacing = new Size(5, 5);
            mainLayout.Padding = new Padding(5);

            // Bauteil Information GroupBox
            var infoGroupBox = new GroupBox();
            infoGroupBox.Text = "Bauteil Information";
            infoGroupBox.Padding = new Padding(5);
            
            var infoLayout = new TableLayout();
            infoLayout.Spacing = new Size(5, 5);
            
            _nameTextBox = new TextBox();
            _nameTextBox.TextChanged += NameTextBox_TextChanged;
            _descriptionTextBox = new TextBox();
            _descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            
            infoLayout.Rows.Add(new TableRow(
                new Label { Text = "Name:", Width = 80 },
                new TableCell(_nameTextBox, true)
            ));
            infoLayout.Rows.Add(new TableRow(
                new Label { Text = "Description:", Width = 80 },
                new TableCell(_descriptionTextBox, true)
            ));
            
            infoGroupBox.Content = infoLayout;

            // Calculations GroupBox
            var calcGroupBox = new GroupBox();
            calcGroupBox.Text = "Calculations";
            calcGroupBox.Padding = new Padding(5);
            
            var calcLayout = new TableLayout();
            calcLayout.Spacing = new Size(3, 3);
            
            _totalThicknessLabel = new Label { Text = "Total Thickness: - mm" };
            _totalWeightLabel = new Label { Text = "Weight: - kg/m²" };
            _paintConsumptionLabel = new Label { Text = "Paint: - ml/m²" };
            
            calcLayout.Rows.Add(new TableRow(_totalThicknessLabel));
            calcLayout.Rows.Add(new TableRow(_totalWeightLabel));
            calcLayout.Rows.Add(new TableRow(_paintConsumptionLabel));
            
            calcGroupBox.Content = calcLayout;

            // Action Buttons Panel
            var buttonPanel = new StackLayout();
            buttonPanel.Orientation = Orientation.Horizontal;
            buttonPanel.HorizontalContentAlignment = HorizontalAlignment.Left;
            buttonPanel.Spacing = 5;
            
            var applyButton = new Button { 
                Text = "Apply", 
                Size = new Size(50, 18) // Compact button
            };
            applyButton.Click += ApplyButton_Click;
            
            _removeButton = new Button { 
                Text = "Remove Data", 
                Size = new Size(80, 18) // Wider for full text visibility
            };
            _removeButton.Click += RemoveButton_Click;
            
            buttonPanel.Items.Add(applyButton);
            buttonPanel.Items.Add(_removeButton);

            // Main layout setup
            mainLayout.Rows.Add(new TableRow(infoGroupBox) { ScaleHeight = false });
            mainLayout.Rows.Add(new TableRow(calcGroupBox) { ScaleHeight = false });
            mainLayout.Rows.Add(new TableRow(buttonPanel) { ScaleHeight = false });
            mainLayout.Rows.Add(new TableRow() { ScaleHeight = true }); // Spacer

            mainPanel.Content = mainLayout;
            return mainPanel;
        }

        private Control CreateBottomPanel()
        {
            // Create horizontal splitter for layers and edges
            var horizontalSplitter = new Splitter
            {
                Orientation = Orientation.Horizontal,
                Position = 250, // Initial position
                FixedPanel = SplitterFixedPanel.None // Allow both panels to be resized
            };

            // Left panel for material layers
            var layersPanel = CreateLayersPanel();
            horizontalSplitter.Panel1 = layersPanel;

            // Right panel for edge configuration
            var edgesPanel = CreateEdgesPanel();
            horizontalSplitter.Panel2 = edgesPanel;

            return horizontalSplitter;
        }

        private Control CreateLayersPanel()
        {
            var panel = new Panel();
            
            // Create GroupBox for Material Layers
            var groupBox = new GroupBox();
            groupBox.Text = "Material Layers";
            groupBox.Padding = new Padding(5);
            
            // Create table layout with explicit size control
            var tableLayout = new TableLayout();
            tableLayout.Spacing = new Size(5, 5);
            
            // Create editable GridView for material layers
            _schichtenGridView = new GridView();
            _schichtenGridView.ShowHeader = true;
            _schichtenGridView.AllowMultipleSelection = false;
            _schichtenGridView.GridLines = GridLines.Both;
            _schichtenGridView.AllowColumnReordering = true;
            _schichtenGridView.Border = BorderType.Line;
            
            // Add editable columns with proportional widths
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Layer",
                DataCell = new TextBoxCell("SchichtName"),
                Width = 60
            });
            
            var materialDropDown = new ComboBoxCell("MaterialType")
            {
                DataStore = Enum.GetValues(typeof(MaterialType)).Cast<MaterialType>()
                    .Select(m => new { Key = m, Value = GetMaterialTypeDisplayName(m) }).ToList()
            };
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Material",
                DataCell = materialDropDown,
                Width = 100
            });
            
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Thickness",
                DataCell = new TextBoxCell("DickeText"),
                Width = 50
            });

            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Density",
                DataCell = new TextBoxCell("DichteText"),
                Width = 50
            });

            var grainDropDown = new ComboBoxCell("Laufrichtung")
            {
                DataStore = Enum.GetValues(typeof(GrainDirection)).Cast<GrainDirection>()
                    .Select(g => new { Key = g, Value = g.ToString() }).ToList()
            };
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Grain",
                DataCell = grainDropDown,
                Width = 40
            });
            
            _schichtenGridView.CellEdited += SchichtenGridView_CellEdited;
            
            // Create button panel with very small buttons
            var buttonPanel = new StackLayout();
            buttonPanel.Orientation = Orientation.Horizontal;
            buttonPanel.HorizontalContentAlignment = HorizontalAlignment.Left;
            buttonPanel.Spacing = 3;
            
            _addSchichtButton = new Button { Text = "Add", Size = new Size(30, 16) };
            _addSchichtButton.Click += AddSchichtButton_Click;
            _removeSchichtButton = new Button { Text = "Remove", Size = new Size(45, 16) };
            _removeSchichtButton.Click += RemoveSchichtButton_Click;
            _editSchichtButton = new Button { Text = "Edit", Size = new Size(30, 16) };
            _editSchichtButton.Click += EditSchichtButton_Click;
            
            buttonPanel.Items.Add(_addSchichtButton);
            buttonPanel.Items.Add(_removeSchichtButton);
            buttonPanel.Items.Add(_editSchichtButton);
            
            // Setup table layout: table fills most space, buttons at bottom
            tableLayout.Rows.Add(new TableRow(_schichtenGridView) { ScaleHeight = true });
            tableLayout.Rows.Add(new TableRow(buttonPanel) { ScaleHeight = false });
            
            groupBox.Content = tableLayout;
            panel.Content = groupBox;
            
            return panel;
        }

        private Control CreateEdgesPanel()
        {
            var panel = new Panel();
            
            // Create GroupBox for Edge Configuration
            var groupBox = new GroupBox();
            groupBox.Text = "Edge Configuration";
            groupBox.Padding = new Padding(5);
            
            // Create table layout with explicit size control
            var tableLayout = new TableLayout();
            tableLayout.Spacing = new Size(5, 5);
            
            // Create editable GridView for edges
            _kantenGridView = new GridView();
            _kantenGridView.ShowHeader = true;
            _kantenGridView.AllowMultipleSelection = false;
            _kantenGridView.GridLines = GridLines.Both;
            _kantenGridView.AllowColumnReordering = true;
            _kantenGridView.Border = BorderType.Line;
            
            var edgeTypeDropDown = new ComboBoxCell("KantenTyp")
            {
                DataStore = Enum.GetValues(typeof(EdgeType)).Cast<EdgeType>()
                    .Select(e => new { Key = e, Value = GetEdgeTypeDisplayName(e) }).ToList()
            };
            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Edge",
                DataCell = edgeTypeDropDown,
                Width = 80
            });
            
            var processingDropDown = new ComboBoxCell("BearbeitungsTyp")
            {
                DataStore = Enum.GetValues(typeof(EdgeProcessingType)).Cast<EdgeProcessingType>()
                    .Select(p => new { Key = p, Value = GetProcessingTypeDisplayName(p) }).ToList()
            };
            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Processing",
                DataCell = processingDropDown,
                Width = 100
            });

            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Thickness",
                DataCell = new TextBoxCell("DickeText"),
                Width = 50
            });

            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Visible",
                DataCell = new CheckBoxCell("IsVisible"),
                Width = 40
            });
            
            _kantenGridView.CellEdited += KantenGridView_CellEdited;
            
            // Create button panel with very small buttons
            var buttonPanel = new StackLayout();
            buttonPanel.Orientation = Orientation.Horizontal;
            buttonPanel.HorizontalContentAlignment = HorizontalAlignment.Left;
            buttonPanel.Spacing = 3;
            
            _addKanteButton = new Button { Text = "Add", Size = new Size(30, 16) };
            _addKanteButton.Click += AddKanteButton_Click;
            _removeKanteButton = new Button { Text = "Remove", Size = new Size(45, 16) };
            _removeKanteButton.Click += RemoveKanteButton_Click;
            _editKanteButton = new Button { Text = "Edit", Size = new Size(30, 16) };
            _editKanteButton.Click += EditKanteButton_Click;
            
            buttonPanel.Items.Add(_addKanteButton);
            buttonPanel.Items.Add(_removeKanteButton);
            buttonPanel.Items.Add(_editKanteButton);
            
            // Setup table layout: table fills most space, buttons at bottom
            tableLayout.Rows.Add(new TableRow(_kantenGridView) { ScaleHeight = true });
            tableLayout.Rows.Add(new TableRow(buttonPanel) { ScaleHeight = false });
            
            groupBox.Content = tableLayout;
            panel.Content = groupBox;
            
            return panel;
        }

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            // Always display this page for single object selection (like IFC or Visual ARQ)
            return e.ObjectCount == 1;
        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {
            if (e.ObjectCount != 1) return;
            
            _rhinoObject = e.Objects.FirstOrDefault();
            if (_rhinoObject == null) return;
            
            _bauteil = _rhinoObject.UserData.Find(typeof(Bauteil)) as Bauteil;
            // If no bauteil data exists, create a default one but don't attach it yet
            if (_bauteil == null)
            {
                _bauteil = CreateDefaultBauteil();
            }
            
            UpdateDisplay();
        }

        private Bauteil CreateDefaultBauteil()
        {
            var bauteil = new Bauteil
            {
                Name = "New Bauteil",
                BauteilDescription = "Created from Properties panel"
            };
            
            // Add default realistic 3-layer structure
            bauteil.Schichten.Add(new MaterialSchicht
            {
                SchichtName = "Top Surface",
                Material = new BauteilMaterial { Name = "Melamin", Typ = MaterialType.MDF },
                Dicke = 0.8,
                Dichte = 1450,
                Laufrichtung = GrainDirection.X
            });
            
            bauteil.Schichten.Add(new MaterialSchicht
            {
                SchichtName = "Core",
                Material = new BauteilMaterial { Name = "Chipboard", Typ = MaterialType.Chipboard },
                Dicke = 16.4,
                Dichte = 650,
                Laufrichtung = GrainDirection.Y
            });
            
            bauteil.Schichten.Add(new MaterialSchicht
            {
                SchichtName = "Bottom Surface",
                Material = new BauteilMaterial { Name = "Melamin", Typ = MaterialType.MDF },
                Dicke = 0.8,
                Dichte = 1450,
                Laufrichtung = GrainDirection.X
            });
            
            // Add default 4 edges
            var edges = new[] { EdgeType.Top, EdgeType.Bottom, EdgeType.Front, EdgeType.Back };
            foreach (var edge in edges)
            {
                bauteil.Kantenbilder.Add(new Kantenbild
                {
                    KantenTyp = edge,
                    BearbeitungsTyp = EdgeProcessingType.Raw,
                    Dicke = 0.4,
                    IsVisible = true
                });
            }
            
            return bauteil;
        }

        private void UpdateDisplay()
        {
            if (_bauteil == null || _isUpdating) return;
            
            _isUpdating = true;
            
            try
            {
                // Update basic information
                _nameTextBox.Text = _bauteil.Name ?? "";
                _descriptionTextBox.Text = _bauteil.BauteilDescription ?? "";
                
                // Update calculations
                _totalThicknessLabel.Text = $"Total Thickness: {_bauteil.GetTotalThickness():F1} mm";
                _totalWeightLabel.Text = $"Weight: {_bauteil.GetWeightPerSquareMeter():F2} kg/m²";
                _paintConsumptionLabel.Text = $"Paint: {_bauteil.GetPaintConsumptionPerSquareMeter():F2} ml/m²";
                
                // Update material layers with editable items
                var schichtenData = _bauteil.Schichten.Select(s => new SchichtEditItem
                {
                    SchichtName = s.SchichtName,
                    MaterialType = s.Material.Typ,
                    DickeText = s.Dicke.ToString("F1"),
                    DichteText = s.Dichte.ToString("F0"),
                    Laufrichtung = s.Laufrichtung,
                    OriginalSchicht = s
                }).ToList();
                
                _schichtenGridView.DataStore = schichtenData;
                
                // Update edge configuration with editable items
                var kantenData = _bauteil.Kantenbilder.Select(k => new KanteEditItem
                {
                    KantenTyp = k.KantenTyp,
                    BearbeitungsTyp = k.BearbeitungsTyp,
                    DickeText = k.Dicke.ToString("F1"),
                    IsVisible = k.IsVisible,
                    OriginalKante = k
                }).ToList();
                
                _kantenGridView.DataStore = kantenData;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        // Event handlers for editable controls
        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_bauteil == null || _isUpdating) return;
            
            _bauteil.Name = _nameTextBox.Text;
            SaveChanges();
            // Notify editor panel about changes
            BauteilDataManager.NotifyBauteilChanged(_bauteil);
        }

        private void DescriptionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_bauteil == null || _isUpdating) return;
            
            _bauteil.BauteilDescription = _descriptionTextBox.Text;
            SaveChanges();
            BauteilDataManager.NotifyBauteilChanged(_bauteil);
        }

        private void SchichtenGridView_CellEdited(object sender, GridViewCellEventArgs e)
        {
            if (_bauteil == null || _isUpdating) return;

            var editedItem = e.Item as SchichtEditItem;
            if (editedItem?.OriginalSchicht == null) return;

            var schicht = editedItem.OriginalSchicht;

            try
            {
                switch (e.Column)
                {
                    case 0: // Layer name
                        schicht.SchichtName = editedItem.SchichtName;
                        break;
                    case 1: // Material type
                        schicht.Material.Typ = editedItem.MaterialType;
                        schicht.Material.SetDefaultPropertiesForType(editedItem.MaterialType);
                        break;
                    case 2: // Thickness
                        if (double.TryParse(editedItem.DickeText, out double dicke))
                            schicht.Dicke = dicke;
                        break;
                    case 3: // Density
                        if (double.TryParse(editedItem.DichteText, out double dichte))
                            schicht.Dichte = dichte;
                        break;
                    case 4: // Grain direction
                        schicht.Laufrichtung = editedItem.Laufrichtung;
                        break;
                }

                SaveChanges();
                UpdateCalculations();
                BauteilDataManager.NotifyBauteilChanged(_bauteil);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error editing material layer: {ex.Message}");
            }
        }

        private void KantenGridView_CellEdited(object sender, GridViewCellEventArgs e)
        {
            if (_bauteil == null || _isUpdating) return;

            var editedItem = e.Item as KanteEditItem;
            if (editedItem?.OriginalKante == null) return;

            var kante = editedItem.OriginalKante;

            try
            {
                switch (e.Column)
                {
                    case 0: // Edge type
                        kante.KantenTyp = editedItem.KantenTyp;
                        break;
                    case 1: // Processing type
                        kante.BearbeitungsTyp = editedItem.BearbeitungsTyp;
                        kante.SetDefaultPropertiesForProcessingType(editedItem.BearbeitungsTyp);
                        break;
                    case 2: // Thickness
                        if (double.TryParse(editedItem.DickeText, out double dicke))
                            kante.Dicke = dicke;
                        break;
                    case 3: // Visible
                        kante.IsVisible = editedItem.IsVisible;
                        break;
                }

                SaveChanges();
                BauteilDataManager.NotifyBauteilChanged(_bauteil);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error editing edge: {ex.Message}");
            }
        }

        private void AddSchichtButton_Click(object sender, EventArgs e)
        {
            if (_bauteil == null) return;
            
            try
            {
                var newSchicht = new MaterialSchicht
                {
                    SchichtName = $"Layer {_bauteil.Schichten.Count + 1}",
                    Material = new BauteilMaterial { Name = "New Material", Typ = MaterialType.Wood },
                    Dicke = 18.0
                };
                
                _bauteil.Schichten.Add(newSchicht);
                SaveChanges();
                UpdateDisplay();
                BauteilDataManager.NotifyBauteilChanged(_bauteil);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error adding material layer: {ex.Message}");
            }
        }

        private void RemoveSchichtButton_Click(object sender, EventArgs e)
        {
            if (_bauteil == null || _schichtenGridView.SelectedItem == null) return;
            
            try
            {
                var selectedItem = _schichtenGridView.SelectedItem as SchichtEditItem;
                if (selectedItem?.OriginalSchicht != null)
                {
                    _bauteil.Schichten.Remove(selectedItem.OriginalSchicht);
                    SaveChanges();
                    UpdateDisplay();
                    BauteilDataManager.NotifyBauteilChanged(_bauteil);
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error removing material layer: {ex.Message}");
            }
        }

        private void EditSchichtButton_Click(object sender, EventArgs e)
        {
            // TODO: Open detailed material layer editor dialog
            MessageBox.Show("Detailed layer editor will be implemented in future version.", "Edit Layer", MessageBoxType.Information);
        }

        private void AddKanteButton_Click(object sender, EventArgs e)
        {
            if (_bauteil == null) return;
            
            try
            {
                var newKante = new Kantenbild
                {
                    KantenTyp = EdgeType.Top,
                    BearbeitungsTyp = EdgeProcessingType.Raw
                };
                
                _bauteil.Kantenbilder.Add(newKante);
                SaveChanges();
                UpdateDisplay();
                BauteilDataManager.NotifyBauteilChanged(_bauteil);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error adding edge: {ex.Message}");
            }
        }

        private void RemoveKanteButton_Click(object sender, EventArgs e)
        {
            if (_bauteil == null || _kantenGridView.SelectedItem == null) return;
            
            try
            {
                var selectedItem = _kantenGridView.SelectedItem as KanteEditItem;
                if (selectedItem?.OriginalKante != null)
                {
                    _bauteil.Kantenbilder.Remove(selectedItem.OriginalKante);
                    SaveChanges();
                    UpdateDisplay();
                    BauteilDataManager.NotifyBauteilChanged(_bauteil);
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error removing edge: {ex.Message}");
            }
        }

        private void EditKanteButton_Click(object sender, EventArgs e)
        {
            // TODO: Open detailed edge editor dialog
            MessageBox.Show("Detailed edge editor will be implemented in future version.", "Edit Edge", MessageBoxType.Information);
        }

        private void UpdateCalculations()
        {
            if (_bauteil == null) return;
            
            _totalThicknessLabel.Text = $"Total Thickness: {_bauteil.GetTotalThickness():F1} mm";
            _totalWeightLabel.Text = $"Weight: {_bauteil.GetWeightPerSquareMeter():F2} kg/m²";
            _paintConsumptionLabel.Text = $"Paint: {_bauteil.GetPaintConsumptionPerSquareMeter():F2} ml/m²";
        }

        private void SaveChanges()
        {
            if (_rhinoObject == null || _bauteil == null) return;
            
            try
            {
                // Check if bauteil is already attached to object
                var existingBauteil = _rhinoObject.UserData.Find(typeof(Bauteil)) as Bauteil;
                if (existingBauteil == null)
                {
                    // First time saving - attach bauteil to object
                    _rhinoObject.UserData.Add(_bauteil);
                }
                
                // Mark the object as modified
                _rhinoObject.CommitChanges();
                
                // Update the document
                RhinoDoc.ActiveDoc.Views.Redraw();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error saving changes: {ex.Message}");
            }
        }



        private void ApplyButton_Click(object sender, EventArgs e)
        {
            if (_rhinoObject == null || _bauteil == null) return;
            
            try
            {
                // Force save the current bauteil to object
                var existingBauteil = _rhinoObject.UserData.Find(typeof(Bauteil)) as Bauteil;
                if (existingBauteil == null)
                {
                    _rhinoObject.UserData.Add(_bauteil);
                    RhinoApp.WriteLine("Bauteil data applied to object.");
                }
                else
                {
                    RhinoApp.WriteLine("Bauteil data updated.");
                }
                
                SaveChanges();
                BauteilDataManager.NotifyBauteilChanged(_bauteil);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying bauteil data: {ex.Message}", "Error", MessageBoxType.Error);
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (_rhinoObject == null || _bauteil == null) return;
            
            var result = MessageBox.Show(
                "Are you sure you want to remove the Bauteil data from this object?",
                "Remove Bauteil Data",
                MessageBoxButtons.YesNo,
                MessageBoxType.Question
            );
            
            if (result == DialogResult.Yes)
            {
                // Remove the Bauteil UserData
                _rhinoObject.UserData.Remove(_bauteil);
                
                // Update the document
                _rhinoObject.CommitChanges();
                RhinoDoc.ActiveDoc.Views.Redraw();
                
                // Notify manager
                BauteilDataManager.NotifyBauteilRemoved(_bauteil);
                
                RhinoApp.WriteLine("Bauteil data removed from object.");
            }
        }

        // Helper methods for display names
        private string GetMaterialTypeDisplayName(MaterialType materialType)
        {
            switch (materialType)
            {
                case MaterialType.Wood: return "Wood / Holz";
                case MaterialType.Plywood: return "Plywood / Sperrholz";
                case MaterialType.MDF: return "MDF / Melamin";
                case MaterialType.Chipboard: return "Chipboard / Spanplatte";
                case MaterialType.Metal: return "Metal / Metall";
                case MaterialType.Plastic: return "Plastic / Kunststoff";
                case MaterialType.Glass: return "Glass / Glas";
                case MaterialType.Composite: return "Composite / Verbund";
                case MaterialType.Other: return "Other / Sonstiges";
                default: return materialType.ToString();
            }
        }

        private string GetEdgeTypeDisplayName(EdgeType edgeType)
        {
            switch (edgeType)
            {
                case EdgeType.Top: return "Top / Oben";
                case EdgeType.Bottom: return "Bottom / Unten";
                case EdgeType.Front: return "Front / Vorne";
                case EdgeType.Back: return "Back / Hinten";
                case EdgeType.Left: return "Left / Links";
                case EdgeType.Right: return "Right / Rechts";
                default: return edgeType.ToString();
            }
        }

        private string GetProcessingTypeDisplayName(EdgeProcessingType processingType)
        {
            switch (processingType)
            {
                case EdgeProcessingType.Raw: return "Raw / Roh";
                case EdgeProcessingType.EdgeBanded: return "EdgeBanded / Bekantet";
                case EdgeProcessingType.Solid: return "Solid / Massiv";
                case EdgeProcessingType.Rounded: return "Rounded / Gerundet";
                case EdgeProcessingType.Chamfered: return "Chamfered / Gefast";
                default: return processingType.ToString();
            }
        }

        // Helper classes for editable grid items
        public class SchichtEditItem
        {
            public string SchichtName { get; set; }
            public MaterialType MaterialType { get; set; }
            public string DickeText { get; set; }
            public string DichteText { get; set; }
            public GrainDirection Laufrichtung { get; set; }
            public MaterialSchicht OriginalSchicht { get; set; }
        }

        public class KanteEditItem
        {
            public EdgeType KantenTyp { get; set; }
            public EdgeProcessingType BearbeitungsTyp { get; set; }
            public string DickeText { get; set; }
            public bool IsVisible { get; set; }
            public Kantenbild OriginalKante { get; set; }
        }
    }

    /// <summary>
    /// Static class to manage data synchronization between PropertyPage and EditorPanel
    /// </summary>
    public static class BauteilDataManager
    {
        public static event Action<Bauteil> BauteilChanged;
        public static event Action<Bauteil> BauteilRemoved;

        public static void NotifyBauteilChanged(Bauteil bauteil)
        {
            BauteilChanged?.Invoke(bauteil);
        }

        public static void NotifyBauteilRemoved(Bauteil bauteil)
        {
            BauteilRemoved?.Invoke(bauteil);
        }
    }
} 