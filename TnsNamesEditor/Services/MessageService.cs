namespace TnsNamesEditor.Services
{
    /// <summary>
    /// Serviço centralizado para exibição de mensagens ao usuário
    /// </summary>
    public static class MessageService
    {
        /// <summary>
        /// Exibe uma mensagem de erro
        /// </summary>
        public static void ShowError(string message, string title = "Erro", Exception? exception = null)
        {
            string fullMessage = exception != null 
                ? $"{message}\n\n{exception.Message}" 
                : message;

            MessageBox.Show(
                fullMessage,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        /// <summary>
        /// Exibe uma mensagem de aviso
        /// </summary>
        public static void ShowWarning(string message, string title = "Atenção")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Exibe uma mensagem de sucesso
        /// </summary>
        public static void ShowSuccess(string message, string title = "Sucesso")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// Exibe uma mensagem de informação
        /// </summary>
        public static void ShowInfo(string message, string title = "Informação")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// Exibe uma mensagem de confirmação e retorna true se o usuário confirmar
        /// </summary>
        public static bool ShowConfirmation(string message, string title = "Confirmação")
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            return result == DialogResult.Yes;
        }

        /// <summary>
        /// Exibe uma mensagem de validação (warning específico para validações de formulário)
        /// </summary>
        public static void ShowValidation(string message, string title = "Validação")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
