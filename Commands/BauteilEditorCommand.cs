using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.UI;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using BauteilPlugin.UI;
using BauteilPlugin.Models;
using System.IO;
using System.Windows.Forms;

namespace BauteilPlugin.Commands
{
    /// <summary>
    /// Command to open the Bauteil editor panel
    /// </summary>
    public class BauteilEditorCommand : Command
    {
        /// <summary>
        /// Constructor for the command
        /// </summary>
        public BauteilEditorCommand()
        {
            Instance = this;
        }

        /// <summary>
        /// Singleton instance of the command
        /// </summary>
        public static BauteilEditorCommand Instance { get; private set; }

        /// <summary>
        /// English name of the command
        /// </summary>
        public override string EnglishName => "BauteilEditor";

        /// <summary>
        /// Run the command
        /// </summary>
        /// <param name="doc">Current Rhino document</param>
        /// <param name="mode">Command run mode</param>
        /// <returns>Command result</returns>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var panel = BauteilEditorPanel.GetPanel();
            if (panel != null)
            {
                var visible = panel.Visible;
                panel.Visible = !visible;
                return Result.Success;
            }
            return Result.Failure;
        }
    }

    /// <summary>
    /// Command to export Bauteil data to CSV
    /// </summary>
    public class BauteilExportCommand : Command
    {
        /// <summary>
        /// Constructor for the command
        /// </summary>
        public BauteilExportCommand()
        {
            Instance = this;
        }

        /// <summary>
        /// Singleton instance of the command
        /// </summary>
        public static BauteilExportCommand Instance { get; private set; }

        /// <summary>
        /// English name of the command
        /// </summary>
        public override string EnglishName => "ExportBauteilCSV";

        /// <summary>
        /// Run the command
        /// </summary>
        /// <param name="doc">Current Rhino document</param>
        /// <param name="mode">Command run mode</param>
        /// <returns>Command result</returns>
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Get selected objects
            var go = new GetObject();
            go.SetCommandPrompt("Select Bauteile to export to CSV");
            go.GeometryFilter = ObjectType.AnyObject;
            go.GetMultiple(1, 0);
            if (go.CommandResult() != Result.Success)
                return go.CommandResult();

            // Check if any selected objects have Bauteil data
            var bauteile = new System.Collections.Generic.List<Bauteil>();
            foreach (var objRef in go.Objects())
            {
                var rhinoObject = objRef.Object();
                if (rhinoObject != null)
                {
                    var bauteil = Bauteil.TryGetBauteil(rhinoObject);
                    if (bauteil != null)
                    {
                        bauteile.Add(bauteil);
                    }
                }
            }

            if (bauteile.Count == 0)
            {
                RhinoApp.WriteLine("No Bauteile found in selection.");
                return Result.Nothing;
            }

            // Ask user for save location
            var saveDialog = new Rhino.UI.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Export Bauteil data to CSV",
                DefaultExt = "csv"
            };
                
            if (!saveDialog.ShowSaveDialog())
                return Result.Cancel;

            string filePath = saveDialog.FileName;
                
            // If only one Bauteil is selected, use the single export method
            if (bauteile.Count == 1)
            {
                if (bauteile[0].ExportToCSV(filePath))
                {
                    RhinoApp.WriteLine($"Successfully exported Bauteil '{bauteile[0].Name}' to {filePath}");
                    return Result.Success;
                }
                else
                {
                    RhinoApp.WriteLine("Failed to export Bauteil data.");
                    return Result.Failure;
                }
            }
            // If multiple Bauteile are selected, create a directory and export each one
            else
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                    
                bool allSuccessful = true;
                foreach (var bauteil in bauteile)
                {
                    string individualFilePath = Path.Combine(directory, 
                        $"{fileNameWithoutExt}_{bauteil.Name.Replace(" ", "_")}.csv");
                        
                    if (!bauteil.ExportToCSV(individualFilePath))
                    {
                        RhinoApp.WriteLine($"Failed to export Bauteil '{bauteil.Name}'.");
                        allSuccessful = false;
                    }
                }
                    
                if (allSuccessful)
                {
                    RhinoApp.WriteLine($"Successfully exported {bauteile.Count} Bauteile to {directory}");
                    return Result.Success;
                }
                else
                {
                    RhinoApp.WriteLine("Some Bauteile could not be exported.");
                    return Result.Failure;
                }
            }
        }
    }
} 