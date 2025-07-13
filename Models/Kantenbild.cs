using System;
using System.Collections.Generic;
using System.Linq;
using BauteilPlugin.Utils;

namespace BauteilPlugin.Models
{
    /// <summary>
    /// Represents edge processing configuration for a building component
    /// Defines how each edge should be processed during CNC production
    /// </summary>
    public class Kantenbild
    {
        /// <summary>
        /// Unique identifier for the edge configuration
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Type of edge (top, bottom, front, back, left, right)
        /// </summary>
        public EdgeType KantenTyp { get; set; } = EdgeType.Top;

        /// <summary>
        /// Type of processing for this edge
        /// </summary>
        public EdgeProcessingType BearbeitungsTyp { get; set; } = EdgeProcessingType.Raw;

        /// <summary>
        /// Edge material (if different from main material)
        /// </summary>
        public string EdgeMaterial { get; set; } = string.Empty;

        /// <summary>
        /// Thickness of edge banding in mm (if applicable)
        /// </summary>
        public double EdgeBandThickness { get; set; } = 0.4;

        /// <summary>
        /// Alias for EdgeBandThickness for UI compatibility
        /// </summary>
        public double Dicke 
        { 
            get => EdgeBandThickness; 
            set => EdgeBandThickness = value; 
        }

        /// <summary>
        /// Width of edge banding in mm (if applicable)
        /// </summary>
        public double EdgeBandWidth { get; set; } = 22.0;

        /// <summary>
        /// Color of edge banding
        /// </summary>
        public System.Drawing.Color EdgeColor { get; set; } = System.Drawing.Color.Brown;

        /// <summary>
        /// Whether this edge is visible in the final product
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Priority for edge processing (higher number = higher priority)
        /// </summary>
        public int ProcessingPriority { get; set; } = 1;

        /// <summary>
        /// Additional machining operations for this edge
        /// </summary>
        public List<EdgeMachiningOperation> MachiningOperations { get; set; } = new List<EdgeMachiningOperation>();

        /// <summary>
        /// Radius for rounded edges (if applicable)
        /// </summary>
        public double RoundingRadius { get; set; } = 0.0;

        /// <summary>
        /// Chamfer size for chamfered edges (if applicable)
        /// </summary>
        public double ChamferSize { get; set; } = 0.0;

        /// <summary>
        /// Additional notes or comments for this edge
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Cost factor for this edge processing
        /// </summary>
        public double CostFactor { get; set; } = 1.0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Kantenbild()
        {
        }

        /// <summary>
        /// Constructor with edge type and processing type
        /// </summary>
        /// <param name="kante">Type of edge</param>
        /// <param name="bearbeitungstyp">Type of processing</param>
        public Kantenbild(EdgeType kante, EdgeProcessingType bearbeitungstyp)
        {
            KantenTyp = kante;
            BearbeitungsTyp = bearbeitungstyp;
            SetDefaultPropertiesForProcessingType(bearbeitungstyp);
        }

        /// <summary>
        /// Set default properties based on processing type
        /// </summary>
        /// <param name="processingType">Processing type</param>
        public void SetDefaultPropertiesForProcessingType(EdgeProcessingType processingType)
        {
            switch (processingType)
            {
                case EdgeProcessingType.Raw:
                    EdgeBandThickness = 0.0;
                    EdgeBandWidth = 0.0;
                    CostFactor = 1.0;
                    break;

                case EdgeProcessingType.EdgeBanded:
                    EdgeBandThickness = 0.4;
                    EdgeBandWidth = 22.0;
                    EdgeColor = System.Drawing.Color.Brown;
                    CostFactor = 1.2;
                    break;

                case EdgeProcessingType.Solid:
                    EdgeBandThickness = 0.0;
                    EdgeBandWidth = 0.0;
                    CostFactor = 1.5;
                    break;

                case EdgeProcessingType.Rounded:
                    RoundingRadius = 3.0;
                    CostFactor = 1.3;
                    break;

                case EdgeProcessingType.Chamfered:
                    ChamferSize = 2.0;
                    CostFactor = 1.2;
                    break;

                case EdgeProcessingType.Grooved:
                    CostFactor = 1.4;
                    break;

                case EdgeProcessingType.Profiled:
                    CostFactor = 1.6;
                    break;

                case EdgeProcessingType.Laminated:
                    CostFactor = 1.8;
                    break;
            }
        }

