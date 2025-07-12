using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.UI;
using Rhino.Input;
using Rhino.DocObjects;
using BauteilPlugin.UI;
using BauteilPlugin.Models;

namespace BauteilPlugin.Commands
{
    /// <summary>
    /// Command to open and manage the Bauteil Editor panel
    /// </summary>
    public class BauteilEditorCommand : Command
    {
        /// <summary>
        /// Command name in English
        /// </summary>
        public override string EnglishName => "BauteilEditor";

        /// <summary>
        /// Command name in German
        /// </summary>
        public override string LocalName => "BauteilEditor";

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="doc">Active document</param>
        /// <param name="mode">Command mode</param>
        /// <returns>Command result</returns>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // Check if the panel is already open
                var panelId = typeof(BauteilEditorPanel).GUID;
                var isVisible = Panels.IsPanelVisible(panelId);

                if (isVisible)
                {
                    // Panel is visible, close it
                    Panels.ClosePanel(panelId);
                    RhinoApp.WriteLine("Bauteil Editor panel closed.");
                }
                else
                {
                    // Panel is not visible, open it
                    Panels.OpenPanel(panelId);
                    RhinoApp.WriteLine("Bauteil Editor panel opened.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error executing BauteilEditor command: {ex.Message}");
                return Result.Failure;
            }
        }
    }

    /// <summary>
    /// Command to create a new Bauteil from selected geometry
    /// </summary>
    public class CreateBauteilCommand : Command
    {
        /// <summary>
        /// Command name in English
        /// </summary>
        public override string EnglishName => "CreateBauteil";

        /// <summary>
        /// Command name in German
        /// </summary>
        public override string LocalName => "BauteilErstellen";

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="doc">Active document</param>
        /// <param name="mode">Command mode</param>
        /// <returns>Command result</returns>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // Get selected objects
                var selectedObjects = doc.Objects.GetSelectedObjects(false, false).ToList();
                
                if (selectedObjects.Count == 0)
                {
                    RhinoApp.WriteLine("Please select geometry to create a Bauteil.");
                    return Result.Cancel;
                }

                // Ask for Bauteil name
                string bauteilName = "New Bauteil";
                var rc = RhinoGet.GetString("Enter Bauteil name", false, ref bauteilName);
                if (rc != Result.Success || string.IsNullOrEmpty(bauteilName))
                {
                    bauteilName = "New Bauteil";
                }

                // Create a new Bauteil with default values
                var newBauteil = new Bauteil
                {
                    Name = bauteilName,
                    BauteilDescription = "Created from selected geometry"
                };

                // Add default material layers
                var defaultMaterial = new Models.Material
                {
                    Name = "MDF",
                    Typ = MaterialType.MDF,
                    StandardDensity = 750.0
                };

                newBauteil.Schichten.Add(new MaterialSchicht
                {
                    SchichtName = "Base Layer",
                    Material = defaultMaterial,
                    Dicke = 18.0,
                    Dichte = 750.0,
                    Laufrichtung = GrainDirection.X
                });

                // Add default edge configuration
                foreach (EdgeType edgeType in Enum.GetValues(typeof(EdgeType)))
                {
                    newBauteil.Kantenbilder.Add(new Kantenbild
                    {
                        KantenTyp = edgeType,
                        BearbeitungsTyp = EdgeProcessingType.Raw
                    });
                }

                // Attach Bauteil data to selected objects
                int attachedCount = 0;
                foreach (var obj in selectedObjects)
                {
                    // Clone the Bauteil for each object (so they can be modified independently)
                    var bauteilClone = newBauteil.Clone();
                    
                    // Remove existing Bauteil data if any
                    var existingBauteil = obj.UserData.Find(typeof(Bauteil)) as Bauteil;
                    if (existingBauteil != null)
                    {
                        obj.UserData.Remove(existingBauteil);
                    }
                    
                    // Attach new Bauteil data
                    obj.UserData.Add(bauteilClone);
                    obj.CommitChanges();
                    attachedCount++;
                }

                // Update the document
                doc.Views.Redraw();

                RhinoApp.WriteLine($"Created Bauteil '{bauteilName}' and attached to {attachedCount} object(s).");
                RhinoApp.WriteLine("Select an object to see Bauteil properties in the Properties panel.");

                // Open the editor panel if not visible
                var panelId = typeof(BauteilEditorPanel).GUID;
                if (!Panels.IsPanelVisible(panelId))
                {
                    Panels.OpenPanel(panelId);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error executing CreateBauteil command: {ex.Message}");
                return Result.Failure;
            }
        }
    }

    /// <summary>
    /// Command to apply Bauteil properties to selected geometry
    /// </summary>
    public class ApplyBauteilCommand : Command
    {
        /// <summary>
        /// Command name in English
        /// </summary>
        public override string EnglishName => "ApplyBauteil";

        /// <summary>
        /// Command name in German
        /// </summary>
        public override string LocalName => "BauteilAnwenden";

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="doc">Active document</param>
        /// <param name="mode">Command mode</param>
        /// <returns>Command result</returns>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // Get selected objects
                var selectedObjects = doc.Objects.GetSelectedObjects(false, false).ToList();
                
                if (selectedObjects.Count == 0)
                {
                    RhinoApp.WriteLine("Please select geometry to apply Bauteil properties.");
                    return Result.Cancel;
                }

                // Find objects with Bauteil data to use as source
                var objectsWithBauteil = new List<RhinoObject>();
                foreach (var obj in doc.Objects)
                {
                    if (obj.UserData.Find(typeof(Bauteil)) != null)
                    {
                        objectsWithBauteil.Add(obj);
                    }
                }
                
                if (objectsWithBauteil.Count == 0)
                {
                    RhinoApp.WriteLine("No objects with Bauteil data found. Create a Bauteil first.");
                    return Result.Cancel;
                }

                // Get the first Bauteil as template (could be extended to let user choose)
                var sourceBauteil = objectsWithBauteil[0].UserData.Find(typeof(Bauteil)) as Bauteil;
                if (sourceBauteil == null)
                {
                    RhinoApp.WriteLine("Failed to retrieve Bauteil data.");
                    return Result.Failure;
                }

                // Apply to selected objects
                int appliedCount = 0;
                foreach (var obj in selectedObjects)
                {
                    // Clone the source Bauteil
                    var bauteilClone = sourceBauteil.Clone();
                    
                    // Remove existing Bauteil data if any
                    var existingBauteil = obj.UserData.Find(typeof(Bauteil)) as Bauteil;
                    if (existingBauteil != null)
                    {
                        obj.UserData.Remove(existingBauteil);
                    }
                    
                    // Attach cloned Bauteil data
                    obj.UserData.Add(bauteilClone);
                    obj.CommitChanges();
                    appliedCount++;
                }

                // Update the document
                doc.Views.Redraw();

                RhinoApp.WriteLine($"Applied Bauteil '{sourceBauteil.Name}' to {appliedCount} object(s).");
                RhinoApp.WriteLine("Select an object to see Bauteil properties in the Properties panel.");

                return Result.Success;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error executing ApplyBauteil command: {ex.Message}");
                return Result.Failure;
            }
        }
    }

    /// <summary>
    /// Simple test command to verify plugin functionality
    /// </summary>
    public class TestBauteilCommand : Command
    {
        /// <summary>
        /// Command name in English
        /// </summary>
        public override string EnglishName => "TestBauteil";

        /// <summary>
        /// Command name in German
        /// </summary>
        public override string LocalName => "BauteilTest";

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="doc">Active document</param>
        /// <param name="mode">Command mode</param>
        /// <returns>Command result</returns>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // Get selected objects - convert to list for easier handling
                var selectedObjectsList = doc.Objects.GetSelectedObjects(false, false).ToList();
                
                RhinoApp.WriteLine($"Found {selectedObjectsList.Count} selected objects.");
                
                // If no objects found with first method, try alternative
                if (selectedObjectsList.Count == 0)
                {
                    // Try alternative method
                    foreach (var rhinoObj in doc.Objects)
                    {
                        if (rhinoObj.IsSelected(false) == 1)
                        {
                            selectedObjectsList.Add(rhinoObj);
                        }
                    }
                    
                    RhinoApp.WriteLine($"Alternative method found {selectedObjectsList.Count} selected objects.");
                }
                
                if (selectedObjectsList.Count == 0)
                {
                    RhinoApp.WriteLine("Please select an object first.");
                    return Result.Cancel;
                }

                var obj = selectedObjectsList[0];
                RhinoApp.WriteLine($"Using object with ID: {obj.Id}");
                
                // Create a simple test Bauteil
                var testBauteil = new Bauteil("Test Bauteil");
                
                // Add a simple material layer
                var material = new Models.Material("Test Material", MaterialType.MDF);
                testBauteil.Schichten.Add(new MaterialSchicht
                {
                    SchichtName = "Test Layer",
                    Material = material,
                    Dicke = 18.0,
                    Dichte = 750.0,
                    Laufrichtung = GrainDirection.X
                });

                // Add edge configurations
                foreach (EdgeType edgeType in Enum.GetValues(typeof(EdgeType)))
                {
                    testBauteil.Kantenbilder.Add(new Kantenbild
                    {
                        KantenTyp = edgeType,
                        BearbeitungsTyp = EdgeProcessingType.Raw
                    });
                }

                // Remove existing Bauteil data
                var existingBauteil = obj.UserData.Find(typeof(Bauteil)) as Bauteil;
                if (existingBauteil != null)
                {
                    obj.UserData.Remove(existingBauteil);
                    RhinoApp.WriteLine("Removed existing Bauteil data.");
                }

                // Attach the test Bauteil
                obj.UserData.Add(testBauteil);
                obj.CommitChanges();
                doc.Views.Redraw();

                RhinoApp.WriteLine("Test Bauteil attached successfully!");
                RhinoApp.WriteLine("Select the object to see 'Bauteil' tab in Properties panel.");
                
                return Result.Success;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in TestBauteil command: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                return Result.Failure;
            }
        }
    }
} 