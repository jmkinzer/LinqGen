﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cathei.LinqGen.Hidden
{
    /// <summary>
    /// Do not use this struct manually, reserved for generated code
    /// </summary>
    public struct PooledList<T> : IDisposable
    {
        private T[] _array;
        private int _count;

        private static readonly T[] EmptyArray = new T[0];

        public PooledList(int capacity)
        {
            _array = capacity > 0 ? SharedArrayPool<T>.Rent(capacity) : EmptyArray;
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncreaseCapacity()
        {
            var newItems = SharedArrayPool<T>.Rent(_count + 1);
            System.Array.Copy(_array, newItems, _count);

            ReturnArray();
            _array = newItems;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReturnArray()
        {
            if (_array.Length == 0)
                return;

            try
            {
                // Clear the elements so that the gc can reclaim the references.
                // TODO no need to clear for unmanaged type
                SharedArrayPool<T>.Return(_array, true);
            }
            catch (ArgumentException)
            {
                // oh well, the array pool didn't like our array
            }

            _array = EmptyArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            var localArray = _array;
            int index = _count;

            // this should remove array bound check
            if ((uint)index < (uint)localArray.Length)
            {
                localArray[index] = item;
            }
            else
            {
                IncreaseCapacity();
                _array[index] = item;
            }

            _count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange<TEnumerator>(TEnumerator iter)
            where TEnumerator : IEnumerator<T>
        {
            var localArray = _array;
            int index = _count;

            while (iter.MoveNext())
            {
                // this should remove array bound check
                if ((uint)index < (uint)localArray.Length)
                {
                    localArray[index] = iter.Current;
                    index++;
                }
                else
                {
                    // resize and assign
                    _count = index;
                    IncreaseCapacity();
                    localArray = _array;
                    localArray[index] = iter.Current;
                }
            }

            _count = index;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public T[] Array => _array;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _array[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            ReturnArray();
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            int count = _count;
            var result = new T[count];
            System.Array.Copy(_array, result, count);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> ToList()
        {
            int count = _count;
            var result = new List<T>(count);
            var listLayout = UnsafeUtils.As<List<T>, ListLayout<T>>(ref result);

            System.Array.Copy(_array, listLayout.Items, _count);
            listLayout.Size = count;
            return result;
        }
    }
}