
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace IncrementalBackupS.Commands;

class BackupCommand : AbstractCommand
{
    public override string Name { get; } = "backup:create";

    public override string Description { get; } = "Create a backup for your files";

    private Option<string> _directoryOption;

    private Option<int> _granularityLevel;

    public BackupCommand()
    {
        _directoryOption = new Option<string>("--directory", "The directory to backup");
        _directoryOption.AddAlias("-d");
        _directoryOption.IsRequired  = true;

        _granularityLevel = new Option<int>("--depth");
        _granularityLevel.AddValidator((validate) => {
            int depth = (int?) validate.GetValueOrDefault() ?? 0;

            if (depth! < 0)
            {
                // Set this ErrorMessage to invalidate this attribute
                validate.ErrorMessage = "-n cannot be less than 0";
            }

        });
        _granularityLevel.AddAlias("-n");
        _granularityLevel.SetDefaultValue(0);

        Options.AddRange(
            new Option[]
            {
                _directoryOption,
                _granularityLevel,
            }
        );
    }

    public override void Action(InvocationContext inv)
    {
        var directoryPath = inv.ParseResult.GetValueForOption(_directoryOption);
        var depth = inv.ParseResult.GetValueForOption(_granularityLevel);

        if (directoryPath is null)
        {
            throw new ArgumentException("Option --directory (-d) cannot be null");
        }

        if (depth == 0)
        {

            try
            {

                ZipFile.CreateFromDirectory(directoryPath, $"{directoryPath}.zip");
                Console.WriteLine("Zip file create succesfully");
            }
            catch (DirectoryNotFoundException e)
            {
                Console.Error.WriteLine("Directory given not found");
                Console.Error.WriteLine($"Complete Trace : {e}");
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("Directory already exists");
                Console.Error.WriteLine($"Complete Trace : {e}");
            }

            return;
        }

        string depthPattern = "";
        for (int i = 0; i < depth; i++)
        {
            depthPattern += $"*{Path.DirectorySeparatorChar}";
        }

        // Console.WriteLine(directoryPath);
        var directoryInfo = new DirectoryInfo(directoryPath);

        var directoryEntries = directoryInfo.GetDirectories("*", new EnumerationOptions
        {
            MaxRecursionDepth = depth - 1,
            RecurseSubdirectories = true,
            AttributesToSkip = default,
        });

        // var directoryEntries = directoryInfo.GetFileSystemInfos();
        directoryPath = Path.Combine(
            directoryInfo.Parent?.FullName ?? Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
            directoryInfo.Name
        );

        var backupPath = $"{directoryPath}.backup";

        // Create the directory if not exist
        new DirectoryInfo(backupPath).Create();

        ArchiveRootFiles(directoryInfo.EnumerateFiles().ToList(), backupPath);
        ArchiveDirectories(directoryPath, directoryEntries, backupPath, depth);

    }

    private void ArchiveDirectories(string directoryPath, DirectoryInfo[] directoryEntries, string backupPath, int depth)
    {

        /// <summary>
        /// This method must return a enumerable of attributes of compressed directories
        /// Template: {
        ///     ExternalPath, // The relative path from the files where the (file|directory has been archived)
        ///     CompressedPath, // The relative path (`the name`) from the backup directory has been ~~stored
        ///
        /// }
        ///
        /// </summary>
        string escapedSeparatorCharacter = Path.DirectorySeparatorChar == '\\' ? "\\\\" : Path.DirectorySeparatorChar.ToString();
        string pattern = $"^([^{escapedSeparatorCharacter}]+({escapedSeparatorCharacter}[^{escapedSeparatorCharacter}]*){{{depth - 1}}}[^{escapedSeparatorCharacter}]*)$";


        directoryEntries.ToList().ForEach((directory) =>
        {
            var relativePath = Path.GetRelativePath(directoryPath, $"{directory.FullName}");
            // Console.WriteLine("Relative path : " + relativePath);

            if (
                directory.GetDirectories().Count() == 0 ||
                Regex.IsMatch(
                    relativePath,
                    pattern
                )
            )
            {
                // here compress the directory
                // Console.WriteLine("V\t==> " + relativePath + " nb dir : " + directory.GetDirectories().Count());


                ZipFile.CreateFromDirectory(directory.FullName, $"{backupPath}{Path.DirectorySeparatorChar}d_{directory.Name}_{Path.GetRandomFileName()}.zip");
                // ZipFile.CreateFromDirectory($"{backupPath}_{Path.GetRandomFileName()}", directory.FullName);

            }
            else
            {
                // compress each files in this directory without touching the directories
                // Console.WriteLine("X\t--> " + relativePath + " nb dir : " + directory.GetDirectories().Count());

                // var backupDirectoryInfo = new DirectoryInfo(backupPath);
                // backupDirectoryInfo.Create();

                using (var fileArchive = ZipFile.Open($"{backupPath}{Path.DirectorySeparatorChar}f_{directory.Name}_{Path.GetRandomFileName()}.zip", ZipArchiveMode.Create))
                {
                    directory.GetFiles().ToList().ForEach(file =>
                    {
                        fileArchive.CreateEntryFromFile(file.FullName, file.Name);

                    });
                }
            }
        });
    }

    private void ArchiveRootFiles(List<FileInfo> files, string backupPath)
    {
        using (var rootFilesArchive = ZipFile.Open( $"{backupPath}{Path.DirectorySeparatorChar}rootfiles.zip", ZipArchiveMode.Create))
        {
	        files.ToList().ForEach(file => {
	            rootFilesArchive.CreateEntryFromFile(file.FullName, file.Name);
	        });
        }

    }
}
