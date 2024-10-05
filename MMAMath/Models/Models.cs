using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMAMath.Models
{
    public class EventDetails
    {
        public required string Name { get; set; }
        public DateTime Date { get; set; }
        public required string Link { get; set; }
    }

    public class FightDetails
    {
        public required string FighterA { get; set; }
        public required string FighterB { get; set; }
        public required string FighterAResult { get; set; }
        public required string FighterBResult { get; set; }
        public required string Method { get; set; }
        public string? Score { get; set; }
        public DateTime Date { get; set; }
        public string Id
        {
            get
            {
                var fighters = new[] { FighterA, FighterB }.OrderBy(f => f).ToArray();
                string idString = $"{fighters[0]}_{fighters[1]}_{Date:yyyy-MM-dd}";
                return idString;
            }
        }
    }

    public class FighterEloRow
    {
        public required string Name { get; set; }
        public string Opponent {  get; set; }
        public string Outcome { get; set; }
        public DateTime? Date { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int Elo { get; set; }
    }

    public class FighterPeakEloDetails
    {
        public int Elo { get; set; }
        public DateTime? Date { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public string LastOpp {  get; set; }
    }

}
