using System.ComponentModel;
using TnsNamesEditor.Models;

namespace TnsNamesEditor.Forms
{
    public partial class MainForm : Form
    {
        private List<TnsEntry> entries = new List<TnsEntry>();
        private List<TnsEntry> filteredEntries = new List<TnsEntry>();
        private string currentFilePath = string.Empty;
        private readonly string defaultTnsPath = @"C:\oracle\product\19.0.0\client_1\network\admin\tnsnames.ora";
        private string? lastSortColumn = null;
        private SortOrder lastSortOrder = SortOrder.None;

        public MainForm()
        {
            InitializeComponent();
            AttachContextMenuHandlers();
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            LoadIcon();
        }

        private void LoadIcon()
        {
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
            }
            catch
            {
                // Se falhar ao carregar o ícone, continua sem ele
            }
        }

        private void AttachContextMenuHandlers()
        {
            menuEdit.Click += (s, e) => btnEdit_Click(this, EventArgs.Empty);
            menuDelete.Click += (s, e) => btnDelete_Click(this, EventArgs.Empty);
            menuCopy.Click += (s, e) => menuCopy_Click(s, e);
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            int selectedCount = dataGridView1.SelectedRows.Count;
            
            // Habilita/desabilita botões baseado na quantidade de seleções
            bool singleSelection = selectedCount == 1;
            bool hasSelection = selectedCount > 0;
            
            // Editar e Copiar: apenas com uma seleção (somente menu contexto)
            menuEdit.Enabled = singleSelection;
            menuCopy.Enabled = singleSelection;
            
            // Excluir: com uma ou mais seleções
            btnDelete.Enabled = hasSelection;
            menuDelete.Enabled = hasSelection;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Tenta carregar o arquivo padrão se existir
            if (File.Exists(defaultTnsPath))
            {
                LoadFile(defaultTnsPath);
            }
            else
            {
                // Tenta o arquivo na área de trabalho
                var desktopPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "tnsnames.ora"
                );
                
                if (File.Exists(desktopPath))
                {
                    LoadFile(desktopPath);
                }
                else
                {
                    UpdateStatus("Nenhum arquivo carregado. Use 'Abrir' para selecionar um arquivo.");
                }
            }
        }

        private void LoadFile(string filePath)
        {
            try
            {
                currentFilePath = filePath;
                entries = TnsNamesParser.ParseFile(filePath);
                
                // Ordena por ordem alfabética pelo nome
                entries = entries.OrderBy(e => e.Name).ToList();
                
                RefreshGrid();
                UpdateStatus($"Arquivo carregado com sucesso: {entries.Count} entrada(s)");
                lblFilePath.Text = filePath;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Erro ao carregar arquivo: {ex.Message}");
            }
        }

        private void RefreshGrid()
        {
            dataGridView1.DataSource = null;
            
            // Se filteredEntries tiver itens, mostra a lista filtrada
            // Se estiver vazia mas o texto de pesquisa não estiver vazio, mostra vazio
            // Se não houver pesquisa ativa, mostra todos os entries
            if (filteredEntries.Any())
            {
                dataGridView1.DataSource = filteredEntries;
            }
            else if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                // Pesquisa ativa mas sem resultados - mostra lista vazia
                dataGridView1.DataSource = new List<TnsEntry>();
            }
            else
            {
                // Sem pesquisa - mostra todos
                dataGridView1.DataSource = entries;
            }
            
            // Oculta colunas desnecessárias
            if (dataGridView1.Columns["RawContent"] != null)
                dataGridView1.Columns["RawContent"].Visible = false;
            
            // Ajusta nomes das colunas
            if (dataGridView1.Columns["Name"] != null)
            {
                dataGridView1.Columns["Name"].HeaderText = "Nome ⇅";
                dataGridView1.Columns["Name"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns["Host"] != null)
            {
                dataGridView1.Columns["Host"].HeaderText = "Host ⇅";
                dataGridView1.Columns["Host"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns["Port"] != null)
            {
                dataGridView1.Columns["Port"].HeaderText = "Porta ⇅";
                dataGridView1.Columns["Port"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns["ServiceName"] != null)
            {
                dataGridView1.Columns["ServiceName"].HeaderText = "Service Name ⇅";
                dataGridView1.Columns["ServiceName"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns["Sid"] != null)
            {
                dataGridView1.Columns["Sid"].HeaderText = "SID ⇅";
                dataGridView1.Columns["Sid"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns["Protocol"] != null)
            {
                dataGridView1.Columns["Protocol"].HeaderText = "Protocolo ⇅";
                dataGridView1.Columns["Protocol"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            if (dataGridView1.Columns["Server"] != null)
            {
                dataGridView1.Columns["Server"].HeaderText = "Servidor ⇅";
                dataGridView1.Columns["Server"].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            
            // Restaura o ícone de ordenação se houver
            if (!string.IsNullOrEmpty(lastSortColumn) && lastSortOrder != SortOrder.None)
            {
                var column = dataGridView1.Columns.Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => c.DataPropertyName == lastSortColumn);
                if (column != null)
                {
                    column.HeaderCell.SortGlyphDirection = lastSortOrder;
                }
            }
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = message;
        }

        private void SaveChanges(string statusMessage)
        {
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                try
                {
                    TnsNamesParser.SaveToFile(currentFilePath, entries);
                    UpdateStatus(statusMessage);
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Erro ao salvar arquivo: {ex.Message}");
                }
            }
            else
            {
                UpdateStatus(statusMessage);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(currentFilePath);
                openFileDialog1.FileName = Path.GetFileName(currentFilePath);
            }
            else
            {
                openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadFile(openFileDialog1.FileName);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = saveFileDialog1.FileName;
                }
                else
                {
                    return;
                }
            }

            try
            {
                TnsNamesParser.SaveToFile(currentFilePath, entries);
                UpdateStatus($"Arquivo salvo com sucesso: {entries.Count} entrada(s)");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Erro ao salvar arquivo: {ex.Message}");
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var entry = new TnsEntry
            {
                Protocol = "TCP",
                Port = "1521"
            };

            using (var editForm = new EditEntryForm(entry, entries))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // Verifica duplicata
                    if (entries.Any(e => e.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        // Remove a entrada antiga
                        var oldEntry = entries.FirstOrDefault(e => e.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase));
                        if (oldEntry != null)
                        {
                            entries.Remove(oldEntry);
                        }
                        entries.Add(entry);
                        RefreshGrid();
                        SaveChanges($"Entrada '{entry.Name}' substituída e salva");
                    }
                    else
                    {
                        entries.Add(entry);
                        RefreshGrid();
                        SaveChanges($"Entrada '{entry.Name}' adicionada e salva");
                    }
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                UpdateStatus("Selecione uma entrada para editar");
                return;
            }

            if (dataGridView1.SelectedRows.Count > 1)
            {
                UpdateStatus("Selecione apenas uma entrada para editar");
                return;
            }

            var selectedEntry = (TnsEntry)dataGridView1.SelectedRows[0].DataBoundItem;
            var originalName = selectedEntry.Name;

            using (var editForm = new EditEntryForm(selectedEntry, entries, originalName))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // Verifica se o nome foi alterado e se já existe outro com o novo nome
                    if (!originalName.Equals(selectedEntry.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (entries.Any(e => e.Name.Equals(selectedEntry.Name, StringComparison.OrdinalIgnoreCase) && 
                                           !e.Name.Equals(originalName, StringComparison.OrdinalIgnoreCase)))
                        {
                            UpdateStatus($"Já existe uma entrada com o nome '{selectedEntry.Name}'. Escolha outro nome");
                            // Restaura o nome original
                            selectedEntry.Name = originalName;
                            return;
                        }
                    }
                    
                    RefreshGrid();
                    SaveChanges($"Entrada '{selectedEntry.Name}' atualizada e salva");
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                UpdateStatus("Selecione uma ou mais entradas para excluir");
                return;
            }

            var selectedEntries = dataGridView1.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (TnsEntry)row.DataBoundItem)
                .ToList();

            foreach (var entry in selectedEntries)
            {
                entries.Remove(entry);
                
                // Se houver filtro ativo, remove também da lista filtrada
                if (filteredEntries.Any())
                {
                    filteredEntries.Remove(entry);
                }
            }
            
            RefreshGrid();
            
            string statusMessage = selectedEntries.Count == 1 
                ? $"Entrada '{selectedEntries[0].Name}' excluída e salva"
                : $"{selectedEntries.Count} entradas excluídas e salvas";
                
            SaveChanges(statusMessage);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                LoadFile(currentFilePath);
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnEdit_Click(sender, e);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + F para ativar a pesquisa
            if (e.Control && e.KeyCode == Keys.F)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
                e.Handled = true;
                return;
            }

            // Ctrl + C para copiar entrada selecionada
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (dataGridView1.SelectedRows.Count == 1)
                {
                    menuCopy_Click(sender, e);
                    e.Handled = true;
                }
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.F2:
                    btnOpen_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.F3:
                    btnSave_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.F4:
                    btnAdd_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.F5:
                    btnRefresh_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.Delete:
                    if (dataGridView1.SelectedRows.Count > 0)
                    {
                        btnDelete_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Keys.Enter:
                    if (dataGridView1.SelectedRows.Count > 0)
                    {
                        btnEdit_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                txtSearch.Clear();
                e.Handled = true;
            }
        }

        private void PerformSearch()
        {
            string searchText = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                filteredEntries.Clear();
                RefreshGrid();
                UpdateStatus($"{entries.Count} entrada(s) carregada(s)");
                return;
            }

            filteredEntries = entries.Where(entry =>
                entry.Name.ToLower().Contains(searchText) ||
                entry.Host.ToLower().Contains(searchText) ||
                entry.Port.ToLower().Contains(searchText) ||
                (entry.ServiceName?.ToLower().Contains(searchText) ?? false) ||
                (entry.Sid?.ToLower().Contains(searchText) ?? false) ||
                (entry.Protocol?.ToLower().Contains(searchText) ?? false) ||
                (entry.Server?.ToLower().Contains(searchText) ?? false)
            ).OrderBy(e => e.Name).ToList();

            RefreshGrid();
            
            if (filteredEntries.Count == 0)
            {
                UpdateStatus("Nenhum resultado encontrado");
            }
            else
            {
                UpdateStatus($"{filteredEntries.Count} resultado(s) encontrado(s)");
            }
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            // Seleciona a linha quando clicado com botão direito
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridView1.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[hitTest.RowIndex].Selected = true;
                }
            }
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.Columns.Count == 0) return;
            
            var column = dataGridView1.Columns[e.ColumnIndex];
            var propertyName = column.DataPropertyName;
            
            // Não ordena pela coluna RawContent
            if (propertyName == "RawContent") return;
            
            // Determina a direção da ordenação
            ListSortDirection direction;
            
            if (column.HeaderCell.SortGlyphDirection == SortOrder.Ascending)
            {
                direction = ListSortDirection.Descending;
                column.HeaderCell.SortGlyphDirection = SortOrder.Descending;
            }
            else
            {
                direction = ListSortDirection.Ascending;
                column.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            }
            
            // Remove o ícone de ordenação das outras colunas
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (col.Index != e.ColumnIndex)
                {
                    col.HeaderCell.SortGlyphDirection = SortOrder.None;
                }
            }
            
            // Salva o estado da ordenação
            lastSortColumn = propertyName;
            lastSortOrder = column.HeaderCell.SortGlyphDirection;
            
            // Ordena a lista apropriada
            if (filteredEntries.Any())
            {
                filteredEntries = SortEntries(filteredEntries, propertyName, direction);
            }
            else
            {
                entries = SortEntries(entries, propertyName, direction);
            }
            
            RefreshGrid();
        }

        private List<TnsEntry> SortEntries(List<TnsEntry> list, string propertyName, ListSortDirection direction)
        {
            IOrderedEnumerable<TnsEntry> sorted;
            
            switch (propertyName)
            {
                case "Name":
                    sorted = direction == ListSortDirection.Ascending 
                        ? list.OrderBy(e => e.Name) 
                        : list.OrderByDescending(e => e.Name);
                    break;
                case "Host":
                    sorted = direction == ListSortDirection.Ascending 
                        ? list.OrderBy(e => e.Host) 
                        : list.OrderByDescending(e => e.Host);
                    break;
                case "Port":
                    sorted = direction == ListSortDirection.Ascending 
                        ? list.OrderBy(e => e.Port) 
                        : list.OrderByDescending(e => e.Port);
                    break;
                case "ServiceName":
                    sorted = direction == ListSortDirection.Ascending 
                        ? list.OrderBy(e => e.ServiceName ?? string.Empty) 
                        : list.OrderByDescending(e => e.ServiceName ?? string.Empty);
                    break;
                case "Sid":
                    sorted = direction == ListSortDirection.Ascending 
                        ? list.OrderBy(e => e.Sid ?? string.Empty) 
                        : list.OrderByDescending(e => e.Sid ?? string.Empty);
                    break;
                case "Protocol":
                    sorted = direction == ListSortDirection.Ascending 
                        ? list.OrderBy(e => e.Protocol ?? string.Empty) 
                        : list.OrderByDescending(e => e.Protocol ?? string.Empty);
                    break;
                case "Server":
                    sorted = direction == ListSortDirection.Ascending 
                        ? list.OrderBy(e => e.Server ?? string.Empty) 
                        : list.OrderByDescending(e => e.Server ?? string.Empty);
                    break;
                default:
                    sorted = direction == ListSortDirection.Ascending 
                        ? list.OrderBy(e => e.Name) 
                        : list.OrderByDescending(e => e.Name);
                    break;
            }
            
            return sorted.ToList();
        }

        private void menuCopy_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                UpdateStatus("Selecione uma entrada para copiar");
                return;
            }

            if (dataGridView1.SelectedRows.Count > 1)
            {
                UpdateStatus("Selecione apenas uma entrada para copiar");
                return;
            }

            var selectedEntry = (TnsEntry)dataGridView1.SelectedRows[0].DataBoundItem;
            var formattedText = selectedEntry.ToTnsFormat();
            Clipboard.SetText(formattedText);
            UpdateStatus($"Entrada '{selectedEntry.Name}' copiada para a área de transferência");
        }
    }
}
