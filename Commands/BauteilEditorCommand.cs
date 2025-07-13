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
} 