using System;
using nrw.frese.miningchallenge.behavior;
using nrw.frese.miningchallenge.block;
using nrw.frese.miningchallenge.blockentity;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace nrw.frese.miningchallenge
{
    public class MiningChallengeMod : ModSystem
    {
        ICoreServerAPI sapi;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("BlockSupport", typeof(BlockSupport));
            api.RegisterBlockClass("BlockSupportWoodReinforcedWood", typeof(BlockSupportWoodReinforcedWood));
            api.RegisterBlockClass("BlockSupportWoodReinforcedStone", typeof(BlockSupportWoodReinforcedStone));
            api.RegisterBlockClass("BlockReinforcement", typeof(block.BlockReinforcement));
            api.RegisterBlockClass("BlockReinforcementWood", typeof(block.BlockReinforcementWood));
            api.RegisterBlockClass("BlockReinforcementStone", typeof(block.BlockReinforcementStone));

            api.RegisterBlockEntityClass("BlockEntitySupportWood", typeof (BlockEntitySupportWood));
            api.RegisterBlockEntityClass("BlockEntitySupportWoodReinforcedWood", typeof (BlockEntitySupportWoodReinforcedWood));
            api.RegisterBlockEntityClass("BlockEntitySupportWoodReinforcedStone", typeof (BlockEntitySupportWoodReinforcedStone));
            api.RegisterBlockEntityClass("BlockEntityReinforcementWood", typeof (BlockEntityReinforcementWood));
            api.RegisterBlockEntityClass("BlockEntityReinforcementStone", typeof (BlockEntityReinforcementStone));

            api.RegisterBlockBehaviorClass("BlockBehaviorPreassureBreaking", typeof(BlockBehaviorPreassureBreaking));
            api.RegisterBlockBehaviorClass("BlockBehaviorSupportable", typeof(BlockBehaviorSupportable));

            api.RegisterCollectibleBehaviorClass("BehaviorStoneReinforcable", typeof(BehaviorStoneReinforcable));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            sapi = api;
            api.Event.RegisterGameTickListener(OnServerTick100ms, 101);
        }

        private void OnServerTick100ms(float dt)
        {
            foreach (IServerPlayer plr in sapi.World.AllOnlinePlayers)
            {
                if (plr.ConnectionState != EnumClientState.Playing) continue;

                var bh = plr.Entity.GetBehavior<EntityBehaviorTemporalStabilityAffected>();
                if (bh == null) continue;
                bh.stabilityOffset = 0;

                var plrPos = plr.Entity.Pos.XYZ;
                var world = sapi.World;
                double sum = 0;

                int dist = 3;

                for (int x = -dist; x <= dist; x++)
                {
                    for (int y = -dist; y <= dist; y++)
                    {
                        for (int z = -dist; z <= dist; z++)
                        {
                            BlockPos pos = new BlockPos(plrPos.XInt + x, plrPos.YInt +  y, plrPos.ZInt + z);
                            Block block = world.BlockAccessor.GetBlock(pos);
                            if (block is BaseBlockSupport)
                            {
                                sum += ((BaseBlockSupport) block).BaseTemporalSupport;
                            }
                            else if (block is BaseBlockReinforcement)
                            {
                                sum += ((BaseBlockReinforcement)block).BaseTemporalSupport;
                            }
                        }
                    }
                }

                if (bh != null)
                {
                    bh.stabilityOffset += sum;
                }
            }
        }

    }
}
