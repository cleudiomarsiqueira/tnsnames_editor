namespace TnsNamesEditor.Forms
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            contextMenuStrip1 = new ContextMenuStrip(components);
            menuEdit = new ToolStripMenuItem();
            menuDelete = new ToolStripMenuItem();
            menuCopy = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            btnOpen = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnAdd = new ToolStripButton();
            btnDelete = new ToolStripButton();
            btnRefresh = new ToolStripButton();
            btnEdit = new ToolStripButton();
            searchPanel = new Panel();
            txtSearch = new TextBox();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lblFilePath = new ToolStripStatusLabel();
            dataGridView1 = new DataGridView();
            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            contextMenuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            searchPanel.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { menuEdit, menuDelete, menuCopy });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(152, 70);
            // 
            // menuEdit
            // 
            menuEdit.Name = "menuEdit";
            menuEdit.ShortcutKeyDisplayString = "Enter";
            menuEdit.Size = new Size(151, 22);
            menuEdit.Text = "Editar";
            // 
            // menuDelete
            // 
            menuDelete.Name = "menuDelete";
            menuDelete.ShortcutKeyDisplayString = "Del";
            menuDelete.Size = new Size(151, 22);
            menuDelete.Text = "Excluir";
            // 
            // menuCopy
            // 
            menuCopy.Name = "menuCopy";
            menuCopy.ShortcutKeyDisplayString = "Ctrl+C";
            menuCopy.Size = new Size(151, 22);
            menuCopy.Text = "Copiar";
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new Size(32, 32);
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnOpen, toolStripSeparator1, btnAdd, btnDelete, btnRefresh });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new Padding(5);
            toolStrip1.Size = new Size(1050, 50);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnOpen
            // 
            btnOpen.AutoSize = false;
            btnOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnOpen.Font = new Font("Segoe UI", 10F);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(90, 35);
            btnOpen.Text = "Abrir (F2)";
            btnOpen.Click += btnOpen_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 40);
            // 
            // btnAdd
            // 
            btnAdd.AutoSize = false;
            btnAdd.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnAdd.Font = new Font("Segoe UI", 10F);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(120, 35);
            btnAdd.Text = "Adicionar (F4)";
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDelete
            // 
            btnDelete.AutoSize = false;
            btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDelete.Font = new Font("Segoe UI", 10F);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(110, 35);
            btnDelete.Text = "Excluir (Del)";
            btnDelete.Click += btnDelete_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Alignment = ToolStripItemAlignment.Right;
            btnRefresh.AutoSize = false;
            btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRefresh.Font = new Font("Segoe UI", 10F);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(120, 35);
            btnRefresh.Text = "Atualizar (F5)";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnEdit
            // 
            btnEdit.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(79, 22);
            btnEdit.Text = "Editar (Enter)";
            btnEdit.Click += btnEdit_Click;
            // 
            // searchPanel
            // 
            searchPanel.BackColor = SystemColors.Control;
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Location = new Point(0, 50);
            searchPanel.Margin = new Padding(3, 2, 3, 2);
            searchPanel.Name = "searchPanel";
            searchPanel.Padding = new Padding(9, 8, 9, 8);
            searchPanel.Size = new Size(1050, 34);
            searchPanel.TabIndex = 2;
            // 
            // txtSearch
            // 
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Location = new Point(9, 8);
            txtSearch.Margin = new Padding(3, 2, 3, 2);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Pesquisar...";
            txtSearch.Size = new Size(1032, 23);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, lblFilePath });
            statusStrip1.Location = new Point(0, 503);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 12, 0);
            statusStrip1.Size = new Size(1050, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(43, 17);
            lblStatus.Text = "Pronto";
            // 
            // lblFilePath
            // 
            lblFilePath.Name = "lblFilePath";
            lblFilePath.Size = new Size(994, 17);
            lblFilePath.Spring = true;
            lblFilePath.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.ContextMenuStrip = contextMenuStrip1;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 84);
            dataGridView1.Margin = new Padding(3, 2, 3, 2);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1050, 419);
            dataGridView1.TabIndex = 2;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;
            dataGridView1.MouseDown += dataGridView1_MouseDown;
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.DefaultExt = "ora";
            saveFileDialog1.Filter = "TNS Names Files|*.ora|All Files|*.*";
            saveFileDialog1.Title = "Salvar arquivo tnsnames.ora";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1050, 525);
            Controls.Add(dataGridView1);
            Controls.Add(searchPanel);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            KeyPreview = true;
            Margin = new Padding(3, 2, 3, 2);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TNS Names Editor - Cleudiomar Siqueira";
            Load += MainForm_Load;
            KeyDown += MainForm_KeyDown;
            contextMenuStrip1.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            searchPanel.ResumeLayout(false);
            searchPanel.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripButton btnOpen;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnAdd;
        private ToolStripButton btnEdit;
        private ToolStripButton btnDelete;
        private ToolStripButton btnRefresh;
        private Panel searchPanel;
        private TextBox txtSearch;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
        private DataGridView dataGridView1;
        private OpenFileDialog openFileDialog1;
        private SaveFileDialog saveFileDialog1;
        private ToolStripStatusLabel lblFilePath;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem menuEdit;
        private ToolStripMenuItem menuDelete;
        private ToolStripMenuItem menuCopy;
    }
}
