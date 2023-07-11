// See https://aka.ms/new-console-template for more information

Console.WriteLine("Please enter a number of chairs: ");
var input = Console.ReadLine();
int intInput;
if (!int.TryParse(input, out intInput))
{
    Console.WriteLine("Invalid input!");
    return;
}

IOutputWriter outputWriter = new ConsoleOutputWriter();

IRequirementFactory requirementFactory = new RequirementFactory();

ChairManager chairManager = new ChairManager(outputWriter);

List<IStore> stores = new List<IStore>
    {
        new Store("Store01", new Dictionary<string, int>
        {
            { Component.Screw, 20 },
            { Component.Wheel, 8 },
            { Component.Armbar, 4 },
            { Component.Nut, 20 }
        }),
        new Store("Store02", new Dictionary<string, int>
        {
            { Component.Screw, 100 },
            { Component.Wheel, 100 },
            { Component.Armbar, 100 },
            { Component.Nut, 20 }
        }),
        new Store("Store03", new Dictionary<string, int>
        {
            { Component.Screw, 2000 },
            { Component.Wheel, 200 },
            { Component.Armbar, 100 },
            { Component.Nut, 1000 }
        })
    };

for (int number = 0; number < intInput; number++)
{
    Console.WriteLine($"BoxFern{number + 1}");

    RequirementManager requirementManager = new RequirementManager();
    requirementManager.AddRequirement(requirementFactory, Component.Screw, 10);
    requirementManager.AddRequirement(requirementFactory, Component.Wheel, 4);
    requirementManager.AddRequirement(requirementFactory, Component.Armbar, 2);
    requirementManager.AddRequirement(requirementFactory, Component.Nut, 30);

    if (!chairManager.ProcessChair(stores, requirementManager))
    {
        Console.WriteLine("Error! Component out of stock, boxing stopped.");
        break;
    }
    Console.WriteLine();
}

public static class Component
{
    public const string Screw = "Screw";
    public const string Wheel = "Wheel";
    public const string Armbar = "Armbar";
    public const string Nut = "Nut";
}

interface IStore
{
    string Name { get; }
    Dictionary<string, int> Components { get; }
}

interface IChairManager
{
    bool ProcessChair(List<IStore> stores, IRequirementManager requirementManager);
}

interface IRequirementManager
{
    void ProcessRequirements(IStore store);
    bool AreAllRequirementsFulfilled();
    string GetOutput();
    void AddRequirement(IRequirementFactory requirementFactory, string componentType, int requiredQty);
}

interface IRequirement
{
    void ProcessRequirement(IStore store);
    bool IsFulfilled();
    string GetOutput();
}

interface IRequirementFactory
{
    IRequirement CreateRequirement(string componentType, int requiredQty);
}

interface IOutputWriter
{
    void WriteLine(string message);
}

class Store : IStore
{
    public string Name { get; }
    public Dictionary<string, int> Components { get; }

    public Store(string name, Dictionary<string, int> components)
    {
        Name = name;
        Components = components;
    }
}

class ChairManager : IChairManager
{
    private readonly IOutputWriter outputWriter;

    public ChairManager(IOutputWriter outputWriter)
    {
        this.outputWriter = outputWriter;
    }

    public bool ProcessChair(List<IStore> stores, IRequirementManager requirementManager)
    {
        bool isCompleted = false;
        foreach (var store in stores)
        {
            requirementManager.ProcessRequirements(store);
            outputWriter.WriteLine(requirementManager.GetOutput().TrimEnd());

            if (requirementManager.AreAllRequirementsFulfilled())
            {
                isCompleted = true;
                break;
            }
        }

        return isCompleted;
    }
}

class RequirementManager : IRequirementManager
{
    private readonly List<IRequirement> requirements;
    private string output = "";

    public RequirementManager()
    {
        requirements = new List<IRequirement>();
    }

    public void ProcessRequirements(IStore store)
    {
        output = "";
        foreach (var requirement in requirements)
        {
            requirement.ProcessRequirement(store);
            if (requirement.GetOutput().Length > 0)
            {
                output += requirement.GetOutput() + Environment.NewLine;
            }
        }
    }

    public bool AreAllRequirementsFulfilled()
    {
        return requirements.All(r => r.IsFulfilled());
    }

    public string GetOutput()
    {
        return output;
    }

    public void AddRequirement(IRequirementFactory requirementFactory, string componentType, int requiredQty)
    {
        IRequirement requirement = requirementFactory.CreateRequirement(componentType, requiredQty);
        requirements.Add(requirement);
    }
}

class Requirement : IRequirement
{
    private int currentRequiredQty;
    private string componentType;
    private string output = "";

    public Requirement(int requiredQty, string componentType)
    {
        currentRequiredQty = requiredQty;
        this.componentType = componentType;
    }

    public void ProcessRequirement(IStore store)
    {
        if (currentRequiredQty > 0 && store.Components.TryGetValue(componentType, out int availableQty) && availableQty > 0)
        {
            if (availableQty > currentRequiredQty)
            {
                store.Components[componentType] -= currentRequiredQty;
                output = $"{store.Name} | {componentType.ToLower()} = {currentRequiredQty}";
                currentRequiredQty = 0;
            }
            else
            {
                output = $"{store.Name} | {componentType.ToLower()} = {availableQty}";
                currentRequiredQty -= availableQty;
                store.Components[componentType] = 0;
            }
        }
        else
        {
            output = "";
        }
    }

    public bool IsFulfilled()
    {
        return currentRequiredQty == 0;
    }

    public string GetOutput()
    {
        return output;
    }
}

class RequirementFactory : IRequirementFactory
{
    public IRequirement CreateRequirement(string componentType, int requiredQty)
    {
        return new Requirement(requiredQty, componentType);
    }
}

class ConsoleOutputWriter : IOutputWriter
{
    public void WriteLine(string message)
    {
        if (message.Length == 0) return;
        Console.WriteLine(message);
    }
}
