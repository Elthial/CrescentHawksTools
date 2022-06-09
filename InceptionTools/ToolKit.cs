using InceptionTools.Data;
using InceptionTools.Data.Mechs;
using log4net.Repository;
using System;
using System.IO;
using System.Reflection;

namespace InceptionTools
{
    class ToolKit
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            LoadLog4Net();
            log.Info("Log4Net Loaded");

            //var data = new GameData();

            //Chameleon C = new Chameleon();

            //Mech Test = new Mech(C.RawData);
            //Console.WriteLine(Test.ToString());




            //foreach(byte[] b in data.WeaponData)
            //{
            //    var w = new Weapon(b);
            //    Console.WriteLine("------------------------");
            //    Console.WriteLine("Name:" + w.Name);
            //    Console.WriteLine("Damage:" + w.Damage);
            //    Console.WriteLine("???:" + w.Variable2);
            //    Console.WriteLine("Heat:" + w.Heat);
            //    Console.WriteLine("???:" + w.Variable4);
            //    Console.WriteLine("Range:" + w.Range);
            //    Console.WriteLine("Skill:" + w.Skill.ToString());
            //}


            Console.WriteLine("Please enter path to btech folder");
            Console.WriteLine(@"Example: D:\btech\");
            string sourceFolder = Console.ReadLine();

            var Assets = new AssetExtractor();
            Assets.ExtractToFileSystem(sourceFolder);
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