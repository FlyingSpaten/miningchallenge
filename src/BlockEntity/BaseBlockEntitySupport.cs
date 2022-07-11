using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using nrw.frese.miningchallenge.behavior;
using nrw.frese.miningchallenge.block;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace nrw.frese.miningchallenge.blockentity
{
    public abstract class BaseBlockEntitySupport : BlockEntity, ITexPositionSource
    {
        public ICoreClientAPI capi;

        const string MODEL = "Model";
        const string ROTATION = "Rotation";
        const string ORIENTATION = "Orientation";
        const string CONNECT_N = "ConnectN";
        const string CONNECT_E = "ConnectE";
        const string CONNECT_S = "ConnectS";
        const string CONNECT_W = "ConnectW";
        const string SUPPORT_T = "SupportT";
        const string SUPPORT_N = "SupportN";
        const string SUPPORT_E = "SupportE";
        const string SUPPORT_S = "SupportS";
        const string SUPPORT_W = "SupportW";

        const int CONNECT_INDICATOR = 1;
        const int SUPPORT_INDICATOR = 2;

        Cuboidf baseCollisionBox;
        Dictionary<string, Cuboidf> vConnectionCollisionBoxes;
        Dictionary<string, Cuboidf> hConnectionCollisionBoxes;
        Dictionary<string, Cuboidf> supportCollisionBoxes;
        
        Cuboidf[] collisionBoxes = null;

        public string Model;
        public int Rotation = 0;

        private string orientation = null;
        private bool connectN = false;
        private bool connectE = false;
        private bool connectS = false;
        private bool connectW = false;
        private bool supportT = false;
        private bool supportN = false;
        private bool supportE = false;
        private bool supportS = false;
        private bool supportW = false;
        
        public abstract bool CanSupport { get; }

        public abstract string BaseModelPath { get; }

        public abstract string BlockCode{ get; }

        public abstract List<Type> ConnectingBlockCodes { get; }

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

            baseCollisionBox = new Cuboidf(0.25f, 0, 0.25f, 0.75f, 1, 0.75f);

            vConnectionCollisionBoxes = new Dictionary<string, Cuboidf>();
            vConnectionCollisionBoxes.Add("n", new Cuboidf(0.25f, 0.5f, 0, 0.75f, 1, 0.25f));
            vConnectionCollisionBoxes.Add("e", new Cuboidf(0.75f, 0.5f, 0.25f, 1, 1, 0.75f));
            vConnectionCollisionBoxes.Add("s", new Cuboidf(0.25f, 0.5f, 0.75f, 0.75f, 1, 1));
            vConnectionCollisionBoxes.Add("w", new Cuboidf(0, 0.5f, 0.25f, 0.25f, 1, 0.75f));

            hConnectionCollisionBoxes = new Dictionary<string, Cuboidf>();
            hConnectionCollisionBoxes.Add("ns", new Cuboidf(0.25f, 0.5f, 0, 0.75f, 1, 1));
            hConnectionCollisionBoxes.Add("ew", new Cuboidf(0, 0.5f, 0.25f, 1, 1, 0.75f));

            supportCollisionBoxes = new Dictionary<string, Cuboidf>();
            supportCollisionBoxes.Add("n", new Cuboidf(0, 0, 0, 1, 1, 0.25f));
            supportCollisionBoxes.Add("e", new Cuboidf(0.75f, 0, 0, 1, 1, 1));
            supportCollisionBoxes.Add("s", new Cuboidf(0, 0, 0.75f, 1, 1, 1));
            supportCollisionBoxes.Add("w", new Cuboidf(0, 0, 0, 0.25f, 1, 1));
            supportCollisionBoxes.Add("t", new Cuboidf(0, 0.75f, 0, 1, 1, 1));
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            Model = tree.GetString(MODEL);
            Rotation = tree.GetInt(ROTATION);
            orientation = tree.GetString(ORIENTATION);
            connectN = tree.GetBool(CONNECT_N);
            connectE = tree.GetBool(CONNECT_E);
            connectS = tree.GetBool(CONNECT_S);
            connectW = tree.GetBool(CONNECT_W);
            supportT = tree.GetBool(SUPPORT_T);
            supportN = tree.GetBool(SUPPORT_N);
            supportE = tree.GetBool(SUPPORT_E);
            supportS = tree.GetBool(SUPPORT_S);
            supportW = tree.GetBool(SUPPORT_W);

            if(Model == null)
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
            tree.SetBool(CONNECT_N, connectN);
            tree.SetBool(CONNECT_E, connectE);
            tree.SetBool(CONNECT_S, connectS);
            tree.SetBool(CONNECT_W, connectW);
            tree.SetBool(SUPPORT_T, supportT);
            tree.SetBool(SUPPORT_N, supportN);
            tree.SetBool(SUPPORT_E, supportE);
            tree.SetBool(SUPPORT_S, supportS);
            tree.SetBool(SUPPORT_W, supportW);
        }

        public Cuboidf[] getCollisionSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            if(Model == null || orientation == null || Model.Length == 0 || orientation.Length == 0)
            {
                RecalculateModel(blockAccessor, pos);
            }
            if (collisionBoxes == null || collisionBoxes.Length == 0)
            {
                collisionBoxes = new Cuboidf[0];
                if (orientation.Equals("h"))
                {
                    if(connectN || connectS)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, hConnectionCollisionBoxes["ns"]);
                    }else
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, hConnectionCollisionBoxes["ew"]);
                    }
                } else if (orientation.Equals("v"))
                {
                    collisionBoxes = addCuboidf(collisionBoxes, baseCollisionBox);
                    if (connectN)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, vConnectionCollisionBoxes["n"]);
                    }
                    if (connectE)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, vConnectionCollisionBoxes["e"]);
                    }
                    if (connectS)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, vConnectionCollisionBoxes["s"]);
                    }
                    if (connectW)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, vConnectionCollisionBoxes["w"]);
                    }
                }
                if (CanSupport)
                {
                    if (supportT)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, supportCollisionBoxes["t"]);
                    }
                    if (supportN)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, supportCollisionBoxes["n"]);
                    }
                    if (supportE)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, supportCollisionBoxes["e"]);
                    }
                    if (supportS)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, supportCollisionBoxes["s"]);
                    }
                    if (supportW)
                    {
                        collisionBoxes = addCuboidf(collisionBoxes, supportCollisionBoxes["w"]);
                    }
                }
            }
            return collisionBoxes;
        }

        private Cuboidf[] addCuboidf(Cuboidf[] oldArray, Cuboidf cuboidf)
        {
            Cuboidf[] newArray = new Cuboidf[oldArray.Length + 1];

            for(int i = 0; i < oldArray.Length; i++) 
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
            int[] connections = CalculateConnectionPoints(blockAccessor, blockPos);
            int[] supports = CalculateSupportPoints(blockAccessor, blockPos);
            string topSupportOrientation = CalculateTopOrientation(blockAccessor, blockPos, orientation, connections);

            int tempRotation = 0;
            string tempModel = getShapePath(orientation, connections, supports, topSupportOrientation);

            IAsset asset = Api.Assets.TryGet(tempModel);
            while(asset == null && tempRotation <= 270)
            {
                RightShiftArray(connections);
                RightShiftArray(supports);
                topSupportOrientation = shiftTopOrientation(topSupportOrientation);
                tempModel = getShapePath(orientation, connections, supports, topSupportOrientation);
                asset = Api.Assets.TryGet(tempModel);
                tempRotation += 90;
            }

            if(asset != null)
            {
                bool noChange = tempModel.Equals(Model) && ("" + tempRotation).Equals(Rotation);
                Model = tempModel;
                Rotation = tempRotation;
                return !noChange;
            }

            return false;
        }

        private string shiftTopOrientation(string currentOrientation)
        {
            if (currentOrientation.Equals("ns"))
            {
                return "ew";
            }
            else if (currentOrientation.Equals("ew"))
            {
                return "ns";
            }
            else
            {
                return "x";
            }
        }

        private void resetParameters()
        {
            collisionBoxes = null;
            orientation = null;
            connectN = false;
            connectE = false;
            connectS = false;
            connectW = false;
            supportT = false;
            supportN = false;
            supportE = false;
            supportS = false;
            supportW = false;
    }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (Model != null && Model.Length > 0)
            {
                Dictionary<string, Shape> shapeDictionary = ObjectCacheUtil.GetOrCreate(Api, BlockCode + "-shapes", () => new Dictionary<string, Shape>());
                Shape shape;
                if(!shapeDictionary.TryGetValue(Model, out shape))
                {
                    capi.Logger.Error("New support shape loaded.");
                    IAsset asset = Api.Assets.TryGet(Model);
                    shape = asset.ToObject<Shape>();
                    shapeDictionary.Add(Model, shape);
                }
                MeshData meshdata;
                Vec3f rotationVector = new Vec3f();
                rotationVector.Set( 0, Rotation, 0);
                capi.Tesselator.TesselateShape(BlockCode, shape, out meshdata, this, rotationVector);

                mesher.AddMeshData(meshdata);
                return true;
            }
            else
            {
                return base.OnTesselation(mesher, tessThreadTesselator);
            }
        }

        protected string getShapePath(string orientation, int[] connections, int[] supports, string topSupportOrientation)
        {
            return BaseModelPath
                + "/" + orientation
                + "/" + countConnections(connections)
                + "/" + convertToCode(connections)
                + (CanSupport ? "-" + convertToCode(supports) + "-" + topSupportOrientation: "")
                +".json";
        }

        private int countConnections(int[] connections)
        {
            int count = 0;
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i] > 0) { count++; }
            }
            return count;
        }

        public void RightShiftArray(int[] arr)
        {
            int t = arr[arr.Length - 1];
            for (int i = arr.Length - 1; i > 0; i--)
                arr[i] = arr[i - 1];
            arr[0] = t;
        }

        protected string convertToCode(int[] sides)
        {
            if(sides.Length != 4)
            {
                return "";
            }

            string code = "";
            if(sides[0] > 0)
            {
                code += "n";
            }
            if (sides[1] > 0)
            {
                code += "e";
            }
            if (sides[2] > 0)
            {
                code += "s";
            }
            if (sides[3] > 0)
            {
                code += "w";
            }
            return code.Length > 0 ? code : "x";
        }

        protected string CalculateTopOrientation(IBlockAccessor blockAccessor, BlockPos blockPos, String orientation, int[] connectionPoints)
        {
            if(ShouldReinforceAt(blockAccessor, blockPos.UpCopy()))
            {
                supportT = true;
                if(ShouldReinforceAt(blockAccessor, blockPos.NorthCopy()) || ShouldReinforceAt(blockAccessor, blockPos.SouthCopy()))
                {
                    return "ew";
                }
                if (ShouldReinforceAt(blockAccessor, blockPos.EastCopy()) || ShouldReinforceAt(blockAccessor, blockPos.WestCopy()))
                {
                    return "ns";
                }
                if (orientation.Equals("h"))
                {
                    if(connectionPoints[0] == 1 || connectionPoints[2] == 1)
                    {
                        return "ew";
                    }
                    if (connectionPoints[1] == 1 || connectionPoints[3] == 1)
                    {
                        return "ns";
                    }
                }
                else
                {
                    return "ew";
                }
            }
            return "x";
        }

        protected string CalculateOriantation(IBlockAccessor blockAccessor, BlockPos blockPos)
        {
            Block BlockBeneath = blockAccessor.GetBlock(blockPos.DownCopy());
            Block BlockAbove = blockAccessor.GetBlock(blockPos.UpCopy());
            if((BlockBeneath != null && (BlockBeneath.BlockId > 0 && !(BlockBeneath is BaseBlockReinforcement))) 
                || (BlockAbove != null && (BlockAbove.BlockId > 0 &&  BlockAbove is BaseBlockSupport)))
            {
                return "v";
            }
            else
            {
                return "h";
            }
        }

        protected int[] CalculateConnectionPoints(IBlockAccessor blockAccessor, BlockPos blockPos)
        {
            int[] connections = new int[4];

            if (ShouldConnectAt(blockAccessor, blockPos.NorthCopy()))
            {
                connections[0] = CONNECT_INDICATOR;
                connectN = true;
            }
            if (ShouldConnectAt(blockAccessor, blockPos.EastCopy()))
            {
                connections[1] = CONNECT_INDICATOR;
                connectE = true;
            }
            if (ShouldConnectAt(blockAccessor, blockPos.SouthCopy()))
            {
                connections[2] = CONNECT_INDICATOR;
                connectS = true;
            }
            if (ShouldConnectAt(blockAccessor, blockPos.WestCopy()))
            {
                connections[3] = CONNECT_INDICATOR;
                connectW = true;
            }

            return connections;
        }

        private bool ShouldConnectAt(IBlockAccessor blockAccessor, BlockPos blockPos)
        {
            return ConnectingBlockCodes.Contains(blockAccessor.GetBlock(blockPos).GetType());
        }

        protected int[] CalculateSupportPoints(IBlockAccessor blockAccessor, BlockPos blockPos)
        {
            int[] connections = new int[4];

            if (ShouldReinforceAt(blockAccessor, blockPos.NorthCopy()))
            {
                connections[0] = SUPPORT_INDICATOR;
                supportN = true;
            }
            if (ShouldReinforceAt(blockAccessor, blockPos.EastCopy()))
            {
                connections[1] = SUPPORT_INDICATOR;
                supportE = true;
            }
            if (ShouldReinforceAt(blockAccessor, blockPos.SouthCopy()))
            {
                connections[2] = SUPPORT_INDICATOR;
                supportS = true;
            }
            if (ShouldReinforceAt(blockAccessor, blockPos.WestCopy()))
            {
                connections[3] = SUPPORT_INDICATOR;
                supportW = true;
            }

            return connections;
        }

        private bool ShouldReinforceAt(IBlockAccessor blockAccessor, BlockPos blockPos)
        {
            return blockAccessor.GetBlock(blockPos).GetBehavior(typeof(BlockBehaviorSupportable), true) != null;
        }

    }
}
