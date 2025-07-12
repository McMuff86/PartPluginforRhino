using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;

namespace BauteilPlugin.Models
{
    /// <summary>
    /// Represents a professional building component (Bauteil) with complex material layers
    /// and edge configurations for CNC production and ERP integration
    /// </summary>
    public class Bauteil : UserData
    {
        /// <summary>
        /// Unique identifier for the component
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Display name of the component
        /// </summary>
        public string Name { get; set; } = "Neues Bauteil";

        /// <summary>
        /// Description of the component
        /// </summary>
        public string BauteilDescription { get; set; } = string.Empty;

        /// <summary>
        /// List of material layers from bottom to top
        /// </summary>
        public List<MaterialSchicht> Schichten { get; set; } = new List<MaterialSchicht>();

        /// <summary>
        /// List of edge configurations for all sides
        /// </summary>
        public List<Kantenbild> Kantenbilder { get; set; } = new List<Kantenbild>();

        /// <summary>
        /// Creation date of the component
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Last modification date
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Constructor for creating a new component
        /// </summary>
        public Bauteil()
        {
            // Initialize with default edge configurations
            InitializeDefaultEdges();
        }

        /// <summary>
        /// Constructor for creating a named component
        /// </summary>
        /// <param name="name">Name of the component</param>
        public Bauteil(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Initialize default edge configurations (top, bottom, front, back, left, right)
        /// </summary>
        private void InitializeDefaultEdges()
        {
            var edgeTypes = new[] { EdgeType.Top, EdgeType.Bottom, EdgeType.Front, EdgeType.Back, EdgeType.Left, EdgeType.Right };
            
            foreach (var edgeType in edgeTypes)
            {
                Kantenbilder.Add(new Kantenbild
                {
                    KantenTyp = edgeType,
                    BearbeitungsTyp = EdgeProcessingType.Raw
                });
            }
        }

        /// <summary>
        /// Calculate total thickness of all layers
        /// </summary>
        /// <returns>Total thickness in mm</returns>
        public double GetTotalThickness()
        {
            return Schichten.Sum(s => s.Dicke);
        }

        /// <summary>
        /// Calculate total weight based on volume and layer densities
        /// </summary>
        /// <param name="volume">Volume of the component in mm³</param>
        /// <returns>Total weight in kg</returns>
        public double CalculateWeight(double volume)
        {
            if (volume <= 0 || !Schichten.Any())
                return 0;

            double totalWeight = 0;
            double totalThickness = GetTotalThickness();

            if (totalThickness <= 0)
                return 0;

            // Calculate weight proportionally for each layer
            foreach (var schicht in Schichten)
            {
                double layerRatio = schicht.Dicke / totalThickness;
                double layerVolume = volume * layerRatio;
                double layerWeight = (layerVolume / 1000000) * schicht.Dichte; // Convert mm³ to cm³, then to kg
                totalWeight += layerWeight;
            }

            return totalWeight;
        }

        /// <summary>
        /// Calculate total paint consumption based on surface area
        /// </summary>
        /// <param name="surfaceArea">Surface area in mm²</param>
        /// <returns>Total paint consumption in ml</returns>
        public double CalculatePaintConsumption(double surfaceArea)
        {
            if (surfaceArea <= 0)
                return 0;

            double totalPaintConsumption = 0;
            double surfaceAreaInM2 = surfaceArea / 1000000; // Convert mm² to m²

            foreach (var schicht in Schichten)
            {
                if (schicht.Lackmenge > 0)
                {
                    totalPaintConsumption += schicht.Lackmenge * surfaceAreaInM2;
                }
            }

            return totalPaintConsumption;
        }

        /// <summary>
        /// Calculate weight per square meter
        /// </summary>
        /// <returns>Weight per square meter in kg/m²</returns>
        public double GetWeightPerSquareMeter()
        {
            double totalWeight = 0;
            double totalThickness = GetTotalThickness();

            if (totalThickness <= 0)
                return 0;

            // Calculate weight for 1 m² with the total thickness
            double volumePerM2 = totalThickness * 1000000; // 1 m² = 1000000 mm²

            foreach (var schicht in Schichten)
            {
                double layerRatio = schicht.Dicke / totalThickness;
                double layerVolume = volumePerM2 * layerRatio;
                double layerWeight = (layerVolume / 1000000) * schicht.Dichte; // Convert mm³ to cm³, then to kg
                totalWeight += layerWeight;
            }

            return totalWeight;
        }

        /// <summary>
        /// Calculate paint consumption per square meter
        /// </summary>
        /// <returns>Paint consumption per square meter in ml/m²</returns>
        public double GetPaintConsumptionPerSquareMeter()
        {
            double totalPaintConsumption = 0;

            foreach (var schicht in Schichten)
            {
                if (schicht.Lackmenge > 0)
                {
                    totalPaintConsumption += schicht.Lackmenge;
                }
            }

            return totalPaintConsumption;
        }

        /// <summary>
        /// Add a new material layer
        /// </summary>
        /// <param name="schicht">Material layer to add</param>
        public void AddSchicht(MaterialSchicht schicht)
        {
            if (schicht != null)
            {
                Schichten.Add(schicht);
                ModifiedDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Remove a material layer
        /// </summary>
        /// <param name="schicht">Material layer to remove</param>
        public bool RemoveSchicht(MaterialSchicht schicht)
        {
            if (Schichten.Remove(schicht))
            {
                ModifiedDate = DateTime.Now;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get edge configuration for a specific edge type
        /// </summary>
        /// <param name="edgeType">Type of edge</param>
        /// <returns>Edge configuration or null if not found</returns>
        public Kantenbild GetKantenbild(EdgeType edgeType)
        {
            return Kantenbilder.FirstOrDefault(k => k.KantenTyp == edgeType);
        }

        /// <summary>
        /// Update edge configuration
        /// </summary>
        /// <param name="edgeType">Type of edge</param>
        /// <param name="processingType">Processing type for the edge</param>
        public void UpdateKantenbild(EdgeType edgeType, EdgeProcessingType processingType)
        {
            var kantenbild = GetKantenbild(edgeType);
            if (kantenbild != null)
            {
                kantenbild.BearbeitungsTyp = processingType;
                ModifiedDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Validate the component configuration
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Bauteil name is required");

            if (!Schichten.Any())
                errors.Add("At least one material layer is required");

            foreach (var schicht in Schichten)
            {
                var schichtErrors = schicht.Validate();
                errors.AddRange(schichtErrors);
            }

            return errors;
        }

        /// <summary>
        /// Create a deep copy of the component
        /// </summary>
        /// <returns>Deep copy of the component</returns>
        public Bauteil Clone()
        {
            var clone = new Bauteil(Name + " (Copy)")
            {
                Id = Guid.NewGuid(),
                BauteilDescription = BauteilDescription,
                Schichten = Schichten.Select(s => s.Clone()).ToList(),
                Kantenbilder = Kantenbilder.Select(k => k.Clone()).ToList(),
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            return clone;
        }

        /// <summary>
        /// Convert to string representation
        /// </summary>
        /// <returns>String representation of the component</returns>
        public override string ToString()
        {
            return $"{Name} ({Schichten.Count} layers, {GetTotalThickness():F1}mm thick)";
        }

        #region UserData Implementation

        /// <summary>
        /// Gets a brief description of this user data for Rhino
        /// </summary>
        public override string Description => "Professional Bauteil Component Data";

        /// <summary>
        /// Writes the content of this user data to a binary archive
        /// </summary>
        protected override void OnDuplicate(UserData source)
        {
            if (source is Bauteil sourceBauteil)
            {
                Id = sourceBauteil.Id;
                Name = sourceBauteil.Name;
                BauteilDescription = sourceBauteil.BauteilDescription;
                Schichten = sourceBauteil.Schichten.Select(s => s.Clone()).ToList();
                Kantenbilder = sourceBauteil.Kantenbilder.Select(k => k.Clone()).ToList();
                CreatedDate = sourceBauteil.CreatedDate;
                ModifiedDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Writes user data to a binary archive
        /// </summary>
        protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            try
            {
                // Write version number
                archive.Write3dmChunkVersion(1, 0);
                
                // Write basic properties
                archive.WriteGuid(Id);
                archive.WriteString(Name);
                archive.WriteString(BauteilDescription);
                archive.WriteString(CreatedDate.ToString());
                archive.WriteString(ModifiedDate.ToString());
                
                // Write material layers count and data
                archive.WriteInt(Schichten.Count);
                foreach (var schicht in Schichten)
                {
                    archive.WriteString(schicht.SchichtName);
                    archive.WriteString(schicht.Material?.Name ?? "Unknown");
                    archive.WriteDouble(schicht.Dicke);
                    archive.WriteDouble(schicht.Dichte);
                    archive.WriteInt((int)schicht.Laufrichtung);
                    archive.WriteDouble(schicht.Lackmenge);
                }
                
                // Write edge configurations count and data
                archive.WriteInt(Kantenbilder.Count);
                foreach (var kante in Kantenbilder)
                {
                    archive.WriteInt((int)kante.KantenTyp);
                    archive.WriteInt((int)kante.BearbeitungsTyp);
                    archive.WriteBool(kante.IsVisible);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads user data from a binary archive
        /// </summary>
        protected override bool Read(Rhino.FileIO.BinaryArchiveReader archive)
        {
            try
            {
                // Read version
                int major, minor;
                archive.Read3dmChunkVersion(out major, out minor);
                
                if (major == 1)
                {
                    // Read basic properties
                    Id = archive.ReadGuid();
                    Name = archive.ReadString();
                    BauteilDescription = archive.ReadString();
                    DateTime.TryParse(archive.ReadString(), out var createdDate);
                    CreatedDate = createdDate;
                    DateTime.TryParse(archive.ReadString(), out var modifiedDate);
                    ModifiedDate = modifiedDate;
                    
                    // Read material layers
                    int schichtCount = archive.ReadInt();
                    Schichten.Clear();
                    for (int i = 0; i < schichtCount; i++)
                    {
                        var schichtName = archive.ReadString();
                        var materialName = archive.ReadString();
                        var dicke = archive.ReadDouble();
                        var dichte = archive.ReadDouble();
                        var laufrichtung = (GrainDirection)archive.ReadInt();
                        var lackmenge = archive.ReadDouble();
                        
                        var schicht = new MaterialSchicht
                        {
                            SchichtName = schichtName,
                            Material = new Material(materialName),
                            Dicke = dicke,
                            Dichte = dichte,
                            Laufrichtung = laufrichtung,
                            Lackmenge = lackmenge
                        };
                        Schichten.Add(schicht);
                    }
                    
                    // Read edge configurations
                    int kantenCount = archive.ReadInt();
                    Kantenbilder.Clear();
                    for (int i = 0; i < kantenCount; i++)
                    {
                        var kantenTyp = (EdgeType)archive.ReadInt();
                        var bearbeitungsTyp = (EdgeProcessingType)archive.ReadInt();
                        var isVisible = archive.ReadBool();
                        
                        var kante = new Kantenbild
                        {
                            KantenTyp = kantenTyp,
                            BearbeitungsTyp = bearbeitungsTyp,
                            IsVisible = isVisible
                        };
                        Kantenbilder.Add(kante);
                    }
                    
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
} 