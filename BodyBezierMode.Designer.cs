using System;

namespace PE多功能信息处理插件
{
    partial class BodyBezierMode
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
            try
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }
            catch (Exception)
            {

            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.ShowBezier = new System.Windows.Forms.PictureBox();
            this.metroGrid2 = new MetroFramework.Controls.MetroGrid();
            this.metroGrid1 = new MetroFramework.Controls.MetroGrid();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.metroToggle1 = new MetroFramework.Controls.MetroToggle();
            this.metroToggle2 = new MetroFramework.Controls.MetroToggle();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel4 = new MetroFramework.Controls.MetroLabel();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.metroButton2 = new MetroFramework.Controls.MetroButton();
            this.metroStyleManager1 = new MetroFramework.Components.MetroStyleManager(this.components);
            this.metroStyleExtender1 = new MetroFramework.Components.MetroStyleExtender(this.components);
            this.XLastNummer = new MetroFramework.Controls.MetroTextBox();
            this.XFirstNummer = new MetroFramework.Controls.MetroTextBox();
            this.FirstColor = new MetroFramework.Controls.MetroTrackBar();
            this.SecondColor = new MetroFramework.Controls.MetroTrackBar();
            this.LinkSize = new MetroFramework.Controls.MetroTrackBar();
            this.Reset = new MetroFramework.Controls.MetroButton();
            ((System.ComponentModel.ISupportInitialize)(this.ShowBezier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.metroGrid2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.metroGrid1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // ShowBezier
            // 
            this.ShowBezier.Location = new System.Drawing.Point(135, 87);
            this.ShowBezier.Name = "ShowBezier";
            this.ShowBezier.Size = new System.Drawing.Size(255, 250);
            this.ShowBezier.TabIndex = 1;
            this.ShowBezier.TabStop = false;
            this.ShowBezier.Paint += new System.Windows.Forms.PaintEventHandler(this.ShowBezier_Paint);
            this.ShowBezier.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ShowBezier_MouseDown);
            this.ShowBezier.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ShowBezier_MouseMove);
            this.ShowBezier.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ShowBezier_MouseUp);
            // 
            // metroGrid2
            // 
            this.metroGrid2.AllowUserToAddRows = false;
            this.metroGrid2.AllowUserToDeleteRows = false;
            this.metroGrid2.AllowUserToResizeColumns = false;
            this.metroGrid2.AllowUserToResizeRows = false;
            this.metroGrid2.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.metroGrid2.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.metroGrid2.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.metroGrid2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.metroGrid2.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.metroGrid2.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.metroGrid2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.metroGrid2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.metroGrid2.DefaultCellStyle = dataGridViewCellStyle2;
            this.metroGrid2.EnableHeadersVisualStyles = false;
            this.metroGrid2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.metroGrid2.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.metroGrid2.Location = new System.Drawing.Point(396, 87);
            this.metroGrid2.Name = "metroGrid2";
            this.metroGrid2.ReadOnly = true;
            this.metroGrid2.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.metroGrid2.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.metroGrid2.RowHeadersVisible = false;
            this.metroGrid2.RowHeadersWidth = 20;
            this.metroGrid2.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.metroGrid2.RowTemplate.Height = 23;
            this.metroGrid2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.metroGrid2.Size = new System.Drawing.Size(157, 250);
            this.metroGrid2.TabIndex = 7;
            this.metroGrid2.UseStyleColors = true;
            // 
            // metroGrid1
            // 
            this.metroGrid1.AllowUserToResizeRows = false;
            this.metroGrid1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.metroGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.metroGrid1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.metroGrid1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.metroGrid1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.metroGrid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.metroGrid1.DefaultCellStyle = dataGridViewCellStyle5;
            this.metroGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroGrid1.EnableHeadersVisualStyles = false;
            this.metroGrid1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.metroGrid1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.metroGrid1.Location = new System.Drawing.Point(20, 60);
            this.metroGrid1.Name = "metroGrid1";
            this.metroGrid1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.metroGrid1.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.metroGrid1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.metroGrid1.RowTemplate.Height = 23;
            this.metroGrid1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.metroGrid1.Size = new System.Drawing.Size(520, 300);
            this.metroGrid1.TabIndex = 8;
            this.metroGrid1.UseStyleColors = true;
            this.metroGrid1.Paint += new System.Windows.Forms.PaintEventHandler(this.metroGrid1_Paint);
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(10, 49);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(54, 19);
            this.metroLabel1.TabIndex = 9;
            this.metroLabel1.Text = "最终值:";
            this.metroLabel1.UseStyleColors = true;
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(10, 291);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(54, 19);
            this.metroLabel2.TabIndex = 10;
            this.metroLabel2.Text = "初始值:";
            this.metroLabel2.UseStyleColors = true;
            // 
            // metroToggle1
            // 
            this.metroToggle1.AutoSize = true;
            this.metroToggle1.Checked = true;
            this.metroToggle1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.metroToggle1.Location = new System.Drawing.Point(461, 38);
            this.metroToggle1.Name = "metroToggle1";
            this.metroToggle1.Size = new System.Drawing.Size(80, 16);
            this.metroToggle1.TabIndex = 11;
            this.metroToggle1.Text = "On";
            this.metroToggle1.UseMnemonic = false;
            this.metroToggle1.UseSelectable = true;
            this.metroToggle1.UseStyleColors = true;
            this.metroToggle1.CheckedChanged += new System.EventHandler(this.metroToggle1_CheckedChanged);
            // 
            // metroToggle2
            // 
            this.metroToggle2.AutoSize = true;
            this.metroToggle2.Location = new System.Drawing.Point(461, 60);
            this.metroToggle2.Name = "metroToggle2";
            this.metroToggle2.Size = new System.Drawing.Size(80, 16);
            this.metroToggle2.TabIndex = 12;
            this.metroToggle2.Text = "Off";
            this.metroToggle2.UseSelectable = true;
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(396, 59);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(65, 19);
            this.metroLabel3.TabIndex = 14;
            this.metroLabel3.Text = "实时更新";
            this.metroLabel3.UseMnemonic = false;
            this.metroLabel3.UseStyleColors = true;
            // 
            // metroLabel4
            // 
            this.metroLabel4.AutoSize = true;
            this.metroLabel4.Location = new System.Drawing.Point(396, 37);
            this.metroLabel4.Name = "metroLabel4";
            this.metroLabel4.Size = new System.Drawing.Size(65, 19);
            this.metroLabel4.TabIndex = 15;
            this.metroLabel4.Text = "窗口前置";
            this.metroLabel4.UseMnemonic = false;
            this.metroLabel4.UseStyleColors = true;
            // 
            // metroButton1
            // 
            this.metroButton1.Location = new System.Drawing.Point(396, 343);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(84, 23);
            this.metroButton1.TabIndex = 16;
            this.metroButton1.Text = "保存并应用";
            this.metroButton1.UseSelectable = true;
            this.metroButton1.UseStyleColors = true;
            this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // metroButton2
            // 
            this.metroButton2.Location = new System.Drawing.Point(486, 343);
            this.metroButton2.Name = "metroButton2";
            this.metroButton2.Size = new System.Drawing.Size(67, 23);
            this.metroButton2.TabIndex = 17;
            this.metroButton2.Text = "应用";
            this.metroButton2.UseSelectable = true;
            this.metroButton2.UseStyleColors = true;
            this.metroButton2.Click += new System.EventHandler(this.metroButton2_Click);
            // 
            // metroStyleManager1
            // 
            this.metroStyleManager1.Owner = this;
            // 
            // XLastNummer
            // 
            // 
            // 
            // 
            this.XLastNummer.CustomButton.Image = null;
            this.XLastNummer.CustomButton.Location = new System.Drawing.Point(53, 1);
            this.XLastNummer.CustomButton.Name = "";
            this.XLastNummer.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.XLastNummer.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.XLastNummer.CustomButton.TabIndex = 1;
            this.XLastNummer.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.XLastNummer.CustomButton.UseSelectable = true;
            this.XLastNummer.Lines = new string[0];
            this.XLastNummer.Location = new System.Drawing.Point(10, 71);
            this.XLastNummer.MaxLength = 32767;
            this.XLastNummer.Name = "XLastNummer";
            this.XLastNummer.PasswordChar = '\0';
            this.XLastNummer.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.XLastNummer.SelectedText = "";
            this.XLastNummer.SelectionLength = 0;
            this.XLastNummer.SelectionStart = 0;
            this.XLastNummer.ShortcutsEnabled = true;
            this.XLastNummer.ShowButton = true;
            this.XLastNummer.ShowClearButton = true;
            this.XLastNummer.Size = new System.Drawing.Size(75, 23);
            this.XLastNummer.TabIndex = 18;
            this.XLastNummer.UseSelectable = true;
            this.XLastNummer.UseStyleColors = true;
            this.XLastNummer.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.XLastNummer.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.XLastNummer.ButtonClick += new MetroFramework.Controls.MetroTextBox.ButClick(this.ChangeNummer);
            this.XLastNummer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MoreThanZeroNumCheck);
            // 
            // XFirstNummer
            // 
            // 
            // 
            // 
            this.XFirstNummer.CustomButton.Image = null;
            this.XFirstNummer.CustomButton.Location = new System.Drawing.Point(53, 1);
            this.XFirstNummer.CustomButton.Name = "";
            this.XFirstNummer.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.XFirstNummer.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.XFirstNummer.CustomButton.TabIndex = 1;
            this.XFirstNummer.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.XFirstNummer.CustomButton.UseSelectable = true;
            this.XFirstNummer.Lines = new string[0];
            this.XFirstNummer.Location = new System.Drawing.Point(10, 313);
            this.XFirstNummer.MaxLength = 32767;
            this.XFirstNummer.Name = "XFirstNummer";
            this.XFirstNummer.PasswordChar = '\0';
            this.XFirstNummer.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.XFirstNummer.SelectedText = "";
            this.XFirstNummer.SelectionLength = 0;
            this.XFirstNummer.SelectionStart = 0;
            this.XFirstNummer.ShortcutsEnabled = true;
            this.XFirstNummer.ShowButton = true;
            this.XFirstNummer.ShowClearButton = true;
            this.XFirstNummer.Size = new System.Drawing.Size(75, 23);
            this.XFirstNummer.TabIndex = 19;
            this.XFirstNummer.UseSelectable = true;
            this.XFirstNummer.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.XFirstNummer.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.XFirstNummer.ButtonClick += new MetroFramework.Controls.MetroTextBox.ButClick(this.ChangeNummer);
            this.XFirstNummer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MoreThanZeroNumCheck);
            // 
            // FirstColor
            // 
            this.FirstColor.BackColor = System.Drawing.Color.Transparent;
            this.FirstColor.Location = new System.Drawing.Point(10, 200);
            this.FirstColor.Maximum = 140;
            this.FirstColor.Minimum = 1;
            this.FirstColor.MouseWheelBarPartitions = 100;
            this.FirstColor.Name = "FirstColor";
            this.FirstColor.Size = new System.Drawing.Size(75, 23);
            this.FirstColor.TabIndex = 22;
            this.FirstColor.Text = "FirstColor";
            this.FirstColor.Value = 1;
            this.FirstColor.ValueChanged += new System.EventHandler(this.ValueChange);
            // 
            // SecondColor
            // 
            this.SecondColor.BackColor = System.Drawing.Color.Transparent;
            this.SecondColor.Location = new System.Drawing.Point(10, 232);
            this.SecondColor.Maximum = 140;
            this.SecondColor.Minimum = 1;
            this.SecondColor.MouseWheelBarPartitions = 100;
            this.SecondColor.Name = "SecondColor";
            this.SecondColor.Size = new System.Drawing.Size(75, 23);
            this.SecondColor.TabIndex = 23;
            this.SecondColor.Text = "metroTrackBar1";
            this.SecondColor.Value = 1;
            this.SecondColor.ValueChanged += new System.EventHandler(this.ValueChange);
            // 
            // LinkSize
            // 
            this.LinkSize.BackColor = System.Drawing.Color.Transparent;
            this.LinkSize.Location = new System.Drawing.Point(10, 264);
            this.LinkSize.Maximum = 120;
            this.LinkSize.Minimum = 20;
            this.LinkSize.Name = "LinkSize";
            this.LinkSize.Size = new System.Drawing.Size(75, 23);
            this.LinkSize.TabIndex = 24;
            this.LinkSize.Text = "metroTrackBar1";
            this.LinkSize.Value = 20;
            this.LinkSize.ValueChanged += new System.EventHandler(this.ValueChange);
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(224, 57);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(67, 23);
            this.Reset.TabIndex = 25;
            this.Reset.Text = "重置";
            this.Reset.UseSelectable = true;
            this.Reset.UseStyleColors = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // BodyBezierMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 380);
            this.Controls.Add(this.Reset);
            this.Controls.Add(this.LinkSize);
            this.Controls.Add(this.SecondColor);
            this.Controls.Add(this.FirstColor);
            this.Controls.Add(this.XFirstNummer);
            this.Controls.Add(this.XLastNummer);
            this.Controls.Add(this.metroButton2);
            this.Controls.Add(this.metroButton1);
            this.Controls.Add(this.metroLabel4);
            this.Controls.Add(this.metroToggle2);
            this.Controls.Add(this.metroToggle1);
            this.Controls.Add(this.metroLabel3);
            this.Controls.Add(this.metroLabel2);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.metroGrid2);
            this.Controls.Add(this.ShowBezier);
            this.Controls.Add(this.metroGrid1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(560, 380);
            this.MinimumSize = new System.Drawing.Size(560, 380);
            this.Name = "BodyBezierMode";
            this.Resizable = false;
            this.ShadowType = MetroFramework.Forms.MetroFormShadowType.AeroShadow;
            this.ShowIcon = false;
            this.Style = MetroFramework.MetroColorStyle.Default;
            this.Text = "Form2";
            this.Theme = MetroFramework.MetroThemeStyle.Default;
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form2_FormClosed);
            this.Shown += new System.EventHandler(this.Form2_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.ShowBezier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.metroGrid2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.metroGrid1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox ShowBezier;
        private MetroFramework.Controls.MetroGrid metroGrid2;
        private MetroFramework.Controls.MetroGrid metroGrid1;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroToggle metroToggle1;
        private MetroFramework.Controls.MetroToggle metroToggle2;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroLabel metroLabel4;
        private MetroFramework.Controls.MetroButton metroButton1;
        private MetroFramework.Controls.MetroButton metroButton2;
        private MetroFramework.Components.MetroStyleManager metroStyleManager1;
        private MetroFramework.Components.MetroStyleExtender metroStyleExtender1;
        private MetroFramework.Controls.MetroTextBox XFirstNummer;
        private MetroFramework.Controls.MetroTextBox XLastNummer;
        private MetroFramework.Controls.MetroTrackBar FirstColor;
        private MetroFramework.Controls.MetroTrackBar SecondColor;
        private MetroFramework.Controls.MetroTrackBar LinkSize;
        private MetroFramework.Controls.MetroButton Reset;
    }
}