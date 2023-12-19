// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "PokerEngine.h"
#include "GameFramework/PlayerState.h"
#include "PokerPlayerState.generated.h"

/**
 * 
 */
UCLASS()
class AITH_UNREAL_API APokerPlayerState : public APlayerState
{
	GENERATED_BODY()

public:
	UPROPERTY(Category = "Cards", Replicated, BlueprintReadWrite)
	FCoinAmount BetAmount = FCoinAmount::Random();
	void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;
	
	UFUNCTION(BlueprintCallable, Category = "Bets")
	FString GetBetState(APokerPlayerState* player)
	{
		return FString::FromInt(player->BetAmount.CalculateTotalAmount(BetAmount, FCoinValueTable()));
	}
};
