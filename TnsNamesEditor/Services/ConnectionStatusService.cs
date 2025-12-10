using System.ComponentModel;
using System.Diagnostics;
using TnsNamesEditor.Models;

namespace TnsNamesEditor.Services
{
    public class ConnectionStatusService : IDisposable
    {
        private readonly Dictionary<string, string> statusCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> pendingStatusRefresh = new(StringComparer.OrdinalIgnoreCase);
        private CancellationTokenSource? cancellationTokenSource;
        private readonly int maxParallelChecks;
        private readonly Action? onStatusUpdated;

        public ConnectionStatusService(int maxParallelChecks = 5, Action? onStatusUpdated = null)
        {
            this.maxParallelChecks = maxParallelChecks;
            this.onStatusUpdated = onStatusUpdated;
        }

        public void InitializeConnectionStatus(IEnumerable<TnsEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    entry.ConnectionStatus = "Offline";
                    continue;
                }

                if (statusCache.TryGetValue(entry.Name, out var cachedStatus))
                {
                    entry.ConnectionStatus = cachedStatus;
                }
                else
                {
                    entry.ConnectionStatus = string.Empty;
                }
            }
        }

        public void MarkEntryForStatusRefresh(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            statusCache.Remove(name);
            pendingStatusRefresh.Add(name);
        }

        public void ClearCache(string? name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                statusCache.Clear();
                pendingStatusRefresh.Clear();
            }
            else
            {
                statusCache.Remove(name);
                pendingStatusRefresh.Remove(name);
            }
        }

        public async Task StartConnectionStatusRefreshAsync(IEnumerable<TnsEntry> targetEntries, bool forceRefresh, Action<TnsEntry, string>? onEntryStatusChanged = null)
        {
            var entriesToCheck = targetEntries
                .Where(e => !string.IsNullOrWhiteSpace(e.Name))
                .Where(e => forceRefresh || pendingStatusRefresh.Contains(e.Name) || !statusCache.ContainsKey(e.Name))
                .ToList();

            if (entriesToCheck.Count == 0)
            {
                return;
            }

            // Marca como "Verificando..." apenas as entradas que realmente serão verificadas
            foreach (var entry in entriesToCheck)
            {
                entry.ConnectionStatus = "Verificando...";
            }
            onStatusUpdated?.Invoke();

            CancelPendingChecks();
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            await Task.Run(() => UpdateConnectionStatusesAsync(entriesToCheck, token, onEntryStatusChanged), token);
        }

        public void CancelPendingChecks()
        {
            if (cancellationTokenSource == null)
            {
                return;
            }

            try
            {
                cancellationTokenSource.Cancel();
            }
            catch
            {
                // Ignora erros ao cancelar
            }

            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        private async Task UpdateConnectionStatusesAsync(IReadOnlyList<TnsEntry> entriesToCheck, CancellationToken token, Action<TnsEntry, string>? onEntryStatusChanged)
        {
            if (entriesToCheck.Count == 0)
            {
                return;
            }

            using var semaphore = new SemaphoreSlim(maxParallelChecks, maxParallelChecks);
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

                        entry.ConnectionStatus = status;
                        statusCache[entry.Name] = status;
                        pendingStatusRefresh.Remove(entry.Name);

                        onEntryStatusChanged?.Invoke(entry, status);
                        onStatusUpdated?.Invoke();
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

        public void Dispose()
        {
            CancelPendingChecks();
        }
    }
}
