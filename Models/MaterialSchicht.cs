using System;
using System.Collections.Generic;

namespace BauteilPlugin.Models
{
    /// <summary>
    /// Represents a material layer within a building component
    /// Contains all material properties needed for CNC production and weight calculations
    /// </summary>
    public class MaterialSchicht
    {
        /// <summary>
        /// Unique identifier for the material layer
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Name of the material layer
        /// </summary>
        public string SchichtName { get; set; } = "Neue Schicht";

        /// <summary>
        /// Material information
        /// </summary>
        public Material Material { get; set; } = new Material();

        /// <summary>
        /// Thickness of the layer in millimeters
        /// </summary>
        public double Dicke { get; set; } = 18.0;

        /// <summary>
        /// Grain direction or orientation of the material
        /// </summary>
        public GrainDirection Laufrichtung { get; set; } = GrainDirection.X;

        /// <summary>
        /// Custom angle for grain direction (used when Laufrichtung is Custom)
        /// </summary>
        public double CustomAngle { get; set; } = 0.0;

        /// <summary>
        /// Density of the material in kg/m³
        /// </summary>
        public double Dichte { get; set; } = 650.0;

        /// <summary>
        /// Paint consumption per square meter in ml/m² (optional)
        /// </summary>
        public double Lackmenge { get; set; } = 0.0;

        /// <summary>
        /// Order/position of the layer (0 = bottom layer)
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// Whether this layer is visible in the 3D model
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Color for visual representation (RGB values)
        /// </summary>
        public System.Drawing.Color LayerColor { get; set; } = System.Drawing.Color.SandyBrown;

        /// <summary>
        /// Additional notes or comments for the layer
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MaterialSchicht()
        {
        }

        /// <summary>
        /// Constructor with basic parameters
        /// </summary>
        /// <param name="schichtName">Name of the layer</param>
        /// <param name="dicke">Thickness in mm</param>
        /// <param name="materialName">Material name</param>
        public MaterialSchicht(string schichtName, double dicke, string materialName)
        {
            SchichtName = schichtName;
            Dicke = dicke;
            Material = new Material(materialName);
        }

        /// <summary>
        /// Constructor with all basic parameters
        /// </summary>
        /// <param name="schichtName">Name of the layer</param>
        /// <param name="dicke">Thickness in mm</param>
        /// <param name="materialName">Material name</param>
        /// <param name="dichte">Density in kg/m³</param>
        /// <param name="laufrichtung">Grain direction</param>
        public MaterialSchicht(string schichtName, double dicke, string materialName, double dichte, GrainDirection laufrichtung)
            : this(schichtName, dicke, materialName)
        {
            Dichte = dichte;
            Laufrichtung = laufrichtung;
        }

        /// <summary>
        /// Calculate weight of this layer for a given volume
        /// </summary>
        /// <param name="volume">Volume in mm³</param>
        /// <returns>Weight in kg</returns>
        public double CalculateWeight(double volume)
        {
            if (volume <= 0 || Dichte <= 0)
                return 0;

            // Convert mm³ to m³, then calculate weight
            double volumeInM3 = volume / 1000000000;
            return volumeInM3 * Dichte;
        }

        /// <summary>
        /// Calculate paint consumption for this layer
        /// </summary>
        /// <param name="surfaceArea">Surface area in mm²</param>
        /// <returns>Paint consumption in ml</returns>
        public double CalculatePaintConsumption(double surfaceArea)
        {
            if (surfaceArea <= 0 || Lackmenge <= 0)
                return 0;

            // Convert mm² to m²
            double surfaceAreaInM2 = surfaceArea / 1000000;
            return surfaceAreaInM2 * Lackmenge;
        }

        /// <summary>
        /// Get effective grain direction angle in degrees
        /// </summary>
        /// <returns>Angle in degrees (0-360)</returns>
        public double GetEffectiveGrainAngle()
        {
            switch (Laufrichtung)
            {
                case GrainDirection.X:
                    return 0.0;
                case GrainDirection.Y:
                    return 90.0;
                case GrainDirection.Custom:
                    return CustomAngle;
                default:
                    return 0.0;
            }
        }

        /// <summary>
        /// Validate the material layer configuration
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SchichtName))
                errors.Add("Material layer name is required");

            if (Dicke <= 0)
                errors.Add("Layer thickness must be greater than 0");

            if (Dichte <= 0)
                errors.Add("Material density must be greater than 0");

            if (Lackmenge < 0)
                errors.Add("Paint consumption cannot be negative");

            if (Material != null)
            {
                var materialErrors = Material.Validate();
                errors.AddRange(materialErrors);
            }
            else
            {
                errors.Add("Material information is required");
            }

            return errors;
        }

        /// <summary>
        /// Create a deep copy of the material layer
        /// </summary>
        /// <returns>Deep copy of the material layer</returns>
        public MaterialSchicht Clone()
        {
            return new MaterialSchicht
            {
                Id = Guid.NewGuid(),
                SchichtName = SchichtName + " (Copy)",
                Material = Material?.Clone(),
                Dicke = Dicke,
                Laufrichtung = Laufrichtung,
                CustomAngle = CustomAngle,
                Dichte = Dichte,
                Lackmenge = Lackmenge,
                Order = Order,
                IsVisible = IsVisible,
                LayerColor = LayerColor,
                Notes = Notes
            };
        }

        /// <summary>
        /// Convert to string representation
        /// </summary>
        /// <returns>String representation of the material layer</returns>
        public override string ToString()
        {
            return $"{SchichtName} - {Material?.Name} ({Dicke:F1}mm, {Dichte:F0}kg/m³)";
        }
    }

    /// <summary>
    /// Defines the grain direction or orientation of a material layer
    /// </summary>
    public enum GrainDirection
    {
        /// <summary>
        /// Grain runs parallel to X-axis (0°)
        /// </summary>
        X,

        /// <summary>
        /// Grain runs parallel to Y-axis (90°)
        /// </summary>
        Y,

        /// <summary>
        /// Custom angle defined by user
        /// </summary>
        Custom
    }
} 