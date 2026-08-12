using Microsoft.VisualBasic.FileIO;

namespace ConfigCrate.EmptyFolderRadar
{
    internal sealed class RecycleService
    {
        public void RecycleDirectory(string path)
        {
            FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs,
                RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
        }
    }
}
