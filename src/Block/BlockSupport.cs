using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace nrw.frese.miningchallenge.block
{
    internal class BlockSupport : BaseBlockSupport
    {
        public override double BaseTemporalSupport { get { return 0.0025; } }

        public override string UpgradeItemCode { get { return "plank-[^-]*"; } }

        public override string UpgradedBlockCode { get { return "support_wood_reinforced_wood"; } }

        public override string CreateReinforcementItemCode { get { return "plank[^-]*"; } }

        public override string GetUpgradeItemCode(Block currentBlock)
        {
            return "game:plank-" + Variant["wood"];
        }

        public override string GetUpgradedBlockCode(Block currentBlock, Item upgradeItem)
        {
            return "miningchallenge:" + UpgradedBlockCode + "-" + Variant["wood"];
        }

        public override string GetCreateReinforcementItemCode(Block currentBlock)
        {
            return null;
        }

        public override string GetReinforcementBlockCode(Block currentBlock, Item reinforcementItem)
        {
            return null;
        }
    }
}
