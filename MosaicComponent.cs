using ColorThiefDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Runtime.CompilerServices;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Mosaic
{
    public class MosaicComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MosaicComponent()
          : base("Mosaic", "msc",
              "Create mosaic",
              "Mosaic", "Mosaic")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File", "F", "Image source file", GH_ParamAccess.item, "");
            pManager.AddPlaneParameter("Plane", "P", "Base plane for mosaic", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("Num_Row", "nR", "Number of rows", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("Num_Col", "nC", "Number of rows", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("Width", "w", "Width of mosaic", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Height", "h", "Height of mosaic", GH_ParamAccess.item, 10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Planes", "P", "Generated planes", GH_ParamAccess.tree);
            pManager.AddRectangleParameter("Rectangles", "R", "Generated rectangles", GH_ParamAccess.tree);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string file = "";
            Plane plane = new Plane();
            int n_row = 0;
            int n_col = 0;
            double width = 0;
            double height = 0;
            DA.GetData("File", ref file);
            DA.GetData("Plane", ref plane);
            DA.GetData("Num_Row", ref n_row);
            DA.GetData("Num_Col", ref n_col);
            DA.GetData("Width", ref width);
            DA.GetData("Height", ref height);

            System.Drawing.Bitmap bitmap = null;
            DataTree<Plane> plns = new DataTree<Plane>();
            DataTree<Rectangle3d> rects = new DataTree<Rectangle3d>();
            for (int i = 0; i < n_col; i++)
            {
                for (int j = 0; j < n_row; j++)
                {
                    GH_Path path = new GH_Path(i);
                    Vector3d x = plane.XAxis * i * width;
                    Vector3d y = plane.YAxis * j * height;
                    Vector3d t = x + y;
                    Plane pln = plane.Clone();
                    pln.Translate(t);
                    plns.Add(pln, path);

                    Rectangle3d rect = new Rectangle3d(pln, width, height);
                    rects.Add(rect, path);
                }
            }

            try
            {
                bitmap = new System.Drawing.Bitmap(file);
            }
            catch
            {

            }

            DA.SetDataTree(0, plns);
            DA.SetDataTree(1, rects);

            if (bitmap == null) return;
            ColorThief ct = new ColorThief();
            var colors = ct.GetPalette(bitmap, 6, 10, false).Select(e=>e.Color);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6f87ba32-2394-4736-978a-f04c40a5be0d"); }
        }
    }

}
