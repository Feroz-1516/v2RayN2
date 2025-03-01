﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;

namespace v2rayUpgrade
{
    public partial class MainForm : Form
    {
        private readonly string defaultFilename = "v2ray-windows.zip";
        private string? fileName;

        public MainForm(string[] args)
        {
            InitializeComponent();
            if (args.Length > 0)
            {
                fileName = Uri.UnescapeDataString(string.Join(" ", args));
            }
            else
            {
                fileName = defaultFilename;
            }
        }

        private void ShowWarn(string message)
        {
            MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                Process[] existing = Process.GetProcessesByName("v2rayN");
                foreach (Process p in existing)
                {
                    string? path = p.MainModule?.FileName;
                    if (path == GetPath("v2rayN.exe"))
                    {
                        p.Kill();
                        p.WaitForExit(100);
                    }
                }
            }
            catch (Exception ex)
            {
                
                ShowWarn("Failed to close v2rayN(关闭v2rayN失败).\n" +
                    "Close it manually, or the upgrade may fail.(请手动关闭正在运行的v2rayN，否则可能升级失败。\n\n" + ex.StackTrace);
            }

            if (!File.Exists(fileName))
            {
                if (File.Exists(defaultFilename))
                {
                    fileName = defaultFilename;
                }
                else
                {
                    ShowWarn("Upgrade Failed, File Not Exist(升级失败,文件不存在).");
                    return;
                }
            }

            StringBuilder sb = new();
            try
            {
                string thisAppOldFile = $"{Application.ExecutablePath}.tmp";
                File.Delete(thisAppOldFile);
                string startKey = "v2rayN/";

                using ZipArchive archive = ZipFile.OpenRead(fileName);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    try
                    {
                        if (entry.Length == 0)
                        {
                            continue;
                        }
                        string fullName = entry.FullName;
                        if (fullName.StartsWith(startKey))
                        {
                            fullName = fullName[startKey.Length..];
                        }
                        if (string.Equals(Application.ExecutablePath, GetPath(fullName), StringComparison.OrdinalIgnoreCase))
                        {
                            File.Move(Application.ExecutablePath, thisAppOldFile);
                        }

                        string entryOutputPath = GetPath(fullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(entryOutputPath)!);
                        entry.ExtractToFile(entryOutputPath, true);
                    }
                    catch (Exception ex)
                    {
                        sb.Append(ex.StackTrace);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowWarn("Upgrade Failed(升级失败)." + ex.StackTrace);
                return;
            }
            if (sb.Length > 0)
            {
                ShowWarn("Upgrade Failed,Hold ctrl + c to copy to clipboard.\n" +
                    "(升级失败,按住ctrl+c可以复制到剪贴板)." + sb.ToString());
                return;
            }

            Process.Start("v2rayN.exe");
            MessageBox.Show("Upgrade successed(升级成功)", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        public static string GetExePath()
        {
            return Application.ExecutablePath;
        }

        public static string StartupPath()
        {
            return Application.StartupPath;
        }

        public static string GetPath(string fileName)
        {
            string startupPath = StartupPath();
            if (string.IsNullOrEmpty(fileName))
            {
                return startupPath;
            }
            return Path.Combine(startupPath, fileName);
        }
    }
}
