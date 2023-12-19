// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "PokerEngine.h"
#include "GameFramework/GameStateBase.h"
#include "PokerTableState.generated.h"

/** Please add a class description */
UCLASS(Blueprintable, BlueprintType)
class AITH_UNREAL_API APokerTableState : public AGameStateBase
{
	GENERATED_BODY()
public:
	void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

public:
	UFUNCTION()
	virtual void OnRep_Deck();
	
	/** Please add a variable description */
	UPROPERTY(Replicated, BlueprintReadOnly)
	UCardDeck* Deck;

	UFUNCTION()
	virtual void BeginPlay() override;
};
