﻿using System.Text;
using System.Text.Json;
using Microsoft.VisualBasic;
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

        private int[] CalculateElo(int fighterAElo, int fighterBElo, string fighterAResult, string fighterBResult, string method)
        {
            double k = 32;

            double fighterAExpectedResult = 1 / (1 + Math.Pow(10, ((fighterBElo - fighterAElo) / 400.0)));
            double fighterBExpectedResult = 1 / (1 + Math.Pow(10, ((fighterAElo - fighterBElo) / 400.0)));

            double winAdjustment = 0;
            if (method.Contains("KO") || method.Contains("Submission")) { winAdjustment = 0; }
            else if (method.Contains("Decision"))
            {
                if (method.Contains("Unanimous")) { winAdjustment = .1; }
                else if (method.Contains("Majority")) { winAdjustment = .2; }
                else if (method.Contains("Split")) { winAdjustment = .3; }
            }
            else if (method.Contains("Disqualification")) { winAdjustment = .45; }

            double AResult = 0;
            switch(fighterAResult)
            {
                case "W":
                    AResult = 1 - winAdjustment;
                    break;
                case "L":
                    AResult = 0 + winAdjustment;
                    break;
                case "D":
                    AResult = 0.5;
                    break;
            }

            double BResult = 0;
            switch(fighterBResult)
            {
                case "W":
                    BResult = 1 - winAdjustment;
                    break;
                case "L":
                    BResult = 0 + winAdjustment;
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

        public void ListUniqueMethods()
        {
            string jsonString = File.ReadAllText(_allFightsJSONPath);
            var allFights = JsonSerializer.Deserialize<List<FightDetails>>(jsonString);
            var uniqueMethods = allFights.Select(f => f.Method).Distinct().ToList();
            uniqueMethods = uniqueMethods.OrderBy(m => m).ToList();

            StringBuilder csvContent = new StringBuilder();
            foreach (var method in uniqueMethods)
            {
                csvContent.AppendLine(method);
            }

            string filePath = Path.Combine(AppContext.BaseDirectory, "methods.csv");
            File.WriteAllText(filePath, csvContent.ToString());
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
                    bool fighterAFirstFight = !_allFightersEloTable.ContainsKey(currFight.FighterAId);
                    bool fighterBFirstFight = !_allFightersEloTable.ContainsKey(currFight.FighterBId);

                    var fighterACurrElo = fighterAFirstFight ? 1000
                            : _allFightersEloTable[currFight.FighterAId][_allFightersEloTable[currFight.FighterAId].Count-1].Elo;
                    var fighterBCurrElo = (fighterBFirstFight) ? 1000
                            : _allFightersEloTable[currFight.FighterBId][_allFightersEloTable[currFight.FighterBId].Count-1].Elo;

                    if (fighterAFirstFight)
                    {
                        _allFightersEloTable.Add(currFight.FighterAId, new List<FighterEloRow>
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
                        _allFightersEloTable.Add(currFight.FighterBId, new List<FighterEloRow>
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

                    var newElos = CalculateElo(fighterACurrElo, fighterBCurrElo, currFight.FighterAResult, currFight.FighterBResult, currFight.Method);

                    var lastFightA = _allFightersEloTable[currFight.FighterAId][_allFightersEloTable[currFight.FighterAId].Count-1];
                    _allFightersEloTable[currFight.FighterAId].Add(new FighterEloRow
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

                    var lastFightB = _allFightersEloTable[currFight.FighterBId][_allFightersEloTable[currFight.FighterBId].Count-1];
                    _allFightersEloTable[currFight.FighterBId].Add(new FighterEloRow
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
                    Name = fighterPeakElo.Name,
                    Elo = fighterPeakElo.Elo,
                    Date = fighterPeakElo.Date,
                    Wins = fighterPeakElo.Wins,
                    Losses  = fighterPeakElo.Losses,
                    Draws = fighterPeakElo.Draws,
                    LastOpp = fighterPeakElo.Opponent,
                };
                fighterPeakEloTable.Add(fighter.Key, fighterPeakEloDetails);
            }

            var orderedElo = fighterPeakEloTable.OrderByDescending(t => t.Value.Elo).ToDictionary(t => t.Key, t => t.Value);

            SaveFighterPeakEloRecords(orderedElo);
        }
    }
}
