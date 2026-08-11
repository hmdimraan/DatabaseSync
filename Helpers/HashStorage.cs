using Newtonsoft.Json;//Convert C# objects → JSON AND vv

namespace DatabaseSync.Helpers
{
    public static class HashStorage
    {
        private static readonly string FilePath = //??
            Path.Combine(// uses correct seperators
                AppDomain.CurrentDomain.BaseDirectory,
                "Hashes",
                "TableHashes.json");
        //C:\Projects\DatabaseSync\bin\Debug\net8.0\Hashes\TableHashes.json
        public static Dictionary<string, string> LoadHashes()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Directory.CreateDirectory(
                        Path.GetDirectoryName(
                            FilePath)!);

                    return new Dictionary<string, string>();
                }

                string json =
                    File.ReadAllText(FilePath);

                if (string.IsNullOrWhiteSpace(
                    json))
                {
                    return new Dictionary<string, string>();
                }

                return JsonConvert.DeserializeObject // json -> c# obj
                    <Dictionary<string, string>>
                    (json)
                    ?? //if (A != null)  return A; else  return B;
                    new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        public static void SaveHashes(
            Dictionary<string, string>
            hashes)
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(
                    FilePath)!);

            string json =
                JsonConvert.SerializeObject(
                    hashes,
                    Formatting.Indented);//Adds spaces and indentation.

            File.WriteAllText(
                FilePath,
                json);
        }
    }
}

