using System.Collections.Generic;

namespace ConfigCrate.EmptyFolderRadar
{
    internal sealed class ScanResult
    {
        public ScanResult()
        {
            Items = new List<EmptyFolderItem>();
            Warnings = new List<string>();
        }

        public string RootPath { get; set; }
        public int FoldersInspected { get; set; }
        public int FilesSeen { get; set; }
        public int EmptyFoldersRepresented { get; set; }
        public List<EmptyFolderItem> Items { get; private set; }
        public List<string> Warnings { get; private set; }
    }
}
