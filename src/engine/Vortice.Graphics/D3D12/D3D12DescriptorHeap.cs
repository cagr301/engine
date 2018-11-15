﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SharpDX.Direct3D12;

namespace Vortice.Graphics.D3D12
{
    internal class D3D12DescriptorHeap : IDisposable
    {
        public readonly D3D12GraphicsDevice Device;
        private int _persistentAllocated;
        private readonly int _persistentCount;
        private readonly int _temporaryCount;
        private readonly DescriptorHeapType _heapType;
        private readonly bool _shaderVisible;
        private readonly int _heapCount;
        private readonly DescriptorHeap[] _heaps;
        private readonly CpuDescriptorHandle[] _cpuStart;
        private readonly GpuDescriptorHandle[] _gpuStart;

        public readonly int DescriptorSize;

        private readonly int[] _deadList;
        private int _heapIndex;
        private readonly object _lockObject = new object();
        private volatile int _temporaryAllocated = 0;

        public D3D12DescriptorHeap(
            D3D12GraphicsDevice device,
            int persistentCount,
            int temporaryCount,
            DescriptorHeapType heapType,
            bool shaderVisible)
        {
            Device = device;

            var totalDescriptors = persistentCount + temporaryCount;
            Debug.Assert(totalDescriptors > 0);

            _persistentCount = persistentCount;
            _temporaryCount = temporaryCount;
            _heapType = heapType;
            _shaderVisible = shaderVisible;

            if (heapType == DescriptorHeapType.RenderTargetView
                || heapType == DescriptorHeapType.DepthStencilView)
            {
                _shaderVisible = false;
            }

            _heapCount = _shaderVisible ? 2 : 1;
            _heaps = new DescriptorHeap[_heapCount];
            _cpuStart = new CpuDescriptorHandle[_heapCount];
            _gpuStart = new GpuDescriptorHandle[_heapCount];

            var heapDesc = new DescriptorHeapDescription()
            {
                Type = _heapType,
                DescriptorCount = totalDescriptors,
                Flags = _shaderVisible ? DescriptorHeapFlags.ShaderVisible : DescriptorHeapFlags.None,
                NodeMask = 0,
            };

            for (var i = 0; i < _heapCount; i++)
            {
                _heaps[i] = device.Device.CreateDescriptorHeap(heapDesc);
                _cpuStart[i] = _heaps[i].CPUDescriptorHandleForHeapStart;
                if (_shaderVisible)
                {
                    _gpuStart[i] = _heaps[i].GPUDescriptorHandleForHeapStart;
                }
            }

            DescriptorSize = device.Device.GetDescriptorHandleIncrementSize(_heapType);

            _deadList = new int[persistentCount];
            for (var i = 0; i < persistentCount; ++i)
            {
                _deadList[i] = i;
            }
        }

        public void Dispose()
        {
            Debug.Assert(_persistentAllocated == 0);
            for (var i = 0; i < _heapCount; i++)
            {
                _heaps[i].Dispose();
            }
        }

        public void EndFrame()
        {
            Debug.Assert(_heaps[0] != null);
            _temporaryAllocated = 0;
            _heapIndex = (_heapIndex + 1) % _heapCount;
        }

        public DescriptorHeap CurrentHeap()
        {
            Debug.Assert(_heaps[0] != null);
            return _heaps[_heapIndex];
        }

        public int TotalNumDescriptors => _persistentCount + _temporaryCount;

        public int IndexFromHandle(CpuDescriptorHandle handle)
        {
            Debug.Assert(_heaps[0] != null);
            long handlePtr = handle.Ptr;
            Debug.Assert(handlePtr >= _cpuStart[_heapIndex].Ptr);
            Debug.Assert(handlePtr < _cpuStart[_heapIndex].Ptr + (DescriptorSize * TotalNumDescriptors));
            Debug.Assert((handlePtr - _cpuStart[_heapIndex].Ptr) % DescriptorSize == 0);
            return (handlePtr - _cpuStart[_heapIndex].Ptr) / DescriptorSize;
        }

        public int IndexFromHandle(GpuDescriptorHandle handle)
        {
            Debug.Assert(_heaps[0] != null);
            Debug.Assert(handle.Ptr >= _gpuStart[_heapIndex].Ptr);
            Debug.Assert(handle.Ptr < _gpuStart[_heapIndex].Ptr + (DescriptorSize * TotalNumDescriptors));
            Debug.Assert((handle.Ptr - _gpuStart[_heapIndex].Ptr) % DescriptorSize == 0);
            return (int)(handle.Ptr - _gpuStart[_heapIndex].Ptr) / DescriptorSize;
        }

        public PersistentDescriptorAlloc AllocatePersistent()
        {
            Debug.Assert(_heaps[0] != null);

            lock (_lockObject)
            {
                Debug.Assert(_persistentAllocated < _persistentCount);
                var index = _deadList[_persistentAllocated];
                ++_persistentAllocated;

                var handles = new CpuDescriptorHandle[_heapCount];
                for (var i = 0; i < _heapCount; ++i)
                {
                    handles[i] = _cpuStart[i];
                    handles[i].Ptr += index * DescriptorSize;
                }

                return new PersistentDescriptorAlloc(handles, index);
            }
        }

        public void FreePersistent(ref int index)
        {
            if (index == -1)
                return;

            Debug.Assert(index < _persistentCount);
            Debug.Assert(_heaps[0] != null);

            lock (_lockObject)
            {
                Debug.Assert(_persistentAllocated > 0);
                _deadList[_persistentAllocated - 1] = index;
                --_persistentAllocated;
            }

            index = -1;
        }

        public void FreePersistent(ref CpuDescriptorHandle handle)
        {
            Debug.Assert(_heapCount == 1);
            if (handle.Ptr != 0)
            {
                var index = IndexFromHandle(handle);
                FreePersistent(ref index);
                handle = default;
            }
        }

        public void FreePersistent(ref GpuDescriptorHandle handle)
        {
            Debug.Assert(_heapCount == 1);
            if (handle.Ptr != 0)
            {
                var index = IndexFromHandle(handle);
                FreePersistent(ref index);
                handle = default;
            }
        }
    }

    public readonly struct PersistentDescriptorAlloc
    {
        public readonly CpuDescriptorHandle[] Handles;
        public readonly int Index;

        public PersistentDescriptorAlloc(CpuDescriptorHandle[] handles, int index)
        {
            Handles = handles;
            Index = index;
        }
    };
}
