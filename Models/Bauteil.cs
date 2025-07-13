using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.IO;
using System.Text;

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
        /// Defines the local coordinate system (Plane) for the component.
        /// The Z-axis represents the thickness direction.
        /// </summary>
        public Plane OrientationPlane { get; set; }

        /// <summary>
        /// The length of the component, corresponding to its local X-axis.
        /// </summary>
        public double Length { get; set; } = 1800;

        /// <summary>
        /// The width of the component, corresponding to its local Y-axis.
        /// </summary>
        public double Width { get; set; } = 600;

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
            // Initialize with a default plane
            OrientationPlane = Plane.WorldXY;
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
        /// <returns>A deep copy of the component</returns>
        public Bauteil Clone()
        {
            var aClone = new Bauteil
            {
                Name = this.Name,
                BauteilDescription = this.BauteilDescription,
                Schichten = this.Schichten.Select(s => s.Clone()).ToList(),
                Kantenbilder = this.Kantenbilder.Select(k => k.Clone()).ToList(),
                CreatedDate = this.CreatedDate,
                ModifiedDate = this.ModifiedDate,
                Id = this.Id,
                OrientationPlane = this.OrientationPlane,
                Length = this.Length,
                Width = this.Width
            };
            return aClone;
        }

        /// <summary>
        /// Convert to string representation
        /// </summary>
        /// <returns>String representation of the component</returns>
        public override string ToString()
        {
            return $"{Name} ({Schichten.Count} layers, {GetTotalThickness():F1}mm thick)";
        }

        /// <summary>
        /// Exports the component data to a CSV file
        /// </summary>
        /// <param name="filePath">Path where the CSV file should be saved</param>
        /// <returns>True if export was successful</returns>
        public bool ExportToCSV(string filePath)
        {
            try
            {
                using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write header
                    sw.WriteLine("Property,Value");
                    
                    // Write basic properties
                    sw.WriteLine($"ID,{Id}");
                    sw.WriteLine($"Name,{Name}");
                    sw.WriteLine($"Description,{BauteilDescription}");
                    sw.WriteLine($"Length,{Length}");
                    sw.WriteLine($"Width,{Width}");
                    sw.WriteLine($"Total Thickness,{GetTotalThickness()}");
                    sw.WriteLine($"Weight per m²,{GetWeightPerSquareMeter():F2}");
                    sw.WriteLine($"Created Date,{CreatedDate}");
                    sw.WriteLine($"Modified Date,{ModifiedDate}");
                    
                    // Add empty line as separator
                    sw.WriteLine();
                    
                    // Write material layers
                    sw.WriteLine("Layer Index,Layer Name,Material,Thickness,Density,Grain Direction");
                    for (int i = 0; i < Schichten.Count; i++)
                    {
                        var schicht = Schichten[i];
                        sw.WriteLine($"{i},{schicht.SchichtName},{schicht.Material.Name},{schicht.Dicke:F1},{schicht.Dichte:F0},{schicht.Laufrichtung}");
                    }
                    
                    // Add empty line as separator
                    sw.WriteLine();
                    
                    // Write edge configurations
                    sw.WriteLine("Edge Type,Processing Type,Thickness,Material,Is Visible");
                    foreach (var kante in Kantenbilder)
                    {
                        sw.WriteLine($"{kante.KantenTyp},{kante.BearbeitungsTyp},{kante.Dicke:F1},{kante.Material?.Name ?? "None"},{kante.IsVisible}");
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Rhino.RhinoApp.WriteLine($"Error exporting to CSV: {ex.Message}");
                return false;
            }
        }

        #region UserData Implementation

        /// <summary>
        /// Gets a brief description of this user data for Rhino
        /// </summary>
        public override string Description => "Professional Bauteil Component Data";

        /// <summary>
        /// Helper method to retrieve a Bauteil from a RhinoObject
        /// </summary>
        /// <param name="rhinoObject">The RhinoObject to check for Bauteil data</param>
        /// <returns>Bauteil if found, null otherwise</returns>
        public static Bauteil TryGetBauteil(RhinoObject rhinoObject)
        {
            if (rhinoObject == null)
                return null;
                
            var bauteil = rhinoObject.Geometry.UserData.Find(typeof(Bauteil)) as Bauteil;
            return bauteil;
        }

        /// <summary>
        /// Writes the content of this user data to a binary archive
        /// </summary>
        protected override void OnDuplicate(UserData source)
        {
            if (source is Bauteil src)
            {
                // Clone all data to ensure the new object has a separate copy
                Name = src.Name;
                BauteilDescription = src.BauteilDescription;
                Schichten = src.Schichten.Select(s => s.Clone()).ToList();
                Kantenbilder = src.Kantenbilder.Select(k => k.Clone()).ToList();
                // Don't copy creation date, but set modified date
                CreatedDate = DateTime.Now;
                ModifiedDate = DateTime.Now;
                // Give it a new Guid
                Id = Guid.NewGuid();
                OrientationPlane = src.OrientationPlane;
                Length = src.Length;
                Width = src.Width;
            }
        }

        /// <summary>
        /// Writes the component data to a binary archive for saving
        /// </summary>
        /// <param name="archive">The archive to write to</param>
        /// <returns>True on success</returns>
        protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            archive.Write3dmChunkVersion(1, 2);
            archive.WriteGuid(Id);
            archive.WriteString(Name);
            archive.WriteString(BauteilDescription);
            archive.WriteInt64(CreatedDate.Ticks);
            archive.WriteInt64(ModifiedDate.Ticks);

            archive.WriteInt(Schichten.Count);
            foreach (var schicht in Schichten)
            {
                schicht.Write(archive);
            }

            archive.WriteInt(Kantenbilder.Count);
            foreach (var kante in Kantenbilder)
            {
                kante.Write(archive);
            }

            archive.WritePlane(OrientationPlane);

            // Version 1.2 additions
            archive.WriteDouble(Length);
            archive.WriteDouble(Width);

            return true;
        }

        /// <summary>
        /// Reads the component data from a binary archive
        /// </summary>
        /// <param name="archive">The archive to read from</param>
        /// <returns>True on success</returns>
        protected override bool Read(Rhino.FileIO.BinaryArchiveReader archive)
        {
            int major = 0;
            int minor = 0;
            archive.Read3dmChunkVersion(out major, out minor);

            if (major == 1)
            {
                Id = archive.ReadGuid();
                Name = archive.ReadString();
                BauteilDescription = archive.ReadString();
                CreatedDate = new DateTime(archive.ReadInt64());
                ModifiedDate = new DateTime(archive.ReadInt64());

                int schichtenCount = archive.ReadInt();
                Schichten.Clear();
                for (int i = 0; i < schichtenCount; i++)
                {
                    var schicht = new MaterialSchicht();
                    schicht.Read(archive);
                    Schichten.Add(schicht);
                }

                int kantenbilderCount = archive.ReadInt();
                Kantenbilder.Clear();
                for (int i = 0; i < kantenbilderCount; i++)
                {
                    var kante = new Kantenbild();
                    kante.Read(archive);
                    Kantenbilder.Add(kante);
                }

                if (minor >= 1)
                {
                    OrientationPlane = archive.ReadPlane();
                }
                else
                {
                    OrientationPlane = Plane.WorldXY;
                }

                if (minor >= 2)
                {
                    Length = archive.ReadDouble();
                    Width = archive.ReadDouble();
                }

                // Handle potential missing default edges in older versions
                if (Kantenbilder.Count == 0)
                {
                    InitializeDefaultEdges();
                }
            }
            return true;
        }

        #endregion
    }
} 