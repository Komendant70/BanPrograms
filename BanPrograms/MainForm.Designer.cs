using System.Windows.Forms;

namespace BanPrograms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lstPrograms = new MaterialSkin.Controls.MaterialListView();
            this.btnAdd = new MaterialSkin.Controls.MaterialButton();
            this.btnRemove = new MaterialSkin.Controls.MaterialButton();
            this.btnToggle = new MaterialSkin.Controls.MaterialSwitch();
            this.lblToggle = new MaterialSkin.Controls.MaterialLabel();
            this.btnEnableStartup = new MaterialSkin.Controls.MaterialSwitch();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.SuspendLayout();
            // 
            // lstPrograms
            // 
            this.lstPrograms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstPrograms.AutoSizeTable = false;
            this.lstPrograms.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lstPrograms.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstPrograms.Depth = 0;
            this.lstPrograms.FullRowSelect = true;
            this.lstPrograms.HideSelection = false;
            this.lstPrograms.Location = new System.Drawing.Point(40, 154);
            this.lstPrograms.Margin = new System.Windows.Forms.Padding(6);
            this.lstPrograms.MinimumSize = new System.Drawing.Size(400, 192);
            this.lstPrograms.MouseLocation = new System.Drawing.Point(-1, -1);
            this.lstPrograms.MouseState = MaterialSkin.MouseState.OUT;
            this.lstPrograms.Name = "lstPrograms";
            this.lstPrograms.OwnerDraw = true;
            this.lstPrograms.Size = new System.Drawing.Size(994, 434);
            this.lstPrograms.TabIndex = 0;
            this.lstPrograms.UseCompatibleStateImageBehavior = false;
            this.lstPrograms.View = System.Windows.Forms.View.Details;
            this.lstPrograms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                new System.Windows.Forms.ColumnHeader() { Text = "Program", Width = 450 }
            });

            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAdd.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAdd.Depth = 0;
            this.btnAdd.HighEmphasis = true;
            this.btnAdd.Icon = null;
            this.btnAdd.Location = new System.Drawing.Point(437, 667);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.btnAdd.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAdd.Size = new System.Drawing.Size(64, 36);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAdd.UseAccentColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRemove.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRemove.Depth = 0;
            this.btnRemove.HighEmphasis = true;
            this.btnRemove.Icon = null;
            this.btnRemove.Location = new System.Drawing.Point(40, 667);
            this.btnRemove.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.btnRemove.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRemove.Size = new System.Drawing.Size(80, 36);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "Remove";
            this.btnRemove.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRemove.UseAccentColor = false;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnToggle
            // 
            this.btnToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggle.Depth = 0;
            this.btnToggle.Location = new System.Drawing.Point(974, 626);
            this.btnToggle.Margin = new System.Windows.Forms.Padding(0);
            this.btnToggle.MouseLocation = new System.Drawing.Point(-1, -1);
            this.btnToggle.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnToggle.Name = "btnToggle";
            this.btnToggle.Ripple = true;
            this.btnToggle.Size = new System.Drawing.Size(80, 77);
            this.btnToggle.TabIndex = 3;
            this.btnToggle.CheckedChanged += new System.EventHandler(this.btnToggle_CheckedChanged);
            // 
            // lblToggle
            // 
            this.lblToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblToggle.Depth = 0;
            this.lblToggle.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblToggle.Location = new System.Drawing.Point(769, 658);
            this.lblToggle.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblToggle.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblToggle.Name = "lblToggle";
            this.lblToggle.Size = new System.Drawing.Size(180, 22);
            this.lblToggle.TabIndex = 4;
            this.lblToggle.Text = "Block System: ON/OFF";
            // 
            // btnEnableStartup
            // 
            this.btnEnableStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEnableStartup.Depth = 0;
            this.btnEnableStartup.Location = new System.Drawing.Point(974, 689);
            this.btnEnableStartup.Margin = new System.Windows.Forms.Padding(0);
            this.btnEnableStartup.MouseLocation = new System.Drawing.Point(-1, -1);
            this.btnEnableStartup.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnEnableStartup.Name = "btnEnableStartup";
            this.btnEnableStartup.Ripple = true;
            this.btnEnableStartup.Size = new System.Drawing.Size(80, 77);
            this.btnEnableStartup.TabIndex = 5;
            this.btnEnableStartup.Click += new System.EventHandler(this.btnEnableStartup_Click);
            // 
            // materialLabel1
            // 
            this.materialLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.Location = new System.Drawing.Point(769, 721);
            this.materialLabel1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(170, 18);
            this.materialLabel1.TabIndex = 6;
            this.materialLabel1.Text = "Block System: ON/OFF";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1074, 780);
            this.Controls.Add(this.materialLabel1);
            this.Controls.Add(this.btnEnableStartup);
            this.Controls.Add(this.lblToggle);
            this.Controls.Add(this.btnToggle);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstPrograms);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(6, 123, 6, 6);
            this.Text = "ProcessLock";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private MaterialSkin.Controls.MaterialListView lstPrograms;
        private MaterialSkin.Controls.MaterialButton btnAdd;
        private MaterialSkin.Controls.MaterialButton btnRemove;
        private MaterialSkin.Controls.MaterialSwitch btnToggle;
        private MaterialSkin.Controls.MaterialLabel lblToggle;
        private MaterialSkin.Controls.MaterialSwitch btnEnableStartup;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
    }
}