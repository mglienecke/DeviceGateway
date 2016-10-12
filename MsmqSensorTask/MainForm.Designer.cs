namespace MsmqSensorTask
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dataGridViewValues = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxJumpToCurrentOutput = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonStopReceive = new System.Windows.Forms.Button();
            this.buttonStartReceive = new System.Windows.Forms.Button();
            this.sensorIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sensorValueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sensorDescriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.retrievedSensorValueBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewValues)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.retrievedSensorValueBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutput.Location = new System.Drawing.Point(12, 99);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxOutput.Size = new System.Drawing.Size(518, 171);
            this.textBoxOutput.TabIndex = 3;
            this.textBoxOutput.WordWrap = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(204, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Communication with the Device Gateway:";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(7, 21);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Enabled = false;
            this.buttonStop.Location = new System.Drawing.Point(88, 21);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 296);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(270, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Values received from the Device Gateway by actuators:";
            // 
            // dataGridViewValues
            // 
            this.dataGridViewValues.AllowUserToAddRows = false;
            this.dataGridViewValues.AllowUserToDeleteRows = false;
            this.dataGridViewValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewValues.AutoGenerateColumns = false;
            this.dataGridViewValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewValues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.sensorIdDataGridViewTextBoxColumn,
            this.sensorValueDataGridViewTextBoxColumn,
            this.sensorDescriptionDataGridViewTextBoxColumn});
            this.dataGridViewValues.DataSource = this.retrievedSensorValueBindingSource;
            this.dataGridViewValues.Location = new System.Drawing.Point(12, 312);
            this.dataGridViewValues.Name = "dataGridViewValues";
            this.dataGridViewValues.ReadOnly = true;
            this.dataGridViewValues.RowHeadersVisible = false;
            this.dataGridViewValues.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewValues.Size = new System.Drawing.Size(522, 240);
            this.dataGridViewValues.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonStop);
            this.groupBox1.Controls.Add(this.buttonStart);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(176, 57);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sending and reading values";
            // 
            // checkBoxJumpToCurrentOutput
            // 
            this.checkBoxJumpToCurrentOutput.AutoSize = true;
            this.checkBoxJumpToCurrentOutput.Checked = true;
            this.checkBoxJumpToCurrentOutput.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxJumpToCurrentOutput.Location = new System.Drawing.Point(12, 276);
            this.checkBoxJumpToCurrentOutput.Name = "checkBoxJumpToCurrentOutput";
            this.checkBoxJumpToCurrentOutput.Size = new System.Drawing.Size(171, 17);
            this.checkBoxJumpToCurrentOutput.TabIndex = 7;
            this.checkBoxJumpToCurrentOutput.Text = "Autoscroll to the current output";
            this.checkBoxJumpToCurrentOutput.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonStopReceive);
            this.groupBox2.Controls.Add(this.buttonStartReceive);
            this.groupBox2.Location = new System.Drawing.Point(194, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(176, 57);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Reading values";
            // 
            // buttonStopReceive
            // 
            this.buttonStopReceive.Enabled = false;
            this.buttonStopReceive.Location = new System.Drawing.Point(88, 21);
            this.buttonStopReceive.Name = "buttonStopReceive";
            this.buttonStopReceive.Size = new System.Drawing.Size(75, 23);
            this.buttonStopReceive.TabIndex = 1;
            this.buttonStopReceive.Text = "Stop";
            this.buttonStopReceive.UseVisualStyleBackColor = true;
            this.buttonStopReceive.Click += new System.EventHandler(this.buttonStopReceive_Click);
            // 
            // buttonStartReceive
            // 
            this.buttonStartReceive.Location = new System.Drawing.Point(7, 21);
            this.buttonStartReceive.Name = "buttonStartReceive";
            this.buttonStartReceive.Size = new System.Drawing.Size(75, 23);
            this.buttonStartReceive.TabIndex = 0;
            this.buttonStartReceive.Text = "Start";
            this.buttonStartReceive.UseVisualStyleBackColor = true;
            this.buttonStartReceive.Click += new System.EventHandler(this.buttonStartReceive_Click);
            // 
            // sensorIdDataGridViewTextBoxColumn
            // 
            this.sensorIdDataGridViewTextBoxColumn.DataPropertyName = "SensorId";
            this.sensorIdDataGridViewTextBoxColumn.HeaderText = "Sensor Id";
            this.sensorIdDataGridViewTextBoxColumn.Name = "sensorIdDataGridViewTextBoxColumn";
            this.sensorIdDataGridViewTextBoxColumn.ReadOnly = true;
            this.sensorIdDataGridViewTextBoxColumn.Width = 150;
            // 
            // sensorValueDataGridViewTextBoxColumn
            // 
            this.sensorValueDataGridViewTextBoxColumn.DataPropertyName = "SensorValue";
            this.sensorValueDataGridViewTextBoxColumn.HeaderText = "Value";
            this.sensorValueDataGridViewTextBoxColumn.Name = "sensorValueDataGridViewTextBoxColumn";
            this.sensorValueDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // sensorDescriptionDataGridViewTextBoxColumn
            // 
            this.sensorDescriptionDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.sensorDescriptionDataGridViewTextBoxColumn.DataPropertyName = "SensorDescription";
            this.sensorDescriptionDataGridViewTextBoxColumn.HeaderText = "Description";
            this.sensorDescriptionDataGridViewTextBoxColumn.Name = "sensorDescriptionDataGridViewTextBoxColumn";
            this.sensorDescriptionDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // retrievedSensorValueBindingSource
            // 
            this.retrievedSensorValueBindingSource.DataSource = typeof(MsmqSensorTask.RetrievedSensorValue);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 564);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.checkBoxJumpToCurrentOutput);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridViewValues);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxOutput);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Performance counters (MSMQ)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewValues)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.retrievedSensorValueBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dataGridViewValues;
        private System.Windows.Forms.DataGridViewTextBoxColumn sensorIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn sensorValueDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn sensorDescriptionDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource retrievedSensorValueBindingSource;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxJumpToCurrentOutput;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonStopReceive;
        private System.Windows.Forms.Button buttonStartReceive;
    }
}

