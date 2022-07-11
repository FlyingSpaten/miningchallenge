using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace nrw.frese.miningchallenge.block
{
    internal class BlockSupportWoodReinforcedStone : BaseBlockSupport
    {
        public override double BaseTemporalSupport { get { return 0.01; } }

        public override string UpgradeItemCode { get { return null; } }

        public override string UpgradedBlockCode { get { return null; } }

        public override string CreateReinforcementItemCode { get { return "plank"; } }

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
            return null;
        }

        public override string GetUpgradeItemCode(Block currentBlock)
        {
            return null;
        }
    }
}
