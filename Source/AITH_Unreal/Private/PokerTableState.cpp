// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerTableState.h"

#include "Net/UnrealNetwork.h"

void APokerTableState::BeginPlay()
{
	Super::BeginPlay();
	if (GetLocalRole() == ROLE_Authority)
	{
		Cards.Emplace(Draw());
		Cards.Emplace(Draw());
		Cards.Emplace(Draw());
	}
}
FUCard APokerTableState::Draw()
{
	FUCard Card3;
	Card3.Number = static_cast<ENumber>(FMath::RandRange(0, 12));
	Card3.Suit = static_cast<ESuit>(FMath::RandRange(0, 3));
	return Card3;
}
void APokerTableState::OnRep_Deck(){}
void APokerTableState::GetLifetimeReplicatedProps(TArray <FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(APokerTableState, Cards);
}
