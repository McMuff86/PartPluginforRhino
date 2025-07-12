using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using BauteilPlugin.Utils;

namespace BauteilPlugin.UI
{
    /// <summary>
    /// Native panel for material search integrated into BauteilPropertyPage
    /// </summary>
    public class MaterialSearchPanel : Panel
    {
        private TextBox _searchTextBox;
        private ListBox _resultsList;
        private Button _applyButton;
        private Button _cancelButton;
        private Label _statusLabel;
        private List<MaterialSearchItem> _allMaterials;
        private List<MaterialSearchItem> _filteredMaterials;
        private string _initialSearchText;

        public event EventHandler<MaterialSearchItem> MaterialApplied;
        public event EventHandler SearchCancelled;

        public MaterialSearchPanel()
        {
            InitializeComponent();
            LoadMaterials();
            PopulateInitialResults();
        }

        private void InitializeComponent()
        {
            this.Padding = new Padding(8);
            // Remove the explicit background color to inherit from parent
            // this.BackgroundColor = SystemColors.ControlBackground;

            var layout = new TableLayout();
            layout.Spacing = new Size(5, 5);

            // Header with subtle styling
            var headerLabel = new Label
            {
                Text = "Material suchen",
                Font = new Eto.Drawing.Font(SystemFonts.Default().Family, 11, FontStyle.Bold),
                TextColor = SystemColors.ControlText
            };
            layout.Rows.Add(new TableRow(headerLabel) { ScaleHeight = false });

            // Search box with full width
            _searchTextBox = new TextBox
            {
                PlaceholderText = "Material eingeben... (z.B. spanplatte, mdf, edelstahl)",
                Height = 24
            };
            _searchTextBox.TextChanged += OnSearchTextChanged;
            _searchTextBox.KeyDown += OnSearchKeyDown;
            layout.Rows.Add(new TableRow(new TableCell(_searchTextBox, true)) { ScaleHeight = false });

            // Status with improved styling
            _statusLabel = new Label
            {
                Text = "Verfügbare Materialien:",
                Font = new Eto.Drawing.Font(SystemFonts.Default().Family, 9, FontStyle.Italic),
                TextColor = SystemColors.DisabledText
            };
            layout.Rows.Add(new TableRow(_statusLabel) { ScaleHeight = false });

            // Results list with proper sizing - full width
            _resultsList = new ListBox
            {
                Height = 120
            };
            _resultsList.MouseDoubleClick += OnResultDoubleClick;
            _resultsList.KeyDown += OnResultKeyDown;
            layout.Rows.Add(new TableRow(new TableCell(_resultsList, true)) { ScaleHeight = true });

            // Buttons with equal width spanning full groupbox width
            var buttonLayout = new TableLayout();
            buttonLayout.Spacing = new Size(5, 0);
            
            _cancelButton = new Button
            {
                Text = "Abbrechen",
                Height = 26
            };
            _cancelButton.Click += OnCancelClick;
            
            _applyButton = new Button
            {
                Text = "Anwenden",
                Height = 26,
                Enabled = false
            };
            _applyButton.Click += OnApplyClick;
            
            buttonLayout.Rows.Add(new TableRow(
                new TableCell(_cancelButton, true),
                new TableCell(_applyButton, true)
            ) { ScaleHeight = false });
            
            layout.Rows.Add(new TableRow(new TableCell(buttonLayout, true)) { ScaleHeight = false });

            this.Content = layout;
        }

        private void LoadMaterials()
        {
            MaterialSearch.Initialize();
            _allMaterials = MaterialSearch.SearchMaterials("", 100);
            _filteredMaterials = new List<MaterialSearchItem>(_allMaterials);
        }

        private void PopulateInitialResults()
        {
            var topMaterials = _allMaterials.Take(20).ToList();
            UpdateResultsList(topMaterials);
        }

        private void OnSearchTextChanged(object sender, EventArgs e)
        {
            var searchTerm = _searchTextBox.Text?.Trim();
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                PopulateInitialResults();
                _statusLabel.Text = "Verfügbare Materialien:";
                return;
            }

            var results = MaterialSearch.SearchMaterials(searchTerm, 30);
            UpdateResultsList(results);
            _statusLabel.Text = $"{results.Count} Materialien gefunden:";
        }

        private void UpdateResultsList(List<MaterialSearchItem> materials)
        {
            _resultsList.Items.Clear();
            _filteredMaterials = materials;

            foreach (var material in materials)
            {
                var displayText = $"{material.Name} ({material.Type}, {material.Thickness}mm, {material.Density}kg/m³)";
                _resultsList.Items.Add(new ListItem
                {
                    Text = displayText,
                    Tag = material
                });
            }

            // Auto-select first item
            if (_resultsList.Items.Count > 0)
            {
                _resultsList.SelectedIndex = 0;
                _applyButton.Enabled = true;
            }
            else
            {
                _applyButton.Enabled = false;
            }
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Down:
                    if (_resultsList.Items.Count > 0)
                    {
                        _resultsList.Focus();
                        if (_resultsList.SelectedIndex < 0)
                            _resultsList.SelectedIndex = 0;
                    }
                    e.Handled = true;
                    break;
                    
                case Keys.Enter:
                    ApplySelectedMaterial();
                    e.Handled = true;
                    break;
                    
                case Keys.Escape:
                    OnCancelClick(null, null);
                    e.Handled = true;
                    break;
            }
        }

        private void OnResultKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Enter:
                    ApplySelectedMaterial();
                    e.Handled = true;
                    break;
                    
                case Keys.Escape:
                    _searchTextBox.Focus();
                    e.Handled = true;
                    break;
            }
        }

        private void OnResultDoubleClick(object sender, MouseEventArgs e)
        {
            ApplySelectedMaterial();
        }

        private void OnApplyClick(object sender, EventArgs e)
        {
            ApplySelectedMaterial();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            SearchCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void ApplySelectedMaterial()
        {
            var selectedIndex = _resultsList.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _filteredMaterials.Count)
            {
                var selectedMaterial = _filteredMaterials[selectedIndex];
                MaterialApplied?.Invoke(this, selectedMaterial);
            }
            else if (!string.IsNullOrEmpty(_searchTextBox.Text))
            {
                // Create custom material
                var customMaterial = new MaterialSearchItem
                {
                    Name = _searchTextBox.Text.Trim(),
                    Type = "Custom",
                    Density = 700.0f,
                    Thickness = 18.0f,
                    Keywords = new[] { _searchTextBox.Text.Trim().ToLower() }
                };
                MaterialApplied?.Invoke(this, customMaterial);
            }
        }

        public void SetInitialSearch(string searchText)
        {
            _initialSearchText = searchText;
            if (!string.IsNullOrEmpty(searchText))
            {
                _searchTextBox.Text = searchText;
                OnSearchTextChanged(null, null);
            }
        }

        public void FocusSearchBox()
        {
            _searchTextBox.Focus();
        }

        public void ClearSearch()
        {
            _searchTextBox.Text = "";
            PopulateInitialResults();
            _statusLabel.Text = "Verfügbare Materialien:";
        }
    }
} 