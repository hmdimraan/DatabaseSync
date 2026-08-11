using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseSync.Services
{
    public class SqlServerService
    {
        private readonly string _connectionString;

        public SqlServerService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }


        public List<string> GetAllTables()
        {
            List<string> tables = new();

            using SqlConnection conn =
                new SqlConnection(_connectionString);

            conn.Open();

            using SqlCommand cmd =
                new SqlCommand("GetAllTables", conn);

            cmd.CommandType =
                CommandType.StoredProcedure;

            using SqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }
        public DataTable GetTableData(
    string tableName)
        {
            DataTable dt = new();

            using SqlConnection conn =
                new SqlConnection(_connectionString);

            conn.Open();

            string query =
                $"SELECT * FROM [{tableName}]";

            using SqlDataAdapter da =
                new SqlDataAdapter(
                    query,
                    conn);

            da.Fill(dt);//execute + load

            return dt;
        }
        public DataTable GetTableSchema(
    string tableName)
        {
            DataTable schema =
                new DataTable();

            using SqlConnection conn =
                new SqlConnection(
                    _connectionString);

            conn.Open();

            string query =
            @"
      SELECT
           COLUMN_NAME,
           DATA_TYPE,
           IS_NULLABLE
      FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_NAME=@TableName
      ORDER BY ORDINAL_POSITION
    ";

            using SqlCommand cmd =
                new SqlCommand(
                    query,
                    conn);

            cmd.Parameters.AddWithValue(
                "@TableName",
                tableName);

            using SqlDataAdapter da =
                new SqlDataAdapter(cmd);

            da.Fill(schema);

            return schema;
        }
    }
}