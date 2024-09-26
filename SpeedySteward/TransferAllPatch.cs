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
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SpeedySteward
{
    [HarmonyPatch(typeof(SPInventoryVM), "TransferAll")]
    public class TransferAllPatch
    {
        public static bool Prefix(SPInventoryVM __instance, bool isBuy, InventoryLogic ____inventoryLogic, CharacterObject ____currentCharacter, int ____donationMaxShareableXp)
        {
            __instance.IsRefreshed = false;
            List<TransferCommand> list = new List<TransferCommand>(__instance.LeftItemListVM.Count);
            MBBindingList<SPItemVM> mBBindingList = (isBuy ? __instance.LeftItemListVM : __instance.RightItemListVM);
            MobileParty party = (isBuy ? MobileParty.MainParty : ____inventoryLogic.OppositePartyFromListener?.MobileParty);
            float num = 0f;
            var getCapacityBudget = AccessTools.Method(typeof(SPInventoryVM), "GetCapacityBudget");
            float capacityBudget = (float) getCapacityBudget.Invoke(__instance, new object[] { party, isBuy });
            float num2 = mBBindingList.FirstOrDefault((SPItemVM x) => !x.IsFiltered && !x.IsLocked)?.ItemRosterElement.EquipmentElement.GetEquipmentElementWeight() ?? 0f;
            bool flag = capacityBudget <= num2;
            InventoryLogic.InventorySide fromSide = ((!isBuy) ? InventoryLogic.InventorySide.PlayerInventory : InventoryLogic.InventorySide.OtherInventory);
            InventoryLogic.InventorySide inventorySide = (isBuy ? InventoryLogic.InventorySide.PlayerInventory : InventoryLogic.InventorySide.OtherInventory);
            List<SPItemVM> list2 = new List<SPItemVM>();
            bool flag2 = ____inventoryLogic.CanInventoryCapacityIncrease(inventorySide);

            int trackedTransferXp = (int) ____inventoryLogic.XpGainFromDonations;
            bool startedInRedXp = false;
            if (trackedTransferXp > ____donationMaxShareableXp)
            {
                startedInRedXp = true;
            }

            for (int i = 0; i < mBBindingList.Count; i++)
            {
                SPItemVM sPItemVM = mBBindingList[i];
                if (sPItemVM == null || sPItemVM.IsFiltered || sPItemVM == null || sPItemVM.IsLocked || sPItemVM == null || !sPItemVM.IsTransferable)
                {
                    continue;
                }

                int num3 = sPItemVM.ItemRosterElement.Amount;
                if (____inventoryLogic.IsDiscardDonating && !isBuy && !startedInRedXp)
                {
                    if (trackedTransferXp >= ____donationMaxShareableXp)
                    {
                        continue;
                    }

                    ItemDiscardModel discardModel = Campaign.Current.Models.ItemDiscardModel;
                    int xpBonusForDiscardingItem = discardModel.GetXpBonusForDiscardingItem(sPItemVM.ItemRosterElement.EquipmentElement.Item);
                    for (int n = 1; n <= num3; n++)
                    {
                        trackedTransferXp += xpBonusForDiscardingItem;
                        if (trackedTransferXp >= ____donationMaxShareableXp)
                        {
                            num3 -= (num3 - n);
                            var expMessage = new InformationMessage(String.Format("Stack for {0} changed from {1} to {2}",
                                sPItemVM.ItemRosterElement.EquipmentElement.Item, num3 + n, num3));
                            InformationManager.DisplayMessage(expMessage);
                            break;
                        }
                    }
                }

                if (!flag)
                {
                    float equipmentElementWeight = sPItemVM.ItemRosterElement.EquipmentElement.GetEquipmentElementWeight();
                    float num4 = num + equipmentElementWeight * (float)num3;
                    if (flag2)
                    {
                        if (____inventoryLogic.GetCanItemIncreaseInventoryCapacity(mBBindingList[i].ItemRosterElement.EquipmentElement.Item))
                        {
                            list2.Add(mBBindingList[i]);
                            continue;
                        }
                        if (num4 >= capacityBudget && list2.Count > 0)
                        {
                            List<TransferCommand> list3 = new List<TransferCommand>(list2.Count);
                            for (int j = 0; j < list2.Count; j++)
                            {
                                SPItemVM sPItemVM2 = list2[j];
                                TransferCommand item = TransferCommand.Transfer(sPItemVM2.ItemRosterElement.Amount, fromSide, inventorySide, sPItemVM2.ItemRosterElement, EquipmentIndex.None, EquipmentIndex.None, ____currentCharacter, !__instance.IsInWarSet);
                                list3.Add(item);
                            }
                            ____inventoryLogic.AddTransferCommands(list3);
                            list3.Clear();
                            list2.Clear();
                            capacityBudget = (float) getCapacityBudget.Invoke(__instance, new object[] { party, isBuy });
                        }
                    }
                    if (num3 > 0 && num4 > capacityBudget)
                    {
                        num3 = MBMath.ClampInt(num3, 0, TaleWorlds.Library.MathF.Floor((capacityBudget - num) / equipmentElementWeight));
                        i = mBBindingList.Count;
                    }
                    num += (float)num3 * equipmentElementWeight;
                }
                if (num3 > 0)
                {
                    TransferCommand item2 = TransferCommand.Transfer(num3, fromSide, inventorySide, sPItemVM.ItemRosterElement, EquipmentIndex.None, EquipmentIndex.None, ____currentCharacter, !__instance.IsInWarSet);
                    list.Add(item2);
                }
            }
            if (num <= capacityBudget)
            {
                foreach (SPItemVM item4 in list2)
                {
                    TransferCommand item3 = TransferCommand.Transfer(item4.ItemRosterElement.Amount, fromSide, inventorySide, item4.ItemRosterElement, EquipmentIndex.None, EquipmentIndex.None, ____currentCharacter, !__instance.IsInWarSet);
                    list.Add(item3);
                }
            }
            ____inventoryLogic.AddTransferCommands(list);
            var refreshInformationValues = AccessTools.Method(typeof(SPInventoryVM), "RefreshInformationValues");
            var executeRemoveZeroCounts = AccessTools.Method(typeof(SPInventoryVM), "ExecuteRemoveZeroCounts");
            refreshInformationValues.Invoke(__instance, null);
            executeRemoveZeroCounts.Invoke(__instance, null);
            __instance.IsRefreshed = true;
            return false;
        }
    }
}
