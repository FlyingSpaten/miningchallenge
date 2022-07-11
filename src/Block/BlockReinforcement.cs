
using nrw.frese.miningchallenge.behavior;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace nrw.frese.miningchallenge.block
{
    internal class BlockReinforcement : Block
    {
        ICoreClientAPI capi;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            capi = api as ICoreClientAPI;
        }

        public string GetOrientations(IWorldAccessor world, BlockPos pos)
        {
            string orientations;
            orientations = GetConnectCode(world, pos, BlockFacing.NORTH) +
                GetConnectCode(world, pos, BlockFacing.EAST) +
                GetConnectCode(world, pos, BlockFacing.SOUTH) +
                GetConnectCode(world, pos, BlockFacing.WEST);
            if (orientations.Length == 0) orientations = "empty";
            return orientations;
        }

        public string GetOrientations(IWorldAccessor world, BlockPos pos, string hv)
        {
            if (hv.Equals("v"))
            {
                return GetOrientations(world, pos);
            }
            else
            {
                if (ShouldReinforceAt(world, pos, BlockFacing.NORTH) || ShouldReinforceAt(world, pos, BlockFacing.SOUTH))
                {
                    return "ns";
                }
                return "ew";
            }
        }

        public string GetHV(IWorldAccessor world, BlockPos blockPos)
        {
           if(ShouldReinforceAt(world, blockPos, BlockFacing.UP))
            {
                return "h";
            }
            return "v";
        }

        private int CountHorizontalSupportConnections(IWorldAccessor world, BlockPos blockPos)
        {
            int count = 0;

            if (ShouldReinforceAt(world, blockPos, BlockFacing.NORTH))
            {
                count++;
            }
            if (ShouldReinforceAt(world, blockPos, BlockFacing.EAST))
            {
                count++;
            }
            if (ShouldReinforceAt(world, blockPos, BlockFacing.SOUTH))
            {
                count++;
            }
            if (ShouldReinforceAt(world, blockPos, BlockFacing.WEST))
            {
                count++;
            }

            return count;
        }

        private string GetConnectCode(IWorldAccessor world, BlockPos pos, BlockFacing facing)
        {
            if (ShouldReinforceAt(world, pos, facing)) return "" + facing.Code[0];
            return "";
        }


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            string hv = GetHV(world, blockSel.Position);
            string orientations = GetOrientations(world, blockSel.Position, hv);
            string[] keys = { "hv", "orientation"};
            string[] values = { hv, orientations};
            Block block = world.BlockAccessor.GetBlock(CodeWithVariants(keys, values));

            if (block == null) block = this;

            if (block.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
            {
                world.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
                return true;
            }

            return false;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            string hv = GetHV(world, pos);
            string orientations = GetOrientations(world, pos, hv);
            string[] keys = { "hv", "orientation" };
            string[] values = { hv, orientations };

            AssetLocation newBlockCode = CodeWithVariants(keys, values);

            if (!Code.Equals(newBlockCode))
            {
                Block block = world.BlockAccessor.GetBlock(newBlockCode);
                if (block == null) return;

                world.BlockAccessor.SetBlock(block.BlockId, pos);
                world.BlockAccessor.TriggerNeighbourBlockUpdate(pos);
            }
            else
            {
                base.OnNeighbourBlockChange(world, pos, neibpos);
            }
        }

        public bool ShouldReinforceAt(IWorldAccessor world, BlockPos ownPos, BlockFacing side)
        {
            Block block = world.BlockAccessor.GetBlock(ownPos.AddCopy(side));

            return block.GetBehavior(typeof(BlockBehaviorSupportable), true) != null;
        }
    }
}
