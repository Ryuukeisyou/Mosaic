using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Runtime.CompilerServices;
using System.Drawing;
using Grasshopper.Kernel.Types;

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
          : base("Mosaic", "Msc",
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
            pManager.AddIntegerParameter("Precision", "Pr", "Precision of reading image, from 1 to 10", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("PaletteCount", "Pc", "Size os color palette", GH_ParamAccess.item, 5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Planes", "P", "Generated planes", GH_ParamAccess.tree);
            pManager.AddRectangleParameter("Rectangles", "R", "Generated rectangles", GH_ParamAccess.tree);
            pManager.AddColourParameter("AvgColors", "aC", "Averaged color for each rectangle", GH_ParamAccess.tree);
            pManager.AddColourParameter("MatchedColors", "mC", "Matched palette color for each rectangle", GH_ParamAccess.tree);
            pManager.AddColourParameter("PaletteColors", "pC", "Analyzed palette colors", GH_ParamAccess.list);
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
            int precision = 0;

            Bitmap bm = null;
            List<ColorThiefDotNet.Color> palette = null;
            int interval = 5;
            int paletteCount = 5;

            DA.GetData("File", ref file);
            DA.GetData("Plane", ref plane);
            DA.GetData("Num_Row", ref n_row);
            DA.GetData("Num_Col", ref n_col);
            DA.GetData("Width", ref width);
            DA.GetData("Height", ref height);
            DA.GetData("Precision", ref precision);
            DA.GetData("PaletteCount", ref paletteCount);
            if (precision < 1) precision = 1;
            if (precision > 10) precision = 10;

            DataTree<Plane> plns = new DataTree<Plane>();
            DataTree<Rectangle3d> rects = new DataTree<Rectangle3d>();
            DataTree<GH_Colour> avgCs = new DataTree<GH_Colour>();
            DataTree<GH_Colour> matCs = new DataTree<GH_Colour>();
            List<GH_Colour> pltCs = new List<GH_Colour>();

            try
            {
                bm = new Bitmap(file);
                ColorThiefDotNet.ColorThief ct = new ColorThiefDotNet.ColorThief();
                palette = ct.GetPalette(bm, paletteCount, 10, false).Select(e => e.Color).ToList();
                interval = (int)Math.Pow((bm.Width * bm.Height) / (n_col * n_row), 0.5) / precision;
                foreach(var c in palette)
                {
                    Color color = Color.FromArgb(c.R, c.G, c.B);
                    pltCs.Add(new GH_Colour(color));
                }
            }
            catch { }
            
            for (int i = 0; i < n_col; i++)
            {
                for (int j = 0; j < n_row; j++)
                {
                    GH_Path path = new GH_Path(i);
                    Vector3d tx = plane.XAxis * i * width;
                    Vector3d ty = plane.YAxis * j * height;
                    Vector3d t = tx + ty;
                    Plane pln = plane.Clone();
                    pln.Translate(t);
                    plns.Add(pln, path);

                    Rectangle3d rect = new Rectangle3d(pln, width, height);
                    rects.Add(rect, path);

                    if (bm != null)
                    {
                        int x = (int)(Convert.ToDouble(bm.Width) / n_col * (i + 0.5));
                        int y = (int)(Convert.ToDouble(bm.Height) / n_row * (j + 0.5));
                        int mWidth = bm.Width / n_col;
                        int mHeight = bm.Height / n_row;

                        
                        Color avgColor = GetAveragedColor(bm, x, y, mWidth, mHeight, interval);
                        GH_Colour avgC = new GH_Colour(avgColor);
                        avgCs.Add(avgC, path);

                        var tempC = GetPaletteClosest(palette, avgColor);
                        Color matColor = Color.FromArgb(tempC.R, tempC.G, tempC.B);
                        GH_Colour matC = new GH_Colour(matColor);
                        matCs.Add(matC, path);
                    }
                    
                }
            }

            DA.SetDataTree(0, plns);
            DA.SetDataTree(1, rects);
            DA.SetDataTree(2, avgCs);
            DA.SetDataTree(3, matCs);
            DA.SetDataList(4, pltCs);
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

        private ColorThiefDotNet.Color GetPaletteClosest(IEnumerable<ColorThiefDotNet.Color> palette, Color color)
        {
            List<KeyValuePair<double, ColorThiefDotNet.Color>> pairs = new List<KeyValuePair<double, ColorThiefDotNet.Color>>();
            foreach(ColorThiefDotNet.Color item in palette)
            {
                double dist = Math.Pow(color.R - item.R, 2) + Math.Pow(color.G - item.G, 2) + Math.Pow(color.B - item.B, 2);
                pairs.Add(new KeyValuePair<double, ColorThiefDotNet.Color>(dist, item));
            }
            pairs.OrderBy(e => e.Key);
            return pairs.First().Value;
        }

        private Color GetAveragedColor(Bitmap bm, int x, int y, int width, int height, int interval = 10)
        {
            List<Color> colors = new List<Color>();
            int itvX = interval > width / 2 ? width / 2 : interval;
            int itvY = interval > height / 2 ? height / 2 : interval;
            for (int i = x - width / 2; i < x + width / 2; i += itvX)
            {
                for (int j = y - height / 2; j < y + height / 2; j += itvY)
                {
                    if (i < 0) continue;
                    if (i > bm.Width) continue;
                    if (j < 0) continue;
                    if (j > bm.Height) continue;
                    try
                    {
                        Color color = bm.GetPixel(i, j);
                        colors.Add(color);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            int r = (int)colors.Average(e => e.R);
            int g = (int)colors.Average(e => e.G);
            int b = (int)colors.Average(e => e.B);
            Color avgColor = Color.FromArgb(r, g, b);
            return avgColor;
        }
    }

}
