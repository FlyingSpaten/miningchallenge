
using System.Collections.Generic;
using nrw.frese.miningchallenge.behavior;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace nrw.frese.miningchallenge.blockentity
{
    public abstract class BaseBlockEntityReinforcement : BlockEntity, ITexPositionSource
    {
        public ICoreClientAPI capi;

        const string MODEL = "Model";
        const string ROTATION = "Rotation";
        const string ORIENTATION = "Orientation";
        const string DIRECTION = "Direction";

        public string Model;
        public int Rotation = 0;

        private string orientation = null;
        private string direction = null;

        Dictionary<string, Cuboidf> vConnectionCollisionBoxes;
        Cuboidf hConnectionCollisionBox;
        Cuboidf[] collisionBoxes = null;

        public abstract string BaseModelPath { get; }

        public abstract string BlockCode { get; }

        public Size2i AtlasSize
        {
            get { return ((ICoreClientAPI)Api).BlockTextureAtlas.Size; }
        }

        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                return capi.BlockTextureAtlas.Positions[Block.Textures[textureCode].Baked.TextureSubId];
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            capi = api as ICoreClientAPI;

            vConnectionCollisionBoxes = new Dictionary<string, Cuboidf>();
            vConnectionCollisionBoxes.Add("n", new Cuboidf(0, 0, 0, 1, 1, 0.25f));
            vConnectionCollisionBoxes.Add("e", new Cuboidf(0.75f, 0, 0, 1, 1, 1));
            vConnectionCollisionBoxes.Add("s", new Cuboidf(0, 0, 0.75f, 1, 1, 1));
            vConnectionCollisionBoxes.Add("w", new Cuboidf(0, 0, 0, 0.25f, 1, 1));

            hConnectionCollisionBox = new Cuboidf(0, 0.85f, 0, 1, 1, 1);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            Model = tree.GetString(MODEL);
            Rotation = tree.GetInt(ROTATION);
            orientation = tree.GetString(ORIENTATION);
            direction = tree.GetString(DIRECTION);

            if (Model == null)
            {
                RecalculateModel(worldAccessForResolve.BlockAccessor, Pos);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString(MODEL, Model);
            tree.SetInt(ROTATION, Rotation);
            tree.SetString(ORIENTATION, orientation);
            tree.SetString(DIRECTION, direction);
        }

        public Cuboidf[] getCollisionSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            if (Model == null || orientation == null || direction == null|| Model.Length == 0 || orientation.Length == 0 || direction.Length == 0)
            {
                RecalculateModel(blockAccessor, pos);
            }
            if (collisionBoxes == null || collisionBoxes.Length == 0)
            {
                collisionBoxes = new Cuboidf[0];
                if (orientation.Equals("h"))
                {
                    collisionBoxes = addCuboidf(collisionBoxes, hConnectionCollisionBox);
                }
                else if (orientation.Equals("v"))
                {
                    if (direction.Equals("n"))
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, vConnectionCollisionBoxes["n"]);
                    }
                    if (direction.Equals("e"))
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, vConnectionCollisionBoxes["e"]);
                    }
                    if (direction.Equals("s"))
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, vConnectionCollisionBoxes["s"]);
                    }
                    if (direction.Equals("w"))
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, vConnectionCollisionBoxes["w"]);
                    }
                }
            }
            return collisionBoxes;
        }

        private Cuboidf[] addCuboidf(Cuboidf[] oldArray, Cuboidf cuboidf)
        {
            Cuboidf[] newArray = new Cuboidf[oldArray.Length + 1];

            for (int i = 0; i < oldArray.Length; i++)
            {
                newArray[i] = oldArray[i];
            }
            newArray[oldArray.Length] = cuboidf;

            return newArray;
        }

        public bool RecalculateModel(IBlockAccessor blockAccessor, BlockPos blockPos)
        {
            resetParameters();
            orientation = CalculateOriantation(blockAccessor, blockPos);
            direction = CalculateDirection(blockAccessor, blockPos, orientation);

            int tempRotation;
            string tempModel = getShapePath(orientation);

            IAsset asset = Api.Assets.TryGet(tempModel);
            if (direction.Equals("n")){
                tempRotation = 0;
            }else if (direction.Equals("e"))
            {
                tempRotation = 270;
            }
            else if (direction.Equals("s"))
            {
                tempRotation = 180;
            }
            else
            {
                tempRotation = 90;
            }

            if (asset != null)
            {
                bool noChange = tempModel.Equals(Model) && ("" + tempRotation).Equals(Rotation);
                Model = tempModel;
                Rotation = tempRotation;
                return !noChange;
            }

            return false;
        }

        private void resetParameters()
        {
            orientation = null;
            direction = null;
        }

        public void RightShiftArray(int[] arr)
        {
            int t = arr[arr.Length - 1];
            for (int i = arr.Length - 1; i > 0; i--)
                arr[i] = arr[i - 1];
            arr[0] = t;
        }

        protected string CalculateOriantation(IBlockAccessor blockAccessor, BlockPos blockPos)
        {
            if (ShouldReinforceAt(blockAccessor, blockPos, BlockFacing.UP))
            {
                return "h";
            }
            return "v";
        }

        protected string CalculateDirection(IBlockAccessor blockAccessor, BlockPos blockPos, string orientation)
        {
            if (orientation.Equals("h"))
            {
                return "ns"; //TODO Logik ausbauen
            }
            else
            {
                if (ShouldReinforceAt(blockAccessor, blockPos, BlockFacing.NORTH))
                {
                    return "n";
                }
                if (ShouldReinforceAt(blockAccessor, blockPos, BlockFacing.EAST))
                {
                    return "e";
                }
                if (ShouldReinforceAt(blockAccessor, blockPos, BlockFacing.SOUTH))
                {
                    return "s";
                }
                if(ShouldReinforceAt(blockAccessor, blockPos, BlockFacing.WEST))
                {
                    return "w";
                }
            }
            return "n";
        }

        public bool ShouldReinforceAt(IBlockAccessor blockAccessor, BlockPos ownPos, BlockFacing side)
        {
            Block block = blockAccessor.GetBlock(ownPos.AddCopy(side));

            return block.GetBehavior(typeof(BlockBehaviorSupportable), true) != null;
        }

        protected string getShapePath(string orientation)
        {
            if (orientation.Equals("h"))
            {
                return BaseModelPath + "/reinforcement-h-ns.json";
            }
            return BaseModelPath + "/reinforcement-v-n.json";
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (Model != null && Model.Length > 0)
            {
                Dictionary<string, Shape> shapeDictionary = ObjectCacheUtil.GetOrCreate(Api, BlockCode + "-shapes", () => new Dictionary<string, Shape>());
                Shape shape;
                if (!shapeDictionary.TryGetValue(Model, out shape))
                {
                    capi.Logger.Error("New support shape loaded.");
                    IAsset asset = Api.Assets.TryGet(Model);
                    shape = asset.ToObject<Shape>();
                    shapeDictionary.Add(Model, shape);
                }
                MeshData meshdata;
                Vec3f rotationVector = new Vec3f();
                rotationVector.Set(0, Rotation, 0);
                capi.Tesselator.TesselateShape(BlockCode, shape, out meshdata, this, rotationVector);

                mesher.AddMeshData(meshdata);
                return true;
            }
            else
            {
                return base.OnTesselation(mesher, tessThreadTesselator);
            }
        }
    }
}
