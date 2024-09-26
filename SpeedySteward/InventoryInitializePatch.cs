using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;

namespace SpeedySteward
{
    [HarmonyPatch(typeof(SPInventoryVM), "InitializeInventory")]
    public class InventoryInitializePatch
    {
        public static void Postfix(SPInventoryVM __instance, int ____donationMaxShareableXp)
        {
            var expMessage = new InformationMessage(String.Format("Maximum troop exp: {0}", ____donationMaxShareableXp));
            InformationManager.DisplayMessage(expMessage);
        }
    }
}
