namespace FileSystemTracker;

class DirectoryCompresser
{
    private string _path;

    public DirectoryCompresser(string path, uint deep = 0)
    {
        _path = path;
    }
}
