﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using SharpDX.Direct3D12;

namespace Vortice.Graphics.D3D12
{
    internal class CommandBufferD3D12 : CommandBuffer
    {
        private readonly int _frameCount;
        private readonly CommandListType _type;
        private int _currentFrameIndex;

        private readonly CommandAllocator[] _commandAllocators;

        public GraphicsCommandList CommandList { get; }

        public CommandBufferD3D12(DeviceD3D12 device, int frameCount, CommandListType type)
            : base(device)
        {
            _frameCount = frameCount;
            _type = type;

            _commandAllocators = new CommandAllocator[frameCount];
            for (var i = 0; i < frameCount; ++i)
            {
                _commandAllocators[i] = device.D3DDevice.CreateCommandAllocator(type);
            }

            CommandList = device.D3DDevice.CreateCommandList(type, _commandAllocators[_currentFrameIndex], null);
            CommandList.Close();
        }

        /// <inheritdoc/>
        protected override void Destroy()
        {
            for (var i = 0; i < _frameCount; ++i)
            {
                _commandAllocators[i].Dispose();
            }

            CommandList.Dispose();
        }

        internal override void BeginRenderPassCore(in RenderPassDescriptor descriptor)
        {
        }

        protected override void EndRenderPassCore()
        {
        }

        protected override void CommitCore()
        {
            CommandList.Close();
            ((DeviceD3D12)Device).GraphicsQueue.ExecuteCommandList(CommandList);

            // 
            _currentFrameIndex = (_currentFrameIndex + 1) % _commandAllocators.Length;
            _commandAllocators[_currentFrameIndex].Reset();
            CommandList.Reset(_commandAllocators[_currentFrameIndex], null);
        }
    }
}
