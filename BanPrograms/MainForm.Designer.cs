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
            this.SuspendLayout();
            // 
            // lstPrograms
            // 
            this.lstPrograms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                new System.Windows.Forms.ColumnHeader() { Text = "Program", Width = 450 }
            });
            this.lstPrograms.Location = new System.Drawing.Point(20, 80);
            this.lstPrograms.Name = "lstPrograms";
            this.lstPrograms.Size = new System.Drawing.Size(460, 220);
            this.lstPrograms.TabIndex = 0;
            this.lstPrograms.View = System.Windows.Forms.View.Details;
            this.lstPrograms.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(200, 320);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(100, 40);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            this.btnAdd.Anchor = AnchorStyles.Bottom;
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(20, 320);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(100, 40);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "Remove";
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            this.btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            // 
            // btnToggle
            // 
            this.btnToggle.Location = new System.Drawing.Point(450, 320);
            this.btnToggle.Name = "btnToggle";
            this.btnToggle.Size = new System.Drawing.Size(40, 40);
            this.btnToggle.TabIndex = 3;
            this.btnToggle.CheckedChanged += new System.EventHandler(this.btnToggle_CheckedChanged);
            this.btnToggle.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            // 
            // lblToggle
            // 
            this.lblToggle.Location = new System.Drawing.Point(320, 320);
            this.lblToggle.Name = "lblToggle";
            this.lblToggle.Size = new System.Drawing.Size(120, 40);
            this.lblToggle.Text = "Block System: ON/OFF";
            this.lblToggle.TabIndex = 4;
            this.lblToggle.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 400);
            this.Controls.Add(this.lblToggle);
            this.Controls.Add(this.btnToggle);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstPrograms);
            this.Name = "MainForm";
            this.Text = "ProcessLock";
            this.ResumeLayout(false);
        }

        private MaterialSkin.Controls.MaterialListView lstPrograms;
        private MaterialSkin.Controls.MaterialButton btnAdd;
        private MaterialSkin.Controls.MaterialButton btnRemove;
        private MaterialSkin.Controls.MaterialSwitch btnToggle;
        private MaterialSkin.Controls.MaterialLabel lblToggle;
    }
}