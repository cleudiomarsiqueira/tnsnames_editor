using TnsNamesEditor.Services;

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
                    MessageService.ShowInfo("Nenhum texto encontrado na área de transferência.", "Aviso");
                }
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Erro ao colar:", "Erro", ex);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTnsContent.Text))
            {
                MessageService.ShowValidation("Por favor, digite ou cole o conteúdo da entrada TNS.");
                txtTnsContent.Focus();
                return;
            }

            TnsContent = txtTnsContent.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
