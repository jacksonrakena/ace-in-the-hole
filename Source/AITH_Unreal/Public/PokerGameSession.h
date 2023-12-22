// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "PokerEngine.h"
#include "Algo/RandomShuffle.h"
#include "GameFramework/GameSession.h"
#include "PokerGameSession.generated.h"

/**
 * 
 */
UCLASS()
class AITH_UNREAL_API APokerGameSession : public AGameSession
{
	GENERATED_BODY()
public:
	UPROPERTY(Category = "Cards", BlueprintReadWrite)
	TArray<FUCard> Deck;
	

	APokerGameSession()
	{
		for (int suit = 0; suit < 13; suit++)
		{
			for (int num = 0; num < 4; num++)
			{
				FUCard Card;
				Card.Number = static_cast<ENumber>(suit);
				Card.Suit = static_cast<ESuit>(num);
				Deck.Emplace(Card);
			}
		}
		Algo::RandomShuffle(Deck);
	}
	
	UFUNCTION(BlueprintCallable, Category="Cards")
	FUCard Draw()
	{
		auto card = Deck.Pop();
		return card;
	}
};
