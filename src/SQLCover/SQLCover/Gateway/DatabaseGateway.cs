using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace SQLCover.Gateway
{
    public class DatabaseGateway
    {
        private readonly string _databaseName;
        private readonly int _commandTimeout;
        private readonly SqlConnectionStringBuilder _connectionStringBuilder;

        public string DataSource => _connectionStringBuilder.DataSource;

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

        public virtual string GetString(string query)
        {
            var command = new CommandWrapper(_connectionStringBuilder, query, _commandTimeout);
            return command.ExecuteScalar().ToString();
        }

        public virtual DataTable GetRecords(string query)
        {
            var command = new CommandWrapper(_connectionStringBuilder, query, _commandTimeout);
            using (var reader = command.ExecuteReader())
            {
                var ds = new DataTable();
                ds.Load(reader);
                return ds;
            }
        }

        public virtual DataTable GetTraceRecords(string query)
        {
            var command = new CommandWrapper(_connectionStringBuilder, query, _commandTimeout);
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
            var command = new CommandWrapper(_connectionStringBuilder, query, _commandTimeout);
            command.ExecuteNonQuery();
        }
    }
}
