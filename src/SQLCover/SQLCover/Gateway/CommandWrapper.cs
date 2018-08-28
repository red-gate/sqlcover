using System;
using System.Data.SqlClient;

namespace SQLCover.Gateway
{
    internal class CommandWrapper : IDisposable

    {
        private readonly SqlCommand _command;
        private readonly SqlConnection _connection;

        private CommandWrapper(string connectionString, string command, int timeout)
        {
            _connection = new SqlConnection(connectionString);
            _command = new SqlCommand(command, _connection) {CommandTimeout = timeout};
        }

        public CommandWrapper(SqlConnectionStringBuilder connectionStringBuilder, string command, int timeout = 30) :
            this(connectionStringBuilder.ToString(), command, timeout)
        { }

        private T OpenConnectionAndDo<T>(Func<T> func)
        {
            _connection.Open();
            return func();
        }

        public int ExecuteNonQuery() => OpenConnectionAndDo(() => _command.ExecuteNonQuery());

        public SqlDataReader ExecuteReader() => OpenConnectionAndDo(() => _command.ExecuteReader());

        public object ExecuteScalar() => OpenConnectionAndDo(() => _command.ExecuteScalar());

        public void Dispose()
        {
            _command?.Dispose();
            _connection?.Dispose();
        }
    }
}
