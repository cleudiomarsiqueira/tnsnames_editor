using TnsNamesEditor.Models;
using TnsNamesEditor.Services;
using System.Text.RegularExpressions;

namespace TnsNamesEditor.Forms
{
    public partial class EditEntryForm : Form
    {
        private TnsEntry entry;
        private List<TnsEntry> allEntries;
        private string originalEntryName;

        public EditEntryForm(TnsEntry tnsEntry, List<TnsEntry>? existingEntries = null, string? originalName = null)
        {
            InitializeComponent();
            entry = tnsEntry;
            allEntries = existingEntries ?? new List<TnsEntry>();
            originalEntryName = originalName ?? tnsEntry.Name;
        }

        private void EditEntryForm_Load(object sender, EventArgs e)
        {
            // Carrega os dados da entrada nos campos
            txtName.Text = entry.Name;
            txtHost.Text = entry.Host;
            txtPort.Text = entry.Port;
            txtServiceName.Text = entry.ServiceName;
            txtSid.Text = entry.Sid;
            txtServer.Text = entry.Server;
            
            // Seleciona o protocolo sem aplicar valores padrão
            cmbProtocol.SelectedIndex = -1;
            if (!string.IsNullOrWhiteSpace(entry.Protocol))
            {
                int protocolIndex = cmbProtocol.FindStringExact(entry.Protocol);
                if (protocolIndex >= 0)
                {
                    cmbProtocol.SelectedIndex = protocolIndex;
                }
            }

            // Adiciona eventos para atualização automática do preview e do estado do botão OK
            txtName.TextChanged += OnEntryFieldChanged;
            txtHost.TextChanged += OnEntryFieldChanged;
            txtPort.TextChanged += OnEntryFieldChanged;
            txtServiceName.TextChanged += OnEntryFieldChanged;
            txtSid.TextChanged += OnEntryFieldChanged;
            txtServer.TextChanged += OnEntryFieldChanged;
            cmbProtocol.SelectedIndexChanged += OnEntryFieldChanged;

            UpdatePreview();
            UpdateOkButtonState();
        }

        private void OnEntryFieldChanged(object? sender, EventArgs e)
        {
            UpdatePreview();
            UpdateOkButtonState();
        }

