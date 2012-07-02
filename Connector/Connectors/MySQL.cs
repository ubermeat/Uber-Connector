﻿/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        Connectors\MySQL.cs
 * Authors:     limpygnome              limpygnome@gmail.com
 * 
 * Provides an interface for MySQL servers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.IO;

namespace UberLib.Connector.Connectors
{
    public class MySQL : Connector
    {
        #region "Constants"
        private const int BYTE_BUFFER_SIZE = 4098;
        #endregion

        #region "Variables"
        private MySqlConnection _rawConnector = new MySqlConnection();
        private string _Settings_Host = "localhost";
        private int _Settings_Port = 3306;
        private string _Settings_User = "root";
        private string _Settings_Pass = "";
        private string _Settings_Database = "";
        private int _Settings_Timeout_Command = 20;
        private int _Settings_Timeout_Connection = 5;
        private bool _Settings_Pooling = false;
        private int _Settings_Pooling_Size_Min = 0;
        private int _Settings_Pooling_Size_Max = 20;
        private string _Settings_ConnectionString = "";
        #endregion

        #region "Properties"
        public string Settings_Connection_String
        {
            get
            {
                return _Settings_ConnectionString;
            }
            set
            {
                _Settings_ConnectionString = value;
            }
        }
        public MySqlConnection Raw_Connector
        {
            get
            {
                return _rawConnector;
            }
            set
            {
                _rawConnector = value;
            }
        }
        public string Settings_Host
        {
            get
            {
                return _Settings_Host;
            }
            set
            {
                if (Uri.CheckHostName(value) == UriHostNameType.Unknown) throw new Exception("Invalid host!");
                _Settings_Host = value;
            }
        }
        public int Settings_Port
        {
            get
            {
                return _Settings_Port;
            }
            set
            {
                // This is only basic validation
                if (value < 1 || value > 65535) throw new Exception("Invalid port!");
                _Settings_Port = value;
            }
        }
        public string Settings_User
        {
            get
            {
                return _Settings_User;
            }
            set
            {
                _Settings_User = value;
            }
        }
        public string Settings_Pass
        {
            get
            {
                return _Settings_Pass;
            }
            set
            {
                _Settings_Pass = value;
            }
        }
        public string Settings_Database
        {
            get
            {
                return _Settings_Database;
            }
            set
            {
                _Settings_Database = value;
            }
        }
        public int Settings_Timeout_Command
        {
            get
            {
                return _Settings_Timeout_Command;
            }
            set
            {
                if (value < 0) throw new Exception("Value cannot be less than zero!");
                _Settings_Timeout_Command = value;
            }
        }
        public int Settings_Timeout_Connection
        {
            get
            {
                return _Settings_Timeout_Connection;
            }
            set
            {
                if (value < 0) throw new Exception("Value cannot be less than zero!");
                _Settings_Timeout_Connection = value;
            }
        }
        public bool Settings_Pooling
        {
            get
            {
                return _Settings_Pooling;
            }
            set
            {
                _Settings_Pooling = value;
            }
        }
        public int Settings_Pooling_Size_Min
        {
            get
            {
                return _Settings_Pooling_Size_Min;
            }
            set
            {
                if (value < 0) throw new Exception("Value cannot be less than zero!");
                _Settings_Pooling_Size_Min = value;
            }
        }
        public int Settings_Pooling_Size_Max
        {
            get
            {
                return _Settings_Pooling_Size_Max;
            }
            set
            {
                if (value < 0) throw new Exception("Value cannot be less than zero!");
                _Settings_Pooling_Size_Max = value;
            }
        }
        public int Logging_Queries_Count
        {
            get
            {
                return _Logging_Queries_Count;
            }
        }
        public List<string> Logging_Queries
        {
            get
            {
                return _Logging_Queries;
            }
        }
        public bool Logging_Enabled
        {
            get
            {
                return _Logging_Enabled;
            }
            set
            {
                _Logging_Enabled = value;
            }
        }
        #endregion

