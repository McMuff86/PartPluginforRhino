using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.UI;
using Rhino.DocObjects;
using BauteilPlugin.Models;
using BauteilMaterial = BauteilPlugin.Models.Material;

namespace BauteilPlugin.UI
{
    /// <summary>
    /// Dockable panel for editing building components (Bauteile)
    /// Provides intuitive interface for managing material layers and edge configurations
    /// </summary>
    [System.Runtime.InteropServices.Guid("E8D4C2A5-7B9F-4E3D-8C1A-9F6E5B2D4C7A")]
    public class BauteilEditorPanel : Panel, IPanel
    {
        private readonly Guid _panelId = new Guid("E8D4C2A5-7B9F-4E3D-8C1A-9F6E5B2D4C7A");
        private Bauteil _currentBauteil;
        private RhinoObject _currentRhinoObject;
        private readonly List<Bauteil> _bauteile = new List<Bauteil>();
        private bool _isUpdating = false;

        // UI Controls
        private DropDown _bauteilDropDown;
        private TextBox _bauteilNameTextBox;
        private TextBox _bauteilDescriptionTextBox;
        private GridView _schichtenGridView;
        private GridView _kantenGridView;
        private Label _totalThicknessLabel;
        private Label _totalWeightLabel;
        private Label _paintConsumptionLabel;
        private Button _addSchichtButton;
        private Button _removeSchichtButton;
        private Button _editSchichtButton;
        private Button _addKanteButton;
        private Button _removeKanteButton;
        private Button _editKanteButton;
        private Button _addBauteilButton;
        private Button _removeBauteilButton;
        private Button _applyToObjectButton;

        /// <summary>
        /// Panel ID for Rhino integration
        /// </summary>
        public Guid PanelId => _panelId;

        /// <summary>
        /// Called when the panel is about to be closed
        /// </summary>
        /// <param name="documentSerialNumber">Document serial number</param>
        /// <param name="onCloseVisible">Whether the panel is visible when closing</param>
        public void PanelClosing(uint documentSerialNumber, bool onCloseVisible)
        {
            // Unsubscribe from events to prevent memory leaks
            BauteilDataManager.BauteilChanged -= OnBauteilChanged;
            BauteilDataManager.BauteilRemoved -= OnBauteilRemoved;
        }

        /// <summary>
        /// Called when the panel is hidden
        /// </summary>
        /// <param name="documentSerialNumber">Document serial number</param>
        /// <param name="reason">Reason for hiding</param>
        public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason)
        {
            // Handle panel hidden event
            // Currently no special handling required
        }

