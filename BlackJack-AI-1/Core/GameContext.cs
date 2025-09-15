using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames.Core
{
    public class GameContext
    {
        // Add properties like current scores, game-specific info
        public List<Participant> Participants { get; set; }
        public Dealer Dealer { get; set; }
        public Deck Deck { get; set; }
        public string RulesDescription { get; set; }
        public CardGame Game { get; set; }
    }
}