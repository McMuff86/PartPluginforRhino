using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using BauteilPlugin.Models;

namespace BauteilPlugin.UI
{
    /// <summary>
    /// Native panel for edge search integrated into BauteilPropertyPage
    /// </summary>
    public class EdgeSearchPanel : Panel
    {
        private RadioButton _edgeTypeRadio;
        private RadioButton _processingTypeRadio;
        private TextBox _searchTextBox;
        private ListBox _resultsList;
        private Button _applyButton;
        private Button _cancelButton;
        private Label _statusLabel;
        private List<EdgeSearchOption> _allOptions;
        private List<EdgeSearchOption> _filteredOptions;
        private bool _isEdgeType = true;

        public event EventHandler<EdgeSearchResultEventArgs> EdgeApplied;
        public event EventHandler SearchCancelled;

        public EdgeSearchPanel()
        {
            InitializeComponent();
            LoadOptions();
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
                Text = "Kanten suchen",
                Font = new Eto.Drawing.Font(SystemFonts.Default().Family, 11, FontStyle.Bold),
                TextColor = SystemColors.ControlText
            };
            layout.Rows.Add(new TableRow(headerLabel) { ScaleHeight = false });

            // Type selection with better grouping
            var radioGroupBox = new GroupBox
            {
                Text = "Suchtyp",
                Padding = new Padding(8, 5)
            };
            
            var radioLayout = new TableLayout();
            radioLayout.Spacing = new Size(3, 3);
            
            _edgeTypeRadio = new RadioButton
            {
                Text = "Kantentyp (Oben, Unten, Links, Rechts, etc.)",
                Checked = true
            };
            _edgeTypeRadio.CheckedChanged += OnTypeChanged;
            radioLayout.Rows.Add(new TableRow(_edgeTypeRadio) { ScaleHeight = false });

            _processingTypeRadio = new RadioButton(_edgeTypeRadio)
            {
                Text = "Bearbeitungstyp (Roh, Bekantet, Massiv, etc.)"
            };
            _processingTypeRadio.CheckedChanged += OnTypeChanged;
            radioLayout.Rows.Add(new TableRow(_processingTypeRadio) { ScaleHeight = false });
            
            radioGroupBox.Content = radioLayout;
            layout.Rows.Add(new TableRow(new TableCell(radioGroupBox, true)) { ScaleHeight = false });

            // Search box with full width
            _searchTextBox = new TextBox
            {
                PlaceholderText = "Begriff eingeben... (z.B. top, bottom, roh, bekantet)",
                Height = 24
            };
            _searchTextBox.TextChanged += OnSearchTextChanged;
            _searchTextBox.KeyDown += OnSearchKeyDown;
            layout.Rows.Add(new TableRow(new TableCell(_searchTextBox, true)) { ScaleHeight = false });

            // Status with improved styling
            _statusLabel = new Label
            {
                Text = "Verfügbare Kantentypen:",
                Font = new Eto.Drawing.Font(SystemFonts.Default().Family, 9, FontStyle.Italic),
                TextColor = SystemColors.DisabledText
            };
            layout.Rows.Add(new TableRow(_statusLabel) { ScaleHeight = false });

            // Results list with proper sizing
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

        private void LoadOptions()
        {
            _allOptions = new List<EdgeSearchOption>();
            LoadEdgeTypes();
            LoadProcessingTypes();
        }

        private void LoadEdgeTypes()
        {
            _allOptions.AddRange(new[]
            {
                new EdgeSearchOption { Name = "Top / Oben", EnumValue = (int)EdgeType.Top, Keywords = new[] { "top", "oben", "obere", "oberseite" }, IsEdgeType = true },
                new EdgeSearchOption { Name = "Bottom / Unten", EnumValue = (int)EdgeType.Bottom, Keywords = new[] { "bottom", "unten", "untere", "unterseite" }, IsEdgeType = true },
                new EdgeSearchOption { Name = "Front / Vorne", EnumValue = (int)EdgeType.Front, Keywords = new[] { "front", "vorne", "vorder", "vorderseite" }, IsEdgeType = true },
                new EdgeSearchOption { Name = "Back / Hinten", EnumValue = (int)EdgeType.Back, Keywords = new[] { "back", "hinten", "hinter", "hinterseite", "rückseite" }, IsEdgeType = true },
                new EdgeSearchOption { Name = "Left / Links", EnumValue = (int)EdgeType.Left, Keywords = new[] { "left", "links", "linke", "linkseite" }, IsEdgeType = true },
                new EdgeSearchOption { Name = "Right / Rechts", EnumValue = (int)EdgeType.Right, Keywords = new[] { "right", "rechts", "rechte", "rechtseite" }, IsEdgeType = true }
            });
        }

