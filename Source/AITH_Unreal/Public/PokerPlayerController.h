// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "PokerEngine.h"
#include "GameFramework/PlayerController.h"
#include "PokerPlayerController.generated.h"

/**
 * 
 */
UCLASS()
class AITH_UNREAL_API APokerPlayerController : public APlayerController
{
	GENERATED_BODY()
	
	UFUNCTION()
	virtual void BeginPlay() override;
	
public:
	void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;
	
	UPROPERTY(Category = "State", EditAnywhere, BlueprintReadOnly, Replicated)
	TArray<FUCard> Cards;
};
