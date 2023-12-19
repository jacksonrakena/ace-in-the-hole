// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Internationalization/Text.h"
#include "Net/UnrealNetwork.h"
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

USTRUCT(BlueprintType)
struct AITH_UNREAL_API FUCard {
	GENERATED_BODY();

public:
	UPROPERTY(Category = "State", EditAnywhere, BlueprintReadOnly)
	ENumber Number = ENumber::Ace;
	UPROPERTY(Category = "State", EditAnywhere, BlueprintReadOnly)
	ESuit Suit = ESuit::Clubs;

	FUCard(){}

	bool operator >(const FUCard Right) const
	{
		return Number > Right.Number;
	}
	bool operator ==(const FUCard Right) const
	{
		return Suit == Right.Suit && Number == Right.Number;
	}
};

inline uint32 GetTypeHash(const FUCard& Card)
{
	return FCrc::MemCrc32(&Card, sizeof(FUCard));
}