        /// <summary>
        /// Called when the panel is shown
        /// </summary>
        /// <param name="documentSerialNumber">Document serial number</param>
        /// <param name="reason">Reason for showing</param>
        public void PanelShown(uint documentSerialNumber, ShowPanelReason reason)
        {
            // Handle panel shown event
            // Refresh UI if needed
            UpdateUI();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BauteilEditorPanel()
        {
            InitializeComponent();
            InitializeData();
            SubscribeToEvents();
        }

        /// <summary>
        /// Subscribe to data manager events
        /// </summary>
        private void SubscribeToEvents()
        {
            BauteilDataManager.BauteilChanged += OnBauteilChanged;
            BauteilDataManager.BauteilRemoved += OnBauteilRemoved;
        }



        /// <summary>
        /// Event handler for bauteil changes from PropertyPage
        /// </summary>
        private void OnBauteilChanged(Bauteil bauteil)
        {
            if (_currentBauteil?.Id == bauteil.Id && !_isUpdating)
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Event handler for bauteil removal
        /// </summary>
        private void OnBauteilRemoved(Bauteil bauteil)
        {
            _bauteile.RemoveAll(b => b.Id == bauteil.Id);
            if (_currentBauteil?.Id == bauteil.Id)
            {
                _currentBauteil = _bauteile.FirstOrDefault();
                _currentRhinoObject = null;
            }
            UpdateUI();
        }

        /// <summary>
        /// Initialize the user interface components
        /// </summary>
        private void InitializeComponent()
        {
            // Set panel properties
            this.Size = new Size(400, 600);
            this.MinimumSize = new Size(350, 500);

            // Create main splitter for resizable sections
            var mainSplitter = new Splitter
            {
                Orientation = Orientation.Vertical,
                Position = 200 // Initial position
            };

            // Top panel for bauteil selection and properties
            var topPanel = CreateTopPanel();
            mainSplitter.Panel1 = topPanel;

            // Bottom panel for layers and edges
            var bottomPanel = CreateBottomPanel();
            mainSplitter.Panel2 = bottomPanel;

            // Set the main layout
            this.Content = mainSplitter;
        }

        private Control CreateTopPanel()
        {
            var layout = new DynamicLayout();
            layout.BeginVertical(spacing: new Size(5, 5), padding: new Padding(10));

            // Bauteil selection section
            layout.BeginGroup("Bauteil Selection", new Padding(5));
            layout.BeginHorizontal();
            layout.Add(new Label { Text = "Bauteil:" });
            _bauteilDropDown = new DropDown();
            _bauteilDropDown.SelectedValueChanged += BauteilDropDown_SelectedValueChanged;
            layout.Add(_bauteilDropDown, true);
            layout.EndHorizontal();

            layout.BeginHorizontal();
            _addBauteilButton = new Button { Text = "Add", Size = new Size(50, 25) };
            _addBauteilButton.Click += AddBauteilButton_Click;
            _removeBauteilButton = new Button { Text = "Remove", Size = new Size(60, 25) };
            _removeBauteilButton.Click += RemoveBauteilButton_Click;
            _applyToObjectButton = new Button { Text = "Apply to Object", Size = new Size(100, 25) };
            _applyToObjectButton.Click += ApplyToObjectButton_Click;
            layout.Add(_addBauteilButton);
            layout.Add(_removeBauteilButton);
            layout.Add(_applyToObjectButton);
            layout.Add(null, true); // Spacer
            layout.EndHorizontal();
            layout.EndGroup();

            // Bauteil properties section
            layout.BeginGroup("Bauteil Properties", new Padding(5));
            layout.BeginHorizontal();
            layout.Add(new Label { Text = "Name:" });
            _bauteilNameTextBox = new TextBox();
            _bauteilNameTextBox.TextChanged += BauteilNameTextBox_TextChanged;
            layout.Add(_bauteilNameTextBox, true);
            layout.EndHorizontal();

            layout.BeginHorizontal();
            layout.Add(new Label { Text = "Description:" });
            _bauteilDescriptionTextBox = new TextBox();
            _bauteilDescriptionTextBox.TextChanged += BauteilDescriptionTextBox_TextChanged;
            layout.Add(_bauteilDescriptionTextBox, true);
            layout.EndHorizontal();
            layout.EndGroup();

            // Calculations section
            layout.BeginGroup("Calculations", new Padding(5));
            _totalThicknessLabel = new Label { Text = "Total Thickness: 0.0 mm" };
            _totalWeightLabel = new Label { Text = "Weight: 0.0 kg/m²" };
            _paintConsumptionLabel = new Label { Text = "Paint: 0.0 ml/m²" };
            
            layout.Add(_totalThicknessLabel);
            layout.Add(_totalWeightLabel);
            layout.Add(_paintConsumptionLabel);
            layout.EndGroup();

            layout.EndVertical();
            return layout;
        }

        private Control CreateBottomPanel()
        {
            // Create horizontal splitter for layers and edges
            var horizontalSplitter = new Splitter
            {
                Orientation = Orientation.Horizontal,
                Position = 250 // Initial position
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
            var layout = new DynamicLayout();
            layout.BeginVertical(spacing: new Size(5, 5), padding: new Padding(5));
            
            layout.BeginGroup("Material Layers", new Padding(5));
            
            // Create editable GridView for material layers
            _schichtenGridView = new GridView();
            _schichtenGridView.ShowHeader = true;
            _schichtenGridView.AllowMultipleSelection = false;
            _schichtenGridView.GridLines = GridLines.Both;
            _schichtenGridView.Size = new Size(-1, 150); // Give it a minimum height
            InitializeSchichtenGridView();
            layout.Add(_schichtenGridView, true); // Use true to expand to full width

            // Material layer buttons - smaller sizes
            layout.BeginHorizontal();
            _addSchichtButton = new Button { Text = "Add", Size = new Size(40, 20) };
            _addSchichtButton.Click += AddSchichtButton_Click;
            _removeSchichtButton = new Button { Text = "Remove", Size = new Size(50, 20) };
            _removeSchichtButton.Click += RemoveSchichtButton_Click;
            _editSchichtButton = new Button { Text = "Edit", Size = new Size(40, 20) };
            _editSchichtButton.Click += EditSchichtButton_Click;
            layout.Add(_addSchichtButton);
            layout.Add(_removeSchichtButton);
            layout.Add(_editSchichtButton);
            layout.Add(null, true); // Spacer
            layout.EndHorizontal();
            
            layout.EndGroup();
            layout.EndVertical();
            return layout;
        }

        private Control CreateEdgesPanel()
        {
            var layout = new DynamicLayout();
            layout.BeginVertical(spacing: new Size(5, 5), padding: new Padding(5));
            
            layout.BeginGroup("Edge Configuration", new Padding(5));
            
            // Create editable GridView for edges
            _kantenGridView = new GridView();
            _kantenGridView.ShowHeader = true;
            _kantenGridView.AllowMultipleSelection = false;
            _kantenGridView.GridLines = GridLines.Both;
            _kantenGridView.Size = new Size(-1, 120); // Give it a minimum height
            InitializeKantenGridView();
            layout.Add(_kantenGridView, true); // Use true to expand to full width

            // Edge buttons - smaller sizes
            layout.BeginHorizontal();
            _addKanteButton = new Button { Text = "Add", Size = new Size(40, 20) };
            _addKanteButton.Click += AddKanteButton_Click;
            _removeKanteButton = new Button { Text = "Remove", Size = new Size(50, 20) };
            _removeKanteButton.Click += RemoveKanteButton_Click;
            _editKanteButton = new Button { Text = "Edit", Size = new Size(40, 20) };
            _editKanteButton.Click += EditKanteButton_Click;
            layout.Add(_addKanteButton);
            layout.Add(_removeKanteButton);
            layout.Add(_editKanteButton);
            layout.Add(null, true); // Spacer
            layout.EndHorizontal();
            
            layout.EndGroup();
            layout.EndVertical();
            return layout;
        }

        /// <summary>
        /// Initialize the material layers grid view with editable cells
        /// </summary>
        private void InitializeSchichtenGridView()
        {
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Layer Name",
                DataCell = new TextBoxCell("SchichtName"),
                Width = 100
            });

            var materialDropDown = new ComboBoxCell("MaterialType")
            {
                DataStore = Enum.GetValues(typeof(MaterialType)).Cast<MaterialType>()
                    .Select(m => new { Key = m, Value = GetMaterialTypeDisplayName(m) })
            };
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Material",
                DataCell = materialDropDown,
                Width = 100
            });

            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Thickness (mm)",
                DataCell = new TextBoxCell("DickeText"),
                Width = 80
            });

            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Density (kg/m³)",
                DataCell = new TextBoxCell("DichteText"),
                Width = 90
            });

            var grainDropDown = new ComboBoxCell("Laufrichtung")
            {
                DataStore = Enum.GetValues(typeof(GrainDirection)).Cast<GrainDirection>()
                    .Select(g => new { Key = g, Value = g.ToString() })
            };
            _schichtenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Grain",
                DataCell = grainDropDown,
                Width = 60
            });

            _schichtenGridView.CellEdited += SchichtenGridView_CellEdited;
        }

        /// <summary>
        /// Initialize the edge configuration grid view with editable cells
        /// </summary>
        private void InitializeKantenGridView()
        {
            var edgeTypeDropDown = new ComboBoxCell("KantenTyp")
            {
                DataStore = Enum.GetValues(typeof(EdgeType)).Cast<EdgeType>()
                    .Select(e => new { Key = e, Value = GetEdgeTypeDisplayName(e) })
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
                    .Select(p => new { Key = p, Value = GetProcessingTypeDisplayName(p) })
            };
            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Processing",
                DataCell = processingDropDown,
                Width = 100
            });

            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Thickness (mm)",
                DataCell = new TextBoxCell("DickeText"),
                Width = 80
            });

            _kantenGridView.Columns.Add(new GridColumn
            {
                HeaderText = "Visible",
                DataCell = new CheckBoxCell("IsVisible"),
                Width = 50
            });

            _kantenGridView.CellEdited += KantenGridView_CellEdited;
        }

        /// <summary>
        /// Initialize data and create default bauteil with proper material layers and edges
        /// </summary>
        private void InitializeData()
        {
            // Create a default bauteil with realistic structure
            var defaultBauteil = new Bauteil("Spanplatte mit Melamin");
            
            // Add realistic material layers for a melamine-faced chipboard
            // Core layer (Spanplatte)
            var coreLayer = new MaterialSchicht("Spanplatte Kern", 16.0, "Chipboard", 650.0, GrainDirection.X);
            coreLayer.Material = new BauteilMaterial("Spanplatte", MaterialType.Chipboard, 16.0, 650.0);
            defaultBauteil.AddSchicht(coreLayer);
            
            // Top surface layer (Melamin)
            var topLayer = new MaterialSchicht("Melamin Oberseite", 0.8, "MDF", 1200.0, GrainDirection.X);
            topLayer.Material = new BauteilMaterial("Melamin Dekor", MaterialType.MDF, 0.8, 1200.0);
            defaultBauteil.AddSchicht(topLayer);
            
            // Bottom surface layer (Melamin)
            var bottomLayer = new MaterialSchicht("Melamin Unterseite", 0.8, "MDF", 1200.0, GrainDirection.X);
            bottomLayer.Material = new BauteilMaterial("Melamin Dekor", MaterialType.MDF, 0.8, 1200.0);
            defaultBauteil.AddSchicht(bottomLayer);

            // Add only 4 standard edges for a panel (not 6 like a full box)
            defaultBauteil.Kantenbilder.Add(new Kantenbild(EdgeType.Top, EdgeProcessingType.EdgeBanded));
            defaultBauteil.Kantenbilder.Add(new Kantenbild(EdgeType.Bottom, EdgeProcessingType.EdgeBanded));
            defaultBauteil.Kantenbilder.Add(new Kantenbild(EdgeType.Front, EdgeProcessingType.EdgeBanded));
            defaultBauteil.Kantenbilder.Add(new Kantenbild(EdgeType.Back, EdgeProcessingType.Raw));

            _bauteile.Add(defaultBauteil);
            _currentBauteil = defaultBauteil;

            UpdateUI();
        }

        /// <summary>
        /// Update the user interface with current data
        /// </summary>
        private void UpdateUI()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            try
            {
                // Update bauteil dropdown
                _bauteilDropDown.DataStore = _bauteile.Select(b => b.Name).ToList();
                if (_currentBauteil != null)
                {
                    _bauteilDropDown.SelectedValue = _currentBauteil.Name;
                    _bauteilNameTextBox.Text = _currentBauteil.Name;
                    _bauteilDescriptionTextBox.Text = _currentBauteil.BauteilDescription ?? "";
                }

                UpdateSchichtenGridView();
                UpdateKantenGridView();
                UpdateCalculations();

                // Enable/disable Apply button based on whether we have a linked object
                _applyToObjectButton.Enabled = (_currentRhinoObject != null);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>
        /// Update the material layers grid view with editable items
        /// </summary>
        private void UpdateSchichtenGridView()
        {
            if (_currentBauteil == null) return;

            var schichtData = _currentBauteil.Schichten.Select(s => new SchichtEditItem
            {
                SchichtName = s.SchichtName,
                MaterialType = s.Material.Typ,
                DickeText = s.Dicke.ToString("F1"),
                DichteText = s.Dichte.ToString("F0"),
                Laufrichtung = s.Laufrichtung,
                OriginalSchicht = s
            }).ToList();

            _schichtenGridView.DataStore = schichtData;
        }

        /// <summary>
        /// Update the edge configuration grid view with editable items
        /// </summary>
        private void UpdateKantenGridView()
        {
            if (_currentBauteil == null) return;

            var kantenData = _currentBauteil.Kantenbilder.Select(k => new KanteEditItem
            {
                KantenTyp = k.KantenTyp,
                BearbeitungsTyp = k.BearbeitungsTyp,
                DickeText = k.Dicke.ToString("F1"),
                IsVisible = k.IsVisible,
                OriginalKante = k
            }).ToList();

            _kantenGridView.DataStore = kantenData;
        }

        /// <summary>
        /// Update calculations display
        /// </summary>
        private void UpdateCalculations()
        {
            if (_currentBauteil == null) return;

            double totalThickness = _currentBauteil.GetTotalThickness();
            _totalThicknessLabel.Text = $"Total Thickness: {totalThickness:F1} mm";

            double weightPerM2 = _currentBauteil.GetWeightPerSquareMeter();
            _totalWeightLabel.Text = $"Weight: {weightPerM2:F2} kg/m²";

            double paintPerM2 = _currentBauteil.GetPaintConsumptionPerSquareMeter();
            _paintConsumptionLabel.Text = $"Paint: {paintPerM2:F1} ml/m²";
        }

        #region Event Handlers

        private void BauteilDropDown_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_isUpdating) return;

            var selectedName = _bauteilDropDown.SelectedValue as string;
            if (!string.IsNullOrEmpty(selectedName))
            {
                _currentBauteil = _bauteile.FirstOrDefault(b => b.Name == selectedName);
                _currentRhinoObject = null; // Clear object reference when switching bauteile
                UpdateUI();
            }
        }

        private void BauteilNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_currentBauteil != null && !_isUpdating)
            {
                _currentBauteil.Name = _bauteilNameTextBox.Text;
                _currentBauteil.ModifiedDate = DateTime.Now;
                NotifyBauteilChanged();
            }
        }

        private void BauteilDescriptionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_currentBauteil != null && !_isUpdating)
            {
                _currentBauteil.BauteilDescription = _bauteilDescriptionTextBox.Text;
                _currentBauteil.ModifiedDate = DateTime.Now;
                NotifyBauteilChanged();
            }
        }

        private void SchichtenGridView_CellEdited(object sender, GridViewCellEventArgs e)
        {
            if (_currentBauteil == null || _isUpdating) return;

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

                UpdateCalculations();
                NotifyBauteilChanged();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error editing material layer: {ex.Message}");
            }
        }

        private void KantenGridView_CellEdited(object sender, GridViewCellEventArgs e)
        {
            if (_currentBauteil == null || _isUpdating) return;

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

                NotifyBauteilChanged();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error editing edge: {ex.Message}");
            }
        }

        private void AddBauteilButton_Click(object sender, EventArgs e)
        {
            var newBauteil = new Bauteil($"New Component {_bauteile.Count + 1}");
            _bauteile.Add(newBauteil);
            _currentBauteil = newBauteil;
            _currentRhinoObject = null;
            UpdateUI();
        }

        private void RemoveBauteilButton_Click(object sender, EventArgs e)
        {
            if (_currentBauteil != null && _bauteile.Count > 1)
            {
                _bauteile.Remove(_currentBauteil);
                _currentBauteil = _bauteile.FirstOrDefault();
                _currentRhinoObject = null;
                UpdateUI();
            }
        }



        private void ApplyToObjectButton_Click(object sender, EventArgs e)
        {
            if (_currentBauteil == null || _currentRhinoObject == null) return;

            try
            {
                // Remove existing bauteil data
                var existingData = _currentRhinoObject.UserData.Find(typeof(Bauteil)) as Bauteil;
                if (existingData != null)
                {
                    _currentRhinoObject.UserData.Remove(existingData);
                }

                // Create a new bauteil with the same data but new ID (avoid reference issues)
                var bauteilToApply = new Bauteil(_currentBauteil.Name)
                {
                    BauteilDescription = _currentBauteil.BauteilDescription,
                    Schichten = _currentBauteil.Schichten.Select(s => s.Clone()).ToList(),
                    Kantenbilder = _currentBauteil.Kantenbilder.Select(k => k.Clone()).ToList(),
                    CreatedDate = _currentBauteil.CreatedDate,
                    ModifiedDate = DateTime.Now
                };
                
                // Add the bauteil to the object
                _currentRhinoObject.UserData.Add(bauteilToApply);
                
                // Commit changes
                _currentRhinoObject.CommitChanges();
                RhinoDoc.ActiveDoc.Views.Redraw();

                RhinoApp.WriteLine($"Applied Bauteil '{_currentBauteil.Name}' to object.");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error applying bauteil to object: {ex.Message}");
            }
        }

        private void AddSchichtButton_Click(object sender, EventArgs e)
        {
            if (_currentBauteil != null)
            {
                var newSchicht = new MaterialSchicht($"Layer {_currentBauteil.Schichten.Count + 1}", 18.0, "MDF");
                _currentBauteil.AddSchicht(newSchicht);
                UpdateUI();
                NotifyBauteilChanged();
            }
        }

        private void RemoveSchichtButton_Click(object sender, EventArgs e)
        {
            if (_currentBauteil != null && _schichtenGridView.SelectedItem != null)
            {
                var selectedItem = _schichtenGridView.SelectedItem as SchichtEditItem;
                if (selectedItem?.OriginalSchicht != null)
                {
                    _currentBauteil.Schichten.Remove(selectedItem.OriginalSchicht);
                    UpdateUI();
                    NotifyBauteilChanged();
                }
            }
        }

        private void EditSchichtButton_Click(object sender, EventArgs e)
        {
            // TODO: Open detailed material layer editor dialog
            MessageBox.Show("Detailed layer editor will be implemented in future version.", "Edit Layer", MessageBoxType.Information);
        }

        private void AddKanteButton_Click(object sender, EventArgs e)
        {
            if (_currentBauteil != null)
            {
                var newKante = new Kantenbild
                {
                    KantenTyp = EdgeType.Top,
                    BearbeitungsTyp = EdgeProcessingType.Raw
                };
                _currentBauteil.Kantenbilder.Add(newKante);
                UpdateUI();
                NotifyBauteilChanged();
            }
        }

        private void RemoveKanteButton_Click(object sender, EventArgs e)
        {
            if (_currentBauteil != null && _kantenGridView.SelectedItem != null)
            {
                var selectedItem = _kantenGridView.SelectedItem as KanteEditItem;
                if (selectedItem?.OriginalKante != null)
                {
                    _currentBauteil.Kantenbilder.Remove(selectedItem.OriginalKante);
                    UpdateUI();
                    NotifyBauteilChanged();
                }
            }
        }

        private void EditKanteButton_Click(object sender, EventArgs e)
        {
            // TODO: Open detailed edge editor dialog
            MessageBox.Show("Detailed edge editor will be implemented in future version.", "Edit Edge", MessageBoxType.Information);
        }

        #endregion

        #region Helper Methods

        private void NotifyBauteilChanged()
        {
            if (_currentBauteil != null)
            {
                BauteilDataManager.NotifyBauteilChanged(_currentBauteil);
            }
        }

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

        #endregion

        #region Helper Classes

        // Use the same classes as PropertyPage for consistency
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

        #endregion
    }
} 