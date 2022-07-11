
using System;
using System.Collections.Generic;
using nrw.frese.miningchallenge.block;

namespace nrw.frese.miningchallenge.blockentity
{
    public class BlockEntitySupportWoodReinforcedStone : BaseBlockEntitySupport
    {
        public override bool CanSupport { get { return true; } }

        public override string BaseModelPath { get { return "miningchallenge:shapes/block/support-wood-reinforced-stone"; } }

        public override List<Type> ConnectingBlockCodes { get { return new List<Type> { typeof(BlockSupport), typeof(BlockSupportWoodReinforcedWood), typeof(BlockSupportWoodReinforcedStone) }; } }

        public override string BlockCode { get { return "support_wood_reinforced_stone"; } }
    }
}
