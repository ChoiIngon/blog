using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Gamnet.Database
{
	class MySQL
	{
		// server=127.0.0.1;port=3309;database=SCANIA_MAPLE_GUILD;user id=maple_dev_app;checkparameters=False;password=~mysql123;convertzerodatetime=True;keepalive=6000;pooling=True;maxpoolsize=2;cacheserverproperties=True
		/*
		 MySqlConnection conn = new MySqlConnection(ConnectionString);
		 try
			{
				conn.Open();
				MySqlCommand SqlCommand = conn.CreateCommand();
				SqlCommand.CommandType = CommandType.StoredProcedure;
				SqlCommand.CommandText = procedureName;
				
			}
			catch (Exception e)
			{
				return RetryConnection(conn, procedureName, retryCount, e);
			}
		
		database.AddParameter<string>("guild_name", System.Data.ParameterDirection.Input, input.guild_name);
					database.AddParameter<Int32>("guild_level", System.Data.ParameterDirection.Input, input.guild_level);
		public void AddParameter<TValue>(string prameterName, ParameterDirection prameterDirection, TValue tValue)
		{
			DbType dbType;
			int size;

			GetDbTypeAndSize(tValue, out dbType, out size);

			MySqlParameter parm = SqlCommand.CreateParameter();
			parm.ParameterName = prameterName;
			parm.Direction = prameterDirection;
			parm.Value = tValue;
			parm.DbType = dbType;
			parm.Size = size;
			SqlCommand.Parameters.Add(parm);
		}
		private static void GetDbTypeAndSize<T>(T t, out DbType dbType, out int size)
		{
			dbType = DbType.Int32;
			size = sizeof(int);

			if (t is int)
			{
				dbType = DbType.Int32;
				size = sizeof(int);
			}
			else if (t is long)
			{
				dbType = DbType.Int64;
				size = sizeof(long);
			}
			else if (t is DateTime)
			{
				dbType = DbType.DateTime;
				size = 8;
			}
			else if (t is string)
			{
				dbType = DbType.String;
				size = (t as string).Length;
			}
			else if (t is byte [])
            {
				dbType = DbType.Binary;
				size = (t as byte[]).Length;
			}
			else
			{
				Console.WriteLine("Database Not Regist Type [T = " + typeof(T) + "]");
				Trace.Assert(false);
			}
		}

		public class Database : IDisposable
	{
		private MySqlConnection Connection { get; set; }
		public MySqlCommand SqlCommand { get; private set; }
		public MySqlDataReader SqlReader { get; private set; }

		public Connection.Factory Factory { get; private set; }
		public string ProcedureName { get; }
		public int WorldIndex { get; }
		public string World { get; }
        public Result DBConnResult { get; private set; }

		public Database(Connection.Factory connectionFactory, string procedureName)
		{
			ProcedureName = procedureName;
			Factory = connectionFactory;
            DBConnResult = MakeConnection(procedureName, 3);
		}

		private Result MakeConnection(string procedureName, int retryCount = 0)
		{
            MySqlConnection conn = Factory.Create();

			try
			{
				conn.Open();
				SqlCommand = conn.CreateCommand();
				SqlCommand.CommandType = CommandType.StoredProcedure;
				SqlCommand.CommandText = procedureName;
                this.Connection = conn;
				return Result.Success();
			}
			catch (Exception e)
			{
                return RetryConnection(conn, procedureName, retryCount, e);
			}
		}

		private Result RetryConnection(MySqlConnection conn, string procedureName, int retryCount, Exception e)
		{
			if (IsConnectionError(e))
				Factory.ClearPool();
			Factory.Destroy(conn);

			if (--retryCount >= 0)
			{
				return MakeConnection(procedureName, retryCount);
			}

            string commonLog;
			if (null == World && 0 == WorldIndex)
                commonLog = $"Connection Exception : Global, {procedureName}";
			else
                commonLog = $"Connection Exception : World({World}), Idx({WorldIndex}), {procedureName}";
            
			Factory = null;

            string log = commonLog + e.ToString();
            string summaryLog = commonLog + e.Message.ToString();

            return Result.Error(Code.Error.SqlConnectionOpenFail, 0, log, summaryLog);
        }

		public static void Ping(string world, Connection.Factory connFactory, int retryCount = 0)
		{
			MySqlConnection conn = connFactory.Create();
			try
			{
				conn.Open();
				conn.Ping();
                conn.Close();
                connFactory.Destroy(conn);
            }
            catch (Exception e)
			{
                TraceLog.Error(e.ToString());

                if (IsConnectionError(e))
					connFactory.ClearPool();
				connFactory.Destroy(conn);

				if (--retryCount >= 0)
				{
					Ping(world, connFactory, retryCount);
				}
				else
				{
					TraceLog.Error($"Connection Exception : World({world})");
					TraceLog.Error(e.ToString());
				}
			}
		}

		public static void Ping(Connection.Factory connFactory, int retryCount = 0)
		{
			if (null == connFactory)
				return;

			MySqlConnection conn = connFactory.Create();
			try
			{
				conn.Open();
				conn.Ping();
				conn.Close();
				connFactory.Destroy(conn);
			}
			catch (Exception e)
			{
				TraceLog.Error(e.ToString());

				if (IsConnectionError(e))
					connFactory.ClearPool();
				connFactory.Destroy(conn);

				if (--retryCount >= 0)
				{
					Ping(connFactory, retryCount);
				}
				else
				{
					TraceLog.Error($"Ping Connection Exception : ConnectionString({conn.ConnectionString})");
					TraceLog.Error(e.ToString());
				}
			}
		}

		public static MySqlConnectionStringBuilder MakeConnectionStringBuilder(uint connection_limit, string connectionString)
		{
			MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder(connectionString);
			sb.ConvertZeroDateTime = true;
			sb.Keepalive = 6000;

			sb.Pooling = true;
			sb.MaximumPoolSize = Math.Max(connection_limit, 1);

			sb.CheckParameters = false;
			sb.CacheServerProperties = true;

			TraceLog.Write("ConnectionString / " + sb);

			return sb;
		}

		public void Dispose()
		{
			if (null != SqlReader)
			{
				SqlReader.Close();
				SqlReader.Dispose();
				SqlReader = null;
			}

			if (null != SqlCommand)
			{
				SqlCommand.Dispose();
				SqlCommand = null;
			}

			if (null != Connection)
			{
				Factory?.Destroy(Connection);

				Factory = null;
				Connection = null;
			}
		}

		private static void GetDbTypeAndSize<T>(T t, out DbType dbType, out int size)
		{
			dbType = DbType.Int32;
			size = sizeof(int);

			if (t is int)
			{
				dbType = DbType.Int32;
				size = sizeof(int);
			}
			else if (t is long)
			{
				dbType = DbType.Int64;
				size = sizeof(long);
			}
			else if (t is DateTime)
			{
				dbType = DbType.DateTime;
				size = 8;
			}
			else if (t is string)
			{
				dbType = DbType.String;
				size = (t as string).Length;
			}
			else if (t is byte [])
            {
				dbType = DbType.Binary;
				size = (t as byte[]).Length;
			}
			else
			{
				Console.WriteLine("Database Not Regist Type [T = " + typeof(T) + "]");
				Trace.Assert(false);
			}
		}

		public void AddParameter<TValue>(string prameterName, ParameterDirection prameterDirection, TValue tValue)
		{
			DbType dbType;
			int size;

			GetDbTypeAndSize(tValue, out dbType, out size);

			MySqlParameter parm = SqlCommand.CreateParameter();
			parm.ParameterName = prameterName;
			parm.Direction = prameterDirection;
			parm.Value = tValue;
			parm.DbType = dbType;
			parm.Size = size;
			SqlCommand.Parameters.Add(parm);
		}

		public void AddReturnParameter()
		{
			int outputValue = 0;
			AddParameter("o_return", ParameterDirection.Output, outputValue);
		}

		public Result ExecuteReader()
		{
			try
			{
				SqlReader = SqlCommand.ExecuteReader();
				Factory.SetSuccess();
				return Result.Success();
			}
			catch (Exception e)
			{
				if (IsConnectionError(e))
					Factory.ClearPool();

                string log = e.ToString();
                string summaryLog = e.Message.ToString();
                return Result.Error(Code.Error.SqlExecuteReaderException, 0, log, summaryLog);
			}
		}

		// Result를 사용하지 않는 서버에서 사용(ex. Scheduler)
		public bool SimpleExecuteReader()
		{
			try
			{
				SqlReader = SqlCommand.ExecuteReader();
				Factory.SetSuccess();
				return true;
			}
			catch (Exception e)
			{
				if (IsConnectionError(e))
					Factory.ClearPool();

				return false;
			}
		}

		public Result ExecuteNonQuery()
		{
			try
			{
				SqlCommand.ExecuteNonQuery();
				Factory.SetSuccess();
				return Result.Success();
			}
			catch (Exception e)
			{
				if (IsConnectionError(e))
					Factory.ClearPool();

                string log = e.ToString();
                string summaryLog = e.Message.ToString();
                return Result.Error(Code.Error.SqlExecuteNonQueryException, 0, log, summaryLog);
			}
		}

		// Result를 사용하지 않는 서버에서 사용(ex. Scheduler)
		public bool SimpleExecuteNonQuery()
		{
			try
			{
				SqlCommand.ExecuteNonQuery();
				Factory.SetSuccess();
				return true;
			}
			catch (Exception e)
			{
				if (IsConnectionError(e))
					Factory.ClearPool();

				return false;
			}
		}

		public int GetReturn()
		{
			const string columnName = "o_return";
			Trace.Assert(DbType.Int32 == SqlCommand.Parameters[columnName].DbType);

			//if (null == this.SqlCommand)
			//{
			//	return -999;
			//}
			if (null == SqlCommand.Parameters[columnName].Value)
			{
				return -998;
			}

			try
			{
				int o_return = (int)SqlCommand.Parameters[columnName].Value;
				if (0 != o_return)
				{
					StringBuilder log = new StringBuilder();
					log.Append(ProcedureName);
					log.Append(" / o_return : ");
					log.Append(o_return);
					TraceLog.Write(log.ToString());
				}
				return o_return;
			}
			catch (InvalidCastException e)
			{
				TraceLog.Error("Database InvalidCastException : " + e);
				return -997;
			}
			catch (Exception e)
			{
				TraceLog.Error("Database Exception : " + e);
				return -996;
			}
		}

#region Connection Error

		// Common non-connection exceptions
		private static readonly MySqlErrorCode[] NonConnErrorCodes =
		{
			MySqlErrorCode.KeyNotFound,
			MySqlErrorCode.DuplicateKey,
			MySqlErrorCode.NoDatabaseSelected,
			MySqlErrorCode.StoredProcedureDoesNotExist,
			MySqlErrorCode.StoredProcedureNumberOfArguments,
			MySqlErrorCode.UnknownProcedure,
			MySqlErrorCode.WrongArguments,
			MySqlErrorCode.WrongColumnName,
			MySqlErrorCode.WrongParametersToProcedure,
		};

		public static bool IsConnectionError(Exception e)
		{
			var ex = e as MySqlException;
			return ex != null && IsConnectionError(ex.ErrorCode);
		}

		public static bool IsConnectionError(MySqlErrorCode code)
		{
			return Array.IndexOf(NonConnErrorCodes, code) < 0;
		}

		public static bool IsConnectionError(int code) => IsConnectionError((MySqlErrorCode)code);

#endregion
	}
		*/
	}

}