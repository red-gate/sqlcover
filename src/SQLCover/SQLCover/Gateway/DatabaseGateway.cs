using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;

namespace SQLCover.Gateway
{
    public class DatabaseGateway
    {
        private readonly IDbConnection _dbConnection;
        private readonly string _databaseName;
        private readonly int _commandTimeout;
        private readonly SqlConnectionStringBuilder _connectionStringBuilder;

        public string DataSource()
        {
            var parameters = _dbConnection.ConnectionString.Split(';');
            var parameter = parameters.FirstOrDefault(p => IsParameter(p, "Server") || IsParameter(p, "Data Source") || IsParameter(p, "DataSource"))?
                .Split('=')[1]
                .TrimStart(' ').
                TrimEnd(' ');

            return parameter ?? string.Empty;

            bool IsParameter(string input, string param) => input.StartsWith(param, StringComparison.InvariantCultureIgnoreCase);
        }

        public DatabaseGateway()
        {
            //for mocking.
        }
        public DatabaseGateway(string connectionString, string databaseName, int commandTimeout = 30)
        {
            _databaseName = databaseName;
            _commandTimeout = commandTimeout;
            _connectionStringBuilder =
                new SqlConnectionStringBuilder(connectionString) {InitialCatalog = _databaseName};
        }

        public DatabaseGateway(IDbConnection dbConnection, string databaseName, int commandTimeout = 30)
        {
            _dbConnection = dbConnection;
            _databaseName = databaseName;
            _commandTimeout = commandTimeout;
        }

        private CommandWrapper CreateCommand(string query)
        {
            return _dbConnection != null ? new CommandWrapper(_dbConnection, query, _commandTimeout) : new CommandWrapper(_connectionStringBuilder, query, _commandTimeout);
        }

        public virtual string GetString(string query)
        {
            using (var command = CreateCommand(query))
            {
                return command.ExecuteScalar()?.ToString();
            }
        }

        public virtual DataTable GetRecords(string query)
        {
            using (var command = CreateCommand(query))
            using (var reader = command.ExecuteReader())
            {
                var ds = new DataTable();
                ds.Load(reader);
                return ds;
            }
        }

        public virtual DataTable GetTraceRecords(string query)
        {
            using (var command = CreateCommand(query))
            using (var reader = command.ExecuteReader())
            {
                var ds = new DataTable();
                ds.Columns.Add(new DataColumn("xml"));
                while (reader.Read())
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(reader[0].ToString());
                 
                    var root = xml.SelectNodes("/event").Item(0);

                    var objectId = xml.SelectNodes("/event/data[@name='object_id']").Item(0);
                    var offset = xml.SelectNodes("/event/data[@name='offset']").Item(0);
                    var offsetEnd = xml.SelectNodes("/event/data[@name='offset_end']").Item(0);

                    root.RemoveAll();
                     
                    root.AppendChild(objectId);
                    root.AppendChild(offset);
                    root.AppendChild(offsetEnd);

                    var row = ds.NewRow();
                    row["xml"] = root.OuterXml;
                    ds.Rows.Add(row);

                }

                return ds;
            }
        }

        public void Execute(string query)
        {
            using (var command = CreateCommand(query))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
