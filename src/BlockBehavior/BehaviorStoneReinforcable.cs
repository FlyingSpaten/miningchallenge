using nrw.frese.miningchallenge.block;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System.Text.RegularExpressions;

namespace nrw.frese.miningchallenge.behavior
{
    public class BehaviorStoneReinforcable : CollectibleBehavior
    {
        public BehaviorStoneReinforcable(CollectibleObject collObj) : base(collObj)
        {
        }

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel != null)
            {
                IPlayer byPlayer = (byEntity as EntityPlayer).Player;
                IWorldAccessor world = byEntity.World;
                BlockPos pos = blockSel.Position;
                Block selectedBlock = world.BlockAccessor.GetBlock(pos);

                if (selectedBlock is BaseBlockSupport)
                {
                    BaseBlockSupport supportBlock = (BaseBlockSupport)selectedBlock;
                    ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
                    if (hotbarSlot.Itemstack.Item != null)
                    {
                        string itemCode = hotbarSlot.Itemstack.Item.Code.ToString();
                        if (Regex.Match(itemCode, supportBlock.GetUpgradeItemCode(supportBlock)).Success)
                        {
                            handHandling = EnumHandHandling.PreventDefaultAction;
                            ICoreAPI api = world.Api;
                            if (api.Side == EnumAppSide.Client)
                            {
                                ((byEntity as EntityPlayer).Player as IClientPlayer).TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                            }
                            handHandling = EnumHandHandling.PreventDefault;
                            byEntity.Attributes.SetInt("aimingCancel", 1);
                        }
                    }
                }
            }
        }
    }
}
