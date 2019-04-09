﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using SharpDirect3D12;
using SharpDXGI;

namespace Vortice.Graphics.D3D12
{
    internal class RenderPipelineStateD3D12 : RenderPipelineState
    {
        public readonly ID3D12PipelineState D3D12PipelineState;

        public RenderPipelineStateD3D12(DeviceD3D12 device, in RenderPipelineDescriptor descriptor)
            : base(device)
        {
            var inputElements = new InputElementDescription[2];
            inputElements[0] = new InputElementDescription("POSITION", 0, SharpDXGI.Format.R32G32B32_Float, 0, 0);
            inputElements[1] = new InputElementDescription("COLOR", 0, SharpDXGI.Format.R32G32B32A32_Float, 12, 0);

            var psoDesc = new GraphicsPipelineStateDescription()
            {
                RootSignature = null,
                VertexShader = ((ShaderD3D12)descriptor.VertexShader).D3D12ShaderBytecode,
                PixelShader = ((ShaderD3D12)descriptor.PixelShader).D3D12ShaderBytecode,
                InputLayout = new InputLayoutDescription(inputElements),
                SampleMask = uint.MaxValue,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RasterizerState = RasterizerDescription.CullCounterClockwise,
                BlendState = BlendDescription.Opaque,
                DepthStencilState = DepthStencilDescription.None,
                RenderTargetFormats = new[] { Format.R8G8B8A8_UNorm },
                DepthStencilFormat =D3DConvert.Convert( descriptor.DepthStencilAttachmentFormat),
                SampleDescription = new SampleDescription(1, 0)
            };
            D3D12PipelineState = device.D3D12Device.CreateGraphicsPipelineState(psoDesc);
        }

        /// <inheritdoc/>
        protected override void Destroy()
        {
            D3D12PipelineState.Dispose();
        }
    }
}