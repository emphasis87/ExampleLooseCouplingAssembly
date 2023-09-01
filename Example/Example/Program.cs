using System.Reflection;
using System.Runtime.Loader;

var assembly = typeof(Program).Assembly;

void Print()
{
    Console.WriteLine("Referenced assemblies:");
    var assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
    foreach (var a in assemblies.Select(x => x.Name ?? "").Where(IsLocal))
    {
        Console.WriteLine(a);
    }
    Console.WriteLine();
}

Print(); // Does not print Example.Extra

bool IsLocal(string assemblyName) => assemblyName?.Contains("Example") is true;

var locations = new List<string>() { assembly.Location };
var localAssemblies = new List<Assembly>() { assembly };
int count;
do
{
    count = locations.Count;
    locations = locations
        .Select(Path.GetDirectoryName)
        .Distinct()
        .SelectMany(x => Directory.GetFiles(x, "Example*.dll"))
        .Where(IsLocal)
        .Where(x => x.EndsWith(".dll"))
        .Except(locations)
        .Select(x =>
        {
            var a = Assembly.LoadFrom(x);
            localAssemblies.Add(a);
            return a;
        })
        .Select(x => x.Location)
        .Concat(locations)
        .ToList();
} while (locations.Count > count);

foreach(var a in localAssemblies)
{
    var resources = a.GetManifestResourceNames()
       .Where(x => x.EndsWith(".json"))
       .Select(x =>
       {
           using var s = a.GetManifestResourceStream(x);
           using var reader = new StreamReader(s);
           var content = reader.ReadToEnd();
           return (x, content);
        });

    foreach(var (name , resource) in resources)
    {
        Console.WriteLine($"{name}:");
        Console.WriteLine(resource);
    }
    Console.WriteLine();
}

Print();

Console.WriteLine("Loaded assemblies:");
var assemblies = AssemblyLoadContext.Default.Assemblies;
foreach (var a in assemblies.Select(x => x.GetName().Name ?? "").Where(IsLocal))
{
    Console.WriteLine(a);
}
Console.WriteLine();