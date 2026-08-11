using System.Data;
using DatabaseSync.Helpers;

namespace DatabaseSync.Services
{
    public class SchemaComparer
    {
        private readonly SqlServerService _sql;
        private readonly MariaDbService _maria;

        public SchemaComparer(
            SqlServerService sql,
            MariaDbService maria)
        {
            _sql = sql;
            _maria = maria;
        }

        public void SynchronizeSchema()
        {
            List<string> sqlTables =
                _sql.GetAllTables();

            List<string> mariaTables =
                _maria.GetAllTables();

            CreateMissingTables(sqlTables);

            DropExtraTables(
                sqlTables,
                mariaTables);

            foreach (var table in sqlTables)
            {
                AddMissingColumns(table);
                DropExtraColumns(table);
            }
        }

        private void CreateMissingTables(
            List<string> sqlTables)
        {
            foreach (var table in sqlTables)
            {
                CreateTableIfNotExists(table);
            }
        }

        private void DropExtraTables(
            List<string> sqlTables,
            List<string> mariaTables)
        {
            foreach (var table in mariaTables)
            {
                if (sqlTables.Contains(
                    table,
                    StringComparer.OrdinalIgnoreCase))// makes case insensitive
                {
                    continue;
                }

                _maria.DropTable(table);

                Logger.Write(
                    $"Table Dropped : {table}");
            }
        }
        private void CreateTableIfNotExists(string tableName)
        {
            if (_maria.TableExists(tableName))
                return;

            DataTable schema = _sql.GetTableSchema(tableName);

            List<string> columns = new();

            string primaryKey = "";

            foreach (DataRow row in schema.Rows)
            {
                string name =
                    row["COLUMN_NAME"].ToString()!;

                string type =
                    ConvertSqlTypeToMaria(
                        row["DATA_TYPE"].ToString()!);

                string nullable =
                    row["IS_NULLABLE"].ToString() == "YES"
                    ? "NULL"
                    : "NOT NULL";

                bool isPrimaryKey =
                    name.Equals("MigrationId", StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith("ID", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Id", StringComparison.OrdinalIgnoreCase);

                string column =
                    $"`{name}` {type} {nullable}";

                if (isPrimaryKey)
                {
                    primaryKey = name;

                    if (type.StartsWith("INT") ||
                        type.StartsWith("BIGINT"))
                    {
                        column += " AUTO_INCREMENT";
                    }
                }

                columns.Add(column);
            }

            if (!string.IsNullOrEmpty(primaryKey))
            {
                columns.Add(
                    $"PRIMARY KEY (`{primaryKey}`)");
            }

            string createSql =
                $@"CREATE TABLE `{tableName}`
(
{string.Join(",\n", columns)}
)
ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4;";

            _maria.ExecuteNonQuery(createSql);

            Logger.Write(
                $"Table Created : {tableName}");
        }

        private void AddMissingColumns(
            string tableName)
        {
            DataTable sqlSchema =
                _sql.GetTableSchema(tableName);

            foreach (DataRow row in sqlSchema.Rows)
            {
                string column =
                    row["COLUMN_NAME"]
                    .ToString()!;

                if (_maria.ColumnExists(
                    tableName,
                    column))
                {
                    continue;
                }

                string type =
                    ConvertSqlTypeToMaria(
                        row["DATA_TYPE"]
                        .ToString()!);

                string nullable =
                    row["IS_NULLABLE"]
                    .ToString() == "YES"
                    ? "NULL"
                    : "NOT NULL";

                string alter =
                    $@"ALTER TABLE `{tableName}`
                       ADD COLUMN `{column}`
                       {type}
                       {nullable}";

                _maria.ExecuteNonQuery(alter);

                Logger.Write(
                    $"Column Added : {tableName}.{column}");
            }
        }

        private void DropExtraColumns(
            string tableName)
        {
            List<string> mariaColumns =
                _maria.GetTableColumns(
                    tableName);

            DataTable sqlSchema =
                _sql.GetTableSchema(tableName);

            List<string> sqlColumns =
                new();

            foreach (DataRow row in sqlSchema.Rows)
            {
                sqlColumns.Add(
                    row["COLUMN_NAME"]
                    .ToString()!);
            }

            foreach (var column in mariaColumns)
            {
                if (sqlColumns.Contains(
                    column,
                    StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                string alter =
                    $@"ALTER TABLE `{tableName}`
                       DROP COLUMN `{column}`";

                _maria.ExecuteNonQuery(alter);

                Logger.Write(
                    $"Column Dropped : {tableName}.{column}");
            }
        }
        private string ConvertSqlTypeToMaria(string sqlType)
        {
            return sqlType.ToLower() switch
            {
                "int" => "INT",
                "bigint" => "BIGINT",
                "smallint" => "SMALLINT",
                "tinyint" => "TINYINT",
                "bit" => "BOOLEAN",
                "decimal" => "DECIMAL(18,2)",
                "money" => "DECIMAL(18,2)",
                "float" => "DOUBLE",
                "real" => "FLOAT",
                "date" => "DATE",
                "datetime" => "DATETIME",
                "datetime2" => "DATETIME",
                "char" => "VARCHAR(255)",
                "nchar" => "VARCHAR(255)",
                "varchar" => "VARCHAR(255)",
                "nvarchar" => "VARCHAR(255)",
                "text" => "TEXT",
                "ntext" => "TEXT",
                "uniqueidentifier" => "CHAR(36)",
                _ => "TEXT"
            };
        }

    }
}