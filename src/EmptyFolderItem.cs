using System;

namespace ConfigCrate.EmptyFolderRadar
{
    internal sealed class EmptyFolderItem
    {
        public string Path { get; set; }
        public int EmptyDescendantCount { get; set; }
        public DateTime LastWriteTime { get; set; }
        public bool Selected { get; set; }
    }
}
