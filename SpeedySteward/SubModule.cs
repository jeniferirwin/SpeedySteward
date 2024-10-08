﻿using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SpeedySteward
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Harmony harmony = new Harmony("speedy_steward");
            harmony.PatchAll();
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            base.InitializeGameStarter(game, starterObject);
        }
    }

}
