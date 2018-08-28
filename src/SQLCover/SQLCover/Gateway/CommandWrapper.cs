using System;
using System.Data.SqlClient;

namespace SQLCover.Gateway
{
    internal class CommandWrapper

    {
        private readonly string _connectionString;
        private readonly string _command;
        private readonly int _timeout;

        public CommandWrapper(string connectionString, string command, int timeout = 30)
        {
            _connectionString = connectionString;
            _command = command;
            _timeout = timeout;
        }

        public CommandWrapper(SqlConnectionStringBuilder connectionStringBuilder, string command, int timeout = 30) :
            this(connectionStringBuilder.ToString(), command, timeout)
        { }

        private T OpenConnectionAndDo<T>(Func<SqlCommand,T> func)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(_command, connection))
            {
                command.CommandTimeout = _timeout;
                connection.Open();
                return func(command);
            }
        }

        public int ExecuteNonQuery() => OpenConnectionAndDo(command => command.ExecuteNonQuery());

        public SqlDataReader ExecuteReader() => OpenConnectionAndDo(command => command.ExecuteReader());

        public object ExecuteScalar() => OpenConnectionAndDo(command => command.ExecuteScalar());
    }
}
