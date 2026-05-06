# BaseCLI

Provides a simple reflection based service setup for creating a cli. Useful for creating internal tools.

Give it a go get the nuget package from [here](https://www.nuget.org/packages/BaseCLI/).

## How to create a command

First create a class to represent the options for a given cli command...

```csharp
public class ExampleOptions
{
    public string Str { get; set; }
    public int Int { get; set; }
    public List<string> StrCol { get; set; }
    public List<int> IntCol { get; set; }
    public bool Bool { get; set; }
}
```

Then implement `ICommandHandler<ExampleOptions>`...

```csharp
public class ExampleCommand : ICommandHandler<ExampleOptions>
{
    public string CommandName { get; } = "C1";
    public string CommandDescription { get; } = "An example of a command";
	
    public OptionsConfiguration<ExampleOptions> OptionsConfiguration { get; } = new OptionsConfigurationBuilder<ExampleOptions>()
        .Add("s", "Str").ForString(x => x.Str)
        .Add("i", "Int").ForInt(x => x.Int)
        .Add("sc", "Str Col").ForStringCollection(x => x.StrCol, true)
        .Add("ic", "Int Col").ForIntCollection(x => x.IntCol)
        .Add("b", "Bool").ForBool(x => x.Bool)
        .Build();
		
    public Task ExecuteCommand(ExampleOptions options)
    {
        Console.WriteLine(options.Str);
        Console.WriteLine(options.Int);

        foreach (var str in options.StrCol)
            Console.WriteLine(str);

        foreach (var i in options.IntCol)
            Console.WriteLine(i);

        Console.WriteLine(options.Bool);
        return Task.CompletedTask;
    }
}
```

### But what does all this mean?

The `CommandName` property is the first argument that you will pass to your console app to instruct it that you want to execute `ExampleCommand`.

The `CommandDescription` property is useful when you ask for help (more on this later).

The `ExecuteCommand` method is the method that will be executed by BaseCLI.

### OptionsConfiguration

It's important to be able to pass options to your commands and as you can see BaseCLI provides a useful builder interface to help set these up. There are five types of options currently supported which are `string`, `int`, `List<string>`, `List<int>` and `bool` and it is also possible to make options mandatory.

You pass a `string` in your console like so... `-flag "test string"`

You pass a `int` in your console like so... `-flag 1`

You pass a `List<string>` in your console like so... `-flag hello -flag world`

You pass a `List<int>` in your console like so... `-flag 10 -flag 20`

You pass a `bool` in your console like so... `-flag`

### How to setup your commands?

Simply call the following code from you console...

```csharp
    internal class Program
    {
        static void Main(string[] args)
        {
            CLI.Execute(args, new[] { typeof(ExampleOptions).Assembly }, services => { });
        }
    }
```

As you can see you simply pass the raw command-line `args` from the main method as well as the assemblies containing your commands, optionally you can wire up your own services too.

## How to execute your commands?
executing `.\TestConsole.exe C1 -s "Test String" -i 1 -sc Hello -sc World -ic 10 -ic 20 -b` from the command line using the example above will result in the following output...
```
Test String
1
Hello
World
10
20
True
```

## Getting help
After you've made your extremely useful cli tool you'll probably forget how to use all the commands you created, don't worry you can easily ask for help.

executing `.\TestConsole.exe -h` will output a list of your commands and their descriptions
```
C1 - An example of a command
C2 - Another example of a command
```

and executing `.\TestConsole.exe C1 -h` will output information about command "C1"
```
C1 - An example of a command
-s Str - A string
-i Int - An integer
-sc Str Col (mandatory) - A collection of strings
-ic Int Col - A collection of integers
-b Bool - A boolean
```
