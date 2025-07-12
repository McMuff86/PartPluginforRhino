using System;
using System.Collections.Generic;
using Rhino;
using Rhino.PlugIns;
using Rhino.UI;
using BauteilPlugin.UI;

namespace BauteilPlugin
{
    /// <summary>
    /// Main plugin class for the Rhino Professional Bauteil-Plugin
    /// Handles plugin initialization and provides access to core functionality
    /// </summary>
    public class BauteilPlugin : PlugIn
    {
        public BauteilPlugin()
        {
            Instance = this;
        }

        /// <summary>
        /// Gets the only instance of the BauteilPlugin plugin
        /// </summary>
        public static BauteilPlugin Instance { get; private set; }

        /// <summary>
        /// Register custom object properties pages
        /// </summary>
        [Obsolete("This method is deprecated")]
        protected override void ObjectPropertiesPages(List<ObjectPropertiesPage> pages)
        {
            pages.Add(new BauteilPropertyPage());
        }

        /// <summary>
        /// Called when the plugin is loaded
        /// </summary>
        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            try
            {
                // Initialize the plugin
                RhinoApp.WriteLine("Professional Bauteil-Plugin loaded successfully");
                
                // Register the dockable panel
                Panels.RegisterPanel(this, typeof(BauteilEditorPanel), "Bauteil Editor", null);
                
                return LoadReturnCode.Success;
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load Bauteil Plugin: {ex.Message}";
                return LoadReturnCode.ErrorShowDialog;
            }
        }

        /// <summary>
        /// Called when the plugin is unloaded
        /// </summary>
        protected override void OnShutdown()
        {
            RhinoApp.WriteLine("Professional Bauteil-Plugin unloaded");
            base.OnShutdown();
        }
    }
} 