        /// <summary>
        /// Add a machining operation to this edge
        /// </summary>
        /// <param name="operation">Machining operation to add</param>
        public void AddMachiningOperation(EdgeMachiningOperation operation)
        {
            if (operation != null)
            {
                MachiningOperations.Add(operation);
            }
        }

        /// <summary>
        /// Remove a machining operation from this edge
        /// </summary>
        /// <param name="operation">Machining operation to remove</param>
        public bool RemoveMachiningOperation(EdgeMachiningOperation operation)
        {
            return MachiningOperations.Remove(operation);
        }

        /// <summary>
        /// Get the display name for this edge
        /// </summary>
        /// <returns>Display name combining edge type and processing type</returns>
        public string GetDisplayName()
        {
            return $"{GetEdgeTypeDisplayName(KantenTyp)} - {GetProcessingTypeDisplayName(BearbeitungsTyp)}";
        }

        /// <summary>
        /// Get display name for edge type
        /// </summary>
        /// <param name="edgeType">Edge type</param>
        /// <returns>Display name</returns>
        private string GetEdgeTypeDisplayName(EdgeType edgeType)
        {
            switch (edgeType)
            {
                case EdgeType.Top: return "Oben";
                case EdgeType.Bottom: return "Unten";
                case EdgeType.Front: return "Vorne";
                case EdgeType.Back: return "Hinten";
                case EdgeType.Left: return "Links";
                case EdgeType.Right: return "Rechts";
                default: return edgeType.ToString();
            }
        }

        /// <summary>
        /// Get display name for edge type (public method)
        /// </summary>
        /// <returns>Display name</returns>
        public string GetKantenTypDisplayName()
        {
            return GetEdgeTypeDisplayName(KantenTyp);
        }

        /// <summary>
        /// Get display name for processing type (public method)
        /// </summary>
        /// <returns>Display name</returns>
        public string GetBearbeitungsTypDisplayName()
        {
            return GetProcessingTypeDisplayName(BearbeitungsTyp);
        }

        /// <summary>
        /// Get display name for processing type
        /// </summary>
        /// <param name="processingType">Processing type</param>
        /// <returns>Display name</returns>
        private string GetProcessingTypeDisplayName(EdgeProcessingType processingType)
        {
            switch (processingType)
            {
                case EdgeProcessingType.Raw: return "Roh";
                case EdgeProcessingType.EdgeBanded: return "Bekantet";
                case EdgeProcessingType.Solid: return "Massiv";
                case EdgeProcessingType.Rounded: return "Gerundet";
                case EdgeProcessingType.Chamfered: return "Gefast";
                case EdgeProcessingType.Grooved: return "Genutet";
                case EdgeProcessingType.Profiled: return "Profiliert";
                case EdgeProcessingType.Laminated: return "Laminiert";
                default: return processingType.ToString();
            }
        }

        /// <summary>
        /// Validate the edge configuration
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (EdgeBandThickness < 0)
                errors.Add("Edge band thickness cannot be negative");

            if (EdgeBandWidth < 0)
                errors.Add("Edge band width cannot be negative");

            if (RoundingRadius < 0)
                errors.Add("Rounding radius cannot be negative");

            if (ChamferSize < 0)
                errors.Add("Chamfer size cannot be negative");

            if (CostFactor <= 0)
                errors.Add("Cost factor must be greater than 0");

