#pragma once
#include "Internationalization/Text.h"
#include "Runtime/CoreUObject/Public/UObject/ObjectMacros.h"

enum ENumber {
	Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
};

enum ESuit { Diamonds, Clubs, Hearts, Spades };

class FNumberUtil {
	FText GetNumber(ENumber num)
	{
		switch (num)
		{
		case Ace: return FText::FromString("A");
		case Two: return FText::FromString("2");
		case Three: return FText::FromString("3");
		case Four: return FText::FromString("4");
		case Five: return FText::FromString("5");
		case Six: return FText::FromString("6");
		case Seven: return FText::FromString("7");
		case Eight: return FText::FromString("8");
		case Nine: return FText::FromString("9");
		case Ten: return FText::FromString("10");
		case Jack: return FText::FromString("J");
		case Queen: return FText::FromString("Q");
		case King: return FText::FromString("K");
		}
		return FText::GetEmpty();
	}
	FText GetName(ESuit esuit)
	{
		switch (esuit)
		{
		case Diamonds: return FText::FromString("D");
		case Clubs: return FText::FromString("C");
		case Hearts: return FText::FromString("H");
		case Spades: return FText::FromString("S");
		}
		return FText::GetEmpty();
	}
};


struct FCard {
	const ENumber Number;
	const ESuit Suit;

	FCard(const ENumber number, const ESuit suit) : Number(number), Suit(suit) {}

	bool operator >(const FCard Left, const FCard Right) const
	{
		return Left.Number > Right.Number;
	}
	bool operator ==(const FCard Left, const FCard Right) const
	{
		return Left.Suit == Right.Suit && Left.Number == Right.Number;
	}
};

USTRUCT(BlueprintType)
struct FCardDeck {
	GENERATED_BODY()
public:
	TSet<FCard> Cards;
	FCardDeck()
	{
		for (int num = 1; num < 14; num++)
		{
			for (int suit = 0; suit < 4; suit++)
			{
				Cards.Emplace(FCard(static_cast<ENumber>(num), static_cast<ESuit>(suit)));
			}
		}
	}
};