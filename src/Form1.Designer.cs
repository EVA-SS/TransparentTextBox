namespace examples
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            input1 = new AntDesign.Input();
            SuspendLayout();
            // 
            // input1
            // 
            input1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            input1.BackColor = Color.FromArgb(20, 0, 0, 0);
            input1.Font = new Font("Microsoft YaHei UI", 24F, FontStyle.Regular, GraphicsUnit.Point);
            input1.Location = new Point(101, 103);
            input1.Name = "input1";
            input1.Size = new Size(508, 68);
            input1.TabIndex = 4;
            input1.Text = "常规文本框🙂";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(708, 310);
            Controls.Add(input1);
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.DimGray;
            Margin = new Padding(4);
            Name = "Form1";
            Text = "透明文本框";
            ResumeLayout(false);
        }

        #endregion

        private AntDesign.Input input1;
    }
}