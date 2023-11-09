using System;
using System.Collections.Generic;
using System.Linq;
namespace AceInTheHole.Engine
{
    public class Deck : List<Card>
    {
        readonly Random _rng;
        public Deck()
        {
            _rng = new Random();
            Reset();
        }
        public Card Draw()
        {
            var card = this.ElementAt(0);
            this.Remove(card);
            return card;
        }

        public void Reset()
        {
            this.Clear();
            this.AddRange(GenerateRandom52());
        }
        
        private IEnumerable<Card> GenerateRandom52()
        {
            return ((Suit[])Enum.GetValues(typeof(Suit)))
                .SelectMany(d => ((Number[])Enum.GetValues(typeof(Number))).Select(n => new Card { Number = n, Suit = d }))
                .OrderBy(a => _rng.Next());
        }
    }
}