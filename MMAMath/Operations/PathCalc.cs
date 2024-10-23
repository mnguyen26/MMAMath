using MMAMath.Models;
using System.Text.Json;

namespace MMAMath.Operations.PathCalc
{
    public class PathCalc
    {
        private string _allFightsJSONPath;
        private Dictionary<string, List<FighterWinNode>> _adjacencyList;

        public PathCalc(string allFightsJSONPath, string fighterPeakEloPath)
        {
            _allFightsJSONPath = allFightsJSONPath;
            _adjacencyList = new Dictionary<string, List<FighterWinNode>>();

            BuildGraph();
            SaveGraph();
        }

        private void AddEdge(string fighterId, FighterWinNode winNode)
        {
            if (!_adjacencyList.ContainsKey(fighterId))
            {
                _adjacencyList[fighterId] = new List<FighterWinNode>();
            }
            else if (_adjacencyList.ContainsKey(fighterId))
            {
                _adjacencyList[fighterId].Add(new FighterWinNode() { Name=winNode.Name, Opponent=winNode.Opponent, Date=winNode.Date });
            }
        }

        private void BuildGraph()
        {
            string jsonString = File.ReadAllText(_allFightsJSONPath);
            var allFights = JsonSerializer.Deserialize<List<FightDetails>>(jsonString);

            foreach (var fight in allFights)
            {
                if (!_adjacencyList.ContainsKey(fight.FighterAId))
                {
                    _adjacencyList[fight.FighterAId] = new List<FighterWinNode>();
                }

                if (!_adjacencyList.ContainsKey(fight.FighterBId))
                {
                    _adjacencyList[fight.FighterBId] = new List<FighterWinNode>();
                }

                if (fight.FighterAResult == "W")
                {
                    AddEdge(fight.FighterAId, new FighterWinNode { Name=fight.FighterA, Opponent=fight.FighterB, Date=fight.Date });
                }
                if (fight.FighterBResult == "W")
                {
                    AddEdge(fight.FighterBId, new FighterWinNode { Name = fight.FighterB, Opponent = fight.FighterA, Date = fight.Date });
                }
            }
        }

        private void SaveGraph()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "fighter_wins_graph.json");
            string jsonString = System.Text.Json.JsonSerializer.Serialize(_adjacencyList, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, jsonString);
        }

        //public List<string> FindShortestPath(string startFighter, string targetFighter)
        //{
        //    if (!_adjacencyList.ContainsKey(startFighter) || !_adjacencyList.ContainsKey(targetFighter))
        //    {
        //        return null;
        //    }

        //    Queue<List<string>> queue = new Queue<List<string>>();
        //    HashSet<string> visited = new HashSet<string>();

        //    queue.Enqueue(new List<string> { startFighter });
        //    visited.Add(startFighter);

        //    while (queue.Count > 0)
        //    {
        //        var path = queue.Dequeue();
        //        var lastFighter = path.Last();

        //        if (lastFighter == targetFighter)
        //        {
        //            return path;
        //        }

        //        foreach (var neighbor in _adjacencyList[lastFighter])
        //        {
        //            if (!visited.Contains(neighbor.Opponent))
        //            {
        //                visited.Add(neighbor.Opponent);
        //                var newPath = new List<string>(path) { neighbor.Opponent };
        //                queue.Enqueue(newPath);
        //            }
        //        }
        //    }

        //    return null;
        //}

        //public List<List<string>> MapConnectedHighestElo(string startFighter, int numFighters)
        //{
        //    var highestEloPaths = new List<List<string>>();

        //    string jsonString = File.ReadAllText(_fighterPeakEloPath);
        //    var fightersPeakElo = JsonSerializer.Deserialize<Dictionary<string, FighterPeakEloDetails>>(jsonString);

        //    foreach(string fighter in fightersPeakElo.Keys)
        //    {
        //        var path = FindShortestPath(startFighter, fighter);
        //        if (path != null)
        //        {
        //            highestEloPaths.Add(path);
        //            numFighters--;
        //        }
        //        if (numFighters == 0) { break; }
        //    }

        //    return highestEloPaths;
        //}
    }
}
