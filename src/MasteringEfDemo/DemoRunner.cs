using BenchmarkDotNet.Running;

namespace MasteringEfDemo;

/// <summary>
///     Represents the runner entry point for the demo system, responsible for inputs & command selection
/// </summary>
public interface IDemoRunner
{
    void RunDemo();
}

public class DemoRunner(IDemoEngine engine) : IDemoRunner
{
    public void RunDemo()
    {
        ListDemoCommands();
        var exitRequested = false;
        while (!exitRequested)
        {
            var input = CollectInput();
            if (input == null)
                continue;

            var inputMatched = Enum.TryParse<DemoCommandOptions>(input, out var commandOption);
            if (!inputMatched)
            {
                Console.WriteLine("Invalid command entered.  Enter 1 to list all commands, 99 to exit");
                continue;
            }

            switch (commandOption)
            {
                case DemoCommandOptions.ListCommands:
                    ListDemoCommands();
                    break;
                case DemoCommandOptions.ChangeTracking:
                    engine.DemoChangeTracking();
                    break;
                case DemoCommandOptions.Projections:
                    engine.DemoProjections();
                    break;
                case DemoCommandOptions.NPlusQuery:
                    engine.DemoMultiQueryExecution();
                    break;
                case DemoCommandOptions.OverInclusions:
                    engine.DemoOverInclusion();
                    break;
                case DemoCommandOptions.PagedResults:
                    engine.DemoPagedResults(); 
                    break;
                case DemoCommandOptions.SplitQuery:
                    engine.DemoSplitQuery();
                    break;
                case DemoCommandOptions.StringAggregation:
                    engine.DemoStringAggregation(); 
                    break;
                case DemoCommandOptions.DateFunctions:
                    engine.DemoDateFunctions();
                    break;
                case DemoCommandOptions.BulkUpdates:
                    engine.DemoBulkUpdate();
                    break;
                case DemoCommandOptions.Exit:
                    exitRequested = true;
                    break;
            }
        }
    }


    private void ListDemoCommands()
    {
        Console.WriteLine("The following commands are available to demo");
        foreach (var item in Enum.GetValues<DemoCommandOptions>())
            Console.WriteLine($"{(int)item} - {item.GetDisplayNameOrStringValue()}");
    }

    private string? CollectInput()
    {
        Console.WriteLine("Please Enter Requested Command?");
        return Console.ReadLine();
    }
}