        private void UpdateOkButtonState()
        {
            bool hasName = !string.IsNullOrWhiteSpace(txtName.Text);
            bool hasHost = !string.IsNullOrWhiteSpace(txtHost.Text);
            bool hasPort = !string.IsNullOrWhiteSpace(txtPort.Text);
            bool portIsValid = int.TryParse(txtPort.Text.Trim(), out _);
            bool hasServiceName = !string.IsNullOrWhiteSpace(txtServiceName.Text);
            bool protocolSelected = cmbProtocol.SelectedItem != null;

            btnOk.Enabled = hasName && hasHost && hasPort && portIsValid && hasServiceName && protocolSelected;
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            try
            {
                // Abre formulário para colar/digitar o texto TNS
                using (var pasteForm = new PasteTnsForm())
                {
                    if (pasteForm.ShowDialog() == DialogResult.OK)
                    {
                        string tnsText = pasteForm.TnsContent;
                        
                        // Parse do texto TNS
                        if (ParseTnsText(tnsText))
                        {
                            MessageService.ShowSuccess("Dados importados com sucesso!");
                        }
                        else
                        {
                            MessageService.ShowError(
                                "Não foi possível interpretar o texto TNS.\n\n" +
                                "Certifique-se de que está no formato:\n\n" +
                                "NOME =\n" +
                                "  (DESCRIPTION =\n" +
                                "    (ADDRESS = (PROTOCOL = TCP)(HOST = ...)(PORT = ...))\n" +
                                "    (CONNECT_DATA =\n" +
                                "      (SERVICE_NAME = ...)\n" +
                                "    )\n" +
                                "  )");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Erro ao colar:", "Erro", ex);
            }
        }

        private bool ParseTnsText(string tnsText)
        {
            if (string.IsNullOrWhiteSpace(tnsText))
                return false;

            // Extrai o nome da entrada
            var nameMatch = Regex.Match(tnsText, @"^([A-Z0-9_]+)\s*=", 
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (nameMatch.Success)
            {
                txtName.Text = nameMatch.Groups[1].Value.Trim();
            }

            // Extrai HOST
            var hostMatch = Regex.Match(tnsText, @"HOST\s*=\s*([^\)]+)\)", 
                RegexOptions.IgnoreCase);
            if (hostMatch.Success)
            {
                txtHost.Text = hostMatch.Groups[1].Value.Trim();
            }

            // Extrai PORT
            var portMatch = Regex.Match(tnsText, @"PORT\s*=\s*(\d+)", 
                RegexOptions.IgnoreCase);
            if (portMatch.Success)
            {
                txtPort.Text = portMatch.Groups[1].Value.Trim();
            }

            // Extrai PROTOCOL
            var protocolMatch = Regex.Match(tnsText, @"PROTOCOL\s*=\s*([^\)]+)\)", 
                RegexOptions.IgnoreCase);
            if (protocolMatch.Success)
            {
                string protocol = protocolMatch.Groups[1].Value.Trim();
                int protocolIndex = cmbProtocol.FindStringExact(protocol);
                if (protocolIndex >= 0)
                {
                    cmbProtocol.SelectedIndex = protocolIndex;
                }
                else
                {
                    cmbProtocol.SelectedIndex = -1;
                }
            }

            // Extrai SERVICE_NAME
            var serviceMatch = Regex.Match(tnsText, @"SERVICE_NAME\s*=\s*([^\)]+)\)", 
                RegexOptions.IgnoreCase);
            if (serviceMatch.Success)
            {
                txtServiceName.Text = serviceMatch.Groups[1].Value.Trim();
            }

            // Extrai SID
            var sidMatch = Regex.Match(tnsText, @"SID\s*=\s*([^\)]+)\)", 
                RegexOptions.IgnoreCase);
            if (sidMatch.Success)
            {
                txtSid.Text = sidMatch.Groups[1].Value.Trim();
            }

            // Extrai SERVER
            var serverMatch = Regex.Match(tnsText, @"SERVER\s*=\s*([^\)]+)\)", 
                RegexOptions.IgnoreCase);
            if (serverMatch.Success)
            {
                txtServer.Text = serverMatch.Groups[1].Value.Trim();
            }

            UpdateOkButtonState();

            // Retorna true se conseguiu extrair pelo menos nome e host
            return nameMatch.Success && hostMatch.Success;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // Validações
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageService.ShowValidation("O nome da entrada é obrigatório.");
                txtName.Focus();
                return;
            }

            // Cria uma entrada temporária para verificar duplicata completa
            var tempEntry = new TnsEntry
            {
                Name = txtName.Text.Trim().ToUpper(),
                Host = txtHost.Text.Trim(),
                Port = txtPort.Text.Trim(),
                ServiceName = txtServiceName.Text.Trim(),
                Sid = txtSid.Text.Trim(),
                Protocol = cmbProtocol.SelectedItem?.ToString() ?? string.Empty,
                Server = txtServer.Text.Trim()
            };

            // Verifica se já existe uma entrada idêntica (todos os campos iguais)
            var identicalEntry = allEntries.FirstOrDefault(e => 
                !e.Name.Equals(originalEntryName, StringComparison.OrdinalIgnoreCase) && 
                e.IsIdenticalTo(tempEntry));
            
            if (identicalEntry != null)
            {
                MessageService.ShowWarning(
                    $"Já existe uma entrada idêntica com o nome '{identicalEntry.Name}'.\n\nTodos os campos são iguais.",
                    "Entrada Duplicada");
                txtName.Focus();
                return;
            }

            // Verifica duplicata de nome (apenas se o nome foi alterado)
            string newName = txtName.Text.Trim().ToUpper();
            if (!newName.Equals(originalEntryName, StringComparison.OrdinalIgnoreCase))
            {
                if (allEntries.Any(e => e.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageService.ShowWarning(
                        $"Já existe uma entrada com o nome '{newName}', mas com dados diferentes.\n\nEscolha outro nome ou cancele para não alterar.",
                        "Nome Duplicado");
                    txtName.Focus();
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageService.ShowValidation("O host é obrigatório.");
                txtHost.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPort.Text))
            {
                MessageService.ShowValidation("A porta é obrigatória.");
                txtPort.Focus();
                return;
            }

            if (!int.TryParse(txtPort.Text, out _))
            {
                MessageService.ShowValidation("A porta deve ser um número válido.");
                txtPort.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtServiceName.Text))
            {
                MessageService.ShowValidation("O Service Name é obrigatório.");
                txtServiceName.Focus();
                return;
            }

            // Atualiza o objeto entry
            entry.Name = txtName.Text.Trim().ToUpper();
            entry.Host = txtHost.Text.Trim();
            entry.Port = txtPort.Text.Trim();
            entry.ServiceName = txtServiceName.Text.Trim();
            entry.Sid = string.IsNullOrWhiteSpace(txtSid.Text) ? string.Empty : txtSid.Text.Trim();
            entry.Server = string.IsNullOrWhiteSpace(txtServer.Text) ? string.Empty : txtServer.Text.Trim();
            entry.Protocol = cmbProtocol.SelectedItem?.ToString() ?? string.Empty;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdatePreview()
        {
            // Cria um objeto temporário para preview
            var tempEntry = new TnsEntry
            {
                Name = txtName.Text.Trim().ToUpper(),
                Host = txtHost.Text.Trim(),
                Port = txtPort.Text.Trim(),
                ServiceName = txtServiceName.Text.Trim(),
                Sid = string.IsNullOrWhiteSpace(txtSid.Text) ? string.Empty : txtSid.Text.Trim(),
                Server = string.IsNullOrWhiteSpace(txtServer.Text) ? string.Empty : txtServer.Text.Trim(),
                Protocol = cmbProtocol.SelectedItem?.ToString() ?? string.Empty
            };

            txtPreview.Text = tempEntry.ToTnsFormat();
        }
    }
}
