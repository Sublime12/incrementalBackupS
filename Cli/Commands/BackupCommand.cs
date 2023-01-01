
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Compression;


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
                Console.Error.WriteLine( $"Complete Trace : {e}");
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("Directory already exists");
                Console.Error.WriteLine( $"Complete Trace : {e}");
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
        // var directoryEntries = directoryInfo.GetDirectories("*", new EnumerationOptions
        // {
        //     // MaxRecursionDepth = depth,
        //     RecurseSubdirectories = true,
        //     MatchCasing = MatchCasing.CaseInsensitive,
        //     MatchType = MatchType.Simple,

        // });

        var directoryEntries = directoryInfo.GetDirectories("*", new EnumerationOptions {
            MaxRecursionDepth = depth,
            RecurseSubdirectories = true,
            AttributesToSkip = default,
        });

        // var directoryEntries = directoryInfo.GetFileSystemInfos();
        directoryPath = Path.Combine(
            directoryInfo.Parent?.FullName ?? Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
            directoryInfo.Name
        );

        directoryEntries.ToList().ForEach((directory) => {
            var relativePath = Path.GetRelativePath(directoryPath, directory.FullName);

            if (directory.GetDirectories().Count() == 0) {
                // here compress the directory

                var backupPath = $"{directoryPath}.backup/{relativePath}.zip";
                ZipFile.CreateFromDirectory(directoryPath, backupPath);
            } else {
                // compress each files in this directory without touching the directories

                var backupPath = $"{directoryPath}.backup/{relativePath}.zip";

                var fileInfo = new FileInfo(backupPath);
                fileInfo.Directory?.Create();

                using (var fileArchive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
                {

                    directory.GetFiles().ToList().ForEach(file =>
                    {

                        fileArchive.CreateEntryFromFile(file.FullName, file.Name);
                    });
                }
            }
        });

    }
}
