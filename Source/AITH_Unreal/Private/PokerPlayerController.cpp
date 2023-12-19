// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerPlayerController.h"

#include "PokerTableState.h"
#include "Net/UnrealNetwork.h"

void APokerPlayerController::GetLifetimeReplicatedProps(TArray <FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(APokerPlayerController, Cards);
}
void APokerPlayerController::BeginPlay()
{
	Super::BeginPlay();
	if (GetLocalRole() == ROLE_Authority)
	{
		auto ts = GetWorld()->GetGameState<APokerTableState>();
		Cards.Emplace(ts->Draw());
		Cards.Emplace(ts->Draw());
		Cards.Emplace(ts->Draw());
	}
}