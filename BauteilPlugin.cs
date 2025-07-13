using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.PlugIns;
using Rhino.UI;
using BauteilPlugin.Models;
using BauteilPlugin.UI;
using Rhino.DocObjects;
using Rhino.Geometry; // Add missing using directive

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

                // Subscribe to events
                RhinoDoc.SelectObjects += OnSelectObjects;
                RhinoDoc.DeselectAllObjects += OnDeselectAllObjects;

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
            // Unsubscribe from events
            RhinoDoc.SelectObjects -= OnSelectObjects;
            RhinoDoc.DeselectAllObjects -= OnDeselectAllObjects;

            RhinoApp.WriteLine("Professional Bauteil-Plugin unloaded");
            base.OnShutdown();
        }

        /// <summary>
        /// Event handler for when all objects are deselected.
        /// </summary>
        private void OnDeselectAllObjects(object sender, EventArgs e)
        {
            // This can be used to clean up visualization artifacts in the future
        }

        /// <summary>
        /// Event handler called when objects are selected in the document.
        /// </summary>
        private void OnSelectObjects(object sender, RhinoObjectSelectionEventArgs e)
        {
            // Only proceed if exactly one object is selected
            if (e.RhinoObjects.Length != 1)
            {
                return;
            }

            var rhinoObject = e.RhinoObjects.FirstOrDefault();
            if (rhinoObject == null)
            {
                return;
            }

            // Check if the selected object has Bauteil data
            var bauteilData = rhinoObject.UserData.Find(typeof(Bauteil)) as Bauteil;
            if (bauteilData != null)
            {
                // If it has Bauteil data, set Gumball alignment to Object
                RhinoApp.RunScript("_-GumballAlignment _Object _Enter", false);

                // Visualize the layers using the Section command
                // VisualizeLayers(rhinoObject, bauteilData); // Temporarily disabled due to issues
            }
        }
        /*
        private void VisualizeLayers(RhinoObject rhinoObject, Bauteil bauteil)
        {
            var doc = rhinoObject.Document;
            if (doc == null || bauteil.Schichten.Count <= 1) return;

            var instance = rhinoObject as InstanceObject;
            if (instance == null) return;

            // --- 1. Delete old section curves for this object ---
            var oldCurves = doc.Objects.FindByUserString("BauteilSectionCurve", instance.Id.ToString(), true);
            foreach (var oldCurve in oldCurves)
            {
                doc.Objects.Delete(oldCurve, true);
            }

            // --- 2. Calculate section planes ---
            var definitionBBox = BoundingBox.Empty;
            foreach (var defObj in instance.InstanceDefinition.GetObjects())
            {
                definitionBBox.Union(defObj.Geometry.GetBoundingBox(Transform.Identity));
            }
            
            var localSectionPlanes = new List<Plane>();
            double currentZ = definitionBBox.Min.Z;
            for (int i = 0; i < bauteil.Schichten.Count - 1; i++)
            {
                currentZ += bauteil.Schichten[i].Dicke;
                localSectionPlanes.Add(new Plane(new Point3d(0, 0, currentZ), Vector3d.ZAxis));
            }

            // --- 3. Create sections using RunScript ---
            doc.Objects.UnselectAll();
            
            var newCurveIds = new List<Guid>();

            foreach (var localPlane in localSectionPlanes)
            {
                var worldPlane = localPlane;
                worldPlane.Transform(instance.InstanceXform);

                var p1 = worldPlane.Origin;
                var p2 = worldPlane.Origin + worldPlane.XAxis;
                var p3 = worldPlane.Origin + worldPlane.YAxis;
                
                string script = $"_-Section _SelID {rhinoObject.Id} _Enter _Point {p1.X},{p1.Y},{p1.Z} {p2.X},{p2.Y},{p2.Z} {p3.X},{p3.Y},{p3.Z} _Enter";
                
                var curvesBefore = doc.Objects.GetObjectList(ObjectType.Curve).Select(c => c.Id).ToList();
                RhinoApp.RunScript(script, false);
                var curvesAfter = doc.Objects.GetObjectList(ObjectType.Curve).Select(c => c.Id).ToList();
                
                newCurveIds.AddRange(curvesAfter.Except(curvesBefore));
            }

            // --- 4. Tag and group new curves ---
            if (newCurveIds.Any())
            {
                var groupName = $"BauteilSections_{instance.Id.ToString().Substring(0, 5)}";
                int groupIndex = doc.Groups.Add(groupName);

                if (groupIndex >= 0)
                {
                    foreach (var curveId in newCurveIds)
                    {
                        var curveObject = doc.Objects.FindId(curveId);
                        if (curveObject != null)
                        {
                            curveObject.Attributes.SetUserString("BauteilSectionCurve", instance.Id.ToString());
                            curveObject.Attributes.AddToGroup(groupIndex);
                            curveObject.CommitChanges();
                        }
                    }
                }
            }
            doc.Views.Redraw();
        }
        */
    }
} 