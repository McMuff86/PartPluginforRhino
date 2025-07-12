using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;
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