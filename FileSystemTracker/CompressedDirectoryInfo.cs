namespace FileSystemTracker;

/// <summary>
/// This class serve to store differentes attributes from
/// 
/// </summary>
public class CompressedDirectoryInfo
{
    /// <summary>
    /// The relative path to the files where came this compressed file
    /// </summary>
    /// <value></value>
    public required string ExternalPath { get; set; }

    /// <summary>
    /// The path of this compressed file
    /// </summary>
    /// <value></value>
    public required string CompressedPath { get; set; }


}
