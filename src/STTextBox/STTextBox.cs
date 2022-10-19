using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TSkin.ST
{
    public partial class STTextBox : Control
    {
        [Browsable(false)]
        public float XDPIZoom { get; private set; }
        [Browsable(false)]
        public float YDPIZoom { get; private set; }

        private int _LineSpacing = 0;

        [Description("行距"), Category("外观"), DefaultValue(0)]
        public int LineSpacing
        {
            get { return _LineSpacing; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("The value must be more than zero");
                }
                _LineSpacing = value;
                this.Invalidate();
            }
        }

        private bool _readOnly = false;
        [Description("是否只读"), Category("外观"), DefaultValue(false)]
        public bool ReadOnly
        {
            get { return _readOnly; }
            set { _readOnly = value; }
        }

        private bool _password = false;
        [Description("是否密码框"), Category("外观"), DefaultValue(false)]
        public bool Password
        {
            get { return _password; }
            set { _password = value; }
        }


        private bool _HandTab = false;

        [Description("处理Tab"), Category("外观"), DefaultValue(false)]
        public bool HandTab
        {
            get { return _HandTab; }
            set
            {
                _HandTab = value;
            }
        }

        private int _CharSpacing = 0;

        [Description("焦距"), Category("外观"), DefaultValue(0)]
        public int CharSpacing
        {
            get { return _CharSpacing; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("The value must be more than zero");
                }
                _CharSpacing = value;
                this.Invalidate();
            }
        }


        private Color _SelectionColor = Color.FromArgb(125, 169, 169, 169);

        [Description("选中颜色"), Category("外观"), DefaultValue(typeof(Color), "125, 169, 169 ,169")]
        public Color SelectionColor
        {
            get { return _SelectionColor; }
            set
            {
                if (value == _SelectionColor)
                {
                    return;
                }
                _SelectionColor = value;
                if (!m_core.Selection.IsEmptySelection)
                {
                    this.Invalidate();
                }
            }
        }

        private bool _AllowScrollBar = true;

        [Description("接受显示"), Category("滚动条"), DefaultValue(true)]
        public bool AllowScrollBar
        {
            get { return _AllowScrollBar; }
            set { _AllowScrollBar = value; }
        }

        private Color _ScrollbarBackColor = Color.Transparent;

        [Description("背景色"), Category("滚动条"), DefaultValue(typeof(Color), "Transparent")]
        public Color ScrollbarBackColor
        {
            get { return _ScrollbarBackColor; }
            set { _ScrollbarBackColor = value; }
        }

        private Color _ScrollBarCornerColor = Color.FromArgb(80, 128, 128, 128);

        [Description("角色"), Category("滚动条"), DefaultValue(typeof(Color), "80, 128, 128, 128")]
        public Color ScrollBarCornerColor
        {
            get { return _ScrollBarCornerColor; }
            set { _ScrollBarCornerColor = value; }
        }

        private Color _ScrollbarThumbHoverColor = Color.FromArgb(80, 128, 128, 128);

        [Description("滑动条点击颜色"), Category("滚动条"), DefaultValue(typeof(Color), "80, 128, 128, 128")]
        public Color ScrollbarThumbHoverColor
        {
            get { return _ScrollbarThumbHoverColor; }
            set { _ScrollbarThumbHoverColor = value; }
        }

        private Color _ScrollbarThumbBackColor = Color.FromArgb(80, 0, 0, 0);

        [Description("滑动条背景颜色"), Category("滚动条"), DefaultValue(typeof(Color), "80, 0, 0, 0")]
        public Color ScrollbarThumbBackColor
        {
            get { return _ScrollbarThumbBackColor; }
            set { _ScrollbarThumbBackColor = value; }
        }

        private int _TabSize = 4;

        private bool _AutoIndent = true;

        private bool _TabToSpace = false;

        private const int WM_MOUSEHWHEEL = 0x020E;
        private object m_obj_sync = new object();
        private Timer m_timer;
        private IntPtr m_hIMC;
        protected Core m_core;


        public STTextBox()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            m_timer = new Timer
            {
                Interval = 1000
            };
            m_timer.Tick += m_timer_Tick;
            m_core = new Core(this);
        }

        #region override

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            using (Graphics g = this.CreateGraphics())
            {
                this.XDPIZoom = g.DpiX / 96;
                this.YDPIZoom = g.DpiY / 96;
            }
        }

        protected override void DestroyHandle()
        {
            Win32.HideCaret(this.Handle);
            Win32.DestroyCaret();
            m_timer.Stop();
            m_timer.Dispose();
            base.DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32.WM_IME_STARTCOMPOSITION:
                    m_hIMC = Win32.ImmGetContext(this.Handle);
                    this.OnImeStartPrivate(m_hIMC);
                    break;
                case Win32.WM_IME_ENDCOMPOSITION:
                    this.OnImeEndPrivate(m_hIMC);
                    break;
                case Win32.WM_IME_COMPOSITION:
                    if (((int)m.LParam & Win32.GCS_RESULTSTR) == Win32.GCS_RESULTSTR)
                    {
                        m.Result = (IntPtr)1;
                        this.OnImeResultStrPrivate(m_hIMC, Win32.ImmGetCompositionString(m_hIMC, Win32.GCS_RESULTSTR));
                        return;//Interrupt, WM_CHAR, WM_IME_CHAR messages will not be generated.
                    }
                    if (((int)m.LParam & Win32.GCS_COMPSTR) == Win32.GCS_COMPSTR)
                    {
                        this.OnImeCompStr(m_hIMC, Win32.ImmGetCompositionString(m_hIMC, Win32.GCS_COMPSTR));
                    }
                    break;
                case WM_MOUSEHWHEEL:
                    Point pt = new Point(((int)m.LParam) >> 16, (ushort)m.LParam);
                    pt = this.PointToClient(pt);
                    MouseButtons mb = MouseButtons.None;
                    int n = (ushort)m.WParam;
                    if ((n & 0x0001) == 0x0001) mb |= MouseButtons.Left;
                    if ((n & 0x0010) == 0x0010) mb |= MouseButtons.Middle;
                    if ((n & 0x0002) == 0x0002) mb |= MouseButtons.Right;
                    if ((n & 0x0020) == 0x0020) mb |= MouseButtons.XButton1;
                    if ((n & 0x0040) == 0x0040) mb |= MouseButtons.XButton2;
                    this.OnMouseHWheel(new MouseEventArgs(mb, 0, pt.X, pt.Y, ((int)m.WParam) >> 16));
                    break;
            }
            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            this.XDPIZoom = e.Graphics.DpiX / 96;
            this.YDPIZoom = e.Graphics.DpiY / 96;
            var render = m_core.ITextBoxRender;
            //try {
            render.OnBeginPaint(e.Graphics);
            m_core.ITextView.OnDrawView(render);
            if (this._AllowScrollBar && m_core.Scroll.CountDown != 0)
            {
                this.OnCalcScrollRectangle(m_core.Scroll);
                bool bFlag = true;
                if (m_core.Scroll.HBackRect != m_core.Scroll.HThumbRect)
                {
                    this.OnDrawHScrollBar(render, m_core.Scroll);
                }
                else { bFlag = false; }
                if (m_core.Scroll.VBackRect != m_core.Scroll.VThumbRect)
                {
                    this.OnDrawVScrollBar(render, m_core.Scroll);
                }
                else { bFlag = false; }
                if (bFlag) this.OnDrawScrollBarCorner(render, m_core.Scroll);
            }
            render.OnEndPaint(e.Graphics);
            //} catch (Exception ex) {
            //    MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            //}
            //sw.Stop();
            //Console.WriteLine("OnPaint - " + sw.ElapsedMilliseconds + "ms");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int nX = this.GetIntXSize(0);
            int nY = this.GetIntYSize(0);
            var rect = new Rectangle(nX, nY, this.Width - nX * 2, this.Height - nY * 2);
            if (rect.Height < 1 || rect.Width < 1 || rect == m_core.ViewRectangle)
            {
                return;
            }
            m_core.ViewRectangle = rect;
            m_core.ITextView.OnCalcTextRectangle();
            m_core.ITextView.OnResize(e);
            m_core.ITextView.OnCalcScroll(m_core.Scroll);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Win32.CreateCaret(this.Handle, IntPtr.Zero, this.GetIntXSize(1), m_core.LineHeight);
            m_core.ITextView.SetCaretPostion(m_core.TextManager.TextLength);
            Win32.ShowCaret(this.Handle);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            m_core.Selection.Clear();
            Win32.HideCaret(this.Handle);
            Win32.DestroyCaret();
            this.Invalidate();
        }

        #endregion

        #region public

        public float GetFloatXSize(int nSize) { return nSize * this.XDPIZoom; }
        public float GetFloatXSize(float fSize) { return fSize * this.XDPIZoom; }
        public int GetIntXSize(int nSize) { return (int)Math.Round(nSize * this.XDPIZoom); }
        public int GetIntXSize(float fSize) { return (int)Math.Round(fSize * this.XDPIZoom); }

        public float GetFloatYSize(int nSize) { return nSize * this.YDPIZoom; }
        public float GetFloatYSize(float fSize) { return fSize * this.YDPIZoom; }
        public int GetIntYSize(int nSize) { return (int)Math.Round(nSize * this.YDPIZoom); }
        public int GetIntYSize(float fSize) { return (int)Math.Round(fSize * this.YDPIZoom); }

        public void ShowScrollBar(int nSecond)
        {
            lock (m_obj_sync)
            {
                m_core.Scroll.CountDown = nSecond;
            }
            m_timer.Start();
        }

        public TextHistoryRecord SetText(string strText)
        {
            return m_core.TextManager.SetText(strText);
        }

        public TextHistoryRecord SetText(int nIndex, string strText)
        {
            return m_core.TextManager.SetText(nIndex, strText);
        }

        public TextHistoryRecord SetText(int nIndex, int nLen, string strText)
        {
            return m_core.TextManager.SetText(nIndex, nLen, strText);
        }

        public override string Text
        {
            get => m_core.TextManager.GetText();
            set
            {
                m_core.TextManager.SetText(value);
            }
        }

        public void SelectAll()
        {
            m_core.Selection.SetSelection(0, m_core.TextManager.TextLength);
            m_core.Caret.IndexOfChar = m_core.Selection.EndIndex;
            m_core.ITextView.SetCaretPostion(m_core.Caret.IndexOfChar);
            this.Invalidate();
        }

        public void Copy()
        {
            this.Copy(false);
        }

        public void Cut()
        {
            this.Copy(true);
        }

        private void Copy(bool bCut)
        {
            var c = m_core;
            if (c.Selection.IsEmptySelection)
            {
                return;
            }
            Clipboard.SetText(c.TextManager.GetText(c.Selection.StartIndex, c.Selection.Length));
            if (bCut)
            {
                this.EnterText("");
            }
        }

        public void Paste()
        {
            string strText = Clipboard.GetText();
            if (string.IsNullOrEmpty(strText))
            {
                return;
            }
            this.EnterText(strText);
        }

        public void Undo()
        {
            this.RunHistory(true);
        }

        public void Redo()
        {
            this.RunHistory(false);
        }

        private void RunHistory(bool isUndo)
        {
            var c = m_core;
            if (c.ITextHistory == null)
            {
                return;
            }
            var histories = isUndo ? c.ITextHistory.GetUndo() : c.ITextHistory.GetRedo();
            if (histories == null || histories.Length == 0)
            {
                return;
            }
            if (isUndo)
            {
                var temp = new TextHistoryRecord[histories.Length];
                for (int i = 0; i < histories.Length; i++)
                {
                    temp[i] = histories[i];
                    temp[i].NewText = histories[i].OldText;
                    temp[i].OldText = histories[i].NewText;
                }
                histories = temp;
            }
            histories = c.TextManager.RunHistory(histories);
            var last = histories[histories.Length - 1];
            c.Selection.SetSelection(last.Index, last.Index + last.NewText.Length);
            c.Caret.IndexOfChar = m_core.Selection.EndIndex;
            c.ITextView.SetCaretPostion(c.Caret.IndexOfChar);
            c.ITextView.ScrollToCaret();
            c.TextBox.OnTextChanged(null);
            this.Invalidate();
        }

        #endregion

        #region protected

        protected virtual void OnImeStart(IntPtr hIMC) { }
        protected virtual void OnImeResultStr(IntPtr hIMC, string strResult) { this.EnterText(strResult); }
        protected virtual void OnImeCompStr(IntPtr hIMC, string strComp) { }
        protected virtual void OnImeEnd(IntPtr hIMC) { }

        protected virtual void OnCalcScrollRectangle(STTextBoxScrollInfo scroll)
        {
            int nSW = this.GetIntXSize(scroll.Size);
            int nSH = this.GetIntYSize(scroll.Size);
            var c = m_core;
            Rectangle rectV = c.ViewRectangle;
            scroll.HBackRect = new Rectangle(rectV.X, rectV.Bottom - nSH, rectV.Width - nSW, nSH);
            scroll.VBackRect = new Rectangle(rectV.Right - nSW, rectV.Y, nSW, rectV.Height - nSH);
            Rectangle rect_h_thumb = scroll.HBackRect;
            Rectangle rect_v_thumb = scroll.VBackRect;

            float fScale = ((float)rectV.Width / scroll.XIncrement) / (rectV.Width / scroll.XIncrement + scroll.MaxXValue);
            rect_h_thumb.Width = (int)(rectV.Width * fScale);
            if (rect_h_thumb.Width > scroll.HBackRect.Width)
            {
                rect_h_thumb.Width = scroll.HBackRect.Width;
            }
            else if (rect_h_thumb.Width < this.GetIntXSize(4))
            {
                rect_h_thumb.Width = this.GetIntXSize(4);
            }

            fScale = ((float)rectV.Height / c.LineHeight) / (rectV.Height / c.LineHeight + scroll.MaxYValue);
            rect_v_thumb.Height = (int)(rectV.Height * fScale);
            if (rect_v_thumb.Height > scroll.VBackRect.Height)
            {
                rect_v_thumb.Height = scroll.VBackRect.Height;
            }
            else if (rect_v_thumb.Height < this.GetIntYSize(4))
            {
                rect_v_thumb.Height = this.GetIntYSize(4);
            }

            rect_h_thumb.X = rectV.X;
            if (scroll.MaxXValue > 0)
            {
                fScale = (float)scroll.XValue / scroll.MaxXValue;
                rect_h_thumb.X += (int)((scroll.HBackRect.Width - rect_h_thumb.Width) * fScale);
            }

            rect_v_thumb.Y = rectV.Y;
            if (scroll.MaxYValue > 0)
            {
                fScale = (float)scroll.YValue / scroll.MaxYValue;
                rect_v_thumb.Y += (int)((scroll.VBackRect.Height - rect_v_thumb.Height) * fScale);
            }
            scroll.HThumbRect = rect_h_thumb;
            scroll.VThumbRect = rect_v_thumb;
            if (scroll.VBackRect == rect_v_thumb)
            {
                rect_h_thumb.Width += nSW;
                scroll.HThumbRect = rect_h_thumb;
                var temp = scroll.HBackRect;
                temp.Width += nSW;
                scroll.HBackRect = temp;
            }
            else if (scroll.HBackRect == rect_h_thumb)
            {
                rect_v_thumb.Height += nSH;
                scroll.VThumbRect = rect_v_thumb;
                var temp = scroll.VBackRect;
                temp.Height += nSH;
                scroll.VBackRect = temp;
            }
            else
            {
            }
            //scroll.HThumbRect = rect_h_thumb;
            //scroll.VThumbRect = rect_v_thumb;
        }

        protected virtual void OnDrawVScrollBar(ISTTextBoxRender render, STTextBoxScrollInfo scroll)
        {
            var clr = scroll.HoverScrollBar == STTextBoxScrollInfo.ScrollBarType.V ? this._ScrollbarThumbHoverColor : this._ScrollbarBackColor;
            render.FillRectangle(clr, scroll.VBackRect);
            render.FillRectangle(clr, scroll.VBackRect.X, scroll.VBackRect.Y, this.GetIntXSize(1), scroll.VBackRect.Height);
            render.FillRectangle(this._ScrollbarThumbBackColor, scroll.VThumbRect);
        }

        protected virtual void OnDrawHScrollBar(ISTTextBoxRender render, STTextBoxScrollInfo scroll)
        {
            var clr = scroll.HoverScrollBar == STTextBoxScrollInfo.ScrollBarType.H ? this._ScrollbarThumbHoverColor : this._ScrollbarBackColor;
            render.FillRectangle(clr, scroll.HBackRect);
            render.FillRectangle(clr, scroll.HBackRect.X, scroll.HBackRect.Y, scroll.HBackRect.Width, this.GetIntYSize(1));
            render.FillRectangle(this._ScrollbarThumbBackColor, scroll.HThumbRect);
        }

        protected virtual void OnDrawScrollBarCorner(ISTTextBoxRender render, STTextBoxScrollInfo scroll)
        {
            int nWidth = this.GetIntXSize(scroll.Size);
            int nHeight = this.GetIntYSize(scroll.Size);
            render.FillRectangle(
                this._ScrollBarCornerColor, m_core.ViewRectangle.Right - nWidth,
                m_core.ViewRectangle.Bottom - nHeight,
                nWidth,
                nHeight);
        }

        #endregion

        #region private

        private bool SetCursor(Cursor c)
        {
            if (this.Cursor == c)
            {
                return false;
            }
            this.Cursor = c;
            return true;
        }

        private void OnImeStartPrivate(IntPtr hIMC)
        {
            var CandidateForm = new Win32.CANDIDATEFORM()
            {
                dwStyle = Win32.CFS_CANDIDATEPOS,
                ptCurrentPos = m_core.Caret.Location
            };
            Win32.ImmSetCandidateWindow(hIMC, ref CandidateForm);
            var CompositionForm = new Win32.COMPOSITIONFORM()
            {
                dwStyle = Win32.CFS_FORCE_POSITION,
                ptCurrentPos = m_core.Caret.Location
            };
            Win32.ImmSetCompositionWindow(hIMC, ref CompositionForm);
            var logFont = new Win32.LOGFONT()
            {
                lfHeight = m_core.FontHeight,
                lfFaceName = this.Font.Name + "\0"
            };
            Win32.ImmSetCompositionFont(hIMC, ref logFont);
            this.OnImeStart(hIMC);
        }

        private void OnImeEndPrivate(IntPtr hIMC)
        {
            Win32.ImmReleaseContext(this.Handle, hIMC);
            this.OnImeEnd(hIMC);
        }

        private void OnImeResultStrPrivate(IntPtr hIMC, string strResult)
        {
            var CompositionForm = new Win32.COMPOSITIONFORM()
            {
                dwStyle = Win32.CFS_FORCE_POSITION,
                ptCurrentPos = m_core.Caret.Location
            };
            Win32.ImmSetCompositionWindow(hIMC, ref CompositionForm);
            this.OnImeResultStr(hIMC, strResult);
        }

        private void m_timer_Tick(object sender, EventArgs e)
        {
            if (m_core.Scroll.HoverScrollBar != STTextBoxScrollInfo.ScrollBarType.None)
            {
                return;
            }
            lock (m_obj_sync)
            {
                m_core.Scroll.CountDown--;
            }
            if (m_core.Scroll.CountDown <= 0)
            {
                m_timer.Stop();
                this.Invalidate();
            }
        }

        #endregion
    }
}
