using System.Drawing.Drawing2D;

namespace examples
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            SetStyle(
               ControlStyles.AllPaintingInWmPaint |
               ControlStyles.OptimizedDoubleBuffer |
               ControlStyles.ResizeRedraw |
               ControlStyles.DoubleBuffer |
               ControlStyles.SupportsTransparentBackColor |
               ControlStyles.ContainerControl |
               ControlStyles.UserPaint, true);
            UpdateStyles();
            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = ClientRectangle;
            if (rect.Width > 0 && rect.Height > 0)
            {
                var g = e.Graphics.High();
                using (var brush = new LinearGradientBrush(rect, Color.White, Color.Transparent, LinearGradientMode.BackwardDiagonal)
                {
                    LinearColors = new Color[] {
                    Color.FromArgb(100, 255, 0, 0),
                    Color.FromArgb(100, 0, 255, 0),
                    Color.FromArgb(100, 0, 0, 255),
                    Color.FromArgb(100, 255,255, 0),
                    Color.FromArgb(100, 0,255,255),
                    Color.FromArgb(100,255,0,255)
                }
                })
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.FillRectangle(brush, rect);
                }
            }
            base.OnPaint(e);
        }
    }
}