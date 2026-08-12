using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigCrate.EmptyFolderRadar
{
    internal static class SafetyGuard
    {
        public static bool IsUnsafeScanRoot(string path)
        {
            string full = Normalize(path);
            if (string.IsNullOrEmpty(full)) return true;
            DirectoryInfo info = new DirectoryInfo(full);
            if (info.Parent == null) return true;
            foreach (string protectedPath in ProtectedPaths())
            {
                if (string.Equals(full, Normalize(protectedPath), StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool CanRecycle(string root, string candidate)
        {
            string normalizedRoot = Normalize(root) + Path.DirectorySeparatorChar;
            string normalizedCandidate = Normalize(candidate);
            return !string.IsNullOrEmpty(normalizedCandidate) &&
                   normalizedCandidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(Normalize(root), normalizedCandidate, StringComparison.OrdinalIgnoreCase) &&
                   Directory.Exists(normalizedCandidate) &&
                   new DirectoryInfo(normalizedCandidate).Parent != null;
        }

        private static IEnumerable<string> ProtectedPaths()
        {
            yield return Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            yield return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            yield return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            yield return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            yield return Environment.GetFolderPath(Environment.SpecialFolder.System);
        }

        private static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            try { return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar); }
            catch { return string.Empty; }
        }
    }
}
