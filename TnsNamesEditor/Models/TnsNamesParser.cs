using System.Text;
using System.Text.RegularExpressions;

namespace TnsNamesEditor.Models
{
    public class TnsEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Sid { get; set; } = string.Empty;
        public string Protocol { get; set; } = "TCP";
        public string Server { get; set; } = string.Empty;
        public string RawContent { get; set; } = string.Empty;

        public string ToTnsFormat()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Name} =");
            sb.AppendLine("  (DESCRIPTION =");
            
            if (!string.IsNullOrEmpty(Server))
            {
                // Formato sem ADDRESS_LIST quando tem SERVER
                sb.AppendLine($"    (ADDRESS = (PROTOCOL = {Protocol})(HOST = {Host})(PORT = {Port}))");
            }
            else
            {
                // Formato com ADDRESS_LIST quando não tem SERVER
                sb.AppendLine("    (ADDRESS_LIST =");
                sb.AppendLine($"      (ADDRESS = (PROTOCOL = {Protocol})(HOST = {Host})(PORT = {Port}))");
                sb.AppendLine("    )");
            }
            
            sb.AppendLine("    (CONNECT_DATA =");
            
            if (!string.IsNullOrEmpty(Server))
            {
                sb.AppendLine($"      (SERVER = {Server})");
            }
            
            if (!string.IsNullOrEmpty(ServiceName))
            {
                sb.AppendLine($"      (SERVICE_NAME = {ServiceName})");
            }
            
            if (!string.IsNullOrEmpty(Sid))
            {
                sb.AppendLine($"      (SID = {Sid})");
            }
            
            sb.AppendLine("    )");
            sb.AppendLine("  )");
            
            return sb.ToString();
        }

        public override string ToString()
        {
            return $"{Name} - {Host}:{Port}";
        }
    }

    public class TnsNamesParser
    {
        public static List<TnsEntry> ParseFile(string filePath)
        {
            var entries = new List<TnsEntry>();
            
            if (!File.Exists(filePath))
            {
                return entries;
            }

            var content = File.ReadAllText(filePath);
            
            // Remove comentários e linhas vazias para facilitar o parse
            var lines = content.Split('\n');
            var cleanedLines = new List<string>();
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("#"))
                {
                    cleanedLines.Add(line);
                }
            }
            
            content = string.Join("\n", cleanedLines);
            
            // Padrão melhorado: captura nome e depois todo o bloco DESCRIPTION com balanceamento de parênteses
            var entryPattern = @"^([A-Z0-9_]+)\s*=\s*\(DESCRIPTION\s*=";
            var matches = Regex.Matches(content, entryPattern, 
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                // Encontra o início da entrada
                int startIndex = match.Index;
                int pos = match.Index + match.Length;
                int parenCount = 2; // Já temos ( de DESCRIPTION e ( inicial
                
                // Conta parênteses para encontrar o fim da entrada
                while (pos < content.Length && parenCount > 0)
                {
                    if (content[pos] == '(')
                        parenCount++;
                    else if (content[pos] == ')')
                        parenCount--;
                    pos++;
                }
                
                // Extrai o conteúdo completo da entrada
                string entryText = content.Substring(startIndex, pos - startIndex);
                
                var entry = new TnsEntry
                {
                    RawContent = entryText
                };

                // Extrai o nome da entrada
                var nameMatch = Regex.Match(entryText, @"^([A-Z0-9_]+)\s*=", RegexOptions.IgnoreCase);
                if (nameMatch.Success)
                {
                    entry.Name = nameMatch.Groups[1].Value.Trim();
                }

                // Extrai valores usando padrão simples: PROPRIEDADE = valor
                entry.Host = ExtractValue(entryText, "HOST");
                entry.Port = ExtractValue(entryText, "PORT");
                entry.Protocol = ExtractValue(entryText, "PROTOCOL");
                entry.ServiceName = ExtractValue(entryText, "SERVICE_NAME");
                entry.Sid = ExtractValue(entryText, "SID");
                entry.Server = ExtractValue(entryText, "SERVER");
                
                // Define valores padrão se estiverem vazios
                if (string.IsNullOrEmpty(entry.Protocol))
                    entry.Protocol = "TCP";

                entries.Add(entry);
            }

            return entries;
        }

        private static string ExtractValue(string text, string propertyName)
        {
            // Padrão simples: PROPRIEDADE = <qualquer coisa até )>
            var pattern = $@"{propertyName}\s*=\s*([^\)]+)\)";
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            
            return string.Empty;
        }

        public static void SaveToFile(string filePath, List<TnsEntry> entries)
        {
            var sb = new StringBuilder();
            
            // Adiciona cabeçalho padrão
            sb.AppendLine("# tnsnames.ora Network Configuration File");
            sb.AppendLine("# Generated by TNS Names Editor");
            sb.AppendLine();
            sb.AppendLine();

            foreach (var entry in entries)
            {
                sb.Append(entry.ToTnsFormat());
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
