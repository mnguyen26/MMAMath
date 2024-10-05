using MMAMath.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;

namespace MMAMath.Operations.PathCalc
{
    public class PathCalc
    {
        private string _allFightsJSONPath;
        private string _fighterPeakEloPath;
        private Dictionary<string, List<string>> _adjacencyList;
        private Dictionary<string, int> _fighterPeakElo;

        public PathCalc(string allFightsJSONPath, string fighterPeakEloPath)
        {
            _allFightsJSONPath = allFightsJSONPath;
            _fighterPeakEloPath = fighterPeakEloPath;
            //_fighterEloRecordspath = fighterEloRecordsPath;
            _adjacencyList = new Dictionary<string, List<string>>();

            BuildGraph();
        }

        private void AddEdge(string fighter1, string fighter2)
        {
            if (!_adjacencyList.ContainsKey(fighter1))
            {
                _adjacencyList[fighter1] = new List<string>();
            }
            if (_adjacencyList.ContainsKey(fighter1))
            {
                _adjacencyList[fighter1].Add(fighter2);
            }
        }

        private void BuildGraph()
        {
            string jsonString = File.ReadAllText(_allFightsJSONPath);
            var allFights = JsonSerializer.Deserialize<List<FightDetails>>(jsonString);

            foreach (var fight in allFights)
            {
                if (!_adjacencyList.ContainsKey(fight.FighterA))
                {
                    _adjacencyList[fight.FighterA] = new List<string>();
                }

                if (!_adjacencyList.ContainsKey(fight.FighterB))
                {
                    _adjacencyList[fight.FighterB] = new List<string>();
                }

                if (fight.FighterAResult == "W")
                {
                    AddEdge(fight.FighterA, fight.FighterB);
                }
                if (fight.FighterBResult == "W")
                {
                    AddEdge(fight.FighterB, fight.FighterA);
                }
            }
        }

        public List<string> FindShortestPath(string startFighter, string targetFighter)
        {
            if (!_adjacencyList.ContainsKey(startFighter) || !_adjacencyList.ContainsKey(targetFighter))
            {
                return null;
            }

            Queue<List<string>> queue = new Queue<List<string>>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(new List<string> { startFighter });
            visited.Add(startFighter);

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var lastFighter = path.Last();

                if (lastFighter == targetFighter)
                {
                    return path;
                }

                foreach (var neighbor in _adjacencyList[lastFighter])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        var newPath = new List<string>(path) { neighbor };
                        queue.Enqueue(newPath);
                    }
                }
            }

            return null;
        }

        public List<List<string>> MapConnectedHighestElo(string startFighter, int numFighters)
        {
            var highestEloPaths = new List<List<string>>();

            string jsonString = File.ReadAllText(_fighterPeakEloPath);
            var fightersPeakElo = JsonSerializer.Deserialize<Dictionary<string, FighterPeakEloDetails>>(jsonString);

            foreach(string fighter in fightersPeakElo.Keys)
            {
                var path = FindShortestPath(startFighter, fighter);
                if (path != null)
                {
                    highestEloPaths.Add(path);
                    numFighters--;
                }
                if (numFighters == 0) { break; }
            }

            return highestEloPaths;
        }
    }
}
