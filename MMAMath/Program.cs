using MMAMath.Operations.Scraper;
using MMAMath.Operations.EloCalculate;
using MMAMath.Operations.PathCalc;

void ShowMenu()
{
    Console.WriteLine("Choose an operation...");
    Console.WriteLine("1. Scrape events from Tapology");
    Console.WriteLine("2. Scrape fights from Tapology");
    Console.WriteLine("3. Calculate Elo scores");
    Console.WriteLine("4. Calculate Peak Elo Scores");
    Console.WriteLine("5. MMA Math: Shortest Path");
    Console.WriteLine("6. MMA Math: Paths to Highest Elo");
    Console.WriteLine("0. Exit");
    Console.WriteLine("Enter your choice:");
}

void ExecuteMenuItem(string input)
{
    switch (input)
    {
        case "1":
            Console.WriteLine("Scraping events from Tapology...");
            Scraper.SaveEvents();
            break;
        case "2":
            Console.WriteLine("Scraping fights from Tapology...");
            Scraper.SaveFights();
            break;
        case "3":
            PerformEloCalculation();
            break;
        case "4":
            PerformPeakEloCalculation();
            break;
        default:
            Console.WriteLine("Invalid choice. Try again.");
            break;
    }
}

void PerformEloCalculation()
{
    Console.WriteLine("Enter path for ufc_fights.json:");
    var path = Console.ReadLine();
    var eloCalculate = new EloCalculate(path);
    eloCalculate.GenerateEloAllFighters();
}

void PerformPeakEloCalculation()
{
    Console.WriteLine("Enter path for fighter_elo_records.json:");
    var path = Console.ReadLine();
    var peakEloCalculate = new EloCalculate();
    peakEloCalculate.GetFightersPeakElo(path);
}

void FindShortestPath()
{
    Console.WriteLine("Enter path for ufc_fights.json:");
    var fightsPath = Console.ReadLine();
    var pathCalc = new PathCalc(fightsPath, "");
    Console.WriteLine("Enter first fighter:");
    var fighter1 = Console.ReadLine();
    Console.WriteLine("Enter second fighter:");
    var fighter2 = Console.ReadLine();

    var shortestPath = pathCalc.FindShortestPath(fighter1, fighter2);
    if (shortestPath != null)
    {
        foreach (var fighter in shortestPath)
        {
            Console.WriteLine($"{fighter} beat -->");
        }
    }
    else
    {
        Console.WriteLine("No path found.");
    }
}

void FindPathsToHighestElo()
{
    Console.WriteLine("Enter path for ufc_fights.json:");
    var allFightsPath = Console.ReadLine();
    Console.WriteLine("Enter path for fighter_peak_elo_records.json:");
    var fightersPeakElo = Console.ReadLine();
    var pathCalc = new PathCalc(allFightsPath, fightersPeakElo);

    Console.WriteLine("Enter start fighter:");
    var startFighter = Console.ReadLine();
    Console.WriteLine("Enter number of fighters to connect:");
    
    if (int.TryParse(Console.ReadLine(), out int numFighters))
    {
        var result = pathCalc.MapConnectedHighestElo(startFighter, numFighters);
        foreach (var path in result)
        {
            foreach (var fighter in path)
            {
                Console.WriteLine($"{fighter} defeated -->");
            }
            Console.WriteLine();
        }
    }
    else
    {
        Console.WriteLine("Invalid number of fighters.");
    }
}


var showMenu = true;
while (showMenu)
{
    ShowMenu();
    var input = Console.ReadLine();
    if (input == "0")
    {
        Console.WriteLine("Exiting...");
        break; 
    }
    ExecuteMenuItem(input);
}
