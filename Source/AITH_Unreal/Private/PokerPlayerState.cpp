// Fill out your copyright notice in the Description page of Project Settings.


#include "PokerPlayerState.h"

#include "PokerPlayerController.h"

void APokerPlayerState::GetLifetimeReplicatedProps(TArray <FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(APokerPlayerState, BetAmount);
	DOREPLIFETIME(APokerPlayerState, Balance);
	DOREPLIFETIME(APokerPlayerState, Folded);
	DOREPLIFETIME(APokerPlayerState, InRound);
}

void APokerPlayerState::Server_ConfirmHandOption_Implementation(FBetAction action, APokerPlayerController* caller)
{
	auto gi = Cast<APokerGameSession>(UGameplayStatics::GetGameMode(this)->GameSession.Get());
	auto state = caller->GetPlayerState<APokerPlayerState>();
	auto bal = state->Balance;
	bal.Amount5 = 9999;
	state->Balance = bal;
	switch (action.Type)
	{
	case EBetActionType::Raise:
		break;
	case EBetActionType::CheckOrCall:
		break;
	case EBetActionType::Fold:
		state->Folded = true;
		break;
	default: ;
	}
}