using System.Reflection;
using Riptide;
using Syncing_Battleship_Common_Typing;

namespace Syncing_Battleship;

public class BehaviourLoader
{
    private readonly Type behaviourType;

    public BehaviourLoader(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Error: Missing command-line arguments.");
            Console.Error.WriteLine("Usage: dotnet run -- <path_to_dll> <fully_qualified_type_name>");
            Console.Error.WriteLine(@"Example: dotnet run -- ""Behavioural plugins/JPong/bin/Debug/net8.0/JPong.dll"" JPong.JPongDataBehaviour");
            Environment.Exit(1);
        }

        string pluginAssemblyPath = args[0];
        string behaviourTypeName = args[1];

        if (!File.Exists(pluginAssemblyPath))
        {
            Console.Error.WriteLine($"Error: Assembly not found at path: {Path.GetFullPath(pluginAssemblyPath)}");
            Environment.Exit(1);
        }

        var pluginAssembly =
            Assembly.LoadFrom(Path.GetFullPath(pluginAssemblyPath))
            ?? throw new TypeLoadException($"Could not find the assembly '{Path.GetFileName(pluginAssemblyPath)}'");

        behaviourType =
            pluginAssembly.GetType(behaviourTypeName)
            ?? throw new TypeLoadException($"Could not find the type '{behaviourTypeName}' in the assembly '{Path.GetFileName(pluginAssemblyPath)}'");

        if (!typeof(IDataBehaviour).IsAssignableFrom(behaviourType))
        {
            throw new InvalidCastException($"The type '{behaviourTypeName}' does not implement the '{nameof(IDataBehaviour)}' interface");
        }
    }

    public IDataBehaviour Configure(MessageSendMode mode)
    {
        object? instance = Activator.CreateInstance(behaviourType);

        if (instance is IDataBehaviour dataBehaviour) return dataBehaviour;

        throw new Exception($"Failed to create an instance of '{behaviourType.Name}'");
    }
}
