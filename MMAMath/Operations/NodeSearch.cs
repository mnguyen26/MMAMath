//using MMAMath.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MMAMath.Operations
//{
//}

////public class FightDetails
////{
////    public string FighterA { get; set; }
////    public string FighterB { get; set; }
////    public string Winner { get; set; }
////}

//public class FighterNetwork
//{
//    private Dictionary<string, List<string>> adjacencyList;

//    public FighterNetwork(List<FightDetails> fights)
//    {
//        adjacencyList = new Dictionary<string, List<string>>();

//        // Build the graph
//        foreach (var fight in fights)
//        {
//            AddEdge(fight.FighterA, fight.FighterB);
//            AddEdge(fight.FighterB, fight.FighterA); // Bi-directional because both fighters are connected
//        }
//    }

//    private void AddEdge(string fighter1, string fighter2)
//    {
//        if (!adjacencyList.ContainsKey(fighter1))
//        {
//            adjacencyList[fighter1] = new List<string>();
//        }
//        if (!adjacencyList[fighter1].Contains(fighter2))
//        {
//            adjacencyList[fighter1].Add(fighter2);
//        }
//    }

//    public List<string> FindShortestPath(string startFighter, string targetFighter)
//    {
//        if (!adjacencyList.ContainsKey(startFighter) || !adjacencyList.ContainsKey(targetFighter))
//        {
//            return null; // One or both fighters not in the network
//        }

//        Queue<List<string>> queue = new Queue<List<string>>();
//        HashSet<string> visited = new HashSet<string>();

//        // Start the BFS with the start fighter
//        queue.Enqueue(new List<string> { startFighter });
//        visited.Add(startFighter);

//        while (queue.Count > 0)
//        {
//            var path = queue.Dequeue();
//            var lastFighter = path.Last();

//            if (lastFighter == targetFighter)
//            {
//                return path; // Found the target fighter, return the path
//            }

//            foreach (var neighbor in adjacencyList[lastFighter])
//            {
//                if (!visited.Contains(neighbor))
//                {
//                    visited.Add(neighbor);
//                    var newPath = new List<string>(path) { neighbor };
//                    queue.Enqueue(newPath);
//                }
//            }
//        }

//        return null; // No path found
//    }
//}

//public class Program
//{
//    public static void Main(string[] args)
//    {
//        var fights = new List<FightDetails>
//        {
//            new FightDetails { FighterA = "Tom Aspinall", FighterB = "Andrei Arlovski", Winner = "Tom Aspinall" },
//            new FightDetails { FighterA = "Stipe Miocic", FighterB = "Daniel Cormier", Winner = "Stipe Miocic" },
//            new FightDetails { FighterA = "Daniel Cormier", FighterB = "Derrick Lewis", Winner = "Daniel Cormier" },
//            new FightDetails { FighterA = "Tom Aspinall", FighterB = "Curtis Blaydes", Winner = "Tom Aspinall" },
//            new FightDetails { FighterA = "Stipe Miocic", FighterB = "Francis Ngannou", Winner = "Francis Ngannou" }
//        };

//        var network = new FighterNetwork(fights);

//        string fighter1 = "Tom Aspinall";
//        string fighter2 = "Francis Ngannou";

//        var shortestPath = network.FindShortestPath(fighter1, fighter2);

//        if (shortestPath != null)
//        {
//            Console.WriteLine($"Shortest path from {fighter1} to {fighter2}:");
//            Console.WriteLine(string.Join(" -> ", shortestPath));
//        }
//        else
//        {
//            Console.WriteLine($"No path found between {fighter1} and {fighter2}");
//        }
//    }
//}
















////public class FightDetails
////{
////    public string FighterA { get; set; }
////    public string FighterB { get; set; }
////    public string Winner { get; set; }
////    public string Method { get; set; }
////    public string? Score { get; set; }
////    public DateTime Date { get; set; }
////    public double EloAfterFight { get; set; } // Assume Elo score after this fight
////}

//public class FighterPeakElo
//{
//    public string Fighter { get; set; }
//    public double PeakElo { get; set; }
//}

//public class Program
//{
//    // Part 1: Create a list of fighters with their peak Elo scores
//    public static List<FighterPeakElo> GetFightersWithPeakElo(Dictionary<string, List<FightDetails>> fighterDict)
//    {
//        List<FighterPeakElo> peakEloList = new List<FighterPeakElo>();

//        foreach (var fighter in fighterDict)
//        {
//            double peakElo = fighter.Value.Max(fight => fight.EloAfterFight);
//            peakEloList.Add(new FighterPeakElo { Fighter = fighter.Key, PeakElo = peakElo });
//        }

//        return peakEloList;
//    }

//    // Part 2: Find the highest Elo path from one fighter to another through wins
//    public static List<string> FindHighestEloPath(string startFighter, string targetFighter, Dictionary<string, List<FightDetails>> fighterDict)
//    {
//        var visited = new HashSet<string>();
//        var path = new List<string>();
//        var bestPath = new List<string>();
//        double highestElo = -1;

//        void DFS(string currentFighter, double currentElo)
//        {
//            // Mark the current fighter as visited
//            visited.Add(currentFighter);
//            path.Add(currentFighter);

//            // If we've reached the target fighter, update the best path if this one is better
//            if (currentFighter == targetFighter)
//            {
//                if (currentElo > highestElo)
//                {
//                    highestElo = currentElo;
//                    bestPath = new List<string>(path);
//                }
//            }
//            else
//            {
//                // Explore the wins (edges) of the current fighter
//                foreach (var fight in fighterDict[currentFighter])
//                {
//                    string opponent = fight.FighterA == currentFighter ? fight.FighterB : fight.FighterA;

//                    // Only continue if this fight is a win for the current fighter and opponent hasn't been visited
//                    if (fight.Winner == currentFighter && !visited.Contains(opponent))
//                    {
//                        DFS(opponent, Math.Max(currentElo, fight.EloAfterFight)); // Continue DFS with the opponent
//                    }
//                }
//            }

//            // Backtrack
//            visited.Remove(currentFighter);
//            path.RemoveAt(path.Count - 1);
//        }

//        DFS(startFighter, 0);

//        return bestPath;
//    }

//    public static void Main(string[] args)
//    {
//        // Example: Dictionary with fighter and their fight details
//        var fighterDict = new Dictionary<string, List<FightDetails>>
//        {
//            {
//                "FighterA", new List<FightDetails>
//                {
//                    new FightDetails { FighterA = "FighterA", FighterB = "FighterB", Winner = "FighterA", EloAfterFight = 1500 },
//                    new FightDetails { FighterA = "FighterA", FighterB = "FighterC", Winner = "FighterC", EloAfterFight = 1400 }
//                }
//            },
//            {
//                "FighterB", new List<FightDetails>
//                {
//                    new FightDetails { FighterA = "FighterA", FighterB = "FighterB", Winner = "FighterA", EloAfterFight = 1400 }
//                }
//            }
//        };

//        // Part 1: Get peak Elo list
//        var peakEloList = GetFightersWithPeakElo(fighterDict);
//        foreach (var fighter in peakEloList)
//        {
//            Console.WriteLine($"{fighter.Fighter}: Peak Elo {fighter.PeakElo}");
//        }

//        // Part 2: Find highest Elo path from one fighter to another
//        var path = FindHighestEloPath("FighterA", "FighterB", fighterDict);
//        Console.WriteLine($"Path from FighterA to FighterB: {string.Join(" -> ", path)}");
//    }
//}


