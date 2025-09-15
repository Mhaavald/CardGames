using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames.Core
{
    public interface IStrategy
    {
        bool DecideToHit(Participant participant, GameContext context);
        string Name { get; }
    }
}