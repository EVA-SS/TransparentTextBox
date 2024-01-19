using System.Drawing.Drawing2D;

namespace examples
{
    public static class Helper
    {
        #region 渲染帮助

        public static Graphics High(this Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            return g;
        }

        #endregion
    }
}