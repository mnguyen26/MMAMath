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
    Console.WriteLine("5. Generate Wins Graph");
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
        case "5":
            GenerateWinGraph();
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

void GenerateWinGraph()
{
    Console.WriteLine("Enter path for ufc_fights.json");
    var fightsPath = Console.ReadLine();
    var pathCalc = new PathCalc(fightsPath, "");
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
