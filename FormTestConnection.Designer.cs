namespace ICM_Universal_Tester
{
	partial class FormTestConnection
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param testName="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
			this.buttonOk = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.comboBoxBoard = new System.Windows.Forms.ComboBox();
			this.comboBoxChannel = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// buttonOk
			// 
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Location = new System.Drawing.Point(107, 153);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(122, 35);
			this.buttonOk.TabIndex = 3;
			this.buttonOk.Text = "OK";
			this.buttonOk.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(29, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(69, 20);
			this.label1.TabIndex = 6;
			this.label1.Text = "Board #:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(29, 89);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 20);
			this.label2.TabIndex = 7;
			this.label2.Text = "Pin #:";
			// 
			// comboBoxBoard
			// 
			this.comboBoxBoard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxBoard.FormattingEnabled = true;
			this.comboBoxBoard.Location = new System.Drawing.Point(119, 39);
			this.comboBoxBoard.Name = "comboBoxBoard";
			this.comboBoxBoard.Size = new System.Drawing.Size(176, 28);
			this.comboBoxBoard.TabIndex = 8;
			// 
			// comboBoxChannel
			// 
			this.comboBoxChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxChannel.FormattingEnabled = true;
			this.comboBoxChannel.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16"});
			this.comboBoxChannel.Location = new System.Drawing.Point(119, 86);
			this.comboBoxChannel.Name = "comboBoxChannel";
			this.comboBoxChannel.Size = new System.Drawing.Size(176, 28);
			this.comboBoxChannel.TabIndex = 8;
			// 
			// FormTestConnection
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(343, 214);
			this.Controls.Add(this.comboBoxChannel);
			this.Controls.Add(this.comboBoxBoard);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonOk);
			this.Name = "FormTestConnection";
			this.Text = "Probe Connection";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		public System.Windows.Forms.ComboBox comboBoxBoard;
		public System.Windows.Forms.ComboBox comboBoxChannel;
	}
}