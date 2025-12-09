using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TnsNamesEditor.Models;
using TnsNamesEditor.Services;

namespace TnsNamesEditor.Forms
{
    public partial class MainForm : Form
    {
        private List<TnsEntry> entries = new List<TnsEntry>();
        private List<TnsEntry> filteredEntries = new List<TnsEntry>();
        private string currentFilePath = string.Empty;
        private readonly List<string> defaultTnsPaths;
        private string? lastSortColumn = null;
        private SortOrder lastSortOrder = SortOrder.None;
        private CancellationTokenSource? pingCancellation;
        private readonly Dictionary<string, string> statusCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> pendingStatusRefresh = new(StringComparer.OrdinalIgnoreCase);

        public MainForm()
        {
            InitializeComponent();
            defaultTnsPaths = BuildDefaultTnsPathList();
            AttachContextMenuHandlers();
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
            dataGridView1.RowPrePaint += DataGridView1_RowPrePaint;
            LoadIcon();
            UpdateFilePathLabel(string.Empty);
            UpdateAvailableActions();
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

        private List<string> BuildDefaultTnsPathList()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddPath(string? candidate)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    return;
                }

                try
                {
                    var normalized = Path.GetFullPath(candidate);
                    paths.Add(normalized);
                }
                catch
                {
                    paths.Add(candidate);
                }
            }

            var tnsAdmin = Environment.GetEnvironmentVariable("TNS_ADMIN");
            if (!string.IsNullOrWhiteSpace(tnsAdmin))
            {
                AddPath(Path.Combine(tnsAdmin, "tnsnames.ora"));
            }

            var oracleHome = Environment.GetEnvironmentVariable("ORACLE_HOME");
            if (!string.IsNullOrWhiteSpace(oracleHome))
            {
                AddPath(Path.Combine(oracleHome, "network", "admin", "tnsnames.ora"));
            }

