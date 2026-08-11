using MySql.Data.MySqlClient;

namespace DatabaseSync.Services
{
    public class MariaDbService
    {
        private readonly string _connectionString;

        public MariaDbService( string connectionString)
        {
            _connectionString =
                connectionString;
        }

        public MySqlConnection GetConnection()//??
        {
            return new MySqlConnection(
                _connectionString);
        }

        public List<string> GetAllTables()
        {
            List<string> tables = new();

            using MySqlConnection conn =
                new MySqlConnection(
                    _connectionString);

            conn.Open();

            string sql =
                "SHOW TABLES";

            using MySqlCommand cmd =
                new MySqlCommand(
                    sql,
                    conn);

            using MySqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                tables.Add(
                    reader.GetString(0));
            }

            return tables;
        }

        public List<string> GetTableColumns(
            string tableName)
        {
            List<string> columns =
                new();

            using MySqlConnection conn =
                new MySqlConnection(
                    _connectionString);

            conn.Open();

            string sql =
                $"SHOW COLUMNS FROM `{tableName}`";

            using MySqlCommand cmd =
                new MySqlCommand(
                    sql,
                    conn);

            using MySqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                columns.Add(
                    reader["Field"]
                    .ToString()!);//Trust me. This is not null
            }

            return columns;
        }

        public bool TableExists(
            string tableName)
        {
            using MySqlConnection conn =
                new MySqlConnection(
                    _connectionString);

            conn.Open();

            string query =
            @"
            SELECT COUNT(*)
            FROM information_schema.tables
            WHERE table_schema = DATABASE()
            AND table_name = @tableName
            ";

            using MySqlCommand cmd =
                new MySqlCommand(
                    query,
                    conn);

            cmd.Parameters.AddWithValue(// @tableName = tablename
                "@tableName",
                tableName);

            int count =
                Convert.ToInt32(//32 bit 
                    cmd.ExecuteScalar());

            return count > 0;
        }

        public bool ColumnExists(
            string tableName,
            string columnName)
        {
            using MySqlConnection conn =
                new MySqlConnection(
                    _connectionString);

            conn.Open();

            string query =
            @"
            SELECT COUNT(*)
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
            AND table_name = @tableName
            AND column_name = @columnName
            ";

            using MySqlCommand cmd =
                new MySqlCommand(
                    query,
                    conn);

            cmd.Parameters.AddWithValue(
                "@tableName",
                tableName);

            cmd.Parameters.AddWithValue(
                "@columnName",
                columnName);

            int count =
                Convert.ToInt32(
                    cmd.ExecuteScalar());

            return count > 0;
        }

        public void ExecuteNonQuery(//executes SQL that does not return rows.
            string sql)
        {
            using MySqlConnection conn =
                new MySqlConnection(
                    _connectionString);

            conn.Open();

            using MySqlCommand cmd =
                new MySqlCommand(
                    sql,
                    conn);

            cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(
            string sql)
        {
            using MySqlConnection conn =
                new MySqlConnection(
                    _connectionString);

            conn.Open();

            using MySqlCommand cmd =
                new MySqlCommand(
                    sql,
                    conn);

            return cmd.ExecuteScalar();
        }

        public void TruncateTable(
            string tableName)
        {
            string sql =
                $"TRUNCATE TABLE `{tableName}`";

            ExecuteNonQuery(
                sql);
        }

        public void DropTable(
            string tableName)
        {
            string sql =
                $"DROP TABLE IF EXISTS `{tableName}`";

            ExecuteNonQuery(
                sql);
        }
    }
}