using System;
using System.IO;
using Avalonia;

namespace PGCTimeTracker
{
    internal sealed class Program
    {
        private static FileStream? lockFileStream;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            if (!EnsureSingleInstance())
            {
                Console.WriteLine("Another instance is already running.");
                return;
            }

            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }
        private static bool EnsureSingleInstance()
        {
            try
            {
                string lockFilePath = Path.Combine(Path.GetTempPath(), "PGCTimeTracker.lock");
                lockFileStream = new FileStream(lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        private static void ReleaseLock()
        {
            lockFileStream?.Close();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
