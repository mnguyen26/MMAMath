using MMAMath.Models;
using System.Text.Json;

namespace MMAMath.Operations.PathCalc
{
    public class PathCalc
    {
        private string _allFightsJSONPath;
        private Dictionary<string, List<FighterWinNode>> _adjacencyList;
        private Dictionary<string, string> _fighterIdNameMap;

        public PathCalc(string allFightsJSONPath, string fighterPeakEloPath)
        {
            _allFightsJSONPath = allFightsJSONPath;
            _adjacencyList = new Dictionary<string, List<FighterWinNode>>();
            _fighterIdNameMap = new Dictionary<string, string>();

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
                _adjacencyList[fighterId].Add(new FighterWinNode() { Name=winNode.Name, Opponent=winNode.Opponent, OpponentId=winNode.OpponentId, Date=winNode.Date, Method=winNode.Method });
            }
        }

        private void BuildGraph()
        {
            string jsonString = File.ReadAllText(_allFightsJSONPath);
            var allFights = JsonSerializer.Deserialize<List<FightDetails>>(jsonString);

            foreach (FightDetails fight in allFights)
            {
                if (!_adjacencyList.ContainsKey(fight.FighterAId))
                {
                    _adjacencyList[fight.FighterAId] = new List<FighterWinNode>();
                    _fighterIdNameMap[fight.FighterAId] = fight.FighterA;
                }

                if (!_adjacencyList.ContainsKey(fight.FighterBId))
                {
                    _adjacencyList[fight.FighterBId] = new List<FighterWinNode>();
                    _fighterIdNameMap[fight.FighterBId] = fight.FighterB;
                }

                if (fight.FighterAResult == "W")
                {
                    AddEdge(fight.FighterAId, new FighterWinNode { Name=fight.FighterA, Opponent=fight.FighterB, OpponentId=fight.FighterBId, Date=fight.Date, Method=fight.Method });
                }
                if (fight.FighterBResult == "W")
                {
                    AddEdge(fight.FighterBId, new FighterWinNode { Name=fight.FighterB, Opponent=fight.FighterA, OpponentId=fight.FighterAId, Date=fight.Date, Method=fight.Method });
                }
            }
        }

        private void SaveGraph()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "fighter_wins_graph.json");
            string jsonString = System.Text.Json.JsonSerializer.Serialize(_adjacencyList, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, jsonString);

            filePath = Path.Combine(AppContext.BaseDirectory, "fighter_id_name_map.json");
            jsonString = System.Text.Json.JsonSerializer.Serialize(_fighterIdNameMap, new JsonSerializerOptions { WriteIndented = true }) ;

            File.WriteAllText(filePath , jsonString);
        }
    }
}
