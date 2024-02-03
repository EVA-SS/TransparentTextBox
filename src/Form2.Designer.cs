namespace examples
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox1 = new TextBox();
            input1 = new AntDesign.TextBox();
            input2 = new AntDesign.TextBox();
            textBox2 = new TextBox();
            input3 = new AntDesign.TextBox();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(473, 86);
            textBox1.Margin = new Padding(3, 4, 3, 4);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(249, 28);
            textBox1.TabIndex = 1;
            textBox1.Text = "八百标兵奔北坡，炮兵并排北边跑。炮兵怕把标兵碰，标兵怕碰炮兵炮。八了百了标了兵了奔了北了坡，炮了兵了并了排了北了边了跑。炮了兵了怕了把了标了兵了碰，标了兵了怕了碰了炮了兵了炮";
            textBox1.UseSystemPasswordChar = true;
            // 
            // input1
            // 
            input1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            input1.BackColor = Color.FromArgb(80, 255, 255, 255);
            input1.Location = new Point(473, 136);
            input1.Margin = new Padding(4);
            input1.Name = "input1";
            input1.PasswordChar = '.';
            input1.Size = new Size(249, 42);
            input1.TabIndex = 2;
            input1.Text = "八百标兵奔北坡，炮兵并排北边跑。炮兵怕把标兵碰，标兵怕碰炮兵炮。八了百了标了兵了奔了北了坡，炮了兵了并了排了北了边了跑。炮了兵了怕了把了标了兵了碰，标了兵了怕了碰了炮了兵了炮";
            input1.UseSystemPasswordChar = true;
            // 
            // input2
            // 
            input2.AcceptsReturn = true;
            input2.AcceptsTab = true;
            input2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            input2.BackColor = Color.FromArgb(80, 255, 255, 255);
            input2.Location = new Point(65, 265);
            input2.Margin = new Padding(4);
            input2.Multiline = true;
            input2.Name = "input2";
            input2.Size = new Size(339, 91);
            input2.TabIndex = 0;
            input2.Text = "八百标兵奔北坡，炮兵并排北边跑。炮兵怕把标兵碰，标兵怕碰炮兵炮。八了百了标了兵了奔了北了坡，炮了兵了并了排了北了边了跑。炮了兵了怕了把了标了兵了碰，标了兵了怕了碰了炮了兵了炮";
            // 
            // textBox2
            // 
            textBox2.AcceptsReturn = true;
            textBox2.AcceptsTab = true;
            textBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox2.Location = new Point(490, 265);
            textBox2.Margin = new Padding(3, 4, 3, 4);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(339, 91);
            textBox2.TabIndex = 1;
            textBox2.Text = "八百标兵奔北坡，炮兵并排北边跑。炮兵怕把标兵碰，标兵怕碰炮兵炮。八了百了标了兵了奔了北了坡，炮了兵了并了排了北了边了跑。炮了兵了怕了把了标了兵了碰，标了兵了怕了碰了炮了兵了炮";
            // 
            // input3
            // 
            input3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            input3.BackColor = Color.FromArgb(80, 255, 255, 255);
            input3.Location = new Point(74, 86);
            input3.Margin = new Padding(4);
            input3.Name = "input3";
            input3.PlaceholderText = "请输入账号";
            input3.Size = new Size(249, 52);
            input3.TabIndex = 2;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(10F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(889, 472);
            Controls.Add(input2);
            Controls.Add(input3);
            Controls.Add(input1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form2";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form2";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private AntDesign.TextBox input1;
        private AntDesign.TextBox input2;
        private TextBox textBox2;
        private AntDesign.TextBox input3;
    }
}