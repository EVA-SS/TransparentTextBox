using System;

namespace ST.Library.Drawing.SvgRender
{
    public class SvgElementAttribute : Attribute
    {
        public string TargetName { get; private set; }

        public SvgElementAttribute(string strTargetName)
        {
            this.TargetName = strTargetName;
        }
    }
}
