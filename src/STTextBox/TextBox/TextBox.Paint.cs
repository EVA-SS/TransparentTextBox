using examples;
using System.Globalization;

namespace AntDesign
{
    partial class TextBox
    {
        #region 渲染

        internal StringFormat stringLeft = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        internal StringFormat stringTL = new StringFormat { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisCharacter };

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics.High();
            bool enabled = Enabled;
            if (enabled)
            {
                using (var bmp = PaintText(ForeColor, Width, Height))
                {
                    g.DrawImage(bmp, rect_text, rect_text, GraphicsUnit.Pixel);
                }
            }
            else
            {
                using (var bmp = PaintText(Color.FromArgb(100, ForeColor), Width, Height))
                {
                    g.DrawImage(bmp, rect_text, rect_text, GraphicsUnit.Pixel);
                }
            }
            base.OnPaint(e);
        }

        #region 渲染帮助

        Bitmap PaintText(Color _fore, int w, int h)
        {
            var bmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp).High())
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                if (cache_font != null)
                {
                    g.TranslateTransform(-ScrollX, -ScrollY);
                    if (selectionLength > 0)
                    {
                        int end = selectionStartTemp + selectionLength - 1;
                        if (end > cache_font.Length - 1) end = cache_font.Length - 1;
                        CacheFont first = cache_font[selectionStartTemp];
                        using (var brush = new SolidBrush(Color.FromArgb(173, 214, 255)))
                        {
                            for (int i = selectionStartTemp; i <= end; i++)
                            {
                                var last = cache_font[i];
                                if (first.rect.Y != last.rect.Y || last.retun)
                                {
                                    //先渲染上一行
                                    g.FillRectangle(brush, new Rectangle(first.rect.X, first.rect.Y, cache_font[i - 1].rect.Right - first.rect.X, first.rect.Height));
                                    if (i == end) g.FillRectangle(brush, last.rect);
                                    first = last;
                                }
                                else if (i == end) g.FillRectangle(brush, new Rectangle(first.rect.X, first.rect.Y, last.rect.Right - first.rect.X, first.rect.Height));
                            }
                        }
                    }
                    using (var fore = new SolidBrush(_fore))
                    {
                        if (HasEmoji)
                        {
                            using (var font = new Font(EmojiFont, Font.Size))
                            {
                                foreach (var it in cache_font)
                                {
                                    if (it.emoji) g.DrawString(it.text, font, fore, it.rect, m_sf);
                                    else g.DrawString(it.text, Font, fore, it.rect, m_sf);
                                }
                            }
                        }
                        else
                        {
                            foreach (var it in cache_font) g.DrawString(it.text, Font, fore, it.rect, m_sf);
                        }
                    }
                }
                else if (placeholderText != null)
                {
                    using (var fore = new SolidBrush(Color.FromArgb(100, ForeColor)))
                    {
                        g.DrawString(placeholderText, Font, fore, rect_text, multiline ? stringTL : stringLeft);
                    }
                }
            }
            return bmp;
        }

        #endregion

        #endregion

        #region 滚动条

        int scrollx = 0, scrolly = 0, ScrollXMin = 0, ScrollXMax = 0, ScrollYMax = 0;
        int ScrollX
        {
            get => scrollx;
            set
            {
                if (value > ScrollXMax) value = ScrollXMax;
                if (value < ScrollXMin) value = ScrollXMin;
                if (scrollx == value) return;
                scrollx = value;
                Invalidate();
                Win32.SetCaretPos(CurrentCaret.X - scrollx, CurrentCaret.Y - scrolly);
            }
        }
        int ScrollY
        {
            get => scrolly;
            set
            {
                if (value > ScrollYMax) value = ScrollYMax;
                if (value < 0) value = 0;
                if (scrolly == value) return;
                scrolly = value;
                Invalidate();
                Win32.SetCaretPos(CurrentCaret.X - scrollx, CurrentCaret.Y - scrolly);
            }
        }

        bool ScrollXShow = false, ScrollYShow = false;
        void ScrollTo(Rectangle r)
        {
            if (ScrollYShow)
            {
                int y = CurrentCaret.Y - scrolly;
                if (y < rect_text.Y) ScrollY -= rect_text.Height;
                else if (y + CurrentCaret.Height > rect_text.Height) ScrollY += rect_text.Height;
            }
            else if (ScrollXShow)
            {
                int x = CurrentCaret.X - scrollx;
                if (x < rect_text.X) ScrollX -= r.Width;
                else if (x + CurrentCaret.Width > rect_text.Width) ScrollX += r.Width;
            }
            else ScrollX = ScrollY = 0;
        }

        #endregion

        #region 坐标

        #region 确定字体宽度

        StringFormat m_sf = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.MeasureTrailingSpaces };

        CacheFont[]? cache_font = null;
        bool HasEmoji = false;
        void FixFontWidth()
        {
            HasEmoji = false;
            cache_font = null;
            using (var bmp = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    float dpi = g.DpiX / 96F;
                    int font_height = 0;
                    if (string.IsNullOrEmpty(_text))
                    {
                        ScrollX = ScrollY = 0;
                        font_height = (int)Math.Ceiling(g.MeasureString("Qq", Font, 10000, m_sf).Height);
                    }
                    else
                    {
                        var font_widths = new List<CacheFont>();
                        if (IsPassWord)
                        {
                            var sizefont = g.MeasureString(PassWordChar, Font, 10000, m_sf);
                            int w = (int)Math.Ceiling(sizefont.Width);
                            font_height = (int)Math.Ceiling(sizefont.Height);
                            foreach (char it in _text) font_widths.Add(new CacheFont(PassWordChar, false, w));
                        }
                        else
                        {
                            bool iseone = false;
                            foreach (char it in _text)
                            {
                                string txt = it.ToString();
                                var unicodeInfo = CharUnicodeInfo.GetUnicodeCategory(it);
                                if (IsEmoji(unicodeInfo))
                                {
                                    HasEmoji = true;
                                    if (unicodeInfo == UnicodeCategory.Surrogate)
                                    {
                                        if (iseone)
                                        {
                                            iseone = false;
                                            font_widths[font_widths.Count - 1].text += txt;
                                            continue;
                                        }
                                        else iseone = true;
                                    }
                                    else iseone = false;
                                    font_widths.Add(new CacheFont(txt, true, 0));
                                }
                                else
                                {
                                    iseone = false;
                                    if (it == '\t')
                                    {
                                        var sizefont = g.MeasureString(" ", Font, 10000, m_sf);
                                        if (font_height < sizefont.Height) font_height = (int)Math.Ceiling(sizefont.Height);
                                        font_widths.Add(new CacheFont(txt, false, (int)Math.Ceiling(sizefont.Width * 8F)));
                                    }
                                    else if (it == '\n') font_widths.Add(new CacheFont(txt, false, 0));
                                    else
                                    {
                                        var sizefont = g.MeasureString(txt, Font, 10000, m_sf);
                                        if (font_height < sizefont.Height) font_height = (int)Math.Ceiling(sizefont.Height);
                                        font_widths.Add(new CacheFont(txt, false, (int)Math.Ceiling(sizefont.Width)));
                                    }
                                }
                            }

                            if (HasEmoji)
                            {
                                using (var font = new Font(EmojiFont, Font.Size))
                                {
                                    foreach (var it in font_widths)
                                    {
                                        if (it.emoji)
                                        {
                                            var sizefont = g.MeasureString(it.text, font, 10000, m_sf);
                                            if (font_height < sizefont.Height) font_height = (int)Math.Ceiling(sizefont.Height);
                                            it.width = (int)Math.Ceiling(sizefont.Width);
                                        }
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < font_widths.Count; i++) { font_widths[i].i = i; }
                        cache_font = font_widths.ToArray();
                    }
                    CurrentCaret.Height = font_height;
                    CalculateRect();
                }
            }
        }

        bool IsEmoji(UnicodeCategory unicodeInfo)
        {
            //return unicodeInfo == UnicodeCategory.Surrogate;
            return unicodeInfo == UnicodeCategory.Surrogate || unicodeInfo == UnicodeCategory.OtherSymbol ||
                 unicodeInfo == UnicodeCategory.MathSymbol ||
                  unicodeInfo == UnicodeCategory.EnclosingMark ||
                   unicodeInfo == UnicodeCategory.NonSpacingMark ||
                  unicodeInfo == UnicodeCategory.ModifierLetter;
        }

        class CacheFont
        {
            public CacheFont(string _text, bool _emoji, int _width)
            {
                text = _text;
                emoji = _emoji;
                width = _width;
            }
            public int i { get; set; }
            public string text { get; set; }
            public Rectangle rect { get; set; }
            public bool emoji { get; set; }
            public bool retun { get; set; }
            public int width { get; set; }
        }

        #endregion

        Rectangle rect_text;
        void CalculateRect()
        {
            var rect = ClientRectangle;
            int sps = (int)(CurrentCaret.Height * .4F), sps2 = sps * 2;
            if (multiline) rect_text = new Rectangle(rect.X + sps, rect.Y + sps, rect.Width - sps2, rect.Height - sps2);
            else rect_text = new Rectangle(rect.X + sps, rect.Y + (rect.Height - CurrentCaret.Height) / 2, rect.Width - sps2, CurrentCaret.Height);

            if (cache_font == null) CurrentCaret.Location = rect_text.Location;
            else
            {
                if (multiline)
                {
                    int usex = 0, usey = 0;
                    foreach (var it in cache_font)
                    {
                        if (it.text == "\r")
                        {
                            it.retun = true;
                            it.rect = new Rectangle(rect_text.X + usex, rect_text.Y + usey, it.width, CurrentCaret.Height);
                            continue;
                        }
                        else if (it.text == "\n")
                        {
                            it.retun = true;
                            usey += CurrentCaret.Height;
                            usex = 0;
                            it.rect = new Rectangle(rect_text.X + usex, rect_text.Y + usey, it.width, CurrentCaret.Height);
                            continue;
                        }
                        else if (usex + it.width > rect_text.Width)
                        {
                            usey += CurrentCaret.Height;
                            usex = 0;
                        }
                        it.rect = new Rectangle(rect_text.X + usex, rect_text.Y + usey, it.width, CurrentCaret.Height);
                        usex += it.width;
                    }
                }
                else
                {
                    int usex = 0;
                    foreach (var it in cache_font)
                    {
                        it.rect = new Rectangle(rect_text.X + usex, rect_text.Y, it.width, CurrentCaret.Height);
                        usex += it.width;
                    }
                }

                if (textalign == HorizontalAlignment.Right)
                {
                    int y = -1;
                    var list = new List<CacheFont>();
                    Action action = () =>
                    {
                        if (list.Count > 0)
                        {
                            int w = rect_text.Right - list[list.Count - 1].rect.Right;
                            foreach (var it in list)
                            {
                                var rect_tmp = it.rect;
                                rect_tmp.Offset(w, 0);
                                it.rect = rect_tmp;
                            }
                            list.Clear();
                        }
                    };
                    foreach (var it in cache_font)
                    {
                        if (it.rect.Y != y)
                        {
                            y = it.rect.Y;
                            action();
                        }
                        list.Add(it);
                    }
                    action();
                }
                else if (textalign == HorizontalAlignment.Center)
                {
                    int y = -1;
                    var list = new List<CacheFont>();
                    Action action = () =>
                    {
                        if (list.Count > 0)
                        {
                            int w = (rect_text.Right - list[list.Count - 1].rect.Right) / 2;
                            foreach (var it in list)
                            {
                                var rect_tmp = it.rect;
                                rect_tmp.Offset(w, 0);
                                it.rect = rect_tmp;
                            }
                            list.Clear();
                        }
                    };
                    foreach (var it in cache_font)
                    {
                        if (it.rect.Y != y)
                        {
                            y = it.rect.Y;
                            action();
                        }
                        list.Add(it);
                    }
                    action();
                }

                var last = cache_font[cache_font.Length - 1];
                ScrollXMax = last.rect.Right - rect.Width + sps;
                switch (textalign)
                {
                    case HorizontalAlignment.Center:
                        if (ScrollXMax > 0) ScrollXMin = -ScrollXMax;
                        else
                        {
                            ScrollXMin = ScrollXMax;
                            ScrollXMax = -ScrollXMax;
                        }
                        break;
                    case HorizontalAlignment.Right:
                        ScrollXMin = cache_font[0].rect.Right - rect.Width + sps;
                        ScrollXMax = 0;
                        break;
                    default:
                        ScrollXMin = 0;
                        break;
                }
                ScrollYMax = last.rect.Bottom - rect.Height + sps;
                if (multiline)
                {
                    ScrollX = 0;
                    ScrollXShow = false;
                    ScrollYShow = last.rect.Bottom > rect.Bottom;
                    if (ScrollYShow)
                    {
                        if (ScrollY > ScrollYMax) ScrollY = ScrollYMax;
                    }
                    else ScrollY = 0;
                }
                else
                {
                    ScrollYShow = false;
                    ScrollY = 0;
                    if (textalign == HorizontalAlignment.Right) ScrollXShow = last.rect.Right < rect.Right;
                    else ScrollXShow = last.rect.Right > rect.Right;
                    if (ScrollXShow)
                    {
                        if (ScrollX > ScrollXMax) ScrollX = ScrollXMax;
                    }
                    else ScrollX = 0;
                }
            }
            SetCaretPostion();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            CalculateRect();
            base.OnSizeChanged(e);
        }

        #endregion
    }
}