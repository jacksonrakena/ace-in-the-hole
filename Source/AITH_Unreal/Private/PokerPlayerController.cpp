// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerPlayerController.h"

#include "PokerTableState.h"
#include "Net/UnrealNetwork.h"
#include "PokerGameSession.h"
#include "Kismet/GameplayStatics.h"

class APokerGameSession;

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
		auto gi = Cast<APokerGameSession>(UGameplayStatics::GetGameMode(this)->GameSession.Get());
		Cards.Emplace(gi->Draw());
		Cards.Emplace(gi->Draw()); 
	}
}