using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptographyProject2019.Controller
{
    // ReSharper disable once InconsistentNaming
    static class FSWatcherController
    {
        public static void ExecuteWatcher(FileSystemWatcher fileSystemWatcher, string path, string filter, FileSystemEventHandler callback, Func<bool> close)
        {
            fileSystemWatcher.Path = path;
            //fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess
            //                                 | NotifyFilters.LastWrite
            //                                 | NotifyFilters.FileName
            //                                 | NotifyFilters.DirectoryName;

            //NotifyFilters.LastAccess
            //| NotifyFilters.LastWrite
            //| NotifyFilters.FileName
            //| NotifyFilters.DirectoryName;
            fileSystemWatcher.Filter = filter;

            //fileSystemWatcher.Changed += callback;
            fileSystemWatcher.Created += callback;
            fileSystemWatcher.Deleted += callback;
            //fileSystemWatcher.Renamed += new RenamedEventHandler(callback);
            fileSystemWatcher.EnableRaisingEvents = true;

            while (!close())
            {
                Task.Delay(1000);
            }
            fileSystemWatcher.EnableRaisingEvents = false;
            fileSystemWatcher.Dispose();
        }
    }
}
