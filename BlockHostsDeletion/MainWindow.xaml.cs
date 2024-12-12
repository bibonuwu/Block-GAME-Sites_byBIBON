using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Diagnostics;

namespace ProtectHostsFile
{
    public partial class MainWindow : Window
    {
        private const string HostsFilePath = @"C:\Windows\System32\drivers\etc\hosts"; // Путь к файлу hosts

        public MainWindow()
        {
            InitializeComponent();
            // Проверяем, если приложение не с правами администратора, перезапускаем его с правами администратора
            if (!IsRunningAsAdministrator())
            {
                RestartAsAdministrator();
            }
        }

        // Проверка, запущено ли приложение с правами администратора
        private bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // Перезапуск приложения с правами администратора
        private void RestartAsAdministrator()
        {
            ProcessStartInfo procInfo = new ProcessStartInfo
            {
                FileName = System.Reflection.Assembly.GetExecutingAssembly().Location,
                UseShellExecute = true,
                Verb = "runas" // Указывает, что приложение должно быть запущено с правами администратора
            };

            try
            {
                Process.Start(procInfo);
                Application.Current.Shutdown(); // Закрываем текущий процесс
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при перезапуске с правами администратора: " + ex.Message);
            }
        }

        // Защита файла
        private void ProtectHostsButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(HostsFilePath))
            {
                try
                {
                    ProtectFileFromDeletion(HostsFilePath);
                    MessageBox.Show("Файл hosts теперь защищен от удаления.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при защите файла: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Файл hosts не найден.");
            }
        }

        // Защита файла от удаления
        private void ProtectFileFromDeletion(string filePath)
        {
            FileSecurity fileSecurity = new FileSecurity(filePath, AccessControlSections.Access);
            var currentUser = WindowsIdentity.GetCurrent();
            var user = currentUser.Name;

            FileSystemAccessRule denyDeleteRule = new FileSystemAccessRule(
                user,
                FileSystemRights.Delete,
                AccessControlType.Deny);

            fileSecurity.AddAccessRule(denyDeleteRule);
            File.SetAccessControl(filePath, fileSecurity);
        }

        // Выполнение командной строки для захвата прав и предоставления доступа
        private void ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo("cmd", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = Process.Start(pro);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении команды: " + ex.Message);
            }
        }
    }
}