        private void LoadProcessingTypes()
        {
            _allOptions.AddRange(new[]
            {
                new EdgeSearchOption { Name = "Raw / Roh", EnumValue = (int)EdgeProcessingType.Raw, Keywords = new[] { "raw", "roh", "unbearbeitet", "natur" }, IsEdgeType = false },
                new EdgeSearchOption { Name = "EdgeBanded / Bekantet", EnumValue = (int)EdgeProcessingType.EdgeBanded, Keywords = new[] { "edgebanded", "bekantet", "umleimer", "kantenbandung" }, IsEdgeType = false },
                new EdgeSearchOption { Name = "Solid / Massiv", EnumValue = (int)EdgeProcessingType.Solid, Keywords = new[] { "solid", "massiv", "vollholz", "massive" }, IsEdgeType = false },
                new EdgeSearchOption { Name = "Rounded / Gerundet", EnumValue = (int)EdgeProcessingType.Rounded, Keywords = new[] { "rounded", "gerundet", "abgerundet", "rund" }, IsEdgeType = false },
                new EdgeSearchOption { Name = "Chamfered / Gefast", EnumValue = (int)EdgeProcessingType.Chamfered, Keywords = new[] { "chamfered", "gefast", "fase", "angefast" }, IsEdgeType = false },
                new EdgeSearchOption { Name = "Grooved / Nut", EnumValue = (int)EdgeProcessingType.Grooved, Keywords = new[] { "grooved", "nut", "nute", "genutet" }, IsEdgeType = false },
                new EdgeSearchOption { Name = "Profiled / Profiliert", EnumValue = (int)EdgeProcessingType.Profiled, Keywords = new[] { "profiled", "profiliert", "profil", "profilbearbeitung" }, IsEdgeType = false },
                new EdgeSearchOption { Name = "Laminated / Laminiert", EnumValue = (int)EdgeProcessingType.Laminated, Keywords = new[] { "laminated", "laminiert", "laminat", "beschichtet" }, IsEdgeType = false }
            });
        }

        private void OnTypeChanged(object sender, EventArgs e)
        {
            _isEdgeType = _edgeTypeRadio.Checked;
            UpdateSearchPlaceholder();
            OnSearchTextChanged(null, null); // Refresh results
        }

        private void UpdateSearchPlaceholder()
        {
            _searchTextBox.PlaceholderText = _isEdgeType ? 
                "Kantentyp suchen... (z.B. top, bottom, oben, unten)" : 
                "Bearbeitungstyp suchen... (z.B. raw, banded, roh, bekantet)";
        }

        private void PopulateInitialResults()
        {
            var currentOptions = _allOptions.Where(o => o.IsEdgeType == _isEdgeType).ToList();
            UpdateResultsList(currentOptions);
        }

        private void OnSearchTextChanged(object sender, EventArgs e)
        {
            var searchTerm = _searchTextBox.Text?.Trim().ToLower();
            var currentOptions = _allOptions.Where(o => o.IsEdgeType == _isEdgeType).ToList();
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                UpdateResultsList(currentOptions);
                _statusLabel.Text = _isEdgeType ? "Verfügbare Kantentypen:" : "Verfügbare Bearbeitungstypen:";
                return;
            }

            var results = currentOptions.Where(option => 
                option.Name.ToLower().Contains(searchTerm) ||
                option.Keywords.Any(keyword => keyword.Contains(searchTerm))
            ).ToList();
            
            UpdateResultsList(results);
            _statusLabel.Text = $"{results.Count} Optionen gefunden:";
        }

        private void UpdateResultsList(List<EdgeSearchOption> options)
        {
            _resultsList.Items.Clear();
            _filteredOptions = options;

            foreach (var option in options)
            {
                _resultsList.Items.Add(new ListItem
                {
                    Text = option.Name,
                    Tag = option
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
                    ApplySelectedOption();
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
                    ApplySelectedOption();
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
            ApplySelectedOption();
        }

        private void OnApplyClick(object sender, EventArgs e)
        {
            ApplySelectedOption();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            SearchCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void ApplySelectedOption()
        {
            var selectedIndex = _resultsList.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _filteredOptions.Count)
            {
                var selectedOption = _filteredOptions[selectedIndex];
                EdgeApplied?.Invoke(this, new EdgeSearchResultEventArgs(selectedOption, _isEdgeType));
            }
        }

        public void SetSearchType(bool isEdgeType)
        {
            _isEdgeType = isEdgeType;
            _edgeTypeRadio.Checked = isEdgeType;
            _processingTypeRadio.Checked = !isEdgeType;
            UpdateSearchPlaceholder();
            PopulateInitialResults();
        }

        public void FocusSearchBox()
        {
            _searchTextBox.Focus();
        }

        public void ClearSearch()
        {
            _searchTextBox.Text = "";
            PopulateInitialResults();
            _statusLabel.Text = _isEdgeType ? "Verfügbare Kantentypen:" : "Verfügbare Bearbeitungstypen:";
        }
    }

    /// <summary>
    /// Event arguments for edge search results
    /// </summary>
    public class EdgeSearchResultEventArgs : EventArgs
    {
        public EdgeSearchOption SelectedOption { get; }
        public bool IsEdgeType { get; }

        public EdgeSearchResultEventArgs(EdgeSearchOption selectedOption, bool isEdgeType)
        {
            SelectedOption = selectedOption;
            IsEdgeType = isEdgeType;
        }
    }

    /// <summary>
    /// Enhanced edge search option with type information
    /// </summary>
    public class EdgeSearchOption
    {
        public string Name { get; set; }
        public int EnumValue { get; set; }
        public string[] Keywords { get; set; }
        public bool IsEdgeType { get; set; }
    }
} 