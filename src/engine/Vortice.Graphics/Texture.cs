﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;

namespace Vortice.Graphics
{
    /// <summary>
    /// Defines a graphics texture class.
    /// </summary>
    public abstract class Texture : GraphicsResource
    {
        /// <summary>
        /// Gets the texture type.
        /// </summary>
        public TextureType TextureType { get; }

        /// <summary>
		/// Gets the total width of this instance, in texels.
		/// </summary>
		public int Width { get; }

        /// <summary>
        /// Gets the total height of this instance, in texels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the total depth of this instance, in texels.
        /// </summary>
        public int Depth { get; }

        /// <summary>
		/// Gets the total number of mipmap levels in this instance.
		/// </summary>
		public int MipLevels { get; }

        /// <summary>
        /// Gets the total number of array layers in this instance.
        /// </summary>
        public int ArrayLayers { get; }

        /// <summary>
		/// Gets the <see cref="PixelFormat"/> of individual texture elements.
		/// </summary>
		public PixelFormat Format { get; }

        /// <summary>
        /// Gets the texture usage.
        /// </summary>
        public TextureUsage Usage { get; }

        /// <summary>
		/// Gets the number of samples.
		/// </summary>
        public SampleCount Samples { get; }

        /// <summary>
        /// Create a new instance of <see cref="Texture"/> class.
        /// </summary>
        /// <param name="device">The creation device.</param>
        /// <param name="descriptor">The texture descriptor.</param>
        protected Texture(GraphicsDevice device, ref TextureDescriptor descriptor)
            : base(device, GraphicsResourceType.Texture, GraphicsResourceUsage.Default)
        {
            Guard.IsTrue(descriptor.TextureType != TextureType.Unknown, nameof(descriptor), $"TextureType cannot be {nameof(TextureType.Unknown)}");
            Guard.MustBeGreaterThanOrEqualTo(descriptor.Width, 1, nameof(descriptor.Width));
            Guard.MustBeGreaterThanOrEqualTo(descriptor.Height, 1, nameof(descriptor.Height));
            Guard.MustBeGreaterThanOrEqualTo(descriptor.Depth, 1, nameof(descriptor.Depth));

            TextureType = descriptor.TextureType;
            Width = descriptor.Width;
            Height = descriptor.Height;
            Depth = descriptor.Depth;
            MipLevels = descriptor.MipLevels;
            ArrayLayers = descriptor.ArrayLayers;
            Format = descriptor.Format;
            Usage = descriptor.Usage;
            Samples = descriptor.Samples;
        }

        public int GetLevelWidth(int mipLevel = 0) => Math.Max(1, Width >> mipLevel);
        public int GetLevelHeight(int mipLevel = 0) => Math.Max(1, Height >> mipLevel);
        public int GetLevelDepth(int mipLevel = 0) => Math.Max(1, Depth >> mipLevel);
    }
}
