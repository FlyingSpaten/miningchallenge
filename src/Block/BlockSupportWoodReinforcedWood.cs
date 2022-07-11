using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace nrw.frese.miningchallenge.block
{
    internal class BlockSupportWoodReinforcedWood : BaseBlockSupport
    {
        public override double BaseTemporalSupport { get { return 0.02; } }

        public override string UpgradeItemCode { get { return "rock-.*"; } }

        public override string UpgradedBlockCode { get { return "support_wood_reinforced_stone"; } }

        public override string CreateReinforcementItemCode { get { return "plank[^-]*"; } }

        public override string GetCreateReinforcementItemCode(Block currentBlock)
        {
            return "game:plank[^-]*";
        }

        public override string GetReinforcementBlockCode(Block currentBlock, Item reinforcementItem)
        {
            return "miningchallenge:reinforcement-" + reinforcementItem.Variant["wood"];
        }

        public override string GetUpgradedBlockCode(Block currentBlock, Item upgradeItem)
        {
            return "miningchallenge:" + UpgradedBlockCode + "-" + Variant["wood"] + "-" + upgradeItem.Variant["rock"];
        }

        public override string GetUpgradeItemCode(Block currentBlock)
        {
            return "game:stone-.*";
        }
    }
}
