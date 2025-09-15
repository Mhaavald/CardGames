using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames.Core
{
    public class Participant
    {
        public string Name { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public IStrategy Strategy { get; set; }

        public void ReceiveCard(Card card)
        {
            Hand.Add(card);
        }

        public void ClearHand()
        {
            Hand.Clear();
        }
    }
}