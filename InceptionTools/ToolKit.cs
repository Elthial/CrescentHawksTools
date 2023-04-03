using InceptionTools.Graphics;
using log4net.Repository;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Data;
using System.Collections.Generic;

namespace InceptionTools
{
    class ToolKit
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            LoadLog4Net();
            log.Info("Log4Net Loaded");

            //var save = new SaveFile(@"G:\btech\GAME_Mission2_Comstar_BakPhar10");
            //var data = new GameData();
            //Chameleon C = new Chameleon();
            //Mech Test = new Mech(C.RawData);
            //Console.WriteLine(Test.ToStats());

            
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