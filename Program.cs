using System.CommandLine;
using IncrementalBackupS.Commands;

var commands = new List<AbstractCommand>() {
    // new AdditionCommand(),
    new BackupCommand(),
};

RootCommand rootCommand = new RootCommand();

/** foreach loops to attach all commands to the root command */
commands.ForEach(command => {
    rootCommand.Add(command.Handle());
});


// var sublimeOption = new Argument<string>("sublime", "This is my first argument");
// sublimeOption.SetDefaultValue("Bonjour Sublime");

// rootCommand.Add(sublimeOption);

rootCommand.SetHandler((ivContext) => {
    Console.WriteLine(@"
        Hi this is the principal menu,
        // documentation is going to follow
        // for help type --help
    ");
});


await rootCommand.InvokeAsync(args);
