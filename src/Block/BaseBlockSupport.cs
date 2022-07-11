using System.Text.RegularExpressions;
using nrw.frese.miningchallenge.blockentity;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace nrw.frese.miningchallenge.block
{

    internal abstract class BaseBlockSupport : Block
    {
        public abstract double BaseTemporalSupport { get; }

        public abstract string UpgradeItemCode { get; }

        public abstract string UpgradedBlockCode { get; }

        public abstract string CreateReinforcementItemCode { get; }

        public abstract string GetUpgradeItemCode(Block currentBlock);

        public abstract string GetUpgradedBlockCode(Block currentBlock, Item upgradeItem);

        public abstract string GetCreateReinforcementItemCode(Block currentBlock);

        public abstract string GetReinforcementBlockCode(Block currentBlock, Item reinforcementItem);

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {

            bool result = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            if (result)
            {
                BlockPos blockPos = blockSel.Position;
                BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(blockPos);
                if (blockEntity != null && blockEntity is BaseBlockEntitySupport)
                {
                    ((BaseBlockEntitySupport)blockEntity).RecalculateModel(world.BlockAccessor, blockPos);
                }
            }
            return result;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(pos);
            if (blockEntity != null && blockEntity is BaseBlockEntitySupport)
            {
                bool change = ((BaseBlockEntitySupport)blockEntity).RecalculateModel(world.BlockAccessor, pos);
                if (change)
                {
                    world.BlockAccessor.TriggerNeighbourBlockUpdate(pos);
                }
            }
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BaseBlockEntitySupport blockEntity = blockAccessor.GetBlockEntity(pos) as BaseBlockEntitySupport;
            if (blockEntity == null)
            {
                string entityClass = blockAccessor.GetBlock(pos).EntityClass;
                if (entityClass != null && entityClass.Length > 0)
                {
                    blockAccessor.SpawnBlockEntity(entityClass, pos);
                    blockEntity = blockAccessor.GetBlockEntity(pos) as BaseBlockEntitySupport;
                }
                else
                {
                    return new Cuboidf[] { new Cuboidf(0.25f, 0, 0.25f, 0.75f, 1, 0.75f) };
                }
            }
            return blockEntity.getCollisionSelectionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BaseBlockEntitySupport blockEntity = blockAccessor.GetBlockEntity(pos) as BaseBlockEntitySupport;
            if (blockEntity == null)
            {
                string entityClass = blockAccessor.GetBlock(pos).EntityClass;
                if (entityClass != null && entityClass.Length > 0)
                {
                    blockAccessor.SpawnBlockEntity(entityClass, pos);
                    blockEntity = blockAccessor.GetBlockEntity(pos) as BaseBlockEntitySupport;
                }
                else
                {
                    return new Cuboidf[] { new Cuboidf(0.25f, 0, 0.25f, 0.75f, 1, 0.75f) };
                }
            }
            
            return blockEntity.getCollisionSelectionBoxes(blockAccessor, pos);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (hotbarSlot.Itemstack != null && hotbarSlot.Itemstack.Item != null)
            {
                string itemCode = hotbarSlot.Itemstack.Item.Code.ToString();
                string upgradeItemCode = GetUpgradeItemCode(this);
                string createReinforcementCode = GetCreateReinforcementItemCode(this);
                if (upgradeItemCode != null && Regex.Match(itemCode, upgradeItemCode).Success)
                {
                    string upgradedBlockCode = GetUpgradedBlockCode(this, hotbarSlot.Itemstack.Item);
                    if (upgradedBlockCode != null)
                    {
                        world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation(upgradedBlockCode)).BlockId, blockSel.Position);
                        hotbarSlot.TakeOut(1);
                        return true;
                    }
                    return false;
                }
                else if(createReinforcementCode != null &&  Regex.Match(itemCode, createReinforcementCode).Success)
                {
                    BlockPos targetPos = blockSel.Position.AddCopy(blockSel.Face);
                    Block targetBlock = world.BlockAccessor.GetBlock(targetPos);
                    if (targetBlock != null && targetBlock.Replaceable > 6000)
                    {
                        string reinforcementBlockCode = GetReinforcementBlockCode(this, hotbarSlot.Itemstack.Item);
                        if (reinforcementBlockCode != null) {
                            world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation(reinforcementBlockCode)).BlockId, targetPos);
                            hotbarSlot.TakeOut(1);
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }

    }

}
