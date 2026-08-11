using System.Data;
using System.Security.Cryptography;
using System.Text;
using DatabaseSync.Helpers;

namespace DatabaseSync.Services
{
    public class DataSynchronizer
    {
        private readonly SqlServerService _sql;
        private readonly MariaDbService _maria;
        private readonly SchemaComparer _schema;

        private Dictionary<string, string>
            _tableHashes;

        public DataSynchronizer(
            SqlServerService sql,
            MariaDbService maria,
            SchemaComparer schema)
        {
            _sql = sql;
            _maria = maria;
            _schema = schema;

            _tableHashes =
                HashStorage.LoadHashes();
        }

        public void Start()
        {
          
            _schema.SynchronizeSchema();

            var tables =
                _sql.GetAllTables();

            List<string> changedTables = new();

            foreach (var table in tables)
            {
                DataTable data =//Loads all rows.
                    _sql.GetTableData(table);

                string newHash =

                    GenerateHash(data);
                // Force full synchronization
                // if (_tableHashes.ContainsKey(table)
                //     &&
                //     _tableHashes[table] == newHash)
                // {
                //     continue;
                // }
                changedTables.Add(table);


                FullRefresh(
                    table,
                    data);

                _tableHashes[table] =
                    newHash;

            
            }

            HashStorage.SaveHashes(
         _tableHashes);

            if (changedTables.Count > 0)
            {
                Logger.Write(
                    "Changes Detected : YES");

                Logger.Write(
                    $"Number of Tables Changed : {changedTables.Count}");
                
                    Logger.Write("");
                
                Logger.Write(
                    "Changed Tables:");

                foreach (var table
                    in changedTables)
                {
                    Logger.Write(table);
                }
                Logger.Write("");
            }
            else
            {
                Logger.Write(
                    "Changes Detected : NO");

                Logger.Write(
                    "No changes found in any table.");
            }

        }

        private void FullRefresh(
            string tableName,
            DataTable data)
        {
            _maria.ExecuteNonQuery(
                $"TRUNCATE TABLE `{tableName}`");

            foreach (DataRow row in data.Rows)
            {
                List<string> columns =
                    new();

                List<string> values =
                    new();

                foreach (DataColumn col
                    in data.Columns)
                {
                    columns.Add(
                        $"`{col.ColumnName}`");

                    object value =
                        row[col];

                    if (value == DBNull.Value)
                    {
                        values.Add("NULL");
                    }
                    else if (value is DateTime dt)
                    {
                        values.Add(
                            $"'{dt:yyyy-MM-dd HH:mm:ss}'");
                    }
                    else if (value is DateOnly dateOnly)
                    {
                        values.Add(
                            $"'{dateOnly:yyyy-MM-dd}'");
                    }
                    else if (value is bool b)
                    {
                        values.Add(
                            b ? "1" : "0");
                    }
                    else
                    {
                        string v =
                            value.ToString()!
                            .Replace(
                                "'",
                                "''");

                        values.Add(
                            $"'{v}'");
                    }
                }

                string insertSql =
                    $@"INSERT INTO `{tableName}`
                    ({string.Join(",", columns)})
                    VALUES
                    ({string.Join(",", values)})";

                _maria.ExecuteNonQuery(
                    insertSql);
            }

           
        }

        private string GenerateHash(
            DataTable table)
        {
            StringBuilder sb =
                new();

            foreach (DataRow row
                in table.Rows)
            {
                foreach (var item
                    in row.ItemArray)
                {
                    if (item == DBNull.Value)
                    {
                        sb.Append("NULL");
                    }
                    else if (item is DateTime dt)
                    {
                        sb.Append(
                            dt.ToString(
                                "yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        sb.Append(
                            item.ToString());
                    }
                    
                    sb.Append("|");
                }
               
                sb.AppendLine();
            }

            using SHA256 sha =
                SHA256.Create();

            byte[] bytes =
                Encoding.UTF8.GetBytes(
                    sb.ToString());

            byte[] hash =
                sha.ComputeHash(bytes);
            /*performs:

            Padding
            Splitting into 512-bit blocks
64 rounds of:
XOR
AND
OR
Right rotations
Modulo additions
Mixing with constants
            produces 256 bits
            */
            return Convert.ToHexString(
                hash);
        }
    }
}

