using System;
using System.IO;
using System.Linq;
using ConfigCrate.EmptyFolderRadar;

internal static class ScannerTests
{
    private static int failed;
    private static void Assert(bool condition, string name) { Console.WriteLine((condition ? "PASS: " : "FAIL: ") + name); if (!condition) failed++; }
    private static int Main()
    {
        string root = Path.Combine(Path.GetTempPath(), "empty-folder-radar-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "empty-leaf"));
            Directory.CreateDirectory(Path.Combine(root, "empty-tree", "a", "b"));
            Directory.CreateDirectory(Path.Combine(root, "mixed", "empty-child"));
            Directory.CreateDirectory(Path.Combine(root, "mixed", "content"));
            File.WriteAllText(Path.Combine(root, "mixed", "content", "keep.txt"), "keep");
            Directory.CreateDirectory(Path.Combine(root, "file-folder")); File.WriteAllText(Path.Combine(root, "file-folder", ".gitkeep"), "");

            EmptyFolderScanner scanner = new EmptyFolderScanner(); ScanResult result = scanner.Scan(root);
            Assert(result.Items.Any(item => item.Path.EndsWith("empty-leaf")), "finds a directly empty folder");
            EmptyFolderItem tree = result.Items.FirstOrDefault(item => item.Path.EndsWith("empty-tree"));
            Assert(tree != null && tree.EmptyDescendantCount == 2, "collapses an empty directory tree into one safe branch");
            Assert(result.Items.Any(item => item.Path.EndsWith("empty-child")), "finds an empty sibling inside a non-empty tree");
            Assert(!result.Items.Any(item => item.Path.EndsWith("file-folder")), "a zero-byte file still makes its folder non-empty");
            Assert(!result.Items.Any(item => item.Path == root), "never offers the selected root for deletion");
            Assert(scanner.IsStillRecursivelyEmpty(Path.Combine(root, "empty-tree")), "revalidates an unchanged empty tree before cleanup");
            Assert(!scanner.IsStillRecursivelyEmpty(Path.Combine(root, "file-folder")), "revalidation refuses a folder that contains a file");
            string changed = Path.Combine(root, "empty-leaf"); File.WriteAllText(Path.Combine(changed, "arrived-after-scan.txt"), "new");
            Assert(!scanner.IsStillRecursivelyEmpty(changed), "refuses cleanup when a file appears after scanning");
            Assert(SafetyGuard.CanRecycle(root, Path.Combine(root, "empty-leaf")), "allows a descendant inside the scan root");
            Assert(!SafetyGuard.CanRecycle(root, root), "refuses to recycle the scan root itself");
            Assert(SafetyGuard.IsUnsafeScanRoot(Path.GetPathRoot(root)), "refuses drive-root scans");
        }
        finally { Directory.Delete(root, true); }
        Console.WriteLine(failed == 0 ? "All tests passed." : failed + " test(s) failed."); return failed == 0 ? 0 : 1;
    }
}