        #region "Methods"
        public override void Connect()
        {
            try
            {
                _rawConnector.ConnectionString = "Host=" + _Settings_Host + "; Port=" + _Settings_Port + "; Database=" + _Settings_Database + "; UID=" + _Settings_User + "; Password=" + _Settings_Pass + "; Pooling=" + _Settings_Pooling + "; " + (_Settings_Timeout_Command > 0 ? "Default Command Timeout=" + _Settings_Timeout_Command + "; " : "") + (_Settings_Timeout_Connection > 0 ? "Connection Timeout=" + _Settings_Timeout_Connection + "; " : "") + (_Settings_Pooling ? "Min Pool Size=" + _Settings_Pooling_Size_Min + "; Max Pool Size=" + _Settings_Pooling_Size_Max + "; " : "") + _Settings_ConnectionString;
                _rawConnector.Open();
                if (_Logging_Enabled) _Logging_Queries = new List<string>();
            }
            catch (Exception ex)
            {
                throw new ConnectionFailureException("Failed to establish a connection using MySQL!", ex);
            }
        }
        public override void Disconnect()
        {
            _rawConnector.Close();
        }
        public override void ChangeDatabase(string database)
        {
            _rawConnector.ChangeDatabase(database);
        }
        public override Result Query_Read(string query)
        {
            lock (_rawConnector)
            {
                CheckConnectionIsReady();
                _Logging_Queries_Count++;
                if (_Logging_Enabled) _Logging_Add_Entry(query);
                Result result = null;
                MySqlCommand command = null;
                MySqlDataReader reader = null;
                byte[] buffer;
                MemoryStream bufferMS;
                int bufferOffset;
                int bytesAvailable;
                try
                {

                    result = new Result();
                    command = new MySqlCommand(query, _rawConnector);
                    command.CommandTimeout = _Settings_Timeout_Command;
                    reader = command.ExecuteReader();
                    ResultRow row;
                    int t;
                    while (reader.Read())
                    {
                        row = new ResultRow();
                        for (t = 0; t < reader.FieldCount; t++)
                        {
                            row.Columns.Add(reader.GetName(t), reader.GetValue(t).ToString());
                            if (reader.GetDataTypeName(t) == "BLOB") // Check if the column of the row is a byte-array, if so -> add to separate byte dictionary
                            {
                                bufferMS = new MemoryStream();
                                try
                                {
                                    bufferOffset = 0;
                                    bytesAvailable = (int)reader.GetBytes(t, 0, null, 0, 0);
                                    while (bufferOffset < bytesAvailable)
                                    {
                                        reader.GetBytes(t, bufferOffset, buffer = new byte[BYTE_BUFFER_SIZE], 0, BYTE_BUFFER_SIZE);
                                        bufferMS.Write(buffer, 0, BYTE_BUFFER_SIZE);
                                        bufferOffset += BYTE_BUFFER_SIZE;
                                    }
                                    bufferMS.Flush();
                                    if (row.ColumnsByteArray == null) row.ColumnsByteArray = new Dictionary<string, byte[]>();
                                    row.ColumnsByteArray.Add(reader.GetName(t), bufferMS.ToArray());
                                }
                                catch { }
                                finally
                                {
                                    bufferMS.Dispose();
                                }
                            }
                        }
                        result[-1] = row;
                    }
                    reader.Close();
                }
                catch(Exception ex)
                {
                    throw new QueryException("Failed to read query '" + query + "'!", ex);
                }
                return result;
            }
        }
        public override Result Query_Read_StoredProcedure(string procedureName, Dictionary<string, string> inputParameters, Dictionary<string, Connector.DataType> outputParameters)
        {
            lock (_rawConnector)
            {
                CheckConnectionIsReady();
                _Logging_Queries_Count++;
                if (_Logging_Enabled) _Logging_Add_Entry("Stored procedure : " + procedureName);
                Result result;
                MySqlCommand command;
                MySqlDataReader reader;
                byte[] buffer;
                MemoryStream bufferMS;
                int bufferOffset;
                int bytesAvailable;
                try
                {

                    result = new Result();
                    command = new MySqlCommand(procedureName, _rawConnector);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = _Settings_Timeout_Command;
                    // Add parameters
                    foreach(KeyValuePair<string, string> inputs in inputParameters)
                    {
                        command.Parameters.Add("@" + inputs.Key, inputs.Value);
                        command.Parameters["@" + inputs.Key].Direction = ParameterDirection.Input;
                    }
                    MySqlDbType type;
                    foreach (KeyValuePair<string, Connector.DataType> outputs in outputParameters)
                    {
                        switch (outputs.Value)
                        {
                            case DataType.Binary:
                                type = MySqlDbType.Binary;
                                break;
                            case DataType.Blob:
                                type = MySqlDbType.Blob;
                                break;
                            case DataType.Byte:
                                type = MySqlDbType.Byte;
                                break;
                            case DataType.Date:
                                type = MySqlDbType.Date;
                                break;
                            case DataType.DateTime:
                                type = MySqlDbType.DateTime;
                                break;
                            case DataType.Decimal:
                                type = MySqlDbType.Decimal;
                                break;
                            case DataType.Double:
                                type = MySqlDbType.Double;
                                break;
                            case DataType.Float:
                                type = MySqlDbType.Float;
                                break;
                            case DataType.Int16:
                                type = MySqlDbType.Int16;
                                break;
                            case DataType.Int24:
                                type = MySqlDbType.Int24;
                                break;
                            case DataType.Int32:
                                type = MySqlDbType.Int32;
                                break;
                            case DataType.Int64:
                                type = MySqlDbType.Int64;
                                break;
                            case DataType.LongBlob:
                                type = MySqlDbType.LongBlob;
                                break;
                            case DataType.LongText:
                                type = MySqlDbType.LongText;
                                break;
                            case DataType.MediumBlob:
                                type = MySqlDbType.MediumBlob;
                                break;
                            case DataType.MediumText:
                                type = MySqlDbType.MediumText;
                                break;
                            case DataType.String:
                                type = MySqlDbType.String;
                                break;
                            case DataType.Text:
                                type = MySqlDbType.Text;
                                break;
                            case DataType.Time:
                                type = MySqlDbType.Time;
                                break;
                            case DataType.Timestamp:
                                type = MySqlDbType.Timestamp;
                                break;
                            case DataType.TinyBlob:
                                type = MySqlDbType.TinyBlob;
                                break;
                            case DataType.TinyText:
                                type = MySqlDbType.TinyText;
                                break;
                            case DataType.UByte:
                                type = MySqlDbType.UByte;
                                break;
                            case DataType.UInt16:
                                type = MySqlDbType.UInt16;
                                break;
                            case DataType.UInt24:
                                type = MySqlDbType.UInt24;
                                break;
                            case DataType.UInt32:
                                type = MySqlDbType.UInt32;
                                break;
                            case DataType.UInt64:
                                type = MySqlDbType.UInt64;
                                break;
                            case DataType.Varchar:
                                type = MySqlDbType.VarChar;
                                break;
                            case DataType.Year:
                                type = MySqlDbType.Year;
                                break;
                            default:
                                throw new Exception("Non-supported type provided for stored procedure!");
                        }
                        command.Parameters.Add("@" + outputs.Key, type);
                        command.Parameters["@" + outputs.Key].Direction = ParameterDirection.Output;
                    }
                    // Begin reading
                    reader = command.ExecuteReader();
                    ResultRow row;
                    int t;
                    // Create result
                    while (reader.Read())
                    {
                        row = new ResultRow();
                        for (t = 0; t < reader.FieldCount; t++)
                        {
                            row.Columns.Add(reader.GetName(t), reader.GetValue(t).ToString());
                            if (reader.GetDataTypeName(t) == "BLOB") // Check if the column of the row is a byte-array, if so -> add to separate byte dictionary
                            {
                                bufferMS = new MemoryStream();
                                try
                                {
                                    bufferOffset = 0;
                                    bytesAvailable = (int)reader.GetBytes(t, 0, null, 0, 0);
                                    while (bufferOffset < bytesAvailable)
                                    {
                                        reader.GetBytes(t, bufferOffset, buffer = new byte[BYTE_BUFFER_SIZE], 0, BYTE_BUFFER_SIZE);
                                        bufferMS.Write(buffer, 0, BYTE_BUFFER_SIZE);
                                        bufferOffset += BYTE_BUFFER_SIZE;
                                    }
                                    bufferMS.Flush();
                                    if (row.ColumnsByteArray == null) row.ColumnsByteArray = new Dictionary<string, byte[]>();
                                    row.ColumnsByteArray.Add(reader.GetName(t), bufferMS.ToArray());
                                }
                                catch { }
                                finally
                                {
                                    bufferMS.Dispose();
                                }
                            }
                        }
                        result[-1] = row;
                    }
                    reader.Close();
                    // Check if we have any rows, else read the parameters into a row
                    if (result.Rows.Count == 0)
                    {
                        row = new ResultRow();
                        MySqlParameter param;
                        for (int i = 0; i < command.Parameters.Count; i++)
                        {
                            param = command.Parameters[i];
                            if (param.MySqlDbType == MySqlDbType.Blob)
                                // Untested...
                                row.ColumnsByteArray.Add(param.ParameterName.Substring(1), (byte[])param.Value);
                            else
                                row.Columns.Add(param.ParameterName.Substring(1), param.Value);
                        }
                        result.Rows.Add(row);
                    }
                }
                catch (Exception ex)
                {
                    throw new QueryException("Failed to execute/read stored procedure '" + procedureName + "'!", ex);
                }
                return result;
            }
        }
        public override int Query_Count(string query)
        {
            return int.Parse((Query_Scalar(query) ?? 0).ToString());
        }
        public override object Query_Scalar(string query)
        {
            lock (_rawConnector)
            {
                CheckConnectionIsReady();
                _Logging_Queries_Count++;
                if (_Logging_Enabled) _Logging_Add_Entry(query);
                try
                {
                    MySqlCommand command = new MySqlCommand(query, _rawConnector);
                    command.CommandTimeout = _Settings_Timeout_Command;
                    return command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw new QueryException("Failed to read query '" + query + "'!", ex);
                }
            }
        }
        public override void Query_Execute(string query)
        {
            lock (_rawConnector)
            {
                CheckConnectionIsReady();
                _Logging_Queries_Count++;
                if (_Logging_Enabled) _Logging_Add_Entry(query);
                try
                {
                    MySqlCommand command = new MySqlCommand(query, _rawConnector);
                    command.CommandTimeout = _Settings_Timeout_Command;
                    command.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    throw new QueryExecuteException("Failed to execute query '" + query + "'!", ex);
                }
            }
        }
        public override void Query_Execute_Parameters(string query, Dictionary<string, object> parameters)
        {
            lock (_rawConnector)
            {
                CheckConnectionIsReady();
                _Logging_Queries_Count++;
                if (_Logging_Enabled) _Logging_Add_Entry(query);
                try
                {
                    MySqlCommand command = new MySqlCommand(query, _rawConnector);
                    command.CommandTimeout = _Settings_Timeout_Command;
                    foreach (KeyValuePair<string, object> key in parameters)
                        command.Parameters.AddWithValue("@" + key.Key, key.Value);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new QueryExecuteException("Failed to execute query '" + query + "'!", ex);
                }
            }
        }
        public override bool CheckConnectionIsReady()
        {
            lock (_rawConnector)
            {
                try
                {
                    // Reopen the connector if it is not ready
                    if (_rawConnector.State == ConnectionState.Closed || _rawConnector.State == ConnectionState.Broken)
                        _rawConnector.Open();
                    return true;
                }
                catch { return false; }
            }
        }
        #endregion
    }
}