            // Validate machining operations
            foreach (var operation in MachiningOperations)
            {
                var operationErrors = operation.Validate();
                errors.AddRange(operationErrors);
            }

            return errors;
        }

        /// <summary>
        /// Create a deep copy of the edge configuration
        /// </summary>
        /// <returns>Deep copy of the edge configuration</returns>
        public Kantenbild Clone()
        {
            var aClone = new Kantenbild
            {
                Id = Guid.NewGuid(),
                KantenTyp = this.KantenTyp,
                BearbeitungsTyp = this.BearbeitungsTyp,
                EdgeMaterial = this.EdgeMaterial,
                EdgeBandThickness = this.EdgeBandThickness,
                EdgeBandWidth = this.EdgeBandWidth,
                EdgeColor = this.EdgeColor,
                IsVisible = this.IsVisible,
                ProcessingPriority = this.ProcessingPriority,
                MachiningOperations = this.MachiningOperations.Select(op => op.CloneWithoutCopySuffix()).ToList(),
                RoundingRadius = this.RoundingRadius,
                ChamferSize = this.ChamferSize,
                Notes = this.Notes,
                CostFactor = this.CostFactor
            };
            return aClone;
        }

        /// <summary>
        /// Convert to string representation
        /// </summary>
        /// <returns>String representation of the edge configuration</returns>
        public override string ToString()
        {
            return $"{GetDisplayName()} ({Dicke:F1}mm)";
        }

        /// <summary>
        /// Writes the edge configuration data to a binary archive.
        /// </summary>
        /// <param name="archive">The archive to write to.</param>
        public void Write(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            archive.Write3dmChunkVersion(1, 0);
            archive.WriteGuid(Id);
            archive.WriteInt((int)KantenTyp);
            archive.WriteInt((int)BearbeitungsTyp);
            archive.WriteString(EdgeMaterial);
            archive.WriteDouble(EdgeBandThickness);
            archive.WriteDouble(EdgeBandWidth);
            archive.WriteColor(EdgeColor);
            archive.WriteBool(IsVisible);
            archive.WriteInt(ProcessingPriority);
            archive.WriteDouble(RoundingRadius);
            archive.WriteDouble(ChamferSize);
            archive.WriteString(Notes);
            archive.WriteDouble(CostFactor);

            archive.WriteInt(MachiningOperations.Count);
            foreach (var op in MachiningOperations)
            {
                op.Write(archive);
            }
        }

