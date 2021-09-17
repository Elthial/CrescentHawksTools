using log4net.Repository;
using System;
using System.IO;
using System.Reflection;

namespace InceptionTools
{

    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            LoadLog4Net();
            log.Info("Log4Net Loaded");

            var Assets = new AssetExtractor();
            Assets.ExtractToFileSystem();
            log.Info("Asset Extract Completed");
        }

        static void LoadLog4Net()
        {
            try
            {

                ILoggerRepository repository = log4net.LogManager.GetRepository(Assembly.GetCallingAssembly());

                var fileInfo = new FileInfo(@"log4net.config");

                log4net.Config.XmlConfigurator.Configure(repository, fileInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }

    }
}