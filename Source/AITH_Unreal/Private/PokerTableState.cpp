// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerTableState.h"

#include "PokerGameSession.h"
#include "Kismet/GameplayStatics.h"
#include "Net/UnrealNetwork.h"

void APokerTableState::BeginPlay()
{
	Super::BeginPlay();
	if (GetLocalRole() == ROLE_Authority)
	{
		auto gi = Cast<APokerGameSession>(UGameplayStatics::GetGameMode(this)->GameSession.Get());
		Cards.Emplace(gi->Draw());
		Cards.Emplace(gi->Draw());
		Cards.Emplace(gi->Draw());
	}
}
void APokerTableState::OnRep_Deck(){}
void APokerTableState::GetLifetimeReplicatedProps(TArray <FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(APokerTableState, Cards);
	DOREPLIFETIME(APokerTableState, Call);
}
