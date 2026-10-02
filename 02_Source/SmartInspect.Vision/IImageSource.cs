namespace SmartInspect.Vision;

// File-backed acquisition boundary; hardware adapters may export a captured frame here.
public interface IImageSource
{
    string GetNextImagePath();
}

public sealed class SingleImageSource(string path) : IImageSource
{
    public string GetNextImagePath()
    {
        if (!File.Exists(path)) throw new FileNotFoundException("图片文件不存在。", path);
        return path;
    }
}

public sealed class FolderImageSource : IImageSource
{
    private readonly string[] files;
    private int next;
    public bool Loop { get; }

    public FolderImageSource(string folder, bool loop = false)
    {
        Loop = loop;
        string[] extensions = [".png", ".jpg", ".jpeg", ".bmp"];
        files = Directory.GetFiles(folder).Where(p => extensions.Contains(Path.GetExtension(p), StringComparer.OrdinalIgnoreCase))
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public string GetNextImagePath()
    {
        if (files.Length == 0 || (next >= files.Length && !Loop)) throw new EndOfStreamException("图像源耗尽，请重新选择文件夹。 ");
        if (next >= files.Length) next = 0;
        return files[next++];
    }
}
