namespace Simulator.UI.WinForms
{
    partial class PIDSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SliderP = new System.Windows.Forms.TrackBar();
            this.SliderMaxP = new System.Windows.Forms.TextBox();
            this.SliderMinP = new System.Windows.Forms.TextBox();
            this.UserValueP = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.SliderMaxI = new System.Windows.Forms.TextBox();
            this.UserValueI = new System.Windows.Forms.TextBox();
            this.SliderI = new System.Windows.Forms.TrackBar();
            this.SliderMinI = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.SliderMaxD = new System.Windows.Forms.TextBox();
            this.UserValueD = new System.Windows.Forms.TextBox();
            this.SliderD = new System.Windows.Forms.TrackBar();
            this.SliderMinD = new System.Windows.Forms.TextBox();
            this.PIDSetup = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SliderActiveP = new System.Windows.Forms.CheckBox();
            this.SliderActiveI = new System.Windows.Forms.CheckBox();
            this.SliderActiveD = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.SliderP)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SliderI)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SliderD)).BeginInit();
            this.SuspendLayout();
            // 
            // SliderP
            // 
            this.SliderP.Location = new System.Drawing.Point(6, 45);
            this.SliderP.Name = "SliderP";
            this.SliderP.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.SliderP.Size = new System.Drawing.Size(45, 109);
            this.SliderP.TabIndex = 0;
            // 
            // SliderMaxP
            // 
            this.SliderMaxP.Location = new System.Drawing.Point(6, 19);
            this.SliderMaxP.Name = "SliderMaxP";
            this.SliderMaxP.Size = new System.Drawing.Size(35, 20);
            this.SliderMaxP.TabIndex = 0;
            // 
            // SliderMinP
            // 
            this.SliderMinP.Location = new System.Drawing.Point(6, 160);
            this.SliderMinP.Name = "SliderMinP";
            this.SliderMinP.Size = new System.Drawing.Size(35, 20);
            this.SliderMinP.TabIndex = 1;
            // 
            // UserValueP
            // 
            this.UserValueP.Location = new System.Drawing.Point(6, 186);
            this.UserValueP.Name = "UserValueP";
            this.UserValueP.ReadOnly = true;
            this.UserValueP.Size = new System.Drawing.Size(35, 20);
            this.UserValueP.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SliderActiveP);
            this.groupBox1.Controls.Add(this.SliderMaxP);
            this.groupBox1.Controls.Add(this.UserValueP);
            this.groupBox1.Controls.Add(this.SliderP);
            this.groupBox1.Controls.Add(this.SliderMinP);
            this.groupBox1.Location = new System.Drawing.Point(3, 43);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(55, 213);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "P";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.SliderActiveI);
            this.groupBox2.Controls.Add(this.SliderMaxI);
            this.groupBox2.Controls.Add(this.UserValueI);
            this.groupBox2.Controls.Add(this.SliderI);
            this.groupBox2.Controls.Add(this.SliderMinI);
            this.groupBox2.Location = new System.Drawing.Point(64, 43);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(55, 213);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "I";
            // 
            // SliderMaxI
            // 
            this.SliderMaxI.Location = new System.Drawing.Point(6, 19);
            this.SliderMaxI.Name = "SliderMaxI";
            this.SliderMaxI.Size = new System.Drawing.Size(35, 20);
            this.SliderMaxI.TabIndex = 0;
            // 
            // UserValueI
            // 
            this.UserValueI.Location = new System.Drawing.Point(6, 186);
            this.UserValueI.Name = "UserValueI";
            this.UserValueI.ReadOnly = true;
            this.UserValueI.Size = new System.Drawing.Size(35, 20);
            this.UserValueI.TabIndex = 1;
            // 
            // SliderI
            // 
            this.SliderI.Location = new System.Drawing.Point(6, 45);
            this.SliderI.Name = "SliderI";
            this.SliderI.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.SliderI.Size = new System.Drawing.Size(45, 109);
            this.SliderI.TabIndex = 0;
            // 
            // SliderMinI
            // 
            this.SliderMinI.Location = new System.Drawing.Point(6, 160);
            this.SliderMinI.Name = "SliderMinI";
            this.SliderMinI.Size = new System.Drawing.Size(35, 20);
            this.SliderMinI.TabIndex = 1;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.SliderActiveD);
            this.groupBox3.Controls.Add(this.SliderMaxD);
            this.groupBox3.Controls.Add(this.UserValueD);
            this.groupBox3.Controls.Add(this.SliderD);
            this.groupBox3.Controls.Add(this.SliderMinD);
            this.groupBox3.Location = new System.Drawing.Point(125, 43);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(55, 213);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "D";
            // 
            // SliderMaxD
            // 
            this.SliderMaxD.Location = new System.Drawing.Point(6, 19);
            this.SliderMaxD.Name = "SliderMaxD";
            this.SliderMaxD.Size = new System.Drawing.Size(35, 20);
            this.SliderMaxD.TabIndex = 0;
            // 
            // UserValueD
            // 
            this.UserValueD.Location = new System.Drawing.Point(6, 186);
            this.UserValueD.Name = "UserValueD";
            this.UserValueD.ReadOnly = true;
            this.UserValueD.Size = new System.Drawing.Size(35, 20);
            this.UserValueD.TabIndex = 1;
            // 
            // SliderD
            // 
            this.SliderD.Location = new System.Drawing.Point(6, 45);
            this.SliderD.Name = "SliderD";
            this.SliderD.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.SliderD.Size = new System.Drawing.Size(45, 109);
            this.SliderD.TabIndex = 0;
            // 
            // SliderMinD
            // 
            this.SliderMinD.Location = new System.Drawing.Point(6, 160);
            this.SliderMinD.Name = "SliderMinD";
            this.SliderMinD.Size = new System.Drawing.Size(35, 20);
            this.SliderMinD.TabIndex = 1;
            // 
            // PIDSetup
            // 
            this.PIDSetup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PIDSetup.FormattingEnabled = true;
            this.PIDSetup.Location = new System.Drawing.Point(3, 16);
            this.PIDSetup.Name = "PIDSetup";
            this.PIDSetup.Size = new System.Drawing.Size(179, 21);
            this.PIDSetup.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "PID Configuration";
            // 
            // SliderActiveP
            // 
            this.SliderActiveP.AutoSize = true;
            this.SliderActiveP.Checked = true;
            this.SliderActiveP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SliderActiveP.Location = new System.Drawing.Point(40, 189);
            this.SliderActiveP.Name = "SliderActiveP";
            this.SliderActiveP.Size = new System.Drawing.Size(15, 14);
            this.SliderActiveP.TabIndex = 2;
            this.SliderActiveP.UseVisualStyleBackColor = true;
            // 
            // SliderActiveI
            // 
            this.SliderActiveI.AutoSize = true;
            this.SliderActiveI.Checked = true;
            this.SliderActiveI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SliderActiveI.Location = new System.Drawing.Point(40, 189);
            this.SliderActiveI.Name = "SliderActiveI";
            this.SliderActiveI.Size = new System.Drawing.Size(15, 14);
            this.SliderActiveI.TabIndex = 2;
            this.SliderActiveI.UseVisualStyleBackColor = true;
            // 
            // SliderActiveD
            // 
            this.SliderActiveD.AutoSize = true;
            this.SliderActiveD.Checked = true;
            this.SliderActiveD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SliderActiveD.Location = new System.Drawing.Point(40, 189);
            this.SliderActiveD.Name = "SliderActiveD";
            this.SliderActiveD.Size = new System.Drawing.Size(15, 14);
            this.SliderActiveD.TabIndex = 2;
            this.SliderActiveD.UseVisualStyleBackColor = true;
            // 
            // PIDSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PIDSetup);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "PIDSettings";
            this.Size = new System.Drawing.Size(185, 266);
            ((System.ComponentModel.ISupportInitialize)(this.SliderP)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SliderI)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SliderD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar SliderP;
        private System.Windows.Forms.TextBox SliderMaxP;
        private System.Windows.Forms.TextBox SliderMinP;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox SliderMaxI;
        private System.Windows.Forms.TrackBar SliderI;
        private System.Windows.Forms.TextBox SliderMinI;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox SliderMaxD;
        private System.Windows.Forms.TrackBar SliderD;
        private System.Windows.Forms.TextBox SliderMinD;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox UserValueP;
        public System.Windows.Forms.TextBox UserValueI;
        public System.Windows.Forms.TextBox UserValueD;
        public System.Windows.Forms.ComboBox PIDSetup;
        private System.Windows.Forms.CheckBox SliderActiveP;
        private System.Windows.Forms.CheckBox SliderActiveI;
        private System.Windows.Forms.CheckBox SliderActiveD;
    }
}


