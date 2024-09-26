using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace SpeedySteward
{
    [HarmonyPatch(typeof(SPInventoryVM), "OnDonationXpChange")]
    public class DonationChangePatch
    {
        public static bool Prefix(SPInventoryVM __instance,
            InventoryLogic ____inventoryLogic,
            int ____donationMaxShareableXp)
        {
            int num = (int)____inventoryLogic.XpGainFromDonations;
            bool isDonationXpGainExceedsMax = false;
            if (num > ____donationMaxShareableXp)
            {
                isDonationXpGainExceedsMax = true;
            }
            __instance.IsDonationXpGainExceedsMax = isDonationXpGainExceedsMax;
            __instance.HasGainedExperience = num > 0;
            MBTextManager.SetTextVariable("XP_AMOUNT", String.Format("{0}/{1}",num, ____donationMaxShareableXp));
            __instance.ExperienceLbl = ((num == 0) ? "" : GameTexts.FindText("str_inventory_donation_label").ToString());
            return false;
        }
    }
}
