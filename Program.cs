using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

RegionEndpoint region = RegionEndpoint.USWest2;

var client = new AmazonDynamoDBClient(region);
Table table = Table.LoadTable(client, "recipe");

if (args.Length == 2 && args[0] == "put")
{
    // put [json-filename] - store a recipe
    var name = args[1];
    var json = File.ReadAllText(name);
    var recipe = Document.FromJson(json);
    Console.WriteLine($"Putting document category:{recipe["category"]} name:{recipe["name"]} to recipe table");
    var response = await table.PutItemAsync(recipe);
}
else if (args.Length == 3 && args[0] == "get")
{
    // get [category] [name] - retrieve and display a recipe
    var partitionKey = args[1];
    var sortKey = args[2];
    var recipe = await table.GetItemAsync(partitionKey, sortKey);
    if (recipe == null)
    {
        Console.WriteLine("Recipe not found");
    }
    else
    {
        Console.WriteLine($"Recipe: {recipe["name"]}");
        Console.WriteLine($"Category: {recipe["category"]}");
        Console.WriteLine($"Link: {recipe["link"]}");

        Console.WriteLine($"Intro: {recipe["intro"]}");
        Console.WriteLine();

        Console.WriteLine("Ingredients");
        foreach (var ingredient in recipe["ingredients"].AsListOfDynamoDBEntry())
        {
            Console.WriteLine($"{ingredient}");
        }
        Console.WriteLine();

        Console.WriteLine("Instructions");
        int number = 1;
        foreach (var step in recipe["instructions"].AsListOfDynamoDBEntry())
        {
            Console.WriteLine($"{number++}. {step}");
        }
    }
}
else if (args.Length==1 && args[0] == "list")
{
    // list - list recipes
    var filter = new ScanFilter();
    filter.AddCondition("name", ScanOperator.IsNotNull);

    var scanConfig = new ScanOperationConfig()
    {
        Filter = filter,
        Select = SelectValues.SpecificAttributes,
        AttributesToGet = new List<string> { "category", "name", "link" }
    };
    Search search = table.Scan(scanConfig);
    List<Document> matches;
    do
    {
        matches = await search.GetNextSetAsync();
        foreach (var match in matches)
        {
            Console.WriteLine($"{match["category"],-20} {match["name"],-40} {match["link"]}");
        }
    } while (!search.IsDone);
}
else if (args.Length==2 && args[0] == "list")
{
    // list [category] - list recipes in a category
    var filter = new QueryFilter();
    filter.AddCondition("category", ScanOperator.Equal, args[1]);

    var filterConfig = new QueryOperationConfig()
    {
        Filter = filter,
        Select = SelectValues.SpecificAttributes,
        AttributesToGet = new List<string> { "category", "name", "link" }
    };
    Search search = table.Query(filterConfig);
    List<Document> matches;
    do
    {
        matches = await search.GetNextSetAsync();
        foreach (var match in matches)
        {
            Console.WriteLine($"{match["category"],-20} {match["name"],-40} {match["link"]}");
        }
    } while (!search.IsDone);
}
else
{
    Console.WriteLine("To store a recipe:              dotnet run -- put [jsonfile]");
    Console.WriteLine("To list all recipes:            dotnet run -- list");
    Console.WriteLine("To list recipes in a category:  dotnet run -- list [category]");
    Console.WriteLine("To retrieve a recipe:           dotnet run -- get [category] [name]");
}
