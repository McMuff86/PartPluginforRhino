using System;
using System.Collections.Generic;

namespace BauteilPlugin.Models
{
    /// <summary>
    /// Represents a material with all its properties
    /// Used for defining material characteristics in building components
    /// </summary>
    public class Material
    {
        /// <summary>
        /// Unique identifier for the material
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Name of the material
        /// </summary>
        public string Name { get; set; } = "Neues Material";

        /// <summary>
        /// Type/Category of the material
        /// </summary>
        public MaterialType Typ { get; set; } = MaterialType.Wood;

        /// <summary>
        /// Manufacturer or supplier of the material
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// Article number or product code
        /// </summary>
        public string ArticleNumber { get; set; } = string.Empty;

        /// <summary>
        /// Material description or specification
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Standard thickness available for this material
        /// </summary>
        public double StandardThickness { get; set; } = 18.0;

        /// <summary>
        /// Standard density of the material in kg/m³
        /// </summary>
        public double StandardDensity { get; set; } = 650.0;

        /// <summary>
        /// Cost per unit (e.g., per m², per m³, per piece)
        /// </summary>
        public double CostPerUnit { get; set; } = 0.0;

        /// <summary>
        /// Unit of measurement for costing
        /// </summary>
        public string CostUnit { get; set; } = "m²";

        /// <summary>
        /// Color for visual representation
        /// </summary>
        public System.Drawing.Color Color { get; set; } = System.Drawing.Color.SandyBrown;

        /// <summary>
        /// Surface texture description
        /// </summary>
        public string SurfaceTexture { get; set; } = string.Empty;

        /// <summary>
        /// Whether this material is active/available
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the material was added
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Additional notes or comments
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Material()
        {
        }

        /// <summary>
        /// Constructor with material name
        /// </summary>
        /// <param name="name">Name of the material</param>
        public Material(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Constructor with name and type
        /// </summary>
        /// <param name="name">Name of the material</param>
        /// <param name="typ">Type of the material</param>
        public Material(string name, MaterialType typ)
        {
            Name = name;
            Typ = typ;
            SetDefaultPropertiesForType(typ);
        }

        /// <summary>
        /// Constructor with all basic parameters
        /// </summary>
        /// <param name="name">Name of the material</param>
        /// <param name="typ">Type of the material</param>
        /// <param name="standardThickness">Standard thickness in mm</param>
        /// <param name="standardDensity">Standard density in kg/m³</param>
        public Material(string name, MaterialType typ, double standardThickness, double standardDensity)
        {
            Name = name;
            Typ = typ;
            StandardThickness = standardThickness;
            StandardDensity = standardDensity;
            SetDefaultPropertiesForType(typ);
        }

        /// <summary>
        /// Set default properties based on material type
        /// </summary>
        /// <param name="typ">Material type</param>
        public void SetDefaultPropertiesForType(MaterialType typ)
        {
            switch (typ)
            {
                case MaterialType.Wood:
                    Color = System.Drawing.Color.SandyBrown;
                    if (StandardDensity <= 0) StandardDensity = 650.0;
                    if (StandardThickness <= 0) StandardThickness = 18.0;
                    CostUnit = "m²";
                    break;

                case MaterialType.Plywood:
                    Color = System.Drawing.Color.BurlyWood;
                    if (StandardDensity <= 0) StandardDensity = 700.0;
                    if (StandardThickness <= 0) StandardThickness = 18.0;
                    CostUnit = "m²";
                    break;

                case MaterialType.MDF:
                    Color = System.Drawing.Color.RosyBrown;
                    if (StandardDensity <= 0) StandardDensity = 750.0;
                    if (StandardThickness <= 0) StandardThickness = 18.0;
                    CostUnit = "m²";
                    break;

                case MaterialType.Chipboard:
                    Color = System.Drawing.Color.Tan;
                    if (StandardDensity <= 0) StandardDensity = 650.0;
                    if (StandardThickness <= 0) StandardThickness = 18.0;
                    CostUnit = "m²";
                    break;

                case MaterialType.Metal:
                    Color = System.Drawing.Color.Silver;
                    if (StandardDensity <= 0) StandardDensity = 7850.0;
                    if (StandardThickness <= 0) StandardThickness = 2.0;
                    CostUnit = "m²";
                    break;

                case MaterialType.Plastic:
                    Color = System.Drawing.Color.LightGray;
                    if (StandardDensity <= 0) StandardDensity = 1000.0;
                    if (StandardThickness <= 0) StandardThickness = 10.0;
                    CostUnit = "m²";
                    break;

                case MaterialType.Glass:
                    Color = System.Drawing.Color.LightBlue;
                    if (StandardDensity <= 0) StandardDensity = 2500.0;
                    if (StandardThickness <= 0) StandardThickness = 6.0;
                    CostUnit = "m²";
                    break;

                case MaterialType.Composite:
                    Color = System.Drawing.Color.Gray;
                    if (StandardDensity <= 0) StandardDensity = 1500.0;
                    if (StandardThickness <= 0) StandardThickness = 15.0;
                    CostUnit = "m²";
                    break;

                case MaterialType.Other:
                    Color = System.Drawing.Color.LightGray;
                    if (StandardDensity <= 0) StandardDensity = 1000.0;
                    if (StandardThickness <= 0) StandardThickness = 10.0;
                    CostUnit = "m²";
                    break;
            }
        }

        /// <summary>
        /// Validate the material configuration
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Material name is required");

            if (StandardThickness <= 0)
                errors.Add("Standard thickness must be greater than 0");

            if (StandardDensity <= 0)
                errors.Add("Standard density must be greater than 0");

            if (CostPerUnit < 0)
                errors.Add("Cost per unit cannot be negative");

            return errors;
        }

        /// <summary>
        /// Create a deep copy of the material
        /// </summary>
        /// <returns>Deep copy of the material</returns>
        public Material Clone()
        {
            return new Material
            {
                Id = Guid.NewGuid(),
                Name = Name + " (Copy)",
                Typ = Typ,
                Manufacturer = Manufacturer,
                ArticleNumber = ArticleNumber,
                Description = Description,
                StandardThickness = StandardThickness,
                StandardDensity = StandardDensity,
                CostPerUnit = CostPerUnit,
                CostUnit = CostUnit,
                Color = Color,
                SurfaceTexture = SurfaceTexture,
                IsActive = IsActive,
                CreatedDate = DateTime.Now,
                Notes = Notes
            };
        }

        /// <summary>
        /// Convert to string representation
        /// </summary>
        /// <returns>String representation of the material</returns>
        public override string ToString()
        {
            return $"{Name} ({Typ}, {StandardThickness:F1}mm, {StandardDensity:F0}kg/m³)";
        }
    }

    /// <summary>
    /// Defines the type/category of a material
    /// </summary>
    public enum MaterialType
    {
        /// <summary>
        /// Solid wood materials
        /// </summary>
        Wood,

        /// <summary>
        /// Plywood materials
        /// </summary>
        Plywood,

        /// <summary>
        /// Medium-density fiberboard
        /// </summary>
        MDF,

        /// <summary>
        /// Chipboard/particleboard
        /// </summary>
        Chipboard,

        /// <summary>
        /// Metal materials (steel, aluminum, etc.)
        /// </summary>
        Metal,

        /// <summary>
        /// Plastic materials
        /// </summary>
        Plastic,

        /// <summary>
        /// Glass materials
        /// </summary>
        Glass,

        /// <summary>
        /// Composite materials
        /// </summary>
        Composite,

        /// <summary>
        /// Other materials not covered above
        /// </summary>
        Other
    }
} 