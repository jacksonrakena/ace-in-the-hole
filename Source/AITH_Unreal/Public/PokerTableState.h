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
	
	// /** Please add a variable description */
	// UPROPERTY(Replicated, BlueprintReadOnly)
	// AUCardDeck* Deck;

	UPROPERTY(Category = "State", EditAnywhere, BlueprintReadOnly, Replicated)
	TArray<FUCard> Cards;

	UFUNCTION()
	virtual void BeginPlay() override;

	UFUNCTION(BlueprintCallable, Category="Cards")
	static FString ToText(FUCard Card)
	{
		FStringFormatOrderedArguments Args;
		Args.Add(FNumberUtil::GetName(Card.Suit));
		Args.Add(FNumberUtil::GetNumber(Card.Number));
		return FString::Format(TEXT("{0}{1}"), Args);
	}
};
