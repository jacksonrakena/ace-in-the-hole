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

USTRUCT(BlueprintType)
struct AITH_UNREAL_API FCoinValueTable {
	GENERATED_BODY();
	float Value1;
	float Value2;
	float Value3;
	float Value4;
	float Value5;
	FCoinValueTable():Value1(0.1),Value2(0.2),Value3(0.5),Value4(1),Value5(2){}
	FCoinValueTable(const float V1, const float V2, const float V3, const float V4, const float V5): Value1(V1),Value2(V2),Value3(V3),Value4(V4),Value5(V5){}
};

inline FCoinValueTable DefaultValueTable()
{
	return FCoinValueTable();
}
USTRUCT(BlueprintType)
struct AITH_UNREAL_API FCoinAmount {
	GENERATED_BODY();
	int32_t Amount1;
	int32_t Amount2;
	int32_t Amount3;
	int32_t Amount4;
	int32_t Amount5;

	static int32_t CalculateTotalAmount(const FCoinAmount& Amount, const FCoinValueTable& ValueTable)
	{
		return (Amount.Amount1*ValueTable.Value1)+(Amount.Amount2*ValueTable.Value2)+(Amount.Amount3*ValueTable.Value3)+(Amount.Amount4*ValueTable.Value4)+(Amount.Amount5*ValueTable.Value5);
	}
};