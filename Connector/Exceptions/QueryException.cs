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
 *      Path:           /Exceptions/QueryException.cs
 * 
 *      Change-Log:
 *                      2013-07-20      Minor improvements, new code-format and clean-up.
 * 
 * *********************************************************************************************************************
 * An exception thrown when an unknown exception occurs during a connector operation.
 * *********************************************************************************************************************
 */
using System;
using System.Runtime.Serialization;

namespace UberLib.Connector
{
    /// <summary>
    /// An exception thrown when an unknown exception occurs during a connector operation.
    /// </summary>
    [Serializable]
    public class QueryException : Exception
    {
        // Methods - Constructors **************************************************************************************
        public QueryException() : base() { }
        public QueryException(string message) : base(message) { }
        public QueryException(string message, Exception innerException) : base(message, innerException) { }
        public QueryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}