using System;
using System.Drawing;
using System.Windows.Forms;

namespace TSkin.ST
{
    public abstract class TextView : ITextView
    {
        public Rectangle TextRectangle { get; protected set; }

        protected STTextBox.Core Core { get; private set; }

        public abstract int GetCurrentCharOffset();

        public void Init(STTextBox.Core textBoxCore)
        {
            this.Core = textBoxCore;
            this.OnInitAllText();
        }

        public abstract void SetCaretPostion(int nCharIndex);
        public virtual FindInfo SetCaretPostion(Point pt)
        {
            var fi = this.FindFromPoint(pt);
            if (!fi.Find)
            {
                return fi;
            }
            this.Core.Caret.CopyFromFindInfo(fi);
            this.SetCaretPostion(fi.Location.X, fi.Location.Y);
            return fi;
        }

        protected virtual void SetCaretPostion(int nX, int nY)
        {
            var caret = this.Core.Caret;
            caret.X = nX;
            caret.Y = nY;
            if (!this.TextRectangle.Contains(new Point(nX, nY)))
            {
                if (!caret.Visable) return;
                Win32.HideCaret(this.Core.TextBox.Handle);
                caret.Visable = false;
            }
            else
            {
                Win32.SetCaretPos(nX, nY);
                if (!caret.Visable)
                {
                    caret.Visable = true;
                    Win32.ShowCaret(this.Core.TextBox.Handle);
                }
            }
        }

        public virtual void ScrollXToMuosePoint(int nX)
        {
            var c = this.Core;
            nX -= c.ViewRectangle.X;
            nX -= c.Scroll.HThumbRect.Width / 2;
            if (nX < 0)
            {
                nX = 0;
            }
            else if (nX + c.Scroll.HThumbRect.Width > c.Scroll.HBackRect.Width)
            {
                nX = c.Scroll.HBackRect.Width - c.Scroll.HThumbRect.Width;
            }
            int nTemp = (int)((float)nX / (c.Scroll.HBackRect.Width - c.Scroll.HThumbRect.Width) * c.Scroll.MaxXValue);
            c.Scroll.XValue = nTemp;
            this.SetCaretPostion(c.Caret.IndexOfChar);
        }

        public virtual void ScrollYToMousePoint(int nY)
        {
            var c = this.Core;
            nY -= c.ViewRectangle.Y;
            nY -= c.Scroll.VThumbRect.Height / 2;
            if (nY < 0)
            {
                nY = 0;
            }
            else if (nY + c.Scroll.VThumbRect.Height > c.Scroll.VBackRect.Height)
            {
                nY = c.Scroll.VBackRect.Height - c.Scroll.VThumbRect.Height;
            }
            int nTemp = (int)((float)nY / (c.Scroll.VBackRect.Height - c.Scroll.VThumbRect.Height) * c.Scroll.MaxYValue);
            c.Scroll.YValue = nTemp;
            this.SetCaretPostion(c.Caret.IndexOfChar);
        }

        public virtual bool ScrollToCaret()
        {
            var c = this.Core;
            bool flag = false;
            Point pt = c.Caret.Location;
            if (pt.Y < this.TextRectangle.Y)
            {
                c.Scroll.YValue += (pt.Y - this.TextRectangle.Y) / c.LineHeight;
                flag = true;
            }
            else if (pt.Y + c.LineHeight > this.TextRectangle.Bottom)
            {
                int nIncrement = (int)Math.Ceiling((pt.Y + c.LineHeight - this.TextRectangle.Bottom) / (float)c.LineHeight);
                c.Scroll.YValue += nIncrement;
                flag = true;
            }
            if (pt.X >= this.TextRectangle.Right)
            {
                int nIncrement = (int)Math.Ceiling((pt.X - this.TextRectangle.Right + 1) / (float)c.Scroll.XIncrement);
                c.Scroll.XValue += nIncrement;
                flag = true;
            }
            else if (pt.X < this.TextRectangle.X)
            {
                int nIncrement = ((int)Math.Ceiling((this.TextRectangle.X - pt.X) / (float)c.Scroll.XIncrement));
                c.Scroll.XValue -= nIncrement;
                flag = true;
            }
            if (flag)
            {
                this.SetCaretPostion(c.Caret.IndexOfChar);
            }
            return flag;
        }