        /// <summary>
        /// Reads the edge configuration data from a binary archive.
        /// </summary>
        /// <param name="archive">The archive to read from.</param>
        public void Read(Rhino.FileIO.BinaryArchiveReader archive)
        {
            archive.Read3dmChunkVersion(out _, out _);
            Id = archive.ReadGuid();
            KantenTyp = (EdgeType)archive.ReadInt();
            BearbeitungsTyp = (EdgeProcessingType)archive.ReadInt();
            EdgeMaterial = archive.ReadString();
            EdgeBandThickness = archive.ReadDouble();
            EdgeBandWidth = archive.ReadDouble();
            EdgeColor = archive.ReadColor();
            IsVisible = archive.ReadBool();
            ProcessingPriority = archive.ReadInt();
            RoundingRadius = archive.ReadDouble();
            ChamferSize = archive.ReadDouble();
            Notes = archive.ReadString();
            CostFactor = archive.ReadDouble();

            int opCount = archive.ReadInt();
            MachiningOperations.Clear();
            for (int i = 0; i < opCount; i++)
            {
                var op = new EdgeMachiningOperation();
                op.Read(archive);
                MachiningOperations.Add(op);
            }
        }
    }

    /// <summary>
    /// Defines the type of edge on a building component
    /// </summary>
    public enum EdgeType
    {
        /// <summary>
        /// Top edge
        /// </summary>
        Top,

        /// <summary>
        /// Bottom edge
        /// </summary>
        Bottom,

        /// <summary>
        /// Front edge
        /// </summary>
        Front,

        /// <summary>
        /// Back edge
        /// </summary>
        Back,

        /// <summary>
        /// Left edge
        /// </summary>
        Left,

        /// <summary>
        /// Right edge
        /// </summary>
        Right
    }

    /// <summary>
    /// Defines the type of processing for an edge
    /// </summary>
    public enum EdgeProcessingType
    {
        /// <summary>
        /// Raw edge, no processing
        /// </summary>
        Raw,

        /// <summary>
        /// Edge banded with veneer or melamine
        /// </summary>
        EdgeBanded,

        /// <summary>
        /// Solid wood edge
        /// </summary>
        Solid,

        /// <summary>
        /// Rounded edge
        /// </summary>
        Rounded,

        /// <summary>
        /// Chamfered edge
        /// </summary>
        Chamfered,

        /// <summary>
        /// Grooved edge
        /// </summary>
        Grooved,

        /// <summary>
        /// Profiled edge (custom profile)
        /// </summary>
        Profiled,

        /// <summary>
        /// Laminated edge
        /// </summary>
        Laminated
    }

    /// <summary>
    /// Represents a machining operation for an edge
    /// </summary>
    public class EdgeMachiningOperation
    {
        /// <summary>
        /// Unique identifier for the machining operation
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Name of the operation (e.g., "Drilling_5mm")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of operation (e.g., "Drill", "Groove", "Pocket")
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// Parameters for the operation (e.g., {"depth": 10, "diameter": 5})
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Order of execution for this operation
        /// </summary>
        public int ExecutionOrder { get; set; } = 1;

        /// <summary>
        /// Validate the machining operation
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Machining operation name is required");

            if (string.IsNullOrWhiteSpace(OperationType))
                errors.Add("Machining operation type is required");

            return errors;
        }

        /// <summary>
        /// Create a deep copy of the machining operation
        /// </summary>
        /// <returns>A deep copy of the operation</returns>
        public EdgeMachiningOperation Clone()
        {
            var aClone = new EdgeMachiningOperation
            {
                Id = Guid.NewGuid(),
                Name = this.Name,
                OperationType = this.OperationType,
                ExecutionOrder = this.ExecutionOrder
            };
            // Note: Deep copy of parameters dictionary is tricky.
            // This is a shallow copy for now.
            foreach (var kvp in this.Parameters)
            {
                aClone.Parameters.Add(kvp.Key, kvp.Value);
            }
            return aClone;
        }

        /// <summary>
        /// Writes the machining operation data to a binary archive.
        /// </summary>
        /// <param name="archive">The archive to write to.</param>
        public void Write(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            archive.Write3dmChunkVersion(1, 0);
            archive.WriteGuid(Id);
            archive.WriteString(Name);
            archive.WriteString(OperationType);
            archive.WriteInt(ExecutionOrder);

            // Serialization of Dictionary<string, object> is complex.
            // For now, we write the count and key-value pairs as strings.
            archive.WriteInt(Parameters.Count);
            foreach (var kvp in Parameters)
            {
                archive.WriteString(kvp.Key);
                archive.WriteString(kvp.Value?.ToString() ?? string.Empty);
            }
        }

        /// <summary>
        /// Reads the machining operation data from a binary archive.
        /// </summary>
        /// <param name="archive">The archive to read from.</param>
        public void Read(Rhino.FileIO.BinaryArchiveReader archive)
        {
            archive.Read3dmChunkVersion(out _, out _);
            Id = archive.ReadGuid();
            Name = archive.ReadString();
            OperationType = archive.ReadString();
            ExecutionOrder = archive.ReadInt();

            int paramCount = archive.ReadInt();
            Parameters.Clear();
            for (int i = 0; i < paramCount; i++)
            {
                string key = archive.ReadString();
                string value = archive.ReadString();
                Parameters[key] = value; // All values are read as strings.
            }
        }
    }
} 