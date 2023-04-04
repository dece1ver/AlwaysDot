using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App
{
    internal static class Program
    {
        public const string BasePath = "C:\\ProgramData\\dece1ver\\AlwaysDot";
        public static string[] PassProcesses = {};
        public static string ConfigPath = Path.Combine(BasePath, "config.ini");
        public static string ExePath = Path.Combine(BasePath, "AlwaysDot.exe");
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (!SingleInstance.Start())
            {
                SingleInstance.ShowFirstInstance();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                ReadConfig(true);
                var mainWindow = new MainWindow();
                Application.Run(mainWindow);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            SingleInstance.Stop();
        }

        public static bool ReadConfig(bool rewrite = false)
        {
            try
            {
                if (File.ReadLines(ConfigPath).First() != "# Конфигурация Always Dot")
                    throw new Exception("Некорректный файл конфигурации");
                PassProcesses = File.ReadLines(ConfigPath).Where(x => !x.StartsWith("#") && !string.IsNullOrWhiteSpace(x)).ToArray();
                return true;
            }
            catch
            {
                if (!rewrite) return false;
                WriteBaseConfig();
                return ReadConfig();
            }
        }

        private static void WriteBaseConfig()
        {
            var baseConfig = Properties.Resources.BaseConfig;
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            File.WriteAllText(ConfigPath, baseConfig);
        }
    }
}
