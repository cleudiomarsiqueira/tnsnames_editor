using TnsNamesEditor.Models;
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
            
            // Seleciona o protocolo
            cmbProtocol.SelectedItem = entry.Protocol;
            if (cmbProtocol.SelectedIndex < 0)
            {
                cmbProtocol.SelectedIndex = 0; // TCP por padrão
            }

            // Adiciona eventos para atualização automática do preview
            txtName.TextChanged += (s, ev) => UpdatePreview();
            txtHost.TextChanged += (s, ev) => UpdatePreview();
            txtPort.TextChanged += (s, ev) => UpdatePreview();
            txtServiceName.TextChanged += (s, ev) => UpdatePreview();
            txtSid.TextChanged += (s, ev) => UpdatePreview();
            txtServer.TextChanged += (s, ev) => UpdatePreview();
            cmbProtocol.SelectedIndexChanged += (s, ev) => UpdatePreview();

            UpdatePreview();
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
                            MessageBox.Show("Dados importados com sucesso!", 
                                "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Não foi possível interpretar o texto TNS.\n\n" +
                                "Certifique-se de que está no formato:\n\n" +
                                "NOME =\n" +
                                "  (DESCRIPTION =\n" +
                                "    (ADDRESS = (PROTOCOL = TCP)(HOST = ...)(PORT = ...))\n" +
                                "    (CONNECT_DATA =\n" +
                                "      (SERVICE_NAME = ...)\n" +
                                "    )\n" +
                                "  )", 
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao colar: {ex.Message}", 
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                cmbProtocol.SelectedItem = protocol;
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

            // Retorna true se conseguiu extrair pelo menos nome e host
            return nameMatch.Success && hostMatch.Success;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // Validações
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("O nome da entrada é obrigatório.", "Validação", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            // Verifica duplicata (apenas ao adicionar ou se o nome foi alterado)
            string newName = txtName.Text.Trim().ToUpper();
            if (!newName.Equals(originalEntryName, StringComparison.OrdinalIgnoreCase))
            {
                if (allEntries.Any(e => e.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show(
                        $"Já existe uma entrada com o nome '{newName}'.\n\nEscolha outro nome ou cancele para não alterar.",
                        "Nome Duplicado", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageBox.Show("O host é obrigatório.", "Validação", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHost.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPort.Text))
            {
                MessageBox.Show("A porta é obrigatória.", "Validação", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPort.Focus();
                return;
            }

            if (!int.TryParse(txtPort.Text, out _))
            {
                MessageBox.Show("A porta deve ser um número válido.", "Validação", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPort.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtServiceName.Text) && 
                string.IsNullOrWhiteSpace(txtSid.Text))
            {
                MessageBox.Show("É necessário informar pelo menos Service Name ou SID.", 
                    "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServiceName.Focus();
                return;
            }

            // Atualiza o objeto entry
            entry.Name = txtName.Text.Trim().ToUpper();
            entry.Host = txtHost.Text.Trim();
            entry.Port = txtPort.Text.Trim();
            entry.ServiceName = txtServiceName.Text.Trim();
            entry.Sid = txtSid.Text.Trim();
            entry.Server = txtServer.Text.Trim();
            entry.Protocol = cmbProtocol.SelectedItem?.ToString() ?? "TCP";

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
                Sid = txtSid.Text.Trim(),
                Server = txtServer.Text.Trim(),
                Protocol = cmbProtocol.SelectedItem?.ToString() ?? "TCP"
            };

            txtPreview.Text = tempEntry.ToTnsFormat();
        }
    }
}
