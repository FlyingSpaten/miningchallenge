using Vintagestory.API.Common;

namespace nrw.frese.miningchallenge.block
{
    internal class BlockReinforcementStone : BaseBlockReinforcement
    {
        public override double BaseTemporalSupport { get { return 0.04; } }

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
