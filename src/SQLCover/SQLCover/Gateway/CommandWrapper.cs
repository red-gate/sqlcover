using System;
using System.Data;
using System.Data.SqlClient;

namespace SQLCover.Gateway
{
    internal class CommandWrapper : IDisposable
    {
        private readonly IDbCommand _command;
        private readonly IDbConnection _connection;

        private CommandWrapper(string connectionString, string command, int timeout)
        {
            _connection = new SqlConnection(connectionString);
            _command = _connection.CreateCommand();
            _command.CommandText = command;
            _command.CommandTimeout = timeout;
        }

        public CommandWrapper(SqlConnectionStringBuilder connectionStringBuilder, string command, int timeout = 30) :
            this(connectionStringBuilder.ToString(), command, timeout)
        { }

        public CommandWrapper(IDbConnection dbConnection, string command, int timeout = 30)
        {
            _connection = dbConnection;
            _command = _connection.CreateCommand();
            _command.CommandText = command;
            _command.CommandTimeout = timeout;
        }

        private T OpenConnectionAndDo<T>(Func<T> func)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }

            return func();
        }

        public int ExecuteNonQuery() => OpenConnectionAndDo(() => _command.ExecuteNonQuery());

        public IDataReader ExecuteReader() => OpenConnectionAndDo(() => _command.ExecuteReader());

        public object ExecuteScalar() => OpenConnectionAndDo(() => _command.ExecuteScalar());

        public void Dispose()
        {
            _command?.Dispose();
        }
    }
}
