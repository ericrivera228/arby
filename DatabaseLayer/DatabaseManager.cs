using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DatabaseLayer
{
	/// <summary>
	/// Class for interacting with the db. Not very sophisticated; it simply executes sql. For every sql statment executed, a conneciton is opend 
	/// and then disposed. 
	/// 
	/// *IMPORTANT NOTE* This class was written to only handle one sql statement at once. While it can techincally execute complicatd sql queries,
	/// this class was not designed with that use in mind. 
	/// </summary>
	public class DatabaseManager
	{
		//Enumeration to describe a sql statement. 'Query' describes select statements, 'NonQuery' describes updates, deletes, and inserts.
		private enum QueryType { Query, NonQuery, Scalar };

		/// <summary>
		/// Executes the given sql string. Opens and then disposes a connection. If there are any errors along the way, an exception is thrown.
		/// Queries are done in a transaction; should there be an error the are rolled back. 
		/// </summary>
		/// <param name="queryType">The type of sql given.</param>
		/// <param name="sql">The sql to be exceuted against the db.</param>
		/// <returns>If given a query, and that query produces results, a data table of those results is returned. For non queries, or queries
		///     that don't return any data, null is returned. For scalars, an integer is returned for the output defined in the SQL.</returns>
		private static object ExecuteSql(QueryType queryType, string sql)
		{
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseForUse"].ConnectionString;

            using (SqlConnection dbConnection = new SqlConnection(connectionString))
			{
				dbConnection.Open();
				SqlTransaction sqlTran = dbConnection.BeginTransaction();

				using (SqlCommand command = dbConnection.CreateCommand())
				{
					try
					{
						command.CommandText = sql;
						command.Transaction = sqlTran;

						if (queryType == QueryType.Query)
						{
							DataTable resultDt = new DataTable();
							resultDt.Load(command.ExecuteReader());

							//Only return the datatable if it has information. Otherwise, 
							//return null.
							if (resultDt.Rows.Count > 0)
							{
								return resultDt;
							}
						}

						else if (queryType == QueryType.Scalar)
						{
							int returnInt = (int)command.ExecuteScalar();
							sqlTran.Commit();

							return returnInt;
						}

						else
						{
							command.ExecuteNonQuery();
							sqlTran.Commit();
						}

					}

					catch (Exception)
					{
						//Not putting the rollback in a try/catch so that the error bubbles up.
						sqlTran.Rollback();

						//After attempting the roll back, throw the error
						throw;
					}
				}
			}

			//If the given sql was a nonquery or a query that didn't return any data, return null.
			return null;
		}

		/// <summary>
		/// Executes select statements. If the statment does not return any information, this method returns null. (So, the returned 
		/// DataTable will always have have rows; a check is not needed.)
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>
		public static DataTable ExecuteQuery(string sql)
		{
			return (DataTable)ExecuteSql(QueryType.Query, sql);
		}

		/// <summary>
		/// Executes updates and deletes. Can also execute selects, but it won't return the id of the object that was inserted
		/// (use ExecuteInsert for that). 
		/// </summary>
		/// <param name="sql"></param>
		public static void ExecuteNonQuery(string sql)
		{
			ExecuteSql(QueryType.NonQuery, sql);
		}

		/// <summary>
		/// Executes an insert statement and returns an integer as defined by the 'output' keyword in the sql statement.
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>An integer as defined by the  'output' keyword in the sql statement.</returns>
		public static int ExecuteInsert(string sql)
		{
			return (int)ExecuteSql(QueryType.Scalar, sql);
		}

	    public static DataTable ExecuteStoredProcedure(string storedProcName, SqlParameter[] parameters)
	    {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseForUse"].ConnectionString;

            using (SqlConnection dbConnection = new SqlConnection(connectionString))
	        {

	            dbConnection.Open();
	            SqlTransaction sqlTran = dbConnection.BeginTransaction();

	            try
	            {
                    SqlCommand command = new SqlCommand(storedProcName, dbConnection)
                    {
                        CommandType = CommandType.StoredProcedure,
                        Transaction = sqlTran
                    };

                    command.Parameters.AddRange(parameters);

                    DataTable resultDt = new DataTable();
                    resultDt.Load(command.ExecuteReader());
                    sqlTran.Commit();

                    //Only return the datatable if it has information. Otherwise, 
                    //return null.
                    if (resultDt.Rows.Count > 0)
                    {
                        return resultDt;
                    }
	            }
                catch (Exception)
                {
                    //Not putting the rollback in a try/catch so that the error bubbles up.
                    sqlTran.Rollback();

                    //After attempting the roll back, throw the error
                    throw;
                }
	        }

            //If the code made it this far; there was a problem execute the stored proc. Return null.
	        return null;
	    }

		/// <summary>
		/// Formats a given datetime to a value that that be put into a SQL statement for the DB. If the given datetime is
		/// not null, a string with the value '(formated date time value)' is returned (including the single quotes). Otherwise,
		/// NULL is returned.
		/// </summary>
		/// <param name="dateTime">Time to be formatted.</param>
		/// <returns>A string represenation of the given date time that can be placed in a sql statement.</returns>
		public static string FormatDateTimeForDb(DateTime? dateTime)
		{
			string returnString = null;
			const string dateTimeformat = "yyyy-MM-dd HH:mm:ss";

			if (dateTime != null)
			{
				returnString = "'" + dateTime.Value.ToString(dateTimeformat) + "'";
			}
			else
			{
				returnString = "NULL";
			}

			return returnString;
		}

		/// <summary>
		/// Formats a given string for insertion into a sql statement. If the given string is null, NULL is returned. 
		/// Otherwise, '(string value)' is returned (including the single quotes). 
		/// </summary>
		/// <param name="stringToFormat">String to be formatted.</param>
		/// <returns>A string representation of the given string that can be placed in a sql statement.</returns>
		public static string FormatStringForDb(string stringToFormat)
		{
			string returnString = null;

			if (!String.IsNullOrEmpty(stringToFormat))
			{
				returnString = "'" + stringToFormat + "'";
			}
			else
			{
				returnString = "NULL";
			}

			return returnString;
		}

	    public static string FormatNullableDecimalForDb(decimal? decimalToFormat)
	    {
	        string returnString = "";

	        if (decimalToFormat != null)
	        {
	            returnString = "" + decimalToFormat;
	        }

	        else
	        {
	            returnString = "NULL";
	        }

	        return returnString;
	    }

        public static string FormatNullableIntegerForDb(int? integerToFormat)
        {
            string returnString = "";

            if (integerToFormat != null)
            {
                returnString = "" + integerToFormat;
            }

            else
            {
                returnString = "NULL";
            }

            return returnString;
        }

		/// <summary>
		/// Runs a simple query (that doens't change any data) to ensure the db connection string is valid.
		/// </summary>
		public static void TestDbConnection()
		{
			ExecuteQuery("select 1");
		}
	}
}
