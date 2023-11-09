using System;
namespace AceInTheHole.Engine
{
    public enum Number
    {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }
    
    public static class NumberExtensions
    {
        public static string GetId(this Suit card)
        {
            return card switch
            {
                Suit.Diamonds => "D",
                Suit.Clubs => "C",
                Suit.Hearts => "H",
                Suit.Spades => "S",
                _ => throw new ArgumentOutOfRangeException(nameof(card), card, null)
            };
        }
        public static string GetId(this Number number)
        {
            return number switch
            {
                Number.Ace => "A",
                Number.Two => "2",
                Number.Three => "3",
                Number.Four => "4",
                Number.Five => "5",
                Number.Six => "6",
                Number.Seven => "7",
                Number.Eight => "8",
                Number.Nine => "9",
                Number.Ten => "10",
                Number.Jack => "J",
                Number.Queen => "Q",
                Number.King => "K",
                _ => throw new ArgumentOutOfRangeException(nameof(number), number, null)
            };
        }    
    }
}