            string[] baseDirs =
            {
                @"C:\\oracle",
                @"C:\\app\\oracle",
                @"C:\\Oracle",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Oracle"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Oracle")
            };

            string[] versions = { "21", "19", "18", "12", "11" };
            string[] homeFolders = { "client_1", "dbhome_1", "home" };

            foreach (var baseDir in baseDirs)
            {
                if (string.IsNullOrWhiteSpace(baseDir))
                {
                    continue;
                }

                AddPath(Path.Combine(baseDir, "network", "admin", "tnsnames.ora"));

                foreach (var version in versions)
                {
                    AddPath(Path.Combine(baseDir, "product", $"{version}.0.0", "network", "admin", "tnsnames.ora"));

                    foreach (var homeFolder in homeFolders)
                    {
                        AddPath(Path.Combine(baseDir, "product", $"{version}.0.0", homeFolder, "network", "admin", "tnsnames.ora"));
                    }
                }
            }

            // Caminhos comuns de exemplos/desktop
            AddPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "tnsnames.ora"));
            AddPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tnsnames.ora"));

            return paths.ToList();
        }

        private void AttachContextMenuHandlers()
        {
            menuEdit.Click += (s, e) => btnEdit_Click(this, EventArgs.Empty);
            menuDelete.Click += (s, e) => btnDelete_Click(this, EventArgs.Empty);
            menuCopy.Click += (s, e) => menuCopy_Click(s!, e);
        }

        private void DataGridView1_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateSelectionDependentActions();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (TryLoadDefaultFileFromKnownLocations())
            {
                return;
            }

            UpdateFilePathLabel(string.Empty);
            UpdateStatus("Nenhum arquivo carregado. Use 'Abrir' para selecionar um arquivo.");

            var message = "Não foi possível localizar o arquivo tnsnames.ora automaticamente nas pastas padrão conhecidas.\n\nDeseja selecionar o arquivo manualmente agora?";
            if (ShowConfirmation(message, "Arquivo não encontrado"))
            {
                btnOpen_Click(this, EventArgs.Empty);
            }
        }

        private void LoadFile(string filePath, string? statusOverride = null)
        {
            try
            {
                CancelPendingPingRequests();

                var parsedEntries = TnsNamesParser.ParseFile(filePath)
                    .OrderBy(e => e.Name)
                    .ToList();

                if (!parsedEntries.Any())
                {
                    ApplyEmptyFileState(filePath, statusOverride);
                    return;
                }

                entries = parsedEntries;
                currentFilePath = filePath;
                filteredEntries.Clear();

                RebindGridAfterDataChange();
                UpdateStatus(statusOverride ?? $"Arquivo carregado com sucesso: {entries.Count} entrada(s)");
                UpdateFilePathLabel(filePath);
                if (statusOverride == null)
                {
                    pendingStatusRefresh.Clear();
                }

                InitializeConnectionStatus(entries);
                StartConnectionStatusRefresh(entries, forceRefresh: statusOverride == null);
            }
            catch (Exception ex)
            {
                ShowError("Erro ao carregar arquivo:", "Erro ao Carregar", ex);
            }
        }

        private void ApplyEmptyFileState(string filePath, string? statusOverride)
        {
            CancelPendingPingRequests();
            entries.Clear();
            filteredEntries.Clear();
            currentFilePath = filePath;
            statusCache.Clear();
            pendingStatusRefresh.Clear();

            RebindGridAfterDataChange();
            UpdateFilePathLabel(filePath);
            UpdateStatus(statusOverride ?? "Arquivo carregado, mas nenhuma entrada válida foi encontrada.");
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
            if (dataGridView1.Columns["ConnectionStatus"] != null)
            {
                var statusColumn = dataGridView1.Columns["ConnectionStatus"];
                statusColumn.HeaderText = "Status";
                statusColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                statusColumn.ReadOnly = true;
                statusColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
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

        private void UpdateFilePathLabel(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                lblFilePath.IsLink = false;
                lblFilePath.Text = string.Empty;
                lblFilePath.ToolTipText = string.Empty;
            }
            else
            {
                lblFilePath.IsLink = true;
                lblFilePath.Text = path;
                lblFilePath.ToolTipText = path;
            }
        }

        private void UpdateAvailableActions()
        {
            bool hasEntries = entries.Any();
            bool hasFile = !string.IsNullOrWhiteSpace(currentFilePath);

            txtSearch.Enabled = hasEntries;
            dataGridView1.Enabled = hasEntries;
            btnAdd.Enabled = hasFile;
            btnRefresh.Enabled = hasFile;

            if (!hasEntries)
            {
                dataGridView1.ClearSelection();
                filteredEntries.Clear();
            }

            UpdateSelectionDependentActions();
        }

        private void UpdateSelectionDependentActions()
        {
            if (!entries.Any() || !dataGridView1.Enabled)
            {
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
                menuEdit.Enabled = false;
                menuDelete.Enabled = false;
                menuCopy.Enabled = false;
                return;
            }

            int selectedCount = dataGridView1.SelectedRows.Count;
            bool singleSelection = selectedCount == 1;
            bool hasSelection = selectedCount > 0;

            btnEdit.Enabled = singleSelection;
            menuEdit.Enabled = singleSelection;
            menuCopy.Enabled = singleSelection;
            btnDelete.Enabled = hasSelection;
            menuDelete.Enabled = hasSelection;
        }

        private bool TryLoadDefaultFileFromKnownLocations()
        {
            foreach (var path in defaultTnsPaths)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    LoadFile(path);
                    return true;
                }
            }

            return false;
        }

        // Métodos de conveniência que delegam para o MessageService
        private void ShowError(string message, string title = "Erro", Exception? exception = null)
            => MessageService.ShowError(message, title, exception);

        private void ShowWarning(string message, string title = "Atenção")
            => MessageService.ShowWarning(message, title);

        private void ShowSuccess(string message, string title = "Sucesso")
            => MessageService.ShowSuccess(message, title);

        private bool ShowConfirmation(string message, string title = "Confirmação")
            => MessageService.ShowConfirmation(message, title);

        private void SaveChanges(string statusMessage)
        {
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                try
                {
                    TnsNamesParser.SaveToFile(currentFilePath, entries);
                    SqlIniUpdateResult sqlIniResult = default;
                    bool hasSqlIniResult = false;

                    try
                    {
                        sqlIniResult = SqlIniUpdater.UpdateRemoteDbNames(entries);
                        hasSqlIniResult = true;
                    }
                    catch (Exception sqlEx)
                    {
                        ShowError("Erro ao atualizar SQL.ini:", "Erro ao Atualizar SQL.ini", sqlEx);
                    }

                    string finalStatus = statusMessage;
                    if (hasSqlIniResult)
                    {
                        finalStatus = $"{statusMessage} | {sqlIniResult.Message}";
                    }

                    LoadFile(currentFilePath, finalStatus);
                }
                catch (Exception ex)
                {
                    ShowError("Erro ao salvar arquivo:", "Erro ao Salvar", ex);
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!btnAdd.Enabled)
            {
                return;
            }

            var entry = new TnsEntry
            {
                ConnectionStatus = "Verificando..."
            };

            using (var editForm = new EditEntryForm(entry, entries))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // Verifica se já existe uma entrada idêntica (todos os campos iguais)
                    var identicalEntry = entries.FirstOrDefault(e => e.IsIdenticalTo(entry));
                    
                    if (identicalEntry != null)
                    {
                        ShowWarning(
                            $"Já existe uma entrada idêntica a esta.\n\nNome: {entry.Name}\nHost: {entry.Host}\nPorta: {entry.Port}\nService Name: {entry.ServiceName}",
                            "Entrada Duplicada");
                        return;
                    }
                    
                    // Verifica se existe uma entrada com o mesmo nome mas dados diferentes
                    var sameNameEntry = entries.FirstOrDefault(e => e.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (sameNameEntry != null)
                    {
                        if (ShowConfirmation(
                            $"Já existe uma entrada com o nome '{entry.Name}' mas com dados diferentes.\n\nDeseja substituir a entrada existente?",
                            "Nome Duplicado"))
                        {
                            entries.Remove(sameNameEntry);
                            entry.ConnectionStatus = "Verificando...";
                            MarkEntryForStatusRefresh(entry.Name);
                            entries.Add(entry);
                            RebindGridAfterDataChange();
                            SaveChanges($"Entrada '{entry.Name}' substituída e salva");
                        }
                        return;
                    }
                    
                    // Entrada nova, adiciona normalmente
                    entries.Add(entry);
                    MarkEntryForStatusRefresh(entry.Name);
                    RebindGridAfterDataChange();
                    SaveChanges($"Entrada '{entry.Name}' adicionada e salva");
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!btnEdit.Enabled)
            {
                return;
            }

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
            var originalIndex = entries.IndexOf(selectedEntry);
            
            // Cria uma cópia da entrada para edição
            var editedEntry = new TnsEntry
            {
                Name = selectedEntry.Name,
                Host = selectedEntry.Host,
                Port = selectedEntry.Port,
                ServiceName = selectedEntry.ServiceName,
                Sid = selectedEntry.Sid,
                Protocol = selectedEntry.Protocol,
                Server = selectedEntry.Server,
                RawContent = selectedEntry.RawContent,
                ConnectionStatus = selectedEntry.ConnectionStatus
            };

            using (var editForm = new EditEntryForm(editedEntry, entries, originalName))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // Verifica se já existe uma entrada idêntica (exceto a que está sendo editada)
                    var identicalEntry = entries.FirstOrDefault(e => 
                        !e.Name.Equals(originalName, StringComparison.OrdinalIgnoreCase) && 
                        e.IsIdenticalTo(editedEntry));
                    
                    if (identicalEntry != null)
                    {
                        ShowWarning(
                            $"Já existe uma entrada idêntica com o nome '{identicalEntry.Name}'.\n\nTodos os campos são iguais.",
                            "Entrada Duplicada");
                        return;
                    }
                    
                    // Verifica se o nome foi alterado e se já existe outro com o novo nome
                    if (!originalName.Equals(editedEntry.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (entries.Any(e => e.Name.Equals(editedEntry.Name, StringComparison.OrdinalIgnoreCase) && 
                                           !e.Name.Equals(originalName, StringComparison.OrdinalIgnoreCase)))
                        {
                            ShowWarning(
                                $"Já existe uma entrada com o nome '{editedEntry.Name}', mas com dados diferentes.\n\nEscolha outro nome.",
                                "Nome Duplicado");
                            return;
                        }
                    }
                    
                    // Remove a entrada original e adiciona a editada na mesma posição
                    entries.RemoveAt(originalIndex);
                    entries.Insert(originalIndex, editedEntry);
                    editedEntry.ConnectionStatus = "Verificando...";
                    MarkEntryForStatusRefresh(editedEntry.Name);
                    if (!originalName.Equals(editedEntry.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        statusCache.Remove(originalName);
                        pendingStatusRefresh.Remove(originalName);
                    }
                    
                    // Salva as alterações
                    SaveChanges($"Entrada '{editedEntry.Name}' atualizada e salva");
                    
                    // Recarrega o arquivo para garantir sincronia
                    LoadFile(currentFilePath);
                    UpdateStatus($"Entrada '{editedEntry.Name}' atualizada com sucesso");
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!btnDelete.Enabled)
            {
                return;
            }

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
                statusCache.Remove(entry.Name);
                pendingStatusRefresh.Remove(entry.Name);
                
                // Se houver filtro ativo, remove também da lista filtrada
                if (filteredEntries.Any())
                {
                    filteredEntries.Remove(entry);
                }
            }
            
            RebindGridAfterDataChange();
            
            string statusMessage = selectedEntries.Count == 1 
                ? $"Entrada '{selectedEntries[0].Name}' excluída e salva"
                : $"{selectedEntries.Count} entradas excluídas e salvas";
                
            SaveChanges(statusMessage);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!btnRefresh.Enabled)
            {
                return;
            }

            if (!string.IsNullOrEmpty(currentFilePath))
            {
                LoadFile(currentFilePath);
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && btnEdit.Enabled)
            {
                btnEdit_Click(sender, e);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + F para ativar a pesquisa
            if (e.Control && e.KeyCode == Keys.F && txtSearch.Enabled)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
                e.Handled = true;
                return;
            }

            // Ctrl + C para copiar entrada selecionada
            if (e.Control && e.KeyCode == Keys.C && menuCopy.Enabled)
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
                case Keys.F4:
                    if (btnAdd.Enabled)
                    {
                        btnAdd_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Keys.F5:
                    if (btnRefresh.Enabled)
                    {
                        btnRefresh_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Keys.Delete:
                    if (btnDelete.Enabled && dataGridView1.SelectedRows.Count > 0)
                    {
                        btnDelete_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Keys.Enter:
                    if (btnEdit.Enabled && dataGridView1.SelectedRows.Count > 0)
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

        private void RebindGridAfterDataChange()
        {
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                PerformSearch(updateStatus: false);
            }
            else
            {
                filteredEntries.Clear();
                RefreshGrid();
            }

            UpdateAvailableActions();
        }

        private void PerformSearch(bool updateStatus = true)
        {
            string searchText = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                filteredEntries.Clear();
                RefreshGrid();
                if (updateStatus)
                {
                    UpdateStatus($"{entries.Count} entrada(s) carregada(s)");
                }
                UpdateAvailableActions();
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
            
            if (!updateStatus)
            {
                UpdateAvailableActions();
                return;
            }

            if (filteredEntries.Count == 0)
            {
                UpdateStatus("Nenhum resultado encontrado");
            }
            else
            {
                UpdateStatus($"{filteredEntries.Count} resultado(s) encontrado(s)");
            }

            UpdateAvailableActions();
        }

        private void lblFilePath_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentFilePath))
            {
                return;
            }

            try
            {
                var directory = Path.GetDirectoryName(currentFilePath);

                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    ShowWarning("A pasta do arquivo não foi encontrada.", "Abrir no Explorer");
                    return;
                }

                string arguments = File.Exists(currentFilePath)
                    ? $"/select,\"{currentFilePath}\""
                    : $"\"{directory}\"";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = arguments,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowError("Não foi possível abrir o Explorer:", "Abrir no Explorer", ex);
            }
        }

        private void DataGridView1_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            var column = dataGridView1.Columns?[e.ColumnIndex];
            if (column == null || column.DataPropertyName != "ConnectionStatus")
            {
                return;
            }

            var cellStyle = e.CellStyle;
            if (cellStyle == null)
            {
                return;
            }

            string status = e.Value?.ToString() ?? string.Empty;

            if (status.Equals("Online", StringComparison.OrdinalIgnoreCase))
            {
                cellStyle.ForeColor = Color.Green;
                cellStyle.BackColor = SystemColors.Window;
            }
            else if (status.Equals("Offline", StringComparison.OrdinalIgnoreCase))
            {
                cellStyle.ForeColor = Color.Red;
                cellStyle.BackColor = Color.MistyRose;
            }
            else
            {
                cellStyle.ForeColor = SystemColors.ControlText;
                cellStyle.BackColor = SystemColors.Window;
            }
        }

        private void DataGridView1_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
            {
                return;
            }

            var row = dataGridView1.Rows[e.RowIndex];
            if (row.DataBoundItem is not TnsEntry entry)
            {
                return;
            }

            bool isOffline = entry.ConnectionStatus?.Equals("Offline", StringComparison.OrdinalIgnoreCase) == true;

            if (isOffline)
            {
                row.DefaultCellStyle.BackColor = Color.MistyRose;
                row.DefaultCellStyle.SelectionBackColor = Color.LightCoral;
            }
            else
            {
                row.DefaultCellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.SelectionBackColor = dataGridView1.DefaultCellStyle.SelectionBackColor;
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
            if (!menuCopy.Enabled)
            {
                return;
            }

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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CancelPendingPingRequests();
            base.OnFormClosing(e);
        }

        private void CancelPendingPingRequests()
        {
            if (pingCancellation == null)
            {
                return;
            }

            try
            {
                pingCancellation.Cancel();
            }
            finally
            {
                pingCancellation.Dispose();
                pingCancellation = null;
            }
        }

        private void InitializeConnectionStatus(IEnumerable<TnsEntry> targetEntries)
        {
            foreach (var entry in targetEntries)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    entry.ConnectionStatus = "Offline";
                    continue;
                }

                if (pendingStatusRefresh.Contains(entry.Name))
                {
                    entry.ConnectionStatus = "Verificando...";
                }
                else if (statusCache.TryGetValue(entry.Name, out var cachedStatus))
                {
                    entry.ConnectionStatus = cachedStatus;
                }
                else
                {
                    entry.ConnectionStatus = "Verificando...";
                }
            }
        }

        private void StartConnectionStatusRefresh(IEnumerable<TnsEntry> targetEntries, bool forceRefresh)
        {
            var entriesToCheck = targetEntries
                .Where(e => !string.IsNullOrWhiteSpace(e.Name))
                .Where(e => forceRefresh || pendingStatusRefresh.Contains(e.Name) || !statusCache.ContainsKey(e.Name))
                .ToList();

            if (entriesToCheck.Count == 0)
            {
                return;
            }

            CancelPendingPingRequests();
            pingCancellation = new CancellationTokenSource();
            var token = pingCancellation.Token;

            _ = Task.Run(() => UpdateConnectionStatusesAsync(entriesToCheck, token), token);
        }

        private void MarkEntryForStatusRefresh(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            statusCache.Remove(name);
            pendingStatusRefresh.Add(name);
        }

        private async Task UpdateConnectionStatusesAsync(IReadOnlyList<TnsEntry> entriesToCheck, CancellationToken token)
        {
            if (entriesToCheck.Count == 0)
            {
                return;
            }

            int maxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount);

            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
            var tasks = new List<Task>();

            foreach (var entry in entriesToCheck)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                tasks.Add(Task.Run(async () =>
                {
                    bool lockTaken = false;

                    try
                    {
                        await semaphore.WaitAsync(token).ConfigureAwait(false);
                        lockTaken = true;

                        var status = await CheckConnectionStatusSafelyAsync(entry, token).ConfigureAwait(false);

                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        try
                        {
                            BeginInvoke(new Action(() =>
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return;
                                }

                                entry.ConnectionStatus = status;
                                statusCache[entry.Name] = status;
                                pendingStatusRefresh.Remove(entry.Name);
                                dataGridView1.Refresh();
                            }));
                        }
                        catch (InvalidOperationException)
                        {
                            // Form disposed
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancelamento solicitado, apenas encerra
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            semaphore.Release();
                        }
                    }
                }, token));
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Cancelamento já tratado individualmente
            }
        }

        private async Task<string> CheckConnectionStatusSafelyAsync(TnsEntry entry, CancellationToken token)
        {
            try
            {
                return await CheckConnectionStatusAsync(entry.Name, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return "Offline";
            }
        }

        private async Task<string> CheckConnectionStatusAsync(string alias, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return "Offline";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "tnsping",
                Arguments = alias,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = startInfo };

                if (!process.Start())
                {
                    return "Offline";
                }

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

                try
                {
                    await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(true);
                        }
                    }
                    catch
                    {
                        // Ignora falhas ao finalizar o processo
                    }

                    if (token.IsCancellationRequested)
                    {
                        throw;
                    }

                    return "Offline";
                }

                var output = await outputTask.ConfigureAwait(false);
                await errorTask.ConfigureAwait(false);

                if (process.ExitCode == 0 && output.IndexOf("OK", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Online";
                }

                return "Offline";
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Win32Exception)
            {
                return "Offline";
            }
            catch
            {
                return "Offline";
            }
        }
    }
}
