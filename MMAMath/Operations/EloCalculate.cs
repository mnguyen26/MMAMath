using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MMAMath.Models;

namespace MMAMath.Operations.EloCalculate
{
    public class EloCalculate
    {
        private string _allFightsJSONPath;
        public Dictionary<string, List<FighterEloRow>> _allFightersEloTable;

        public EloCalculate(string allFightsJSONPath)
        {
            _allFightsJSONPath = allFightsJSONPath;
            _allFightersEloTable = new Dictionary<string, List<FighterEloRow>>();
        }

        public EloCalculate()
        {
            _allFightsJSONPath = "";
            _allFightersEloTable = new Dictionary<string, List<FighterEloRow>>();
        }

        private int[] CalculateElo(int fighterAElo, int fighterBElo, string fighterAResult, string fighterBResult)
        {
            double k = 32;

            double fighterAExpectedResult = 1 / (1 + Math.Pow(10, ((fighterBElo - fighterAElo) / 400.0)));
            double fighterBExpectedResult = 1 / (1 + Math.Pow(10, ((fighterAElo - fighterBElo) / 400.0)));

            double AResult = 0;
            switch(fighterAResult)
            {
                case "W":
                    AResult = 1;
                    break;
                case "L":
                    AResult = 0;
                    break;
                case "D":
                    AResult = 0.5;
                    break;
            }

            double BResult = 0;
            switch(fighterBResult)
            {
                case "W":
                    BResult = 1;
                    break;
                case "L":
                    BResult = 0;
                    break;
                case "D":
                    BResult = 0.5;
                    break;
            }

            var fighterANewElo = fighterAElo + k*(AResult - fighterAExpectedResult);
            var fighterBNewElo = fighterBElo + k*(BResult - fighterBExpectedResult);

            return [Convert.ToInt32(fighterANewElo), Convert.ToInt32(fighterBNewElo)];
        }

        private void SaveFighterEloRecords()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "fighter_elo_records.json");
            string jsonString = System.Text.Json.JsonSerializer.Serialize(_allFightersEloTable, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, jsonString);
        }

        private void SaveFighterPeakEloRecords(Dictionary<string, FighterPeakEloDetails> fighterPeakEloTable)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "fighter_peak_elo_records.json");
            string jsonString = System.Text.Json.JsonSerializer.Serialize(fighterPeakEloTable, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, jsonString);
        }

        public void GenerateEloAllFighters()
        {
            string jsonString = File.ReadAllText(_allFightsJSONPath);
            var allFights = JsonSerializer.Deserialize<List<FightDetails>>(jsonString);
            allFights = allFights.OrderBy(f => f.Date).ToList();

            foreach (var currFight in allFights) 
            {
                if (currFight.FighterAResult == "W" || currFight.FighterAResult == "L" || currFight.FighterAResult == "D")
                {
                    bool fighterAFirstFight = !_allFightersEloTable.ContainsKey(currFight.FighterA);
                    bool fighterBFirstFight = !_allFightersEloTable.ContainsKey(currFight.FighterB);

                    var fighterACurrElo = fighterAFirstFight ? 1000
                            : _allFightersEloTable[currFight.FighterA][_allFightersEloTable[currFight.FighterA].Count-1].Elo;
                    var fighterBCurrElo = (fighterBFirstFight) ? 1000
                            : _allFightersEloTable[currFight.FighterB][_allFightersEloTable[currFight.FighterB].Count-1].Elo;

                    if (fighterAFirstFight)
                    {
                        _allFightersEloTable.Add(currFight.FighterA, new List<FighterEloRow>
                        {
                            new FighterEloRow
                            {
                                Name = currFight.FighterA,
                                Opponent = "",
                                Outcome = "",
                                Date = DateTime.MinValue,
                                Wins = 0,
                                Losses = 0,
                                Draws = 0,
                                Elo = fighterACurrElo
                            }
                        });
                    }
                    if (fighterBFirstFight)
                    {
                        _allFightersEloTable.Add(currFight.FighterB, new List<FighterEloRow>
                        {
                            new FighterEloRow
                            {
                                Name = currFight.FighterB,
                                Opponent = "",
                                Outcome = "",
                                Date = DateTime.MinValue,
                                Wins = 0,
                                Losses = 0,
                                Draws = 0,
                                Elo = fighterBCurrElo
                            }
                        });
                    }

                    var newElos = CalculateElo(fighterACurrElo, fighterBCurrElo, currFight.FighterAResult, currFight.FighterBResult);

                    var lastFightA = _allFightersEloTable[currFight.FighterA][_allFightersEloTable[currFight.FighterA].Count-1];
                    _allFightersEloTable[currFight.FighterA].Add(new FighterEloRow
                    {
                        Name = currFight.FighterA,
                        Opponent = currFight.FighterB,
                        Outcome = currFight.FighterAResult,
                        Date = currFight.Date,
                        Wins = (currFight.FighterAResult == "W") ? lastFightA.Wins+1 : lastFightA.Wins,
                        Losses = (currFight.FighterAResult == "L") ? lastFightA.Losses+1 : lastFightA.Losses,
                        Draws = (currFight.FighterAResult == "D") ? lastFightA.Draws+1 : lastFightA.Draws,
                        Elo = newElos[0]
                    });

                    var lastFightB = _allFightersEloTable[currFight.FighterB][_allFightersEloTable[currFight.FighterB].Count-1];
                    _allFightersEloTable[currFight.FighterB].Add(new FighterEloRow
                    {
                        Name = currFight.FighterB,
                        Opponent = currFight.FighterA,
                        Outcome = currFight.FighterBResult,
                        Date = currFight.Date,
                        Wins = (currFight.FighterBResult == "W") ? lastFightB.Wins + 1 : lastFightB.Wins,
                        Losses = (currFight.FighterBResult == "L") ? lastFightB.Losses + 1 : lastFightB.Losses,
                        Draws = (currFight.FighterBResult == "D") ? lastFightB.Draws + 1 : lastFightB.Draws,
                        Elo = newElos[1]
                    });
                }
            }
            SaveFighterEloRecords();
        }

        public void GetFightersPeakElo(string fighterEloRecordsPath)
        {
            string jsonString = File.ReadAllText(fighterEloRecordsPath);
            var fighterEloRecords = JsonSerializer.Deserialize<Dictionary<string, List<FighterEloRow>>>(jsonString);

            Dictionary<string, FighterPeakEloDetails> fighterPeakEloTable = new Dictionary<string, FighterPeakEloDetails>();
            foreach (var fighter in fighterEloRecords)
            {
                var fighterPeakElo = fighter.Value.MaxBy(r => r.Elo);

                var fighterPeakEloDetails = new FighterPeakEloDetails
                {
                    Elo = fighterPeakElo.Elo,
                    Date = fighterPeakElo.Date,
                    Wins = fighterPeakElo.Wins,
                    Losses  = fighterPeakElo.Losses,
                    Draws = fighterPeakElo.Draws,
                    LastOpp = fighterPeakElo.Opponent,
                };
                fighterPeakEloTable.Add(fighterPeakElo.Name, fighterPeakEloDetails);
            }

            var orderedElo = fighterPeakEloTable.OrderByDescending(t => t.Value.Elo).ToDictionary(t => t.Key, t => t.Value);

            SaveFighterPeakEloRecords(orderedElo);
        }
    }
}
