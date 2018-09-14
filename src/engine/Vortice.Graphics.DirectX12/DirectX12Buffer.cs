﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using SharpDX.Direct3D12;

namespace Vortice.Graphics.DirectX12
{
    internal class DirectX12Buffer : GraphicsBuffer
    {
        public readonly Resource Resource;

        public DirectX12Buffer(DirectX12GraphicsDevice device, in BufferDescriptor descriptor, IntPtr initialData)
            : base(device, descriptor)
        {
        }

        /// <inheritdoc/>
        protected override void Destroy()
        {
            Resource.Dispose();
        }
    }
}
