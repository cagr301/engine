﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;

namespace Vortice.Graphics
{
    /// <summary>
    /// Defines a command context for storing recorded graphics commands.
    /// </summary>
    public abstract class CommandContext : GraphicsResource
    {
        /// <summary>
        /// Create a new instance of <see cref="CommandContext"/> class.
        /// </summary>
        /// <param name="device">The creation device</param>
        protected CommandContext(GraphicsDevice device)
            : base(device, GraphicsResourceType.CommandContext)
        {
        }

        /// <summary>
        /// Begin rendering with current backbuffer texture.
        /// </summary>
        /// <param name="clearColor">The <see cref="Color4"/> to clear with.</param>
        public void BeginRenderPass(Color4 clearColor)
        {
            var renderPassDescription = new RenderPassDescription(
                new[] { new RenderPassColorAttachment(Device.BackbufferTexture, clearColor) }
                );

            BeginRenderPassCore(renderPassDescription);
        }

        public void EndRenderPass()
        {
            EndRenderPassCore();
        }

        protected abstract void BeginRenderPassCore(in RenderPassDescription renderPassDescription);
        protected abstract void EndRenderPassCore();
    }
}
