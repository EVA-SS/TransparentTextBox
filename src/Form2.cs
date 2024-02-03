using System.Drawing.Drawing2D;

namespace examples
{
    public partial class Form2 : Form
    {
        public Form2()
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

                int width = rect.Width / 6;
                Rectangle rect1 = new Rectangle(0, 0, width, rect.Height), rect2 = new Rectangle(rect1.X + width, 0, width, rect.Height),
                    rect3 = new Rectangle(rect1.X + width * 2, 0, width, rect.Height), rect4 = new Rectangle(rect1.X + width * 3, 0, width, rect.Height),
                    rect5 = new Rectangle(rect1.X + width * 4, 0, width, rect.Height), rect6 = new Rectangle(rect1.X + width * 5, 0, width, rect.Height);

                byte alpha = 100;
                using (var brush = new SolidBrush(Color.FromArgb(alpha, 255, 0, 0)))
                {
                    g.FillRectangle(brush, rect);
                }
                using (var brush = new LinearGradientBrush(rect1, Color.FromArgb(alpha, 255, 0, 0), Color.FromArgb(alpha, 255, 255, 0), 0F))
                {
                    g.FillRectangle(brush, rect1);
                }
                using (var brush = new LinearGradientBrush(rect2, Color.FromArgb(alpha, 255, 255, 0), Color.FromArgb(alpha, 0, 255, 0), 0F))
                {
                    g.FillRectangle(brush, rect2);
                }
                using (var brush = new LinearGradientBrush(rect3, Color.FromArgb(alpha, 0, 255, 0), Color.FromArgb(alpha, 0, 255, 255), 0F))
                {
                    g.FillRectangle(brush, rect3);
                }
                using (var brush = new LinearGradientBrush(rect4, Color.FromArgb(alpha, 0, 255, 255), Color.FromArgb(alpha, 0, 0, 255), 0F))
                {
                    g.FillRectangle(brush, rect4);
                }
                using (var brush = new LinearGradientBrush(rect5, Color.FromArgb(alpha, 0, 0, 255), Color.FromArgb(alpha, 255, 0, 255), 0F))
                {
                    g.FillRectangle(brush, rect5);
                }
                using (var brush = new LinearGradientBrush(rect6, Color.FromArgb(alpha, 255, 0, 255), Color.FromArgb(alpha, 255, 0, 0), 0F))
                {
                    g.FillRectangle(brush, rect6);
                }
            }
            base.OnPaint(e);
        }
    }
}
