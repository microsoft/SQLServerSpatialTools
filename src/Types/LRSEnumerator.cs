//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace SQLSpatialTools.Types
{
    /// <summary>
    /// Enumerator for LRS Types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.IEnumerator" />
    internal class LRSEnumerator<T> : IEnumerator
    {
        private readonly List<T> _listOfItems;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        private int _position = -1;

        public LRSEnumerator(List<T> list)
        {
            _listOfItems = list;
        }

        public bool MoveNext()
        {
            _position++;
            return (_position < _listOfItems.Count);
        }

        public void Reset()
        {
            _position = -1;
        }

        object IEnumerator.Current => Current;

        public T Current
        {
            get
            {
                try
                {
                    return _listOfItems[_position];
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}