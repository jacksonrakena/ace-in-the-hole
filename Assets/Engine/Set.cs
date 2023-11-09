using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
namespace AceInTheHole.Engine
{
    public enum SetType
    {
        /**
         * Cards in a row. All of same suit. Royals (K,Q,J) in.
         */
        RoyalFlush,

        /*
         * Cards in a row. All of same suit.
         */
        StraightFlush,

        /*
         * At least four cards of the same number.
         */
        FourOfAKind,

        /*
         * Three cards of the same number, and two cards of another number.
         */
        FullHouse,

        /*
         * All cards of the same suit.
         */
        Flush,

        /*
         * Cards in a row.
         */
        Straight,

        /*
         * Three cards of the same number.
         */
        ThreeOfAKind,

        /*
         * Two pairs of the same number.
         */
        TwoPair,

        /*
         * One pair of the same number.
         */
        OnePair,

        /*
         * A card of a high value.
         */
        HighCard
    }
    public struct Set : INetworkSerializable, IComparable<Set>
    {
        public List<Card> Cards;
        public SetType Type;

        public override string ToString()
        {
            return $"{this.Type}[{string.Join(",", this.Cards.Select(e => e.ToString()))}]";
        }


        private static List<List<Card>> FindStraights(IEnumerable<Card> cards)
        {
            var result = new List<List<Card>>();
            var cardlist = cards.OrderBy(e => e.Number).ToList();

            for (int i = 0; i < cardlist.Count; i++)
            {
                if (i + 5 > cardlist.Count) continue; // Cannot possible start a straight here
                var card = cardlist.ElementAt(i);
                var innerres = new List<Card>();

                var valid = true;
                for (int j = 0; j < 5; j++)
                {
                    var straightCardIndex = i + j;
                    var nextStraightCard = cardlist.ElementAt(straightCardIndex);
                    if (!innerres.Any(d => d.Number != Number.Ace) && j == 4 && cardlist.Any(d => d.Number == Number.Ace) && valid)
                    {
                        innerres.Add(cardlist.First(e => e.Number == Number.Ace));
                    }
                    else if ((int)nextStraightCard.Number != (int)card.Number + j)
                    {
                        valid = false;
                    }
                    else innerres.Add(nextStraightCard);
                }
                if (valid)
                {
                    result.Add(innerres);
                }
            }

            return result;
        }
        
        public static bool operator >(Set left, Set right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator <(Set left, Set right)
        {
            return left.CompareTo(right) < 0;
        }
        
        public static List<Set> FindAllPossibleSets(NetworkList<Card> playerCards, NetworkList<Card> dealerVisible)
        {
            var possibleSets = new List<Set>();
            var allCards = new List<Card>();
            foreach (var card in playerCards) allCards.Add(card);
            foreach (var card in dealerVisible) allCards.Add(card);

            // Straight flush and regular straights
            var straights = FindStraights(allCards);
            foreach (var st in straights)
            {
                // Straight flush
                if (st.GroupBy(e => e.Suit).Count() == 1)
                {
                    possibleSets.Add(new Set { Type = SetType.StraightFlush, Cards = st });
                }
                
                possibleSets.Add(new Set { Type = SetType.Straight, Cards = st});
            }
            
            // Flushes (Royal flush, Flush)
            var flushCards = allCards.GroupBy(k => k.Suit);
            foreach (var suit in flushCards)
            {
                if (suit.Count() >= 5)
                {
                    // Regular flush
                    possibleSets.Add(new Set { Type = SetType.Flush, Cards = suit.ToList() });
                    
                    // Royal flush
                    var royalFlush = new[] { Number.Ace, Number.King, Number.Queen, Number.Jack, Number.Ten };
                    var isRoyalFlush = true;
                    var royalFlushCards = new List<Card>();
                    foreach (var royalFlushCard in royalFlush)
                    {
                        if (suit.All(e => e.Number != royalFlushCard)) isRoyalFlush = false;
                        else royalFlushCards.Add(suit.First(e => e.Number == royalFlushCard));
                    }
                    if (isRoyalFlush)
                    {
                        possibleSets.Add(new Set { Type = SetType.RoyalFlush, Cards = royalFlushCards });
                    }
                }
            }
            
            // 4/3 of a kind, full house
            var numberGroups = allCards.GroupBy(k => k.Number).ToList();
            var pairs = new List<List<Card>>();
            foreach (var number in numberGroups)
            {
                // 4 of a kind
                if (number.Count() >= 4) possibleSets.Add(new Set { Type = SetType.FourOfAKind, Cards = number.ToList() });
                else if (number.Count() == 3)
                {
                    // Full house
                    if (numberGroups.Any(e => e.Count() == 2 && e.Key != number.Key))
                    {
                        possibleSets.Add(new Set { Type = SetType.FullHouse, Cards = number.ToList().Concat(
                            numberGroups.First(e => e.Count() == 2 && e.Key != number.Key).ToList()).ToList() });
                    }
                    // 3 of a kind
                    else
                    {
                        possibleSets.Add(new Set { Type = SetType.ThreeOfAKind, Cards = number.ToList()});
                    }
                }
                else if (number.Count() == 2)
                {
                    pairs.Add(number.ToList());
                }
            }
            pairs = pairs.OrderByDescending(e => e.First().Number).ToList();
            
            // Two and one pair
            if (pairs.Count >= 2) possibleSets.Add(new Set { Type = SetType.TwoPair, Cards = pairs.Take(2).SelectMany(e=>e).ToList()});
            if (pairs.Count >= 1) possibleSets.Add(new Set { Type = SetType.OnePair, Cards = pairs.First() });
            
            // High card 
            foreach (var cards in allCards) possibleSets.Add(new Set { Type = SetType.HighCard, Cards = new List<Card> { cards }});
            

            return possibleSets;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Set set) return false;
            return set.CompareTo(this) == 0;
        }

        public int CompareTo(Set other)
        {
            var typeComparison = other.Type.CompareTo(Type);
            if (typeComparison != 0) return typeComparison;

            return Cards.Max().Number.CompareTo(other.Cards.Max().Number);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Type);
                reader.ReadValueSafe(out int nCardSize);
                Cards = new List<Card>(nCardSize);
                for (int i = 0; i < nCardSize; i++)
                {
                    reader.ReadValueSafe(out Card card);
                    Cards.Add(card);
                }
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Type);
                writer.WriteValueSafe(Cards?.Count ?? 0);
                if (Cards != null)
                {
                    foreach (var c in Cards)
                    {
                        writer.WriteValueSafe(c);
                    }
                }
            }
        }

    }
}