using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames.Core
{
    public class Card
    {
        public string Suit { get; set; }
        public string Rank { get; set; }
        public override string ToString() => $"{Rank} of {Suit}";
    }
}