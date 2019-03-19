﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using SharpD3D11;

namespace Vortice.Graphics.D3D11
{
    internal class TextureViewD3D11 : IDisposable
    {
        public readonly DeviceD3D11 Device;
        public readonly TextureD3D11 Resource;
        public readonly ID3D11RenderTargetView RenderTargetView;
        public readonly ID3D11DepthStencilView DepthStencilView;

        public TextureViewD3D11(DeviceD3D11 device, TextureD3D11 resource, TextureViewDescriptor descriptor)
        {
            Device = device;
            Resource = resource;

            if (resource.TextureUsage.HasFlag(TextureUsage.RenderTarget))
            {
                // TODO: Use TextureViewDescriptor
                if (!PixelFormatUtil.IsDepthStencilFormat(resource.Format))
                {
                    RenderTargetView = device.Device.CreateRenderTargetView(resource.Resource);
                }
                else
                {
                    DepthStencilView = device.Device.CreateDepthStencilView(resource.Resource);
                }
            }
        }

        public void Dispose()
        {
            RenderTargetView?.Dispose();
            DepthStencilView?.Dispose();
        }
    }
}
