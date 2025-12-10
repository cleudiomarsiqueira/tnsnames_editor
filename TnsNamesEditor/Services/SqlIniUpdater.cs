using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TnsNamesEditor.Models;

namespace TnsNamesEditor.Services
{
    public class SqlIniUpdater
    {
        private readonly string sqlIniPath;

        public SqlIniUpdater(string? customPath = null)
        {
            sqlIniPath = customPath ?? @"C:\C5Client\SQL.ini";
        }

        public SqlIniUpdateResult UpdateRemoteDbNames(IEnumerable<TnsEntry> tnsEntries)
        {
            if (!File.Exists(sqlIniPath))
            {
                return SqlIniUpdateResult.NotFound(sqlIniPath);
            }

            var distinctNames = tnsEntries
                .Where(e => !string.IsNullOrWhiteSpace(e.Name))
                .Select(e => e.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var lines = ReadAllLinesPreservingEncoding(sqlIniPath, out var encoding);
            int sectionStart = FindOraGtwySectionStart(lines);

            if (sectionStart < 0)
            {
                AppendNewSection(lines, distinctNames);
                WriteAllLinesPreservingEncoding(sqlIniPath, lines, encoding);
                return SqlIniUpdateResult.Updated(sqlIniPath, distinctNames.Count);
            }

            int sectionEnd = FindSectionEnd(lines, sectionStart);
            var remainingSectionLines = ExtractNonRemoteLines(lines, sectionStart, sectionEnd);

            var updatedLines = new List<string>();
            updatedLines.AddRange(lines.Take(sectionStart + 1));
            updatedLines.AddRange(CreateRemoteDbLines(distinctNames));

            if (remainingSectionLines.Count > 0)
            {
                if (updatedLines.Count > 0 && remainingSectionLines[0].Length > 0 && updatedLines.Last().Length > 0)
                {
                    updatedLines.Add(string.Empty);
                }

                updatedLines.AddRange(remainingSectionLines);
            }

            updatedLines.AddRange(lines.Skip(sectionEnd));

            WriteAllLinesPreservingEncoding(sqlIniPath, updatedLines, encoding);
            return SqlIniUpdateResult.Updated(sqlIniPath, distinctNames.Count);
        }

        private static int FindOraGtwySectionStart(IReadOnlyList<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().Equals("[OraGtwy]", StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindSectionEnd(IReadOnlyList<string> lines, int sectionStart)
        {
            for (int i = sectionStart + 1; i < lines.Count; i++)
            {
                if (IsSectionHeader(lines[i]))
                {
                    return i;
                }
            }

            return lines.Count;
        }

        private static bool IsSectionHeader(string line)
        {
            var trimmed = line.Trim();
            return trimmed.StartsWith("[") && trimmed.EndsWith("]");
        }

        private static List<string> ExtractNonRemoteLines(IReadOnlyList<string> lines, int sectionStart, int sectionEnd)
        {
            var nonRemoteLines = new List<string>();

            for (int i = sectionStart + 1; i < sectionEnd; i++)
            {
                var trimmed = lines[i].Trim();
                if (!trimmed.StartsWith("RemoteDBName=", StringComparison.OrdinalIgnoreCase))
                {
                    nonRemoteLines.Add(lines[i]);
                }
            }

            return nonRemoteLines;
        }

        private static void AppendNewSection(ICollection<string> lines, IReadOnlyCollection<string> names)
        {
            if (lines.Count > 0 && lines.Last().Length > 0)
            {
                lines.Add(string.Empty);
            }

            lines.Add("[OraGtwy]");
            foreach (var name in names)
            {
                lines.Add($"RemoteDBName={name},@{name}");
            }
        }

        private static IEnumerable<string> CreateRemoteDbLines(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                yield return $"RemoteDBName={name},@{name}";
            }
        }

        private static List<string> ReadAllLinesPreservingEncoding(string path, out Encoding encoding)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream, Encoding.Default, detectEncodingFromByteOrderMarks: true);
            var lines = new List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            encoding = reader.CurrentEncoding;
            return lines;
        }

        private static void WriteAllLinesPreservingEncoding(string path, IEnumerable<string> lines, Encoding encoding)
        {
            using var writer = new StreamWriter(path, false, encoding);
            foreach (var line in lines)
            {
                writer.WriteLine(line);
            }
        }
    }

    public readonly record struct SqlIniUpdateResult(bool Success, string Message, bool FileMissing)
    {
        public static SqlIniUpdateResult Updated(string path, int count)
        {
            var suffix = count == 1 ? "1 entrada" : $"{count} entradas";
            return new SqlIniUpdateResult(true, $"SQL.ini atualizado ({suffix}).", false);
        }

        public static SqlIniUpdateResult NotFound(string path)
        {
            return new SqlIniUpdateResult(false, $"SQL.ini não encontrado em '{path}'.", true);
        }
    }
}
