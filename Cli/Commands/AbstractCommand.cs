using System.CommandLine;
using System.CommandLine.Invocation;

namespace IncrementalBackupS.Commands;

abstract class AbstractCommand
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public List<Argument> Arguments { get; } = new List<Argument>();

    public List<Option> Options { get; } = new List<Option>();

    public abstract void Action(InvocationContext inv);

    public Command Handle ()
    {
        var command = new Command(Name, Description);
        command.SetHandler(Action);

        Arguments.ForEach((argument) => {
            command.AddArgument(argument);
        });

        Options.ForEach((option) => {
            command.AddOption(option);
        });

        return command;
    }
}
