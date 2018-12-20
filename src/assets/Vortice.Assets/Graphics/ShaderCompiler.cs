﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using Vortice.Graphics;
using DotNetDxc;

namespace Vortice.Assets.Graphics
{
    public static class ShaderCompiler
    {
        public static byte[] Compile(
            string source,
            ShaderStages stage,
            ShaderLanguage language,
            string entryPoint = "",
            string fileName = "")
        {
            if (string.IsNullOrEmpty(entryPoint))
            {
                entryPoint = GetDefaultEntryPoint(stage);
            }

            bool isDxil = language == ShaderLanguage.DXIL;
            if (isDxil)
            {
                RuntimeHelpers.RunClassConstructor(typeof(Dxc).TypeHandle);
                var shaderProfile = $"{GetShaderProfile(stage)}_6_0";

                var arguments = new string[]
                {
                    "-T", shaderProfile,
                    "-E", entryPoint,
                };

                var compiler = Dxc.CreateDxcCompiler();
                var result = compiler.Compile(
                    Dxc.CreateBlobForText(source),
                    fileName,
                    entryPoint,
                    shaderProfile,
                    arguments,
                    arguments.Length,
                    null,
                    0,
                    Dxc.Library.CreateIncludeHandler()
                    );

                if (result.GetStatus() == 0)
                {
                    var blob = result.GetResult();
                    var bytecode = Dxc.GetBytesFromBlob(blob);

                    var reflection = Dxc.CreateDxcContainerReflection();
                    reflection.Load(blob);
                    int hr = reflection.FindFirstPartKind(Dxc.DFCC_DXIL, out uint index);
                    if (hr < 0)
                    {
                        //MessageBox.Show("Debug information not found in container.");
                        //return;
                    }

                    unsafe
                    {
                        IDxcBlob part = reflection.GetPartContent(index);
                        uint* p = (uint*)part.GetBufferPointer();
                        var v = DescribeProgramVersion(*p);
                    }

                    // Disassemble
                    var disassembleBlob = compiler.Disassemble(blob);
                    string disassemblyText = Dxc.GetStringFromBlob(disassembleBlob);

                    return bytecode;
                }
                else
                {
                    var resultText = Dxc.GetStringFromBlob(result.GetErrors());
                }
            }
            else
            {
                uint flags = 0;
                var shaderProfile = $"{GetShaderProfile(stage)}_5_0";
                int hr = D3DCompiler.D3DCompiler.D3DCompile(
                    source,
                    source.Length,
                    fileName,
                    null,
                    0,
                    entryPoint,
                    shaderProfile,
                    flags,
                    0,
                    out IDxcBlob blob,
                    out IDxcBlob errorMsgs);

                if (hr != 0)
                {
                    if (errorMsgs != null)
                    {
                        var errorText = Dxc.GetStringFromBlob(errorMsgs);
                    }
                }
                else
                {
                    var bytecode = Dxc.GetBytesFromBlob(blob);
                    return bytecode;
                }
            }

            return null;
        }

        private static string GetDefaultEntryPoint(ShaderStages stage)
        {
            switch (stage)
            {
                case ShaderStages.Vertex:
                    return "VSMain";
                case ShaderStages.Hull:
                    return "HSMain";
                case ShaderStages.Domain:
                    return "DSMain";
                case ShaderStages.Geometry:
                    return "GSMain";
                case ShaderStages.Pixel:
                    return "PSMain";
                case ShaderStages.Compute:
                    return "CSMain";
                default:
                    return string.Empty;
            }
        }

        private static string GetShaderProfile(ShaderStages stage)
        {
            switch (stage)
            {
                case ShaderStages.Vertex:
                    return "vs";
                case ShaderStages.Hull:
                    return "hs";
                case ShaderStages.Domain:
                    return "ds";
                case ShaderStages.Geometry:
                    return "gs";
                case ShaderStages.Pixel:
                    return "ps";
                case ShaderStages.Compute:
                    return "cs";
                default:
                    return string.Empty;
            }
        }

        private static string DescribeProgramVersion(uint programVersion)
        {
            uint kind, major, minor;
            kind = ((programVersion & 0xffff0000) >> 16);
            major = (programVersion & 0xf0) >> 4;
            minor = (programVersion & 0xf);
            string[] shaderKinds = "Pixel,Vertex,Geometry,Hull,Domain,Compute".Split(',');
            return shaderKinds[kind] + " " + major + "." + minor;
        }
    }

    public enum ShaderLanguage
    {
        /// <summary>
        /// DXIL bytecode
        /// </summary>
        DXIL,

        /// <summary>
        /// DXC (legacy) bytecode.
        /// </summary>
        DXC,

        /// <summary>
        /// SPIRV bytecode.
        /// </summary>
        SPIRV,

        /// <summary>
        /// OpenGL desktop shader language.
        /// </summary>
        GLSL,

        /// <summary>
        /// OpenGLES shader language.
        /// </summary>
        ESSL,

        /// <summary>
        /// Metal shading language.
        /// </summary>
        MSL
    }
}
