namespace TnsNamesEditor.Forms
{
    public partial class PasteTnsForm : Form
    {
        public string TnsContent { get; private set; } = string.Empty;

        public PasteTnsForm()
        {
            InitializeComponent();
        }

        private void PasteTnsForm_Load(object sender, EventArgs e)
        {
            // Tenta colar automaticamente da área de transferência ao abrir
            if (Clipboard.ContainsText())
            {
                txtTnsContent.Text = Clipboard.GetText();
            }

            txtTnsContent.Focus();
        }

        private void btnPasteFromClipboard_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    txtTnsContent.Text = Clipboard.GetText();
                    txtTnsContent.SelectionStart = txtTnsContent.Text.Length;
                    txtTnsContent.ScrollToCaret();
                }
                else
                {
                    MessageBox.Show("Nenhum texto encontrado na área de transferência.", 
                        "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao colar: {ex.Message}", 
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTnsContent.Text))
            {
                MessageBox.Show("Por favor, digite ou cole o conteúdo da entrada TNS.", 
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTnsContent.Focus();
                return;
            }

            TnsContent = txtTnsContent.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
