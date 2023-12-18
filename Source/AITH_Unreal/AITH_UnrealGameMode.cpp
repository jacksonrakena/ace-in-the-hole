// Copyright Epic Games, Inc. All Rights Reserved.

#include "AITH_UnrealGameMode.h"
#include "AITH_UnrealCharacter.h"
#include "UObject/ConstructorHelpers.h"

AAITH_UnrealGameMode::AAITH_UnrealGameMode()
{
	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(TEXT("/Game/ThirdPerson/Blueprints/BP_ThirdPersonCharacter"));
	if (PlayerPawnBPClass.Class != NULL)
	{
		DefaultPawnClass = PlayerPawnBPClass.Class;
	}
}
