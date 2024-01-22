// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "PokerEngine.h"
#include "PokerGameSession.h"
#include "PokerPlayerController.h"
#include "GameFramework/PlayerState.h"
#include "Kismet/GameplayStatics.h"
#include "GameFramework/GameModeBase.h"
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
	FCoinAmount BetAmount;

	UPROPERTY(Category = "Bank", Replicated, BlueprintReadWrite)
	FCoinAmount Balance;
	virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

	UFUNCTION()
	virtual void BeginPlay() override
	{
		if (GetLocalRole() == ROLE_Authority)
		{
			BetAmount = FCoinAmount::Random();
			Balance = FCoinAmount::Random();
		}
	}

	UFUNCTION(Server, Reliable, BlueprintCallable)
	virtual void Server_ConfirmHandOption(FBetAction action, APokerPlayerController* caller);
};
