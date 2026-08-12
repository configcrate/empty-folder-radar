using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConfigCrate.EmptyFolderRadar
{
    internal sealed class EmptyFolderScanner
    {
        private sealed class NodeResult
        {
            public bool HasContent;
            public int EmptyFolderCount;
            public List<EmptyFolderItem> Candidates = new List<EmptyFolderItem>();
        }

        public ScanResult Scan(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("A folder is required.", "rootPath");
            string root = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar);
            if (!Directory.Exists(root)) throw new DirectoryNotFoundException("The selected folder no longer exists.");

            ScanResult result = new ScanResult { RootPath = root };
            DirectoryInfo rootInfo = new DirectoryInfo(root);
            foreach (DirectoryInfo child in SafeDirectories(rootInfo, result))
            {
                NodeResult node = Inspect(child, result);
                if (!node.HasContent)
                {
                    result.Items.Add(new EmptyFolderItem
                    {
                        Path = child.FullName,
                        EmptyDescendantCount = Math.Max(0, node.EmptyFolderCount - 1),
                        LastWriteTime = SafeLastWriteTime(child),
                        Selected = true
                    });
                    result.EmptyFoldersRepresented += node.EmptyFolderCount;
                }
                else
                {
                    result.Items.AddRange(node.Candidates);
                    result.EmptyFoldersRepresented += node.Candidates.Sum(item => item.EmptyDescendantCount + 1);
                }
            }

            // Files directly in the selected root matter to the summary, but the
            // root itself is never offered for removal.
            result.FilesSeen += SafeFiles(rootInfo, result).Count();
            result.Items.Sort(delegate(EmptyFolderItem left, EmptyFolderItem right)
            {
                return string.Compare(left.Path, right.Path, StringComparison.CurrentCultureIgnoreCase);
            });
            return result;
        }

        private static NodeResult Inspect(DirectoryInfo folder, ScanResult scan)
        {
            NodeResult node = new NodeResult();
            scan.FoldersInspected++;

            if ((folder.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                node.HasContent = true;
                scan.Warnings.Add("Skipped link or junction: " + folder.FullName);
                return node;
            }

            FileInfo[] files;
            DirectoryInfo[] directories;
            try
            {
                files = folder.GetFiles();
                directories = folder.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {
                node.HasContent = true;
                scan.Warnings.Add("Access denied: " + folder.FullName);
                return node;
            }
            catch (IOException)
            {
                node.HasContent = true;
                scan.Warnings.Add("Could not read: " + folder.FullName);
                return node;
            }
            scan.FilesSeen += files.Length;
            node.HasContent = files.Length > 0;
            int emptyChildren = 0;

            foreach (DirectoryInfo child in directories)
            {
                NodeResult childResult = Inspect(child, scan);
                if (childResult.HasContent)
                {
                    node.HasContent = true;
                    node.Candidates.AddRange(childResult.Candidates);
                }
                else
                {
                    emptyChildren += childResult.EmptyFolderCount;
                    node.Candidates.Add(new EmptyFolderItem
                    {
                        Path = child.FullName,
                        EmptyDescendantCount = Math.Max(0, childResult.EmptyFolderCount - 1),
                        LastWriteTime = SafeLastWriteTime(child),
                        Selected = true
                    });
                }
            }

            if (!node.HasContent)
            {
                node.EmptyFolderCount = 1 + emptyChildren;
                node.Candidates.Clear();
            }
            return node;
        }

        public bool IsStillRecursivelyEmpty(string path)
        {
            try
            {
                DirectoryInfo folder = new DirectoryInfo(path);
                if (!folder.Exists || (folder.Attributes & FileAttributes.ReparsePoint) != 0) return false;
                if (folder.GetFiles().Length > 0) return false;
                foreach (DirectoryInfo child in folder.GetDirectories())
                {
                    if (!IsStillRecursivelyEmpty(child.FullName)) return false;
                }
                return true;
            }
            catch { return false; }
        }

        private static IEnumerable<DirectoryInfo> SafeDirectories(DirectoryInfo folder, ScanResult result)
        {
            try { return folder.GetDirectories(); }
            catch (UnauthorizedAccessException) { result.Warnings.Add("Access denied: " + folder.FullName); }
            catch (IOException) { result.Warnings.Add("Could not read: " + folder.FullName); }
            return new DirectoryInfo[0];
        }

        private static IEnumerable<FileInfo> SafeFiles(DirectoryInfo folder, ScanResult result)
        {
            try { return folder.GetFiles(); }
            catch (UnauthorizedAccessException) { result.Warnings.Add("Access denied: " + folder.FullName); }
            catch (IOException) { result.Warnings.Add("Could not read: " + folder.FullName); }
            return new FileInfo[0];
        }

        private static DateTime SafeLastWriteTime(DirectoryInfo folder)
        {
            try { return folder.LastWriteTime; } catch { return DateTime.MinValue; }
        }
    }
}
