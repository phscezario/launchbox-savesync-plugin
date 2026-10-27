using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SaveSyncPlugin.Core.Models;

namespace SaveSyncPlugin.Core.Sync
{
    public enum RobocopyOutcome
    {
        Skipped,
        Success,
        Failed
    }

    public sealed class RobocopyResult
    {
        public RobocopyOutcome Outcome { get; set; }
        public int ExitCode { get; set; } = -1;
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Arguments { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            var suffix = string.IsNullOrWhiteSpace(Message) ? string.Empty : " | " + Message;
            return $"{Outcome} ({ExitCode}): {Source} -> {Destination}{suffix}";
        }
    }

    /// <summary>
    /// Runs robocopy for save sync.
    ///
    /// The sync is NON-DESTRUCTIVE by default:
    /// <list type="bullet">
    /// <item><c>/E</c> copies subdirectories;</item>
    /// <item><c>/XO</c> never overwrites a file that is newer on the destination side;</item>
    /// <item>no <c>/PURGE</c> — a file that exists on only one side is never deleted.</item>
    /// </list>
    ///
    /// Deletion is opt-in through <see cref="SaveSyncSettings.DeleteExtraneous"/> (mirror
    /// mode). Even then it is refused when the source is empty or the destination is not
    /// an absolute path: with <c>/PURGE</c> an empty (not yet synced, unmounted, offline)
    /// source would wipe the other side, which is exactly how saves get lost.
    /// </summary>
    public static class RobocopyRunner
    {
        public static RobocopyResult Run(
            string source,
            string destination,
            string fileFilter = null,
            SaveSyncSettings settings = null,
            IProgress<string> progress = null)
        {
            settings = settings ?? new SaveSyncSettings();

            var result = new RobocopyResult
            {
                Source = source,
                Destination = destination
            };

            try
            {
                if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
                    return Skip(result, "origem/destino vazio");

                if (!Directory.Exists(source))
                    return Skip(result, "origem inexistente");

                if (settings.DeleteExtraneous && !MirrorIsSafe(result, source, destination, fileFilter))
                    return result;

                var args = BuildArguments(source, destination, fileFilter, settings);
                result.Arguments = args;

                progress?.Report($"robocopy {source} -> {destination}");

                var psi = new ProcessStartInfo
                {
                    FileName = "robocopy.exe",
                    Arguments = args,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var proc = Process.Start(psi))
                {
                    if (proc == null)
                        return Fail(result, "nao foi possivel iniciar robocopy.exe");

                    // Drain the pipes so a chatty robocopy cannot block on a full buffer.
                    proc.OutputDataReceived += (_, __) => { };
                    proc.ErrorDataReceived += (_, __) => { };
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();

                    if (!proc.WaitForExit(TimeoutMilliseconds(settings)))
                    {
                        try { proc.Kill(); } catch { }
                        return Fail(result, "timeout");
                    }

                    result.ExitCode = proc.ExitCode;
                }

                // robocopy: 0-7 are success (bitmask), >= 8 means at least one file failed.
                if (result.ExitCode >= 8)
                    return Fail(result, $"robocopy retornou {result.ExitCode}");

                result.Outcome = RobocopyOutcome.Success;
                SyncLog.Write($"OK      {args}");
                return result;
            }
            catch (Exception ex)
            {
                return Fail(result, ex.Message);
            }
        }

        /// <summary>
        /// Builds the robocopy command line. <c>/PURGE</c> is only added when
        /// <see cref="SaveSyncSettings.DeleteExtraneous"/> is enabled.
        /// </summary>
        public static string BuildArguments(
            string source,
            string destination,
            string fileFilter,
            SaveSyncSettings settings)
        {
            settings = settings ?? new SaveSyncSettings();

            var parts = new List<string>
            {
                Quote(source),
                Quote(destination)
            };

            if (!string.IsNullOrWhiteSpace(fileFilter))
                parts.Add(Quote(fileFilter));

            parts.Add("/E");
            parts.Add("/XO");

            if (settings.DeleteExtraneous)
                parts.Add("/PURGE");

            parts.Add("/Z");
            parts.Add($"/MT:{Clamp(settings.RobocopyThreads, 1, 128, 8)}");
            parts.Add($"/R:{Clamp(settings.RobocopyRetries, 0, 30, 3)}");
            parts.Add($"/W:{Clamp(settings.RobocopyWaitSeconds, 1, 60, 10)}");

            // Keep the child process quiet; the log records the command + exit code.
            parts.Add("/NFL");
            parts.Add("/NDL");
            parts.Add("/NJH");
            parts.Add("/NJS");
            parts.Add("/NP");

            return string.Join(" ", parts);
        }

        private static bool MirrorIsSafe(RobocopyResult result, string source, string destination, string fileFilter)
        {
            if (!Path.IsPathRooted(destination))
            {
                Skip(result, "modo espelho cancelado: destino nao e absoluto");
                return false;
            }

            var destFull = Path.GetFullPath(destination);
            if (string.Equals(destFull, Path.GetPathRoot(destFull), StringComparison.OrdinalIgnoreCase))
            {
                Skip(result, "modo espelho cancelado: destino e a raiz de um volume");
                return false;
            }

            if (!HasMatchingContent(source, fileFilter))
            {
                Skip(result, "modo espelho cancelado: origem vazia (nada seria copiado, tudo seria apagado)");
                return false;
            }

            return true;
        }

        private static bool HasMatchingContent(string source, string fileFilter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileFilter))
                    return Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).Any();

                var direct = Path.Combine(source, fileFilter);
                if (File.Exists(direct))
                    return true;

                var pattern = Path.GetFileName(fileFilter);
                if (pattern.IndexOf('*') < 0 && pattern.IndexOf('?') < 0)
                    return false;

                return Directory.EnumerateFiles(source, pattern, SearchOption.AllDirectories).Any();
            }
            catch
            {
                return false;
            }
        }

        private static int TimeoutMilliseconds(SaveSyncSettings settings)
        {
            var retries = Clamp(settings.RobocopyRetries, 0, 30, 3);
            var wait = Clamp(settings.RobocopyWaitSeconds, 1, 60, 10);
            return 300_000 + (retries * wait * 1_000);
        }

        private static int Clamp(int value, int min, int max, int fallback)
        {
            if (value < min || value > max)
                return fallback;
            return value;
        }

        private static string Quote(string value) => "\"" + value + "\"";

        private static RobocopyResult Skip(RobocopyResult result, string reason)
        {
            result.Outcome = RobocopyOutcome.Skipped;
            result.Message = reason;
            SyncLog.Write($"SKIP    {result.Source} -> {result.Destination} | {reason}");
            return result;
        }

        private static RobocopyResult Fail(RobocopyResult result, string reason)
        {
            result.Outcome = RobocopyOutcome.Failed;
            result.Message = reason;
            SyncLog.Write($"FAIL    {result.Source} -> {result.Destination} | {reason} | {result.Arguments}");
            return result;
        }
    }
}
