using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Geometry;
using BauteilPlugin.Models;
using BauteilPlugin.Utils;
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

        // UI Controls
        private TextBox _nameTextBox;
        private TextBox _descriptionTextBox;
        private Label _totalThicknessLabel;
        private Label _totalWeightLabel;
        private Label _paintConsumptionLabel;
        private TextBox _lengthTextBox;
        private TextBox _widthTextBox;
        private GridView _schichtenGridView;
        private GridView _kantenGridView;
        private Button _addSchichtButton;
        private Button _removeSchichtButton;
        private Button _editSchichtButton;
        private Button _addKanteButton;
        private Button _removeKanteButton;
        private Button _editKanteButton;
        private Button _removeButton;

        // Search panels
        private MaterialSearchPanel _materialSearchPanel;
        private EdgeSearchPanel _edgeSearchPanel;
        private Panel _materialSearchContainer;
        private Panel _edgeSearchContainer;
        private Button _searchMaterialButton;
        private Button _searchEdgeButton;

        public override string EnglishPageTitle => "Bauteil";
        public override string LocalPageTitle => "Bauteil";

        public override object PageControl => _mainSplitter;

        public BauteilPropertyPage()
        {
            InitializeComponent();
            MaterialSearch.Initialize(); // Initialize material search database
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
            _lengthTextBox = new TextBox();
            _lengthTextBox.TextChanged += LengthTextBox_TextChanged;
            _widthTextBox = new TextBox();
            _widthTextBox.TextChanged += WidthTextBox_TextChanged;

            infoLayout.Rows.Add(new TableRow(
                new Label { Text = "Name:", Width = 80 },
                new TableCell(_nameTextBox, true)
            ));
            infoLayout.Rows.Add(new TableRow(
                new Label { Text = "Description:", Width = 80 },
                new TableCell(_descriptionTextBox, true)
            ));
            infoLayout.Rows.Add(new TableRow(
                new Label { Text = "Length (X):", Width = 80 },
                new TableCell(_lengthTextBox, true)
            ));
            infoLayout.Rows.Add(new TableRow(
                new Label { Text = "Width (Y):", Width = 80 },
                new TableCell(_widthTextBox, true)
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
            // GridView is editable by default when cells are configured properly
            
            // Add editable columns with resizable widths
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Layer",
                DataCell = new TextBoxCell("SchichtName"),
                Width = 80,
                Resizable = true,
                Editable = true
            });
            
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Material",
                DataCell = new TextBoxCell("MaterialName"),
                Width = 140,
                Resizable = true,
                Editable = true
            });
            
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Thickness",
                DataCell = new TextBoxCell("DickeText"),
                Width = 70,
                Resizable = true,
                Editable = true
            });

            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Density",
                DataCell = new TextBoxCell("DichteText"),
                Width = 70,
                Resizable = true,
                Editable = true
            });

            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Grain",
                DataCell = new TextBoxCell("LaufrichtungText"),
                Width = 60,
                Resizable = true,
                Editable = true
            });
            
            _schichtenGridView.CellEdited += SchichtenGridView_CellEdited;
            // Removed CellDoubleClick handler to prevent conflicts
            
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
            
            // Add material search button
            _searchMaterialButton = new Button { Text = "🔍 Material", Size = new Size(65, 16) };
            _searchMaterialButton.Click += SearchMaterialButton_Click;
            
            buttonPanel.Items.Add(_addSchichtButton);
            buttonPanel.Items.Add(_removeSchichtButton);
            buttonPanel.Items.Add(_editSchichtButton);
            buttonPanel.Items.Add(_searchMaterialButton);
            
            // Setup table layout: table fills most space, buttons at bottom
            tableLayout.Rows.Add(new TableRow(_schichtenGridView) { ScaleHeight = true });
            tableLayout.Rows.Add(new TableRow(buttonPanel) { ScaleHeight = false });
            
            // Create material search panel container (initially hidden)
            _materialSearchPanel = new MaterialSearchPanel();
            _materialSearchPanel.MaterialApplied += OnMaterialApplied;
            _materialSearchPanel.SearchCancelled += OnMaterialSearchCancelled;
            
            // Wrap in GroupBox for better visual integration
            var materialSearchGroupBox = new GroupBox
            {
                Text = "Material Search",
                Content = _materialSearchPanel,
                Padding = new Padding(5)
            };
            
            _materialSearchContainer = new Panel
            {
                Content = materialSearchGroupBox,
                Visible = false,
                Padding = new Padding(0, 5, 0, 0) // Small top margin for visual separation
            };
            
            tableLayout.Rows.Add(new TableRow(new TableCell(_materialSearchContainer, true)) { ScaleHeight = false });
            
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
            // GridView is editable by default when cells are configured properly
            
            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Edge",
                DataCell = new TextBoxCell("KantenTypText"),
                Width = 100,
                Resizable = true,
                Editable = true
            });
            
            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Processing",
                DataCell = new TextBoxCell("BearbeitungsTypText"),
                Width = 120,
                Resizable = true,
                Editable = true
            });

            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Thickness",
                DataCell = new TextBoxCell("DickeText"),
                Width = 70,
                Resizable = true,
                Editable = true
            });

            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Visible",
                DataCell = new CheckBoxCell("IsVisible"),
                Width = 60,
                Resizable = true,
                Editable = true
            });
            
            _kantenGridView.CellEdited += KantenGridView_CellEdited;
            // Removed CellDoubleClick handler to prevent conflicts
            
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
            
            // Add edge search button
            _searchEdgeButton = new Button { Text = "🔍 Edge", Size = new Size(55, 16) };
            _searchEdgeButton.Click += SearchEdgeButton_Click;
            
            buttonPanel.Items.Add(_addKanteButton);
            buttonPanel.Items.Add(_removeKanteButton);
            buttonPanel.Items.Add(_editKanteButton);
            buttonPanel.Items.Add(_searchEdgeButton);
            
            // Setup table layout: table fills most space, buttons at bottom
            tableLayout.Rows.Add(new TableRow(_kantenGridView) { ScaleHeight = true });
            tableLayout.Rows.Add(new TableRow(buttonPanel) { ScaleHeight = false });
            
            // Create edge search panel container (initially hidden)
            _edgeSearchPanel = new EdgeSearchPanel();
            _edgeSearchPanel.EdgeApplied += OnEdgeApplied;
            _edgeSearchPanel.SearchCancelled += OnEdgeSearchCancelled;
            
            // Wrap in GroupBox for better visual integration
            var edgeSearchGroupBox = new GroupBox
            {
                Text = "Edge Search",
                Content = _edgeSearchPanel,
                Padding = new Padding(5)
            };
            
            _edgeSearchContainer = new Panel
            {
                Content = edgeSearchGroupBox,
                Visible = false,
                Padding = new Padding(0, 5, 0, 0) // Small top margin for visual separation
            };
            
            tableLayout.Rows.Add(new TableRow(new TableCell(_edgeSearchContainer, true)) { ScaleHeight = false });
            
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
            if (e.ObjectCount == 1)
            {
                var rhinoObject = e.Objects[0];
                var instanceObject = rhinoObject as InstanceObject;

                if (instanceObject != null)
                {
                    _rhinoObject = instanceObject;
                    _bauteil = _rhinoObject.UserData.Find(typeof(Bauteil)) as Bauteil;

                    if (_bauteil != null)
                    {
                        // Sync the Bauteil's orientation plane with the actual instance transform.
                        // This is critical for the Gumball's "Align to Object" to work correctly,
                        // as it reads the instance's frame.
                        var instancePlane = Plane.WorldXY;
                        instancePlane.Transform(instanceObject.InstanceXform);
                        _bauteil.OrientationPlane = instancePlane;
                    }
                }
                else
                {
                    _rhinoObject = rhinoObject;
                    _bauteil = _rhinoObject.UserData.Find(typeof(Bauteil)) as Bauteil;
                }

                if (_bauteil == null)
                {
                    _bauteil = CreateDefaultBauteil();
                    // Don't attach yet, wait for user to confirm.
                }
            }
            else
            {
                _rhinoObject = null;
                _bauteil = CreateDefaultBauteil();
            }

            UpdateDisplay();
            base.UpdatePage(e);
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
                _lengthTextBox.Text = _bauteil.Length.ToString("F2");
                _widthTextBox.Text = _bauteil.Width.ToString("F2");
                
                // Update calculations
                _totalThicknessLabel.Text = $"Total Thickness: {_bauteil.GetTotalThickness():F1} mm";
                _totalWeightLabel.Text = $"Weight: {_bauteil.GetWeightPerSquareMeter():F2} kg/m²";
                _paintConsumptionLabel.Text = $"Paint: {_bauteil.GetPaintConsumptionPerSquareMeter():F2} ml/m²";
                
                // Update material layers with editable items
                var schichtenData = _bauteil.Schichten.Select(s => new SchichtEditItem
                {
                    SchichtName = s.SchichtName,
                    MaterialType = s.Material.Typ,
                    MaterialName = s.Material.Name,
                    DickeText = s.Dicke.ToString("F1"),
                    DichteText = s.Dichte.ToString("F0"),
                    Laufrichtung = s.Laufrichtung,
                    LaufrichtungText = GetGrainDirectionDisplayName(s.Laufrichtung),
                    OriginalSchicht = s
                }).ToList();
                
                _schichtenGridView.DataStore = schichtenData;
                
                // Update edge configuration with editable items
                var kantenData = _bauteil.Kantenbilder.Select(k => new KanteEditItem
                {
                    KantenTyp = k.KantenTyp,
                    KantenTypText = GetEdgeTypeDisplayName(k.KantenTyp),
                    BearbeitungsTyp = k.BearbeitungsTyp,
                    BearbeitungsTypText = GetProcessingTypeDisplayName(k.BearbeitungsTyp),
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

            // Also update the Rhino Object's name attribute
            if (_rhinoObject != null && _rhinoObject.IsValid)
            {
                var attributes = _rhinoObject.Attributes.Duplicate();
                attributes.Name = _bauteil.Name;
                RhinoDoc.ActiveDoc.Objects.ModifyAttributes(_rhinoObject.Id, attributes, true);
            }

            SaveChanges();
            // Notify editor panel about changes
            BauteilDataManager.NotifyBauteilChanged(_bauteil);
        }

        private void LengthTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_bauteil == null || _isUpdating) return;
            if (double.TryParse(_lengthTextBox.Text, out double length))
            {
                _bauteil.Length = length;
            }
        }

        private void WidthTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_bauteil == null || _isUpdating) return;
            if (double.TryParse(_widthTextBox.Text, out double width))
            {
                _bauteil.Width = width;
            }
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
            if (editedItem?.OriginalSchicht == null) 
            {
                RhinoApp.WriteLine("Error: No valid item selected for editing");
                return;
            }

            var schicht = editedItem.OriginalSchicht;
            if (schicht?.Material == null)
            {
                RhinoApp.WriteLine("Error: Material is null");
                return;
            }

            try
            {
                RhinoApp.WriteLine($"Editing column {e.Column} for layer {schicht.SchichtName}");
                
                switch (e.Column)
                {
                    case 0: // Layer name
                        if (!string.IsNullOrEmpty(editedItem.SchichtName))
                        {
                            schicht.SchichtName = editedItem.SchichtName;
                            RhinoApp.WriteLine($"Updated layer name to: {schicht.SchichtName}");
                        }
                        break;
                    case 1: // Material search
                        HandleMaterialSearch(editedItem, schicht);
                        break;
                    case 2: // Thickness
                        if (double.TryParse(editedItem.DickeText, out double dicke) && dicke > 0)
                        {
                            schicht.Dicke = dicke;
                            RhinoApp.WriteLine($"Updated thickness to: {dicke}");
                        }
                        break;
                    case 3: // Density
                        if (double.TryParse(editedItem.DichteText, out double dichte) && dichte > 0)
                        {
                            schicht.Dichte = dichte;
                            RhinoApp.WriteLine($"Updated density to: {dichte}");
                        }
                        break;
                    case 4: // Grain direction
                        HandleGrainDirectionSearch(editedItem, schicht);
                        break;
                }

                SaveChanges();
                UpdateCalculations();
                BauteilDataManager.NotifyBauteilChanged(_bauteil);
                RhinoApp.WriteLine("Material layer edit completed successfully");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error editing material layer: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void KantenGridView_CellEdited(object sender, GridViewCellEventArgs e)
        {
            if (_bauteil == null || _isUpdating) return;

            var editedItem = e.Item as KanteEditItem;
            if (editedItem?.OriginalKante == null) 
            {
                RhinoApp.WriteLine("Error: No valid edge item selected for editing");
                return;
            }

            var kante = editedItem.OriginalKante;

            try
            {
                RhinoApp.WriteLine($"Editing edge column {e.Column}");
                
                switch (e.Column)
                {
                    case 0: // Edge type
                        HandleEdgeTypeSearch(editedItem, kante);
                        break;
                    case 1: // Processing type
                        HandleProcessingTypeSearch(editedItem, kante);
                        break;
                    case 2: // Thickness
                        if (double.TryParse(editedItem.DickeText, out double dicke) && dicke >= 0)
                        {
                            kante.Dicke = dicke;
                            RhinoApp.WriteLine($"Updated edge thickness to: {dicke}");
                        }
                        break;
                    case 3: // Visible
                        kante.IsVisible = editedItem.IsVisible;
                        RhinoApp.WriteLine($"Updated edge visibility to: {editedItem.IsVisible}");
                        break;
                }

                SaveChanges();
                BauteilDataManager.NotifyBauteilChanged(_bauteil);
                RhinoApp.WriteLine("Edge edit completed successfully");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error editing edge: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
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

        private void SearchMaterialButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a material layer is selected
                if (_schichtenGridView.SelectedItem is SchichtEditItem selectedSchicht)
                {
                    // Toggle the material search panel visibility
                    _materialSearchContainer.Visible = !_materialSearchContainer.Visible;
                    
                    if (_materialSearchContainer.Visible)
                    {
                        // Set initial search to current material name
                        _materialSearchPanel.SetInitialSearch(selectedSchicht.MaterialName);
                        _materialSearchPanel.FocusSearchBox();
                        _searchMaterialButton.Text = "🔍 Hide";
                        
                        // Hide edge search panel if visible
                        _edgeSearchContainer.Visible = false;
                        _searchEdgeButton.Text = "🔍 Edge";
                        
                        RhinoApp.WriteLine("Material search panel opened. Type to search for materials.");
                    }
                    else
                    {
                        _searchMaterialButton.Text = "🔍 Material";
                        RhinoApp.WriteLine("Material search panel closed.");
                    }
                }
                else
                {
                    RhinoApp.WriteLine("Please select a material layer first to search for materials.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in material search: {ex.Message}");
            }
        }

        private void SearchEdgeButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if an edge is selected
                if (_kantenGridView.SelectedItem is KanteEditItem selectedKante)
                {
                    // Toggle the edge search panel visibility
                    _edgeSearchContainer.Visible = !_edgeSearchContainer.Visible;
                    
                    if (_edgeSearchContainer.Visible)
                    {
                        // Set initial search type based on current edge
                        _edgeSearchPanel.SetSearchType(true); // Start with edge types
                        _edgeSearchPanel.FocusSearchBox();
                        _searchEdgeButton.Text = "🔍 Hide";
                        
                        // Hide material search panel if visible
                        _materialSearchContainer.Visible = false;
                        _searchMaterialButton.Text = "🔍 Material";
                        
                        RhinoApp.WriteLine("Edge search panel opened. Select edge type or processing type and start typing to search.");
                    }
                    else
                    {
                        _searchEdgeButton.Text = "🔍 Edge";
                        RhinoApp.WriteLine("Edge search panel closed.");
                    }
                }
                else
                {
                    RhinoApp.WriteLine("Please select an edge first to search for edge options.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in edge search: {ex.Message}");
            }
        }

        // Event handlers for embedded search panels
        private void OnMaterialApplied(object sender, MaterialSearchItem selectedMaterial)
        {
            try
            {
                if (_schichtenGridView.SelectedItem is SchichtEditItem selectedSchicht)
                {
                    // Apply the selected material
                    var newMaterial = MaterialSearch.CreateMaterialFromSearchItem(selectedMaterial);
                    var schicht = selectedSchicht.OriginalSchicht;
                    
                    // Update material properties
                    schicht.Material.Name = newMaterial.Name;
                    schicht.Material.Typ = newMaterial.Typ;
                    schicht.Material.StandardDensity = newMaterial.StandardDensity;
                    schicht.Material.StandardThickness = newMaterial.StandardThickness;
                    schicht.Dichte = newMaterial.StandardDensity;
                    
                    // Update display
                    selectedSchicht.MaterialName = newMaterial.Name;
                    selectedSchicht.MaterialType = newMaterial.Typ;
                    selectedSchicht.DichteText = newMaterial.StandardDensity.ToString("F0");
                    
                    SaveChanges();
                    UpdateDisplay();
                    BauteilDataManager.NotifyBauteilChanged(_bauteil);
                    
                    // Hide the search panel
                    _materialSearchContainer.Visible = false;
                    _searchMaterialButton.Text = "🔍 Material";
                    
                    RhinoApp.WriteLine($"Material '{selectedMaterial.Name}' applied to layer '{schicht.SchichtName}'");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error applying material: {ex.Message}");
            }
        }

        private void OnMaterialSearchCancelled(object sender, EventArgs e)
        {
            // Hide the search panel
            _materialSearchContainer.Visible = false;
            _searchMaterialButton.Text = "🔍 Material";
            RhinoApp.WriteLine("Material search cancelled.");
        }

        private void OnEdgeApplied(object sender, EdgeSearchResultEventArgs e)
        {
            try
            {
                if (_kantenGridView.SelectedItem is KanteEditItem selectedKante)
                {
                    var selectedOption = e.SelectedOption;
                    var isEdgeType = e.IsEdgeType;
                    var kante = selectedKante.OriginalKante;
                    
                    if (isEdgeType)
                    {
                        // Update edge type
                        kante.KantenTyp = (EdgeType)selectedOption.EnumValue;
                        selectedKante.KantenTyp = (EdgeType)selectedOption.EnumValue;
                        selectedKante.KantenTypText = GetEdgeTypeDisplayName((EdgeType)selectedOption.EnumValue);
                        
                        RhinoApp.WriteLine($"Edge type updated to: {selectedOption.Name}");
                    }
                    else
                    {
                        // Update processing type
                        kante.BearbeitungsTyp = (EdgeProcessingType)selectedOption.EnumValue;
                        selectedKante.BearbeitungsTyp = (EdgeProcessingType)selectedOption.EnumValue;
                        selectedKante.BearbeitungsTypText = GetProcessingTypeDisplayName((EdgeProcessingType)selectedOption.EnumValue);
                        
                        // Set default properties
                        kante.SetDefaultPropertiesForProcessingType((EdgeProcessingType)selectedOption.EnumValue);
                        
                        RhinoApp.WriteLine($"Processing type updated to: {selectedOption.Name}");
                    }
                    
                    SaveChanges();
                    UpdateDisplay();
                    BauteilDataManager.NotifyBauteilChanged(_bauteil);
                    
                    // Hide the search panel
                    _edgeSearchContainer.Visible = false;
                    _searchEdgeButton.Text = "🔍 Edge";
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error applying edge option: {ex.Message}");
            }
        }

        private void OnEdgeSearchCancelled(object sender, EventArgs e)
        {
            // Hide the search panel
            _edgeSearchContainer.Visible = false;
            _searchEdgeButton.Text = "🔍 Edge";
            RhinoApp.WriteLine("Edge search cancelled.");
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
                if (_rhinoObject is InstanceObject instanceObject)
                {
                    UpdateBauteilBlock(instanceObject, _bauteil);
                }
                else
                {
                    ConvertObjectToBlockWithBauteil(_rhinoObject, _bauteil);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying bauteil data: {ex.Message}", "Error", MessageBoxType.Error);
            }
        }

        private void UpdateBauteilBlock(InstanceObject instanceObject, Bauteil bauteilFromUi)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null || instanceObject == null || bauteilFromUi == null) return;

            var oldDefinition = instanceObject.InstanceDefinition;
            if (oldDefinition == null || oldDefinition.IsDeleted)
            {
                RhinoApp.WriteLine("The selected block instance has an invalid definition.");
                return;
            }

            // Create new canonical geometry
            var newBrep = Brep.CreateFromBox(new Box(Plane.WorldXY,
                new Interval(0, bauteilFromUi.Length),
                new Interval(0, bauteilFromUi.Width),
                new Interval(0, bauteilFromUi.GetTotalThickness())));

            if (newBrep == null)
            {
                RhinoApp.WriteLine("Failed to create new Bauteil geometry.");
                return;
            }

            // Create a new unique block definition name
            var baseBlockName = $"{bauteilFromUi.Name}_BT";
            var newBlockName = baseBlockName;
            int counter = 1;
            while (doc.InstanceDefinitions.Find(newBlockName) != null)
            {
                newBlockName = $"{baseBlockName}_{counter++}";
            }

            // Add the new block definition
            int newBlockDefIndex = doc.InstanceDefinitions.Add(newBlockName, "Bauteil geometry", Point3d.Origin, new[] { newBrep });

            if (newBlockDefIndex < 0)
            {
                RhinoApp.WriteLine("Failed to create new block definition.");
                return;
            }

            // Add new instance with the transform from the old one
            var newInstanceId = doc.Objects.AddInstanceObject(newBlockDefIndex, instanceObject.InstanceXform);
            if (newInstanceId == Guid.Empty)
            {
                RhinoApp.WriteLine("Failed to create new block instance.");
                return;
            }
            
            var newInstanceObject = doc.Objects.FindId(newInstanceId) as InstanceObject;
            if (newInstanceObject == null) return;
            
            // Copy attributes and add user data
            newInstanceObject.Attributes = instanceObject.Attributes.Duplicate();
            newInstanceObject.Attributes.Name = bauteilFromUi.Name;
            newInstanceObject.UserData.Add(bauteilFromUi.Clone());
            newInstanceObject.CommitChanges();

            // Delete the old object
            Guid oldInstanceId = instanceObject.Id;
            doc.Objects.Delete(oldInstanceId, true);

            // If the old block definition is no longer used, delete it
            var users = doc.Objects.FindByObjectType(ObjectType.InstanceReference)
                .Where(rhObj => (rhObj as InstanceObject)?.InstanceDefinition.Id == oldDefinition.Id);

            if (!users.Any())
            {
                doc.InstanceDefinitions.Delete(oldDefinition.Index, true, true);
            }

            doc.Objects.Select(newInstanceId, true, true, true);
            doc.Views.Redraw();
        }

        private void ConvertObjectToBlockWithBauteil(RhinoObject rhinoObject, Bauteil bauteil)
        {
            var doc = RhinoDoc.ActiveDoc;

            Brep brep = null;
            var geometry = rhinoObject.Geometry;

            if (geometry is Brep brepGeo)
            {
                brep = brepGeo;
            }
            else if (geometry is Extrusion extrusion)
            {
                brep = extrusion.ToBrep();
                RhinoApp.WriteLine("Extrusion object was converted to a Polysurface (Brep).");
            }

            if (brep == null)
            {
                RhinoApp.WriteLine("The selected object is not a valid Brep and could not be converted.");
                return;
            }

            // --- Get the orientation plane from user input (3-Point Method) ---
            RhinoApp.WriteLine("Define the component's orientation using 3 points (Origin, X-Axis, Y-Axis).");

            var gpOrigin = new GetPoint();
            gpOrigin.SetCommandPrompt("Select Bauteil origin point");
            gpOrigin.Get();
            if (gpOrigin.CommandResult() != Rhino.Commands.Result.Success) return;
            var originPoint = gpOrigin.Point();

            var gpX = new GetPoint();
            gpX.SetCommandPrompt("Select point on Bauteil's X-axis");
            gpX.SetBasePoint(originPoint, true);
            gpX.DrawLineFromPoint(originPoint, true);
            gpX.Get();
            if (gpX.CommandResult() != Rhino.Commands.Result.Success) return;
            var xAxisPoint = gpX.Point();

            var gpY = new GetPoint();
            gpY.SetCommandPrompt("Select point on Bauteil's Y-axis");
            gpY.SetBasePoint(originPoint, true);
            gpY.DrawLineFromPoint(originPoint, true);
            gpY.Get();
            if (gpY.CommandResult() != Rhino.Commands.Result.Success) return;
            var yAxisPoint = gpY.Point();

            var orientationPlane = new Plane(originPoint, xAxisPoint, yAxisPoint);

            // --- Dimension Sync Logic ---
            var objectBbox = brep.GetBoundingBox(orientationPlane);
            var objectLength = objectBbox.Max.X - objectBbox.Min.X;
            var objectWidth = objectBbox.Max.Y - objectBbox.Min.Y;

            var userDialog = new Dialog<DialogResult>();
            userDialog.Title = "Dimension Synchronization";
            userDialog.Padding = new Padding(10);
            
            var layout = new DynamicLayout();
            layout.Spacing = new Size(5, 5);
            layout.Add(new Label { Text = "How should dimensions be handled?" });

            var fromObjectButton = new Button { Text = $"Update Bauteil from Object ({objectLength:F1} x {objectWidth:F1} mm)" };
            fromObjectButton.Click += (s, ev) => userDialog.Close(DialogResult.Yes);
            
            var fromUiButton = new Button { Text = $"Scale Object to Bauteil ({_bauteil.Length:F1} x {_bauteil.Width:F1} mm)"};
            fromUiButton.Click += (s, ev) => userDialog.Close(DialogResult.No);
            
            var cancelButton = new Button { Text = "Cancel" };
            cancelButton.Click += (s, ev) => userDialog.Close(DialogResult.Cancel);

            layout.Add(fromObjectButton);
            layout.Add(fromUiButton);
            layout.Add(cancelButton);
            userDialog.Content = layout;
            
            var dialogResult = userDialog.ShowModal(Application.Instance.MainForm);

            if (dialogResult == DialogResult.Yes) // From Object to UI
            {
                _bauteil.Length = objectLength;
                _bauteil.Width = objectWidth;
                _lengthTextBox.Text = objectLength.ToString("F2");
                _widthTextBox.Text = objectWidth.ToString("F2");
                RhinoApp.WriteLine("Bauteil dimensions updated from object geometry.");
            }
            else if (dialogResult == DialogResult.No) // From UI to Object
            {
                var scaleX = _bauteil.Length / objectLength;
                var scaleY = _bauteil.Width / objectWidth;
                if (Math.Abs(scaleX - 1.0) > 0.001 || Math.Abs(scaleY - 1.0) > 0.001)
                {
                    var scaleTransform = Transform.Scale(orientationPlane, scaleX, scaleY, 1.0);
                    brep.Transform(scaleTransform);
                    RhinoApp.WriteLine($"Object scaled in X/Y to {_bauteil.Length:F2} x {_bauteil.Width:F2} mm.");
                }
            }
            else // Cancel
            {
                return;
            }


            // Get edge for thickness measurement
            RhinoApp.WriteLine("Please use Ctrl+Shift to select a single edge.");
            var goEdgeZ = new GetObject();
            goEdgeZ.SetCommandPrompt("Select an edge that represents the component's THICKNESS");
            goEdgeZ.GeometryFilter = ObjectType.Curve;
            goEdgeZ.SubObjectSelect = true;
            goEdgeZ.DeselectAllBeforePostSelect = false;
            goEdgeZ.EnablePreSelect(false, true);
            goEdgeZ.EnableHighlight(true); // Enable highlighting
            goEdgeZ.Get();
            if (goEdgeZ.CommandResult() != Rhino.Commands.Result.Success)
            {
                return;
            }
            var edgeRefZ = goEdgeZ.Object(0);
            var thicknessEdge = edgeRefZ.Edge();
            if (thicknessEdge == null)
            {
                RhinoApp.WriteLine("Selection was not a valid thickness edge.");
                return;
            }

            // Z-Axis is the normal of the calculated plane
            var zAxis = orientationPlane.ZAxis;

            bauteil.OrientationPlane = orientationPlane;
            RhinoApp.WriteLine("WARNING: The new Orientation Plane will not be saved with the document due to a known issue. This will be fixed in a future version.");

            // --- Scale object to match Bauteil thickness ---
            double targetThickness = bauteil.GetTotalThickness();
            if (targetThickness > 0)
            {
                var thicknessVector = thicknessEdge.PointAtEnd - thicknessEdge.PointAtStart;
                double currentThickness = Math.Abs(thicknessVector * zAxis);

                if (currentThickness > 0.001 && Math.Abs(targetThickness - currentThickness) > 0.001)
                {
                    double scaleFactorZ = targetThickness / currentThickness;
                    var scalingTransform = Transform.Scale(orientationPlane, 1.0, 1.0, scaleFactorZ);

                    // Important: Duplicate the brep first, then transform the duplicate
                    var scaledBrep = brep.DuplicateBrep();
                    if (scaledBrep.Transform(scalingTransform))
                    {
                        brep = scaledBrep; // Replace original brep with scaled version
                        RhinoApp.WriteLine($"Object scaled in thickness from {currentThickness:F2} to {targetThickness:F2} mm.");
                    }
                    else
                    {
                        RhinoApp.WriteLine("Failed to apply scaling transform to the object.");
                    }
                }
            }

            // --- Create block definition and instance ---
            var baseBlockName = $"{bauteil.Name}_BT";
            var blockName = baseBlockName;
            int counter = 1;
            while (doc.InstanceDefinitions.Find(blockName) != null)
            {
                blockName = $"{baseBlockName}_{counter}";
                counter++;
            }
            
            var toOrigin = Transform.PlaneToPlane(orientationPlane, Plane.WorldXY);
            var geometryForBlock = brep.DuplicateBrep();
            geometryForBlock.Transform(toOrigin);

            var blockDefIndex = doc.InstanceDefinitions.Add(blockName, "Bauteil geometry", Point3d.Origin, new GeometryBase[] { geometryForBlock }, null);
            if (blockDefIndex < 0)
            {
                RhinoApp.WriteLine("Failed to create block definition.");
                return;
            }

            var instanceTransform = Transform.PlaneToPlane(Plane.WorldXY, orientationPlane);
            var instanceId = doc.Objects.AddInstanceObject(blockDefIndex, instanceTransform);

            if (instanceId == Guid.Empty)
            {
                RhinoApp.WriteLine("Failed to create block instance.");
                return;
            }

            var instanceObject = doc.Objects.FindId(instanceId);
            if (instanceObject != null)
            {
                var attributes = instanceObject.Attributes.Duplicate();
                attributes.Name = bauteil.Name;
                doc.Objects.ModifyAttributes(instanceObject.Id, attributes, true);

                instanceObject.UserData.Add(bauteil.Clone());
                instanceObject.CommitChanges();
            }

            doc.Objects.Delete(rhinoObject, true);
            doc.Views.Redraw();
            
            RhinoApp.WriteLine($"Converted object to a block instance with Bauteil data.");
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

        private string GetGrainDirectionDisplayName(GrainDirection grainDirection)
        {
            switch (grainDirection)
            {
                case GrainDirection.X: return "X";
                case GrainDirection.Y: return "Y";
                default: return grainDirection.ToString();
            }
        }

        private void HandleMaterialSearch(SchichtEditItem editedItem, MaterialSchicht schicht)
        {
            if (string.IsNullOrWhiteSpace(editedItem.MaterialName))
            {
                RhinoApp.WriteLine("Material name is empty");
                return;
            }

            // Search for material in database
            var searchResult = MaterialSearch.GetMaterialByName(editedItem.MaterialName);
            if (searchResult != null)
            {
                // Found exact match - update material properties
                var newMaterial = MaterialSearch.CreateMaterialFromSearchItem(searchResult);
                schicht.Material.Name = newMaterial.Name;
                schicht.Material.Typ = newMaterial.Typ;
                schicht.Material.StandardDensity = newMaterial.StandardDensity;
                schicht.Material.StandardThickness = newMaterial.StandardThickness;
                schicht.Dichte = newMaterial.StandardDensity;
                
                RhinoApp.WriteLine($"Material updated to: {newMaterial.Name} ({newMaterial.Typ})");
                
                // Update display item
                editedItem.MaterialType = newMaterial.Typ;
                editedItem.DichteText = newMaterial.StandardDensity.ToString("F0");
            }
            else
            {
                // Search for similar materials
                var suggestions = MaterialSearch.SearchMaterials(editedItem.MaterialName, 5);
                if (suggestions.Count > 0)
                {
                    RhinoApp.WriteLine($"Material '{editedItem.MaterialName}' not found. Suggestions:");
                    foreach (var suggestion in suggestions)
                    {
                        RhinoApp.WriteLine($"  - {suggestion.Name} ({suggestion.Type})");
                    }
                }
                else
                {
                    // Keep as custom material name
                    schicht.Material.Name = editedItem.MaterialName;
                    RhinoApp.WriteLine($"Custom material name set: {editedItem.MaterialName}");
                }
            }
        }

        private void HandleGrainDirectionSearch(SchichtEditItem editedItem, MaterialSchicht schicht)
        {
            if (string.IsNullOrWhiteSpace(editedItem.LaufrichtungText))
            {
                return;
            }

            var text = editedItem.LaufrichtungText.ToUpper().Trim();
            
            if (text == "X")
            {
                schicht.Laufrichtung = GrainDirection.X;
                editedItem.Laufrichtung = GrainDirection.X;
                RhinoApp.WriteLine("Grain direction set to X");
            }
            else if (text == "Y")
            {
                schicht.Laufrichtung = GrainDirection.Y;
                editedItem.Laufrichtung = GrainDirection.Y;
                RhinoApp.WriteLine("Grain direction set to Y");
            }
            else
            {
                RhinoApp.WriteLine($"Invalid grain direction: {text}. Use 'X' or 'Y'");
            }
        }

        private void HandleEdgeTypeSearch(KanteEditItem editedItem, Kantenbild kante)
        {
            if (string.IsNullOrWhiteSpace(editedItem.KantenTypText))
            {
                return;
            }

            var text = editedItem.KantenTypText.ToLower().Trim();
            EdgeType edgeType;
            
            if (text.Contains("top") || text.Contains("oben"))
            {
                edgeType = EdgeType.Top;
            }
            else if (text.Contains("bottom") || text.Contains("unten"))
            {
                edgeType = EdgeType.Bottom;
            }
            else if (text.Contains("front") || text.Contains("vorne"))
            {
                edgeType = EdgeType.Front;
            }
            else if (text.Contains("back") || text.Contains("hinten"))
            {
                edgeType = EdgeType.Back;
            }
            else if (text.Contains("left") || text.Contains("links"))
            {
                edgeType = EdgeType.Left;
            }
            else if (text.Contains("right") || text.Contains("rechts"))
            {
                edgeType = EdgeType.Right;
            }
            else
            {
                RhinoApp.WriteLine($"Invalid edge type: {text}. Use: Top, Bottom, Front, Back, Left, Right");
                return;
            }

            kante.KantenTyp = edgeType;
            editedItem.KantenTyp = edgeType;
            RhinoApp.WriteLine($"Edge type set to: {edgeType}");
        }

        private void HandleProcessingTypeSearch(KanteEditItem editedItem, Kantenbild kante)
        {
            if (string.IsNullOrWhiteSpace(editedItem.BearbeitungsTypText))
            {
                return;
            }

            var text = editedItem.BearbeitungsTypText.ToLower().Trim();
            EdgeProcessingType processingType;
            
            if (text.Contains("raw") || text.Contains("roh"))
            {
                processingType = EdgeProcessingType.Raw;
            }
            else if (text.Contains("band") || text.Contains("bekantet"))
            {
                processingType = EdgeProcessingType.EdgeBanded;
            }
            else if (text.Contains("solid") || text.Contains("massiv"))
            {
                processingType = EdgeProcessingType.Solid;
            }
            else if (text.Contains("round") || text.Contains("gerundet"))
            {
                processingType = EdgeProcessingType.Rounded;
            }
            else if (text.Contains("chamfer") || text.Contains("gefast"))
            {
                processingType = EdgeProcessingType.Chamfered;
            }
            else if (text.Contains("groove") || text.Contains("nut"))
            {
                processingType = EdgeProcessingType.Grooved;
            }
            else if (text.Contains("profile") || text.Contains("profil"))
            {
                processingType = EdgeProcessingType.Profiled;
            }
            else if (text.Contains("laminate") || text.Contains("laminiert"))
            {
                processingType = EdgeProcessingType.Laminated;
            }
            else
            {
                RhinoApp.WriteLine($"Invalid processing type: {text}. Use: Raw, EdgeBanded, Solid, Rounded, Chamfered, Grooved, Profiled, Laminated");
                return;
            }

            kante.BearbeitungsTyp = processingType;
            editedItem.BearbeitungsTyp = processingType;
            
            // Set default properties
            if (kante.SetDefaultPropertiesForProcessingType != null)
            {
                kante.SetDefaultPropertiesForProcessingType(processingType);
            }
            
            RhinoApp.WriteLine($"Processing type set to: {processingType}");
        }

        // Helper classes for editable grid items
        public class SchichtEditItem
        {
            public string SchichtName { get; set; }
            public MaterialType MaterialType { get; set; }
            public string MaterialName { get; set; }
            public string DickeText { get; set; }
            public string DichteText { get; set; }
            public GrainDirection Laufrichtung { get; set; }
            public string LaufrichtungText { get; set; }
            public MaterialSchicht OriginalSchicht { get; set; }
        }

        public class KanteEditItem
        {
            public EdgeType KantenTyp { get; set; }
            public string KantenTypText { get; set; }
            public EdgeProcessingType BearbeitungsTyp { get; set; }
            public string BearbeitungsTypText { get; set; }
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