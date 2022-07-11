using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace nrw.frese.miningchallenge.behavior
{
    public class BlockBehaviorPreassureBreaking : BlockBehavior
    {
        public BlockBehaviorPreassureBreaking(Block block) : base(block)
        {
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos, ref EnumHandling handled)
        {
            handled = EnumHandling.PassThrough;

            Block neighbourBlock = world.BlockAccessor.GetBlock(neibpos);
            if (neighbourBlock != null && neighbourBlock.BlockId == 0)
            {
                if (IsCeilingBlock(world, pos))
                {
                    int weight = getWeight(world, pos);
                    world.Logger.Notification("Calculated weight: " + weight);
                    world.Logger.Notification("Calculated maxDistance: " + maxWallDistance_Ceiling(world, pos.DownCopy()));
                    world.Logger.Notification("Block Strain: " + calculateFullBlockStrain(world, pos, weight));
                }
            }
            base.OnNeighbourBlockChange(world, pos, neibpos, ref handled);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropQuantityMultiplier, ref EnumHandling handled)
        {
            world.Logger.Notification("GetDrops");
            if (true)
            {
                handled = EnumHandling.PreventSubsequent;
                return new ItemStack[] { new ItemStack(block) };
            }
            else
            {
                handled = EnumHandling.PassThrough;
                return null;
            }
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
        {
            world.Logger.Notification("OnBlockBroken");
            handling = EnumHandling.PassThrough;
            // Whats this good for? it prevents breaking of stone blocks o.o
            /*if (IsSurroundedByNonSolid(world, pos) && byPlayer != null && byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                handling = EnumHandling.PreventSubsequent;
            }*/
        }

        private int getWeight(IWorldAccessor world, BlockPos pos)
        {
            return world.BlockAccessor.GetRainMapHeightAt(pos.X, pos.Z) - pos.Y;
        }

        public bool IsCeilingBlock(IWorldAccessor world, BlockPos pos)
        {
            return world.BlockAccessor.GetBlock(pos.DownCopy(1)).BlockId == 0;
        }

        private int maxWallDistance_Ceiling(IWorldAccessor world, BlockPos pos)
        {
            int maxWallDistance = 0;
            foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
            {
                int directionDistance = 0;
                BlockPos neighborPos = pos.AddCopy(facing.Normali);
                Block neighborBlock = world.BlockAccessor.GetBlock(neighborPos);
                while (neighborBlock.BlockId == 0 && world.BlockAccessor.GetBlock(pos.UpCopy()).BlockId > 0)
                {
                    directionDistance++;
                    neighborPos = neighborPos.AddCopy(facing.Normali);
                    neighborBlock = world.BlockAccessor.GetBlock(neighborPos);
                }
                if (maxWallDistance < directionDistance) { maxWallDistance = directionDistance; }
            }
            return maxWallDistance;
        }

        private double calculateFullBlockStrain(IWorldAccessor world, BlockPos pos, int weight)
        {
            double blockStrain = weight;
            foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
            {
                BlockPos currentPos = pos.AddCopy(facing.Normali);
                for(int i = 0; i < 3; i++)
                {
                    blockStrain += calculateDistanceStrain(world, currentPos, weight, facing, i);
                    currentPos = currentPos.AddCopy(facing.Normali);
                }
            }
            return blockStrain;
        }

        private double calculateDistanceStrain(IWorldAccessor world, BlockPos pos, int weight, BlockFacing facing, int distance)
        {
            double strain = calculateSingleBlockStrain(world, pos, weight);

            Vec3i normal = facing.Normali;

            strain += calculateSideStep(world, pos, weight, distance, new Vec3i(normal.Z, normal.Y, normal.X));
            strain += calculateSideStep(world, pos, weight, distance, new Vec3i(-normal.Z, normal.Y, -normal.X));

            return strain;
        }

        private double calculateSideStep(IWorldAccessor world, BlockPos pos, int weight, int distance, Vec3i orthogonal) 
        {
            double strain = 0;
            
            BlockPos currentPos = pos.AddCopy(orthogonal);
            for (int i = 0; i <= distance; i++)
            {
                double partialStrain = calculateSingleBlockStrain(world, currentPos, weight);
                if (i == distance)
                {
                    partialStrain /= 2;
                }
                strain += partialStrain;
                currentPos = currentPos.AddCopy(orthogonal);
            }
            return strain;
        }

        private double calculateSingleBlockStrain(IWorldAccessor world, BlockPos pos, int weight) 
        {
            Block currentBlock = world.BlockAccessor.GetBlock(pos);
            Block blockDown = world.BlockAccessor.GetBlock(pos.DownCopy());
            Block blockUp = world.BlockAccessor.GetBlock(pos.UpCopy());
            
            if(blockDown.BlockId != 0 || blockUp.BlockId == 0)
            {
                return 0;
            }
            return weight;
        }
    }
}
