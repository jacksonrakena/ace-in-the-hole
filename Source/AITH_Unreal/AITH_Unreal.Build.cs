// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class AITH_Unreal : ModuleRules
{
	public AITH_Unreal(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PrivateDependencyModuleNames.AddRange(new string[] { "Slate", "SlateCore" });
		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "EnhancedInput" });
	}
}
