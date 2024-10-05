using MMAMath.Operations.Scraper;
using MMAMath.Operations.EloCalculate;
using MMAMath.Operations.PathCalc;
using MMAMath.Models;

Console.WriteLine("Choose an operation...");
Console.WriteLine("1. Scrape events from Tapology");
Console.WriteLine("2. Scrape fights from Tapology");
Console.WriteLine("3. Calculate Elo scores");
Console.WriteLine("4. Calculate Peak Elo Scores");
Console.WriteLine("5. MMA Math: Shortest Path");
Console.WriteLine("6. ");

var input = Console.ReadLine();

switch (input)
{
    case "1":
        Console.WriteLine("K hold up...");
        Scraper.SaveEvents();
        break;
    case "2":
        Console.WriteLine("K hold up...");
        Scraper.SaveFights();
        break;
    case "3":
        Console.WriteLine("Enter path for ufc_fights.json");
        var path = Console.ReadLine();
        var eloCalculate = new EloCalculate(path);
        eloCalculate.GenerateEloAllFighters();
        break;
    case "4":
        Console.WriteLine("Enter path for fighter_elo_records.json");
        var eloPath = Console.ReadLine();
        var peakEloCalculate = new EloCalculate();
        peakEloCalculate.GetFightersPeakElo(eloPath);
        break;
    case "5":
        Console.WriteLine("Enter path for ufc_fights.json");
        var fightsPath = Console.ReadLine();
        Console.WriteLine("Enter path for fighter_elo_records.json");
        var peakEloPath = Console.ReadLine();
        var pathCalc = new PathCalc(fightsPath, peakEloPath);
        Console.WriteLine("Enter first fighter");
        var fighter1 = Console.ReadLine();
        Console.WriteLine("Enter second fighter");
        var fighter2 = Console.ReadLine();
        
        var shortestPath = pathCalc.FindShortestPath(fighter1, fighter2);
        foreach (var fighter in shortestPath)
        {
            Console.WriteLine($"{fighter} beat -->");
        }
        break;
    default:
        Console.WriteLine("Try again");
        break;
}


