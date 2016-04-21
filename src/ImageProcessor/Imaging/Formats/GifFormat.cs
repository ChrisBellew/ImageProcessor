﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GifFormat.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides the necessary information to support gif images.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Formats
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Text;

    using ImageProcessor.Imaging.Quantizers;

    /// <summary>
    /// Provides the necessary information to support gif images.
    /// </summary>
    public class GifFormat : FormatBase, IQuantizableImageFormat, IAnimatedImageFormat
    {
        /// <summary>
        /// The quantizer for reducing the image palette.
        /// </summary>
        private IQuantizer quantizer = new OctreeQuantizer(255, 8);

        /// <summary>
        /// Gets or sets the process mode for frames in animated images.
        /// </summary>
        public AnimationProcessMode AnimationProcessMode { get; set; }

        /// <summary>
        /// Gets the file headers.
        /// </summary>
        public override byte[][] FileHeaders
        {
            get
            {
                return new[] { Encoding.ASCII.GetBytes("GIF") };
            }
        }

        /// <summary>
        /// Gets the list of file extensions.
        /// </summary>
        public override string[] FileExtensions
        {
            get
            {
                return new[] { "gif" };
            }
        }

        /// <summary>
        /// Gets the standard identifier used on the Internet to indicate the type of data that a file contains. 
        /// </summary>
        public override string MimeType
        {
            get
            {
                return "image/gif";
            }
        }

        /// <summary>
        /// Gets the <see cref="ImageFormat" />.
        /// </summary>
        public override ImageFormat ImageFormat
        {
            get
            {
                return ImageFormat.Gif;
            }
        }

        /// <summary>
        /// Gets or sets the quantizer for reducing the image palette.
        /// </summary>
        public IQuantizer Quantizer
        {
            get
            {
                return this.quantizer;
            }

            set
            {
                this.quantizer = value;
            }
        }

        /// <summary>
        /// Applies the given processor the current image.
        /// </summary>
        /// <param name="processor">The processor delegate.</param>
        /// <param name="factory">The <see cref="ImageFactory" />.</param>
        public override void ApplyProcessor(Func<ImageFactory, Image> processor, ImageFactory factory)
        {
            GifDecoder decoder = new GifDecoder(factory.Image, factory.AnimationProcessMode);
            Image factoryImage = factory.Image;
            GifEncoder encoder = new GifEncoder(null, null, decoder.LoopCount);

            for (int i = 0; i < decoder.FrameCount; i++)
            {
                GifFrame frame = decoder.GetFrame(factoryImage, i);
                factory.Image = frame.Image;
                frame.Image = this.Quantizer.Quantize(processor.Invoke(factory));
                encoder.AddFrame(frame);
            }

            factoryImage.Dispose();
            factory.Image = encoder.Save();
        }
    }
}