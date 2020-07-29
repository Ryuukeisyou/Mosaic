using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Mosaic
{
    public class MosaicInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Mosaic";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("6dde1ed7-6d3f-45d6-8187-f90209833299");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Ryuukeisyou";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
