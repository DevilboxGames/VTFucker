using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using ToxicRagers.CarmageddonReincarnation.Formats;
using ToxicRagers.Stainless.Formats;
using ToxicRagers.CarmageddonReincarnation.Formats;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;


namespace VTFucker
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VTConstantBuffer
    {
        public Int32 Width;
        public Int32 Height;
        //public Int32 PosX;
        //public Int32 PosY;
        public Int32 PosOffsetX;
        public Int32 PosOffsetY;
        public Int32 TilesX;
        public Int32 TilesY;
        public Int32 ViewportWidth;
        public Int32 ViewportHeight;
        public Int32 TextureWidth;
        public Int32 TextureHeight;
        //public Int32[] TilePosX;
        //public Int32[] TilePosY;
    }
    public class Renderer
    {

        Device device;
        SwapChain swapChain;
        ShaderSignature inputSignature;
        VertexShader vertexShader;
        PixelShader pixelShader;
        DeviceContext context;
        RenderTargetView renderTarget;
        SwapChainDescription description;
        Viewport viewport;
        SamplerState sampleState;
        SamplerState indSampleState;
        InputLayout layout;
        DataStream vertices;
        SlimDX.Direct3D11.Buffer vertexBuffer;
        SlimDX.Direct3D11.Buffer indexBuffer;
        Stopwatch t;

        Texture2D texture;
        ShaderResourceView resourceView;
        crVTPage vtPage;
        crVTMapEntry currentEntry;

        Effect fx;
        MainForm mainForm;
        Control targetControl;

        public bool UpdateRenderer
        {
            get;
            set;
        }
        public crVTPage VTPage
        {
            set { vtPage = value; }
        }

        public Renderer(MainForm form, Control target)
        {

            t = new Stopwatch();
            mainForm = form;
            targetControl = target;
            form.Renderer = this;
            description = new SwapChainDescription()
            {
                BufferCount = 2,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = target.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, description, out device, out swapChain);

            // create a view of our render target, which is the backbuffer of the swap chain we just created

            using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
                renderTarget = new RenderTargetView(device, resource);

            // setting a viewport is required if you want to actually see anything
            context = device.ImmediateContext;
            viewport = new Viewport(0.0f, 0.0f, target.ClientSize.Width, target.ClientSize.Height);
            context.OutputMerger.SetTargets(renderTarget);
            context.Rasterizer.SetViewports(viewport);

            CreateShaders(File.ReadAllText("vteffect.fx"));

            // prevent DXGI handling of alt+enter, which doesn't work properly with Winforms
            using (var factory = swapChain.GetParent<Factory>())
                factory.SetWindowAssociation(target.Handle, WindowAssociationFlags.IgnoreAltEnter);

            // handle alt+enter ourselves
            form.KeyDown += (o, e) =>
            {
                //if (e.Alt && e.KeyCode == Keys.Enter)
                //swapChain.IsFullScreen = !swapChain.IsFullScreen;
            };

            // handle form size changes
            form.Resize += (o, e) =>
            {
                renderTarget.Dispose();

                viewport = new Viewport(0.0f, 0.0f, target.ClientSize.Width, target.ClientSize.Height);
                swapChain.ResizeBuffers(2, 0, 0, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
                using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
                    renderTarget = new RenderTargetView(device, resource);

                context.OutputMerger.SetTargets(renderTarget);
                context.Rasterizer.SetViewports(viewport);
                SetTexture(currentEntry);
            };

            MessagePump.Run(form, Render);

            // clean up all resources
            // anything we missed will show up in the debug output
            vertices.Close();
            vertexBuffer.Dispose();
            layout.Dispose();
            inputSignature.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            renderTarget.Dispose();
            swapChain.Dispose();
            device.Dispose();
            UpdateRenderer = true;
        }

        public void CreateShaders(string code)
        {
            UpdateRenderer = true;
            // load and compile the vertex shader
            using (var bytecode = ShaderBytecode.Compile(code, "VShader", "vs_5_0", ShaderFlags.None, EffectFlags.None))
            {
                inputSignature = ShaderSignature.GetInputSignature(bytecode);
                vertexShader = new VertexShader(device, bytecode);
            }

            // load and compile the pixel shader
            using (var bytecode = ShaderBytecode.Compile(code, "PShader", "ps_5_0", ShaderFlags.None, EffectFlags.None))
                pixelShader = new PixelShader(device, bytecode);

            string compilationError = "";
            //ShaderBytecode compiledShader = ShaderBytecode.CompileFromFile("vteffect.fx", "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null, out compilationError);

            //fx = new Effect(device, compiledShader);

            // create test vertex data, making sure to rewind the stream afterward
            vertices = new DataStream(20 * 4, true, true);

            vertices.Write(new Vector3(-1f, -1f, 0.5f)); vertices.Write(new Vector2(0f, 1f));
            vertices.Write(new Vector3(-1f, 1f, 0.5f)); vertices.Write(new Vector2(0f, 0f));
            vertices.Write(new Vector3(1f, -1f, 0.5f)); vertices.Write(new Vector2(1f, 1f));
            vertices.Write(new Vector3(1f, 1f, 0.5f)); vertices.Write(new Vector2(1f, 0f));
            vertices.Position = 0;

            // create the vertex layout and buffer
            var elements = new[] {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
            };
            layout = new InputLayout(device, inputSignature, elements);
            vertexBuffer = new SlimDX.Direct3D11.Buffer(device, vertices, 20 * 4, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            List<int> indices = new List<int>();
            indices.Add(0);
            indices.Add(1);
            indices.Add(2);
            indices.Add(2);
            indices.Add(1);
            indices.Add(3);
            var ibd = new BufferDescription(sizeof(int) * indices.Count, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            indexBuffer = new SlimDX.Direct3D11.Buffer(device, new DataStream(indices.ToArray(), false, false), ibd);

            // configure the Input Assembler portion of the pipeline with the vertex data
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 20, 0));
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

            // set the shaders
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);


            SamplerDescription sampleDesc = new SamplerDescription();
            sampleDesc.Filter = Filter.MinMagMipPoint;
            sampleDesc.AddressU = TextureAddressMode.Clamp;
            sampleDesc.AddressV = TextureAddressMode.Clamp;
            sampleDesc.AddressW = TextureAddressMode.Clamp;
            sampleDesc.MipLodBias = 0.0f;
            sampleDesc.ComparisonFunction = Comparison.Always;
            sampleDesc.BorderColor = new Color4(0, 0, 0, 0);
            sampleDesc.MinimumLod = 0;
            sampleDesc.MaximumLod = 1;

            sampleState = SamplerState.FromDescription(device, sampleDesc);

            SamplerDescription indSampleDesc = new SamplerDescription();
            sampleDesc.Filter = Filter.MinMagMipPoint;
            sampleDesc.AddressU = TextureAddressMode.Wrap;
            sampleDesc.AddressV = TextureAddressMode.Wrap;
            sampleDesc.AddressW = TextureAddressMode.Wrap;
            sampleDesc.MipLodBias = 0.0f;
            sampleDesc.ComparisonFunction = Comparison.Always;
            sampleDesc.BorderColor = new Color4(0, 0, 0, 0);
            sampleDesc.MinimumLod = 0;
            sampleDesc.MaximumLod = 1;

            indSampleState = SamplerState.FromDescription(device, sampleDesc);

            ImageLoadInformation loadInfo = new ImageLoadInformation() { Width = 2, Height = 2 };
            loadInfo.BindFlags = BindFlags.ShaderResource;
            loadInfo.CpuAccessFlags = CpuAccessFlags.None;
            loadInfo.Depth = 4;
            loadInfo.FilterFlags = FilterFlags.Point;
            loadInfo.FirstMipLevel = 0;
            loadInfo.Format = Format.R8G8B8A8_SInt;
            loadInfo.MipLevels = 0;
            loadInfo.Usage = ResourceUsage.Default;
            texture = new Texture2D(device, new Texture2DDescription
            {
                BindFlags = BindFlags.ShaderResource,
                ArraySize = 1024,
                Width = 128,
                Height = 128,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None
            });//Texture2D.FromFile(device,"Tourism_Industrial_d.png");
            resourceView = new ShaderResourceView(device, texture);
            device.ImmediateContext.PixelShader.SetShaderResource(resourceView, 0);
            context.PixelShader.SetShaderResource(resourceView, 0);
            context.PixelShader.SetSampler(sampleState, 0);
            context.PixelShader.SetSampler(indSampleState, 1);
            if (currentEntry != null) SetTexture(currentEntry);
        }
        public void SetTexture(crVTMapEntry vtEntry)
        {
            currentEntry = vtEntry;
            if (vtEntry == null || vtPage == null) return;
            var tiles = vtPage.GetTiles(vtEntry);
            if (tiles.Count == 0) return;
            mainForm.tileListSource.DataSource = tiles;
            var uniqueTiles = (from tile in tiles select tile.TDXTile.Texture.Name).Distinct();
            Format textureformat = Format.BC3_UNorm;
            if (tiles[0].TDXTile.Texture == null) tiles[0].TDXTile.GetTextureFromZAD();
            if (tiles[0].TDXTile.Texture == null)
            {
                return;
            }
            switch (tiles[0].TDXTile.Texture.Format)
            {
                case ToxicRagers.Helpers.D3DFormat.DXT1:
                    textureformat = Format.BC1_UNorm;
                    break;
                case ToxicRagers.Helpers.D3DFormat.DXT5:
                    textureformat = Format.BC3_UNorm;
                    break;
                case ToxicRagers.Helpers.D3DFormat.ATI2:
                    textureformat = Format.BC5_UNorm;
                    break;
                case ToxicRagers.Helpers.D3DFormat.A8R8G8B8:
                    textureformat = Format.B8G8R8A8_UNorm_SRGB;
                    break;
                case ToxicRagers.Helpers.D3DFormat.A8:
                    textureformat = Format.A8_UNorm;
                    break;

            }
            List<ShaderResourceView> resourceViews = new List<ShaderResourceView>();
            List<DataRectangle> rects = new List<DataRectangle>();
            List<Int32> TileRows = new List<int>();
            List<Int32> TileCols = new List<int>();

            List<float> IndirectionData = new List<float>();
            int divisor = vtPage.GetDivisor();

            var cb = new VTConstantBuffer();
            cb.Width = vtEntry.Width / vtPage.GetDivisor();
            cb.Height = vtEntry.Height / vtPage.GetDivisor();
            cb.TilesX = (int)Math.Ceiling(cb.Width / 120.0f) + 1;
            cb.TilesY = (int)Math.Ceiling(cb.Height / 120.0f) + 1;
            cb.ViewportWidth = targetControl.ClientSize.Width;
            cb.ViewportHeight = targetControl.ClientSize.Height;
            cb.PosOffsetX = vtEntry.Column / divisor - tiles[0].Column * 120;
            cb.PosOffsetY = vtEntry.Row / divisor - tiles[0].Row * 120;
            float widthRatio = (float)cb.Height / cb.Width;
            float heightRatio = (float)cb.Width / cb.Height;
            float polyWidth = 1f;
            float polyHeight = 1f;
            if (cb.ViewportWidth > cb.ViewportHeight)
            {
                polyWidth = cb.ViewportWidth / cb.ViewportHeight;
            }
            else if (cb.ViewportWidth < cb.ViewportHeight)
            {
                polyHeight = cb.ViewportHeight / cb.ViewportWidth;
            }
            if (cb.Width > cb.Height)
            {
                if (cb.ViewportWidth > cb.Width)
                {
                    //polyWidth = (float)cb.Width / cb.ViewportWidth;
                    //polyHeight = (float)cb.Height / cb.ViewportHeight;
                }
                cb.TextureWidth = cb.ViewportWidth < cb.Width ? cb.ViewportWidth : cb.Width;
                cb.TextureHeight = (int)(cb.TextureWidth * ((float)cb.Height / cb.Width));
                polyHeight = (1f - ((float)cb.TextureHeight / cb.ViewportHeight) * widthRatio) * 2 - 1;
                polyWidth = ((float)cb.TextureWidth / cb.ViewportWidth) * 2 - 1;
            }
            else if (cb.Width < cb.Height)
            {
                if (cb.ViewportHeight > cb.Height)
                {
                    //polyHeight = (float)cb.Height / cb.ViewportHeight;
                    //polyWidth = (float)cb.Height / cb.ViewportHeight;
                }
                cb.TextureHeight = cb.ViewportHeight < cb.Height ? cb.ViewportHeight : cb.Height;
                cb.TextureWidth = (int)(cb.TextureHeight * ((float)cb.Width / cb.Height));
                polyHeight = (1f - ((float)cb.TextureHeight / cb.ViewportHeight)) * 2 - 1;
                polyWidth = ((float)cb.TextureWidth / cb.ViewportWidth) * heightRatio * 2 - 1;

            }
            else
            {
                if (cb.ViewportWidth < cb.ViewportHeight)
                {
                    if (cb.ViewportWidth < cb.Width)
                    {
                        cb.TextureWidth = cb.ViewportWidth;
                        cb.TextureHeight = (int)Math.Floor(cb.ViewportWidth * widthRatio);
                        //polyWidth = (float)cb.Width / cb.ViewportWidth;
                        //polyHeight = (float)cb.Height / cb.ViewportHeight; // polyWidth;
                    }
                }
                cb.TextureHeight = cb.ViewportHeight < cb.Height ? cb.ViewportHeight : cb.Height;
                cb.TextureWidth = cb.ViewportWidth < cb.Width ? cb.ViewportWidth : cb.Width;
                polyHeight = (1f - ((float)cb.TextureHeight / cb.ViewportHeight)) * 2 - 1;
                polyWidth = ((float)cb.TextureWidth / cb.ViewportWidth) * 2 - 1;
            }

            var mappedData = context.MapSubresource(vertexBuffer, 0, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);


            mappedData.Data.Write(new Vector3(-1f, polyHeight, 0.5f)); mappedData.Data.Write(new Vector2(0f, 1f));
            mappedData.Data.Write(new Vector3(-1f, 1f, 0.5f)); mappedData.Data.Write(new Vector2(0f, 0f));
            mappedData.Data.Write(new Vector3(polyWidth, polyHeight, 0.5f)); mappedData.Data.Write(new Vector2(1f, 1f));
            mappedData.Data.Write(new Vector3(polyWidth, 1f, 0.5f)); mappedData.Data.Write(new Vector2(1f, 0f));

            context.UnmapSubresource(vertexBuffer, 0);
            if (cb.PosOffsetX > 0 || cb.PosOffsetY > 0)
            {
                Console.Write("");
            }
            //cb.PosX = vtEntry.Column / vtPage.GetDivisor();
            //cb.PosY = vtEntry.Row / vtPage.GetDivisor();

            for (int y = 0; y <= cb.TilesY; y++)
            {
                for (int x = 0; x <= cb.TilesX; x++)
                {
                    IndirectionData.Add(0);
                }
            }
            int vtDivisor = vtPage.GetDivisor();
            foreach (var tile in tiles)
            {
                //using (MemoryStream stream = new MemoryStream())
                {
                    /*var dds = tile.GetDDS();
                    BinaryWriter bw = new BinaryWriter(stream);
                    dds.Save(bw);
                    stream.Seek(0, SeekOrigin.Begin);
                    Texture2D tex = Texture2D.FromStream(device, stream, (int)stream.Length);*/
                    DataRectangle rect = new DataRectangle(128 * 4, new DataStream(tile.TDXTile.Texture.DecompressToBytes(), false, false));
                    rects.Add(rect);
                    TileRows.Add(tile.Row);
                    TileCols.Add(tile.Column);

                    int indIndex = (((tile.Row * 120 - (vtEntry.Row / vtDivisor)) / 120) * cb.TilesX) + (tile.Column * 120 - vtEntry.Column / vtDivisor) / 120;
                    IndirectionData[indIndex] = (float)((rects.Count - 1) / tiles.Count);
                    //resourceViews.Add(new ShaderResourceView(device, tex));
                    //bw.Dispose();
                }
            }
            Texture2D textureArray = new Texture2D(device, new Texture2DDescription
            {
                BindFlags = BindFlags.ShaderResource,
                ArraySize = rects.Count,
                Width = 128,
                Height = 128,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None
            }
                , rects.ToArray());
            ShaderResourceView resourceView = new ShaderResourceView(device, textureArray);
            context.PixelShader.SetShaderResource(resourceView, 0);
            //cb.TilePosX = TileCols.ToArray();
            //cb.TilePosY = TileRows.ToArray();
            int sizeOfCB = ((sizeof(Int32) * 10 + 15) / 16) * 16;
            using (DataStream data = new DataStream(sizeOfCB, true, true))
            {
                data.Write(cb);
                data.Position = 0;
                context.PixelShader.SetConstantBuffer(new SlimDX.Direct3D11.Buffer(device, data, new BufferDescription
                {
                    Usage = ResourceUsage.Default,
                    SizeInBytes = sizeOfCB,// + cb.TilePosX.Length + cb.TilePosY.Length),
                    BindFlags = BindFlags.ConstantBuffer
                }), 0);
            }
            Texture2D inderectionTexture = new Texture2D(device, new Texture2DDescription
            {
                BindFlags = BindFlags.ShaderResource,
                ArraySize = 1,
                Width = cb.TilesX,
                Height = cb.TilesY,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R32_Float,
                SampleDescription = new SampleDescription(1, 0),
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None
            }
                , new DataRectangle(cb.TilesX * 4, new DataStream(IndirectionData.ToArray(), true, true)));
            ShaderResourceView indresourceview = new ShaderResourceView(device, inderectionTexture);
            context.PixelShader.SetShaderResource(indresourceview, 1);
            UpdateRenderer = true;
            //fx.GetVariableByName("shaderTexture").AsResource().SetResourceArray(resourceViews.ToArray());
            /*fx.GetVariableByName("Width").AsScalar().Set(vtEntry.Width);
            fx.GetVariableByName("Height").AsScalar().Set(vtEntry.Height);
            fx.GetVariableByName("TilesX").AsScalar().Set((int)(vtEntry.Width / 128.0f));
            fx.GetVariableByName("TilesY").AsScalar().Set((int)(vtEntry.Height / 128.0f));
            device.ImmediateContext.PixelShader.SetShaderResource(resourceView, 0);*/
            //context.PixelShader.SetShaderResource(resourceView, 0);
            //context.PixelShader.SetSampler(sampleState, 0);
        }
        public void Render()
        {
            //if (!t.IsRunning) t.Start();

            if (UpdateRenderer)
            {
                // clear the render target to a soothing blue
                context.ClearRenderTargetView(renderTarget, new Color4(0f, 0.5f, 0f));
                //fx.GetTechniqueByIndex(0).GetPassByIndex(0).Apply(device.ImmediateContext);
                // draw the triangle
                context.DrawIndexed(6, 0, 0);
                swapChain.Present(0, PresentFlags.None);
                //t.Restart();
                UpdateRenderer = false;
            }
        }

    }
}
