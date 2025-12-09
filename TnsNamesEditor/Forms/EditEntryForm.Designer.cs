namespace TnsNamesEditor.Forms
{
    partial class EditEntryForm
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
            label1 = new Label();
            txtName = new TextBox();
            label2 = new Label();
            txtHost = new TextBox();
            label3 = new Label();
            txtPort = new TextBox();
            label4 = new Label();
            txtServiceName = new TextBox();
            label5 = new Label();
            txtSid = new TextBox();
            label6 = new Label();
            cmbProtocol = new ComboBox();
            label7 = new Label();
            txtServer = new TextBox();
            btnOk = new Button();
            btnCancel = new Button();
            groupBox1 = new GroupBox();
            txtPreview = new TextBox();
            btnPaste = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 11);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 0;
            label1.Text = "Nome:";
            // 
            // txtName
            // 
            txtName.Location = new Point(18, 28);
            txtName.Margin = new Padding(3, 2, 3, 2);
            txtName.Name = "txtName";
            txtName.Size = new Size(464, 23);
            txtName.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(18, 56);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 2;
            label2.Text = "Host:";
            // 
            // txtHost
            // 
            txtHost.Location = new Point(18, 74);
            txtHost.Margin = new Padding(3, 2, 3, 2);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(307, 23);
            txtHost.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(341, 56);
            label3.Name = "label3";
            label3.Size = new Size(38, 15);
            label3.TabIndex = 4;
            label3.Text = "Porta:";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(341, 74);
            txtPort.Margin = new Padding(3, 2, 3, 2);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(140, 23);
            txtPort.TabIndex = 5;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(18, 101);
            label4.Name = "label4";
            label4.Size = new Size(82, 15);
            label4.TabIndex = 6;
            label4.Text = "Service Name:";
            // 
            // txtServiceName
            // 
            txtServiceName.Location = new Point(18, 118);
            txtServiceName.Margin = new Padding(3, 2, 3, 2);
            txtServiceName.Name = "txtServiceName";
            txtServiceName.Size = new Size(464, 23);
            txtServiceName.TabIndex = 7;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(18, 146);
            label5.Name = "label5";
            label5.Size = new Size(27, 15);
            label5.TabIndex = 8;
            label5.Text = "SID:";
            // 
            // txtSid
            // 
            txtSid.Location = new Point(18, 164);
            txtSid.Margin = new Padding(3, 2, 3, 2);
            txtSid.Name = "txtSid";
            txtSid.Size = new Size(464, 23);
            txtSid.TabIndex = 9;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(18, 191);
            label6.Name = "label6";
            label6.Size = new Size(62, 15);
            label6.TabIndex = 10;
            label6.Text = "Protocolo:";
            // 
            // cmbProtocol
            // 
            cmbProtocol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProtocol.FormattingEnabled = true;
            cmbProtocol.Items.AddRange(new object[] { "TCP", "TCPS", "IPC" });
            cmbProtocol.Location = new Point(18, 208);
            cmbProtocol.Margin = new Padding(3, 2, 3, 2);
            cmbProtocol.Name = "cmbProtocol";
            cmbProtocol.Size = new Size(219, 23);
            cmbProtocol.TabIndex = 11;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(254, 191);
            label7.Name = "label7";
            label7.Size = new Size(53, 15);
            label7.TabIndex = 12;
            label7.Text = "Servidor:";
            // 
            // txtServer
            // 
            txtServer.Location = new Point(254, 208);
            txtServer.Margin = new Padding(3, 2, 3, 2);
            txtServer.Name = "txtServer";
            txtServer.Size = new Size(228, 23);
            txtServer.TabIndex = 13;
            // 
            // btnOk
            // 
            btnOk.Location = new Point(309, 466);
            btnOk.Margin = new Padding(3, 2, 3, 2);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(82, 22);
            btnOk.TabIndex = 14;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(397, 466);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(82, 22);
            btnCancel.TabIndex = 15;
            btnCancel.Text = "Cancelar";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtPreview);
            groupBox1.Location = new Point(18, 270);
            groupBox1.Margin = new Padding(3, 2, 3, 2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 2, 3, 2);
            groupBox1.Size = new Size(464, 192);
            groupBox1.TabIndex = 16;
            groupBox1.TabStop = false;
            groupBox1.Text = "Visualização (Preview em Tempo Real)";
            // 
            // txtPreview
            // 
            txtPreview.Dock = DockStyle.Fill;
            txtPreview.Font = new Font("Consolas", 9F);
            txtPreview.Location = new Point(3, 18);
            txtPreview.Margin = new Padding(3, 2, 3, 2);
            txtPreview.Multiline = true;
            txtPreview.Name = "txtPreview";
            txtPreview.ReadOnly = true;
            txtPreview.ScrollBars = ScrollBars.Both;
            txtPreview.Size = new Size(458, 172);
            txtPreview.TabIndex = 0;
            txtPreview.WordWrap = false;
            // 
            // btnPaste
            // 
            btnPaste.Location = new Point(350, 240);
            btnPaste.Margin = new Padding(3, 2, 3, 2);
            btnPaste.Name = "btnPaste";
            btnPaste.Size = new Size(131, 22);
            btnPaste.TabIndex = 18;
            btnPaste.Text = "Colar TNS";
            btnPaste.UseVisualStyleBackColor = true;
            btnPaste.Click += btnPaste_Click;
            // 
            // EditEntryForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(502, 507);
            Controls.Add(btnPaste);
            Controls.Add(groupBox1);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(txtServer);
            Controls.Add(label7);
            Controls.Add(cmbProtocol);
            Controls.Add(label6);
            Controls.Add(txtSid);
            Controls.Add(label5);
            Controls.Add(txtServiceName);
            Controls.Add(label4);
            Controls.Add(txtPort);
            Controls.Add(label3);
            Controls.Add(txtHost);
            Controls.Add(label2);
            Controls.Add(txtName);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditEntryForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Editar Entrada TNS";
            Load += EditEntryForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtName;
        private Label label2;
        private TextBox txtHost;
        private Label label3;
        private TextBox txtPort;
        private Label label4;
        private TextBox txtServiceName;
        private Label label5;
        private TextBox txtSid;
        private Label label6;
        private ComboBox cmbProtocol;
        private Label label7;
        private TextBox txtServer;
        private Button btnOk;
        private Button btnCancel;
        private GroupBox groupBox1;
        private TextBox txtPreview;
        private Button btnPaste;
    }
}
