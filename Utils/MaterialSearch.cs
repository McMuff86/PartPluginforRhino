using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Rhino;
using BauteilPlugin.Models;

namespace BauteilPlugin.Utils
{
    /// <summary>
    /// Utility class for searching materials from external database
    /// </summary>
    public static class MaterialSearch
    {
        private static List<MaterialSearchItem> _materials = new List<MaterialSearchItem>();
        private static bool _isLoaded = false;
        private static string _materialsFilePath = "";

        /// <summary>
        /// Initialize the material search with the materials database
        /// </summary>
        public static void Initialize()
        {
            if (_isLoaded) return;

            try
            {
                // Try to find Materials.json in various locations
                var possiblePaths = new[]
                {
                    Path.Combine(Environment.CurrentDirectory, "Materials.json"),
                    Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Materials.json"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BauteilPlugin", "Materials.json"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BauteilPlugin", "Materials.json")
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        _materialsFilePath = path;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(_materialsFilePath))
                {
                    // Create default materials if file doesn't exist
                    CreateDefaultMaterialsFile();
                }

                LoadMaterials();
                _isLoaded = true;
                RhinoApp.WriteLine($"Material database loaded successfully with {_materials.Count} materials from: {_materialsFilePath}");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error loading material database: {ex.Message}");
                CreateFallbackMaterials();
                _isLoaded = true;
            }
        }

        /// <summary>
        /// Search for materials based on search term
        /// </summary>
        /// <param name="searchTerm">Term to search for</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>List of matching materials</returns>
        public static List<MaterialSearchItem> SearchMaterials(string searchTerm, int maxResults = 10)
        {
            if (!_isLoaded) Initialize();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return _materials.Take(maxResults).ToList();
            }

            var term = searchTerm.ToLower();
            var results = new List<MaterialSearchItem>();

            // First priority: exact name matches
            results.AddRange(_materials.Where(m => m.Name.ToLower().Contains(term)));

            // Second priority: keyword matches
            var keywordMatches = _materials.Where(m => 
                m.Keywords.Any(k => k.ToLower().Contains(term)) && 
                !results.Contains(m));
            results.AddRange(keywordMatches);

            // Third priority: type matches
            var typeMatches = _materials.Where(m => 
                m.Type.ToLower().Contains(term) && 
                !results.Contains(m));
            results.AddRange(typeMatches);

            return results.Take(maxResults).ToList();
        }

        /// <summary>
        /// Get all material names for autocomplete
        /// </summary>
        /// <returns>List of all material names</returns>
        public static List<string> GetAllMaterialNames()
        {
            if (!_isLoaded) Initialize();
            return _materials.Select(m => m.Name).ToList();
        }

        /// <summary>
        /// Get material by exact name
        /// </summary>
        /// <param name="name">Material name</param>
        /// <returns>Material search item or null if not found</returns>
        public static MaterialSearchItem GetMaterialByName(string name)
        {
            if (!_isLoaded) Initialize();
            return _materials.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Convert MaterialSearchItem to Material object
        /// </summary>
        /// <param name="searchItem">Search item to convert</param>
        /// <returns>Material object</returns>
        public static Material CreateMaterialFromSearchItem(MaterialSearchItem searchItem)
        {
            if (searchItem == null) return null;

            var materialType = ParseMaterialType(searchItem.Type);
            var material = new Material(searchItem.Name, materialType, searchItem.Thickness, searchItem.Density);
            
            return material;
        }

        private static void LoadMaterials()
        {
            var json = File.ReadAllText(_materialsFilePath);
            var data = JsonConvert.DeserializeObject<MaterialDatabase>(json);
            _materials = data.Materials;
        }

        private static void CreateDefaultMaterialsFile()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var pluginFolder = Path.Combine(documentsPath, "BauteilPlugin");
            
            if (!Directory.Exists(pluginFolder))
            {
                Directory.CreateDirectory(pluginFolder);
            }

            _materialsFilePath = Path.Combine(pluginFolder, "Materials.json");
            
            // Create a basic materials file
            var defaultMaterials = new MaterialDatabase
            {
                Materials = new List<MaterialSearchItem>
                {
                    new MaterialSearchItem { Name = "Spanplatte 16mm", Type = "Chipboard", Density = 650, Thickness = 16, Keywords = new[] { "spanplatte", "chipboard", "standard" } },
                    new MaterialSearchItem { Name = "MDF 16mm", Type = "MDF", Density = 750, Thickness = 16, Keywords = new[] { "mdf", "faserplatte", "glatt" } },
                    new MaterialSearchItem { Name = "Multiplex 18mm", Type = "Plywood", Density = 700, Thickness = 18, Keywords = new[] { "multiplex", "sperrholz", "stabil" } },
                    new MaterialSearchItem { Name = "Buche Massiv", Type = "Wood", Density = 720, Thickness = 20, Keywords = new[] { "buche", "massiv", "hartholz" } },
                    new MaterialSearchItem { Name = "Aluminium 2mm", Type = "Metal", Density = 2700, Thickness = 2, Keywords = new[] { "aluminium", "metall", "leicht" } }
                }
            };

            var json = JsonConvert.SerializeObject(defaultMaterials, Formatting.Indented);
            File.WriteAllText(_materialsFilePath, json);
            RhinoApp.WriteLine($"Created default materials file at: {_materialsFilePath}");
        }

        private static void CreateFallbackMaterials()
        {
            _materials = new List<MaterialSearchItem>
            {
                new MaterialSearchItem { Name = "Spanplatte 16mm", Type = "Chipboard", Density = 650, Thickness = 16, Keywords = new[] { "spanplatte", "chipboard" } },
                new MaterialSearchItem { Name = "MDF 16mm", Type = "MDF", Density = 750, Thickness = 16, Keywords = new[] { "mdf", "faserplatte" } },
                new MaterialSearchItem { Name = "Multiplex 18mm", Type = "Plywood", Density = 700, Thickness = 18, Keywords = new[] { "multiplex", "sperrholz" } },
                new MaterialSearchItem { Name = "Buche Massiv", Type = "Wood", Density = 720, Thickness = 20, Keywords = new[] { "buche", "massiv" } },
                new MaterialSearchItem { Name = "Aluminium 2mm", Type = "Metal", Density = 2700, Thickness = 2, Keywords = new[] { "aluminium", "metall" } }
            };
        }

        private static MaterialType ParseMaterialType(string typeString)
        {
            if (Enum.TryParse<MaterialType>(typeString, true, out var result))
            {
                return result;
            }
            return MaterialType.Other;
        }
    }

    /// <summary>
    /// Material database structure for JSON deserialization
    /// </summary>
    public class MaterialDatabase
    {
        [JsonProperty("materials")]
        public List<MaterialSearchItem> Materials { get; set; } = new List<MaterialSearchItem>();
    }

    /// <summary>
    /// Material search item structure
    /// </summary>
    public class MaterialSearchItem
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("density")]
        public double Density { get; set; } = 0;

        [JsonProperty("thickness")]
        public double Thickness { get; set; } = 0;

        [JsonProperty("keywords")]
        public string[] Keywords { get; set; } = new string[0];

        public override string ToString()
        {
            return $"{Name} ({Type}, {Thickness}mm, {Density}kg/m³)";
        }
    }
} 