        public Point ControlToView(Point pt)
        {
            var c = this.Core;
            pt.X = pt.X - this.TextRectangle.X + this.Core.Scroll.XOffset;
            pt.Y = pt.Y - c.TextBox.GetIntYSize(c.ViewRectangle.Y) + c.Scroll.YValue * c.LineHeight;
            return pt;
        }

        public Point ViewToControl(Point pt)
        {
            var c = this.Core;
            pt.X = pt.X - c.Scroll.XOffset + this.TextRectangle.X;
            pt.Y = pt.Y - c.Scroll.YValue * c.LineHeight + c.TextBox.GetIntYSize(c.ViewRectangle.Y);
            return pt;
        }

        public void Invalidate()
        {
            if (this.Core != null && this.Core.TextBox != null)
            {
                this.Core.TextBox.Invalidate();
            }
        }

        public void OnDrawView(ISTTextBoxRender render)
        {
            var c = this.Core;
            int nXStart = this.TextRectangle.X;
            render.SetClip(this.TextRectangle);
            //this.OnDrawSelectionBackground(dt, c.Selection);    // fill selection background
            // start draw line
            int nY = this.TextRectangle.Y;
            int nStartLineIndex = this.GetLineIndexFromYScroll(c.Scroll.YValue);
            for (int i = nStartLineIndex; i < c.TextManager.LineCount; i++)
            {
                TextLine line = c.TextManager[i];
                if (!c.Selection.IsEmptySelection)
                {
                    this.OnDrawLineSelectionBackground(render, line, i, c.Selection, nY);
                }
                nY += this.OnDrawLine(render, line, nXStart, nY);
                if (nY > c.ViewRectangle.Bottom) break;
            }
            render.ResetClip();
        }

        protected abstract void OnDrawLineSelectionBackground(ISTTextBoxRender render, TextLine line, int nLineIndex, STTextBoxSelectionInfo selection, int nY);
        protected abstract int OnDrawLine(ISTTextBoxRender render, TextLine line, int nX, int nY);

        public virtual void OnResize(EventArgs e) { }

        public virtual void OnTextStartChange(ISTTextBoxRender render, TextManagerTextEventArgs e) { }
        public virtual void OnTextChanged(ISTTextBoxRender render, TextManagerTextEventArgs e) { }
        public virtual void OnLineCountChanged(ISTTextBoxRender render, EventArgs e) { }
        public abstract void OnInitAllText();
        public abstract void OnLineAdded(ISTTextBoxRender render, TextManagerLineEventArgs e);
        public abstract void OnLineRemoved(ISTTextBoxRender render, TextManagerLineEventArgs e);
        public abstract void OnLineChanged(ISTTextBoxRender render, TextManagerLineEventArgs e);
        public abstract int GetLineIndexFromYScroll(int nYValue);
        public abstract FindInfo FindFromPoint(Point pt);
        public abstract FindInfo FindFromCharIndex(int nIndex);

        public void OnSetCursor(MouseEventArgs e)
        {
            var c = this.Core;
            if (c.ViewRectangle.Contains(e.Location) && e.X < this.TextRectangle.X)
            {
                c.Scroll.HoverScrollBar = STTextBoxScrollInfo.ScrollBarType.None;
                this.SetCursor(Cursors.Arrow);
            }
            else
            {
                c.Scroll.HoverScrollBar = STTextBoxScrollInfo.ScrollBarType.None;
                this.SetCursor(Cursors.IBeam);
            }
        }

        private bool SetCursor(Cursor c)
        {
            if (this.Core.TextBox.Cursor == c)
            {
                return false;
            }
            this.Core.TextBox.Cursor = c;
            return true;
        }

        public virtual void OnCalcTextRectangle()
        {
            var c = this.Core;

            string strLineCount = c.TextManager.LineCount.ToString();
            this.Core.ITextBoxRender.BeginPaint();
            c.ITextBoxRender.EndPaint();
            Rectangle rect = c.ViewRectangle;
            Rectangle rect_temp = new Rectangle()
            {
                X = rect.X,
                Y = rect.Y,
                Height = rect.Height
            };
            rect_temp.X = this.Core.TextBox.GetIntXSize(2);
            rect_temp.Width = rect.Width - rect_temp.X - this.Core.TextBox.GetIntXSize(2) + rect.X;
            this.TextRectangle = rect_temp;
        }

        public abstract void OnCalcScroll(STTextBoxScrollInfo scrollInfo);
    }
}
