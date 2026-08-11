using System.Text;

namespace DatabaseSync.Helpers
{
    public static class Logger
    {
        private static readonly string _logFile =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Logs",
                "SyncLogs.txt");

        public static void Write(string message)
        {
            string logDirectory =
                Path.GetDirectoryName(_logFile)!;

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            using StreamWriter writer =//A class used to write text into files.
                new StreamWriter(
                    _logFile,
                    true,
                    Encoding.UTF8);

            writer.WriteLine(
                $"[{DateTime.Now:dd-MM-yyyy hh:mm:ss tt}] {message}");
        }

        public static void WriteSeparator()
        {
            Write(
                "=================================================");
        }
    }
}