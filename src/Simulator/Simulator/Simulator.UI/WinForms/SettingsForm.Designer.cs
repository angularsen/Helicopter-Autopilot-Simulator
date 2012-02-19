using HelicopterSim;

namespace Simulator.UI.WinForms
{
    partial class SettingsForm
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
            this.PIDSettings = new Simulator.UI.WinForms.PIDSettings();
            this.SimSettings = new HelicopterSim.SimulatorSettings();
            this.SuspendLayout();
            // 
            // PIDSettings
            // 
            this.PIDSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PIDSettings.Location = new System.Drawing.Point(207, 0);
            this.PIDSettings.Name = "PIDSettings";
            this.PIDSettings.Size = new System.Drawing.Size(191, 258);
            this.PIDSettings.TabIndex = 1;
            // 
            // SimSettings
            // 
            this.SimSettings.AutoSize = true;
            this.SimSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SimSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SimSettings.Location = new System.Drawing.Point(0, 0);
            this.SimSettings.Name = "SimSettings";
            this.SimSettings.Size = new System.Drawing.Size(201, 122);
            this.SimSettings.TabIndex = 0;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 300);
            this.Controls.Add(this.PIDSettings);
            this.Controls.Add(this.SimSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "SettingsForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public SimulatorSettings SimSettings;
        public PIDSettings PIDSettings;

    }
}