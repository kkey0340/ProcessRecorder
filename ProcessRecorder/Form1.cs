using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ProcessRecorder
{
    public partial class Form1 : Form
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        Process[] oldProcess = new Process[0];

        public Form1()
        {
            InitializeComponent();

            logger.Info("Application init");

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();

                logger.Info("Application hide");

                return;

            }
            else if(e.CloseReason == CloseReason.ApplicationExitCall)
            {
                logger.Info("Application close");

            }

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.Focus();

                logger.Info("Application show");

            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Process[] nowProcess = Process.GetProcesses();

            Process[] newProcess = nowProcess.Where(i => !oldProcess.Any(j => i.Id == j.Id && i.ProcessName == j.ProcessName)).ToArray();
            Process[] abortProcess = oldProcess.Where(i => !nowProcess.Any(j => i.Id == j.Id && i.ProcessName == j.ProcessName)).ToArray();

            if(newProcess.Length > 0)
            {
                foreach (var i in newProcess)
                {
                    try
                    {
                        if (i.Id == i.Parent().Id)
                            logger.Info(string.Format($"Create, ID: {i.Id:00000},\tName: {i.ProcessName},\tLocation: {i.MainModule.FileName}"));
                        else
                            logger.Info(string.Format($"Create, ID: {i.Id:00000},\tParentID: {i.Parent().Id:00000},\tName: {i.ProcessName},\tLocation: {i.MainModule.FileName}"));

                    }
                    catch(Exception ex)
                    {
                        Debug.Print($"Error msg: {ex.Message}");
                        logger.Info(string.Format("Create, ID: {0:00000},\tName: {1}", i.Id, i.ProcessName));

                    }

                }

            }

            if (abortProcess.Length > 0)
            {
                foreach (var i in abortProcess)
                {
                    try
                    {
                        logger.Info($"Abort, ID: {i.Id:00000},\tName: {i.ProcessName},\tLocation: {i.MainModule.FileName}");

                    }
                    catch
                    {
                        logger.Info($"Abort, ID: {i.Id:00000},\tName: {i.ProcessName}");

                    }

                }

            }

            if(newProcess.Length > 0 || abortProcess.Length > 0)
                oldProcess = (Process[])nowProcess.Clone();

        }

        private void ToolStripMenuItem01_Click(object sender, EventArgs e)
        {
            logger.Info("Application exit");

            Application.Exit();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logger.Info("Application load");

            oldProcess = Process.GetProcesses();

            this.Hide();

        }

        protected override void SetVisibleCore(bool value)
        {
            if (!this.IsHandleCreated)
            {
                value = false;
                CreateHandle();
            }
            base.SetVisibleCore(value);
        }

    }

    public static class ProcessExtensions
    {
        private static string FindIndexedProcessName(int pid)
        {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexdName = null;

            for (var index = 0; index < processesByName.Length; index++)
            {
                processIndexdName = index == 0 ? processName : processName + "#" + index;
                
                var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);

                if ((int)processId.NextValue() == pid)
                    return processIndexdName;
            }

            return processIndexdName;

        }

        private static Process FindPidFromIndexedProcessName(string indexedProcessName)
        {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
            
            return Process.GetProcessById((int)parentId.NextValue());

        }

        public static Process Parent(this Process process)
            => FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
    }

}
