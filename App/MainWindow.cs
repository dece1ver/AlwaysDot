using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using App.Extensions.Keyboard;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace App
{
    public partial class MainWindow : Form
    {
        private KeyboardListener _listener;
        private FileSystemWatcher configWatcher;
        private static readonly string StartupDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            @"Microsoft\Windows\Start Menu\Programs\Startup");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnConfigChanged(object sender, FileSystemEventArgs eventArgs)
        {
            if (Program.ReadConfig())
            {
                ShowNotification("Конфигурация обновлена", $"Исключенные процессы:\n   {string.Join("\n   ", Program.PassProcesses)}");
            }
            
        }
        private void OnConfigDeleted(object sender, FileSystemEventArgs eventArgs)
        {
            Program.ReadConfig(true);
            ShowNotification("Сброс конфигурации", $"Файл конфигурации восстановлен с параметрами по умолчанию.");
        }

        private void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            notifyIcon.BalloonTipIcon = icon;
            notifyIcon.BalloonTipText = message;
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.ShowBalloonTip(5);
        }
        
        private void MainWindow_Load(object sender, EventArgs e)
        {
            UpdateVisibility();
            configWatcher = new FileSystemWatcher
            {
                Path = Program.BasePath,
                Filter = "config.ini",
                NotifyFilter = NotifyFilters.Attributes
                               | NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               | NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               | NotifyFilters.Security
                               | NotifyFilters.Size
            };
            configWatcher.Changed += OnConfigChanged;
            configWatcher.Deleted += OnConfigDeleted;
            configWatcher.EnableRaisingEvents = true;
            
            _listener = new KeyboardListener();
            _listener.HookKeyboard();
            Hide();
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "При включенной RU раскладке клавиша Del на цифровой клавиатуре вводит точку вместо запятой.\nМожет игнорировать приложения, указанные в файле конфигурации.\n\n" +
                $"Игнорируются сейчас:\n\t{string.Join("\n\t", Program.PassProcesses)}\n\ndece1ver © 2023",
                "О программе Always Dot",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void closeMenuItem_Click(object sender, EventArgs e) => Close();

        private void openConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Program.ConfigPath);
        }

        private void installToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exeResult = string.Empty;
            var linkResult = string.Empty;
            var autoResult = string.Empty;
            var src = Process.GetCurrentProcess().MainModule?.FileName;
            File.Copy(src ?? throw new InvalidOperationException("Не был получен путь к запущенному исполняемому файлу."), Program.ExePath, true);
            if (File.Exists(Program.ExePath)) exeResult = "\nИсполняемый файл установлен.";
            var linkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Always Dot.lnk");
            var shell = new WshShell();
            var link = (IWshShortcut)shell.CreateShortcut(linkPath);
            link.TargetPath = Program.ExePath;
            link.Save();
            if (File.Exists(linkPath)) linkResult = "\nИконка на рабочем столе создана.";
            if (!Directory.Exists(StartupDirectory)) Directory.CreateDirectory(StartupDirectory);
            var autoLink = Path.Combine(StartupDirectory, "Always Dot.lnk");
            File.Copy(linkPath, autoLink, true);
            if (File.Exists(autoLink)) autoResult = "\nПрограмма добавлена в автозагрузку.";
            UpdateVisibility();
            ShowNotification("Установка завершена", exeResult + linkResult + autoResult);
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exe = Process.GetCurrentProcess().MainModule?.FileName;
            if (Program.ExePath == exe && 
                MessageBox.Show("Программа будет автоматически закрыта и удалена, продолжить?", 
                    "Подвтерждение",
                    MessageBoxButtons.OKCancel, 
                    MessageBoxIcon.Warning) == DialogResult.Cancel)
            {
                return;
            }
            if (Program.ExePath != exe && 
                MessageBox.Show("Программа будет удалена, продолжить?",
                    "Подвтерждение",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) == DialogResult.Cancel)
            {
                return;
            }
            var exeResult = string.Empty;
            var linkResult = string.Empty;
            var autoResult = string.Empty;
            var linkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Always Dot.lnk");
            if (File.Exists(linkPath))
            {
                File.Delete(linkPath);
                if (!File.Exists(linkPath)) linkResult = "\nИконка удалена.";
            }
            var autoLink = Path.Combine(StartupDirectory, "Always Dot.lnk");
            if (File.Exists(autoLink))
            {
                try
                {
                    File.Delete(autoLink);
                    autoResult = "\nПрограмма удалена из автозагрузки.";
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, e.GetType().FullName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            switch (File.Exists(Program.ExePath))
            {
                case true when Program.ExePath == exe:
                    Process.Start( new ProcessStartInfo()
                    {
                        Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Program.ExePath + "\"",
                        WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, FileName = "cmd.exe"
                    });
                    Close();
                    break;
                case true when Program.ExePath != exe:
                    try
                    {
                        File.Delete(Program.ExePath);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, e.GetType().FullName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case false:
                    break;
            }
            if (!File.Exists(Program.ExePath)) exeResult = "\nИсполняемый файл удален.";
            UpdateVisibility();
            ShowNotification("Удаление завершено", exeResult + linkResult + autoResult);
        }

        private void UpdateVisibility()
        {
            installToolStripMenuItem.Visible = !IsInstalled();
            uninstallToolStripMenuItem.Visible = IsInstalled();
        }

        private static bool IsInstalled() => File.Exists(Program.ExePath);
    }
}
