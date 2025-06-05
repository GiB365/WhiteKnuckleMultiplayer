using System;
using HarmonyLib;
using WK_Multiplayer_Mod;

namespace WK_Multiplayer_Mod.Patches;

internal class CommandConsolePatch
{
	[HarmonyPatch(typeof(CommandConsole), "Awake")]
	[HarmonyPostfix]
	private static void AddMultiplayerCommands(CommandConsole __instance) {
		Multiplayer_Mod.instance.CreateCommands();
	}
}