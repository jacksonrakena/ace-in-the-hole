// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Internationalization/Text.h"
#include "PokerEngine.generated.h"

/**
 * 
 */
class AITH_UNREAL_API PokerEngine
{
public:
	PokerEngine();
	~PokerEngine();
};

UENUM(BlueprintType)
enum class ENumber {
	Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
};

UENUM(BlueprintType)
enum class ESuit {
	Diamonds, Clubs, Hearts, Spades
};

class FNumberUtil {
public:
	static FString GetNumber(ENumber num)
	{
		switch (num)
		{
		case ENumber::Ace: return FString("A");
		case ENumber::Two: return FString("2");
		case ENumber::Three: return FString("3");
		case ENumber::Four: return FString("4");
		case ENumber::Five: return FString("5");
		case ENumber::Six: return FString("6");
		case ENumber::Seven: return FString("7");
		case ENumber::Eight: return FString("8");
		case ENumber::Nine: return FString("9");
		case ENumber::Ten: return FString("10");
		case ENumber::Jack: return FString("J");
		case ENumber::Queen: return FString("Q");
		case ENumber::King: return FString("K");
		}
		return FString("Unknown");
	}
	static FString GetName(ESuit esuit)
	{
		switch (esuit)
		{
		case ESuit::Diamonds: return FString("D");
		case ESuit::Clubs: return FString("C");
		case ESuit::Hearts: return FString("H");
		case ESuit::Spades: return FString("S");
		}
		return FString("Unknown");
	}
};

UCLASS(BlueprintType)
class AITH_UNREAL_API UCard : public UObject {
	GENERATED_BODY()

public:
	UPROPERTY(Category = "State", EditAnywhere)
	ENumber Number = ENumber::Ace;
	UPROPERTY(Category = "State", EditAnywhere)
	ESuit Suit = ESuit::Clubs;

	UCard(const ENumber number, const ESuit suit) : Number(number), Suit(suit) {}
	UCard(){}

	bool operator >(const UCard Right) const
	{
		return Number > Right.Number;
	}
	bool operator ==(const UCard Right) const
	{
		return Suit == Right.Suit && Number == Right.Number;
	}

	UFUNCTION(BlueprintPure, Category="Bank")
	FORCEINLINE FString ToText() const
	{
		FStringFormatOrderedArguments Args;
		Args.Add(FNumberUtil::GetName(Suit));
		Args.Add(FNumberUtil::GetNumber(Number));
		return FString::Format(TEXT("{0}{1}"), Args);
	}
};

inline uint32 GetTypeHash(const UCard& Card)
{
	return FCrc::MemCrc32(&Card, sizeof(UCard));
}

UCLASS(BlueprintType)
class AITH_UNREAL_API UCardDeck : public UObject {
	GENERATED_BODY()
public:
	UPROPERTY(Category = "State", EditAnywhere, BlueprintReadWrite)
	TSet<UCard*> Cards;
	UCardDeck()
	{
		auto Card = NewObject<UCard>();
		Card->Number = static_cast<ENumber>(FMath::RandRange(0, 12));
		Card->Suit = static_cast<ESuit>(FMath::RandRange(0, 3));
		Cards.Emplace(Card);

		auto Card2 = NewObject<UCard>();
		Card2->Number = static_cast<ENumber>(FMath::RandRange(0, 12));
		Card2->Suit = static_cast<ESuit>(FMath::RandRange(0, 3));
		Cards.Emplace(Card2);
	}
	
	UFUNCTION(BlueprintPure, Category="Bank")
	FORCEINLINE TSet<UCard*> GetCards() const { return Cards; }
};