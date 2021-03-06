﻿/*                       ____               ____________
 *                      |    |             |            |
 *                      |    |             |    ________|
 *                      |    |             |   |
 *                      |    |             |   |    
 *                      |    |             |   |    ____
 *                      |    |             |   |   |    |
 *                      |    |_______      |   |___|    |
 *                      |            |  _  |            |
 *                      |____________| |_| |____________|
 *                        
 *      Author(s):      limpygnome (Marcus Craske)              limpygnome@gmail.com
 * 
 *      License:        Creative Commons Attribution-ShareAlike 3.0 Unported
 *                      http://creativecommons.org/licenses/by-sa/3.0/
 * 
 *      Path:           /Base/Result.cs
 * 
 *      Change-Log:
 *                      2013-07-20      Code cleanup, minor improvements and new comment header.
 *                                      Restricted set of properties to internal for security.
 *                                      Moved class ResultRow into a separate file (ResultRow.cs).
 * 
 * *********************************************************************************************************************
 * A data-collection of result data returned by a query.
 * *********************************************************************************************************************
 */
using System;
using System.Collections.Generic;
using System.Collections;

namespace UberLib.Connector
{
    /// <summary>
    /// A data-collection of result data returned by a query.
    /// </summary>
    public class Result : IEnumerable
    {
        // Fields ******************************************************************************************************
        internal List<ResultRow> tuples = new List<ResultRow>();
        // Methods - Constructors
        internal Result() { }
        // Methods - Properties ****************************************************************************************
        /// <summary>
        /// Fetches a tuple by its index.
        /// </summary>
        /// <param name="tupleIndex">The index of the tuple to be retrieved.</param>
        /// <returns>The tuple at the specified index.</returns>
        public ResultRow this[int tupleIndex]
        {
            get
            {
                return tuples[tupleIndex];
            }
            internal set
            {
                if (tupleIndex == -1)
                    tuples.Add(value);
                else
                    tuples[tupleIndex] = value;
            }
        }
        /// <summary>
        /// Total number of tuples.
        /// </summary>
        public int Count
        {
            get
            {
                return tuples.Count;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return tuples.GetEnumerator();
        }
    }
}