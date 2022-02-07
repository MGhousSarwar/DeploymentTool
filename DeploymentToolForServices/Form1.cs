using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeploymentToolForServices
{
    public partial class Form1 : Form
    {
        Queue<string> folderQueue = new Queue<string>();
        string currentDirectory;
        string textFilePath;
        public MainClass mainClass = new MainClass();
        List<string> listOfExes = new List<string>();
        public Form1()
        {
            currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                _InfoLabel.Text = "Source Folder Loading Please Wait !";
                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    mainClass.source = folderBrowserDialog1.SelectedPath;
                    sourceTextBox.Text = mainClass.source;
                    _InfoLabel.Text = "Source Folder Loaded Successfullys !";
                    listBox1.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        private async Task<Folder> Preparing(string path, bool isSourcePath)
        {
            try
            {
                Folder f = new Folder(path);
                mainClass.foldersCount++;
                List<string> subdirectoryEntries = new List<string>();
                await Task.Run(() =>
                subdirectoryEntries = GetFolderList(path)
                );
                foreach (string subdirectory in subdirectoryEntries)
                {
                    if (CheckForAllowedServices(subdirectory))
                    {
                        f.SubFolders.Add(await Preparing(subdirectory, isSourcePath));
                    }
                }
                f.Files = await Task.Run(() => GetFileList(path, isSourcePath));
                //listBox1.Items.AddRange(f.Files.ToArray());
                mainClass.filesCount += f.Files.Count;
                _InfoLabel.Text = $"Preparing... {mainClass.filesCount} Files, { mainClass.foldersCount} Folder";
                this.Refresh();
                return f;
            }
            catch (Exception ex)
            {
                listBox1.Items.Add($"Exception while preparing at :{path}");
                this.Refresh();
                return null;
            }

            bool CheckForAllowedServices(string subdirectory)
            {
                return 
                (searchchk.Checked && subdirectory.Contains("Search"))
                                        || (communicatorchk.Checked && subdirectory.Contains("Communications"))
                                        || (evidencechk.Checked && subdirectory.Contains("Asset"))
                                        || (setupchk.Checked && subdirectory.Contains("Setup_and_Configuration"));
            }
        }
        private List<string> GetFolderList(string path)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(path);
            return subdirectoryEntries.ToList();
        }
        private List<string> GetFileList(string path, bool isSourcePath)
        {
            //var ls = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).ToList();
            var ls = new List<string>();
            if ((path.Contains(".Service\\bin\\Release") || path.Contains("Communications\\bin\\Release")) && isSourcePath)
            {
                var templs = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).ToList();

                return templs;
            }
            else if (path == mainClass.source && isSourcePath)
            {
                return Directory.GetFiles(path, "Run*", SearchOption.TopDirectoryOnly).ToList();
            }
            else if (!isSourcePath)
            {
                ls = Directory.GetFiles(path, "*exe", SearchOption.TopDirectoryOnly).ToList();
                listOfExes.AddRange(ls);
                return ls;

            }
            return ls;

        }

        private void HandleException(Exception ex)
        {
            listBox1.Items.Add($"{ex.ToString()}");
            mainClass.errorList.Add(ex.ToString());
            label6.Text = $"{mainClass.errorList.Count}";
            this.Refresh();

        }
        private async Task ProcessFolder(Folder folder)
        {
            try
            {
                //Process Files of CurrentFolder
                if (!folder.IsProcessed)
                {
                    await Task.Run(() => ProsessFiles(folder.FolderPath, folder.Files));
                    folder.IsProcessed = true;
                }
                UpdateUI();
                //pop next path and repeat the process
                foreach (var item in folder.SubFolders)
                {
                    await ProcessFolder(item);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

        }
        private void UpdateUI()
        {
            label6.Text = $"{mainClass.errorList.Count}";
            _InfoLabel.Text = $"{mainClass.processedCount} Processed Files  of {mainClass.filesCount} Files, { mainClass.foldersCount} Folder";
            //progressBar1.Value = (int)((mainClass.processedCount / mainClass.filesCount) * 100);
            //progressBar1.Refresh();
            this.Refresh();
            listBox1.TopIndex = listBox1.Items.Count - 1;

            //Thread.Sleep(100);
        }
        private void ProsessFiles(string path, List<string> filePaths)
        {
            try
            {
                //if (FilePathFinal.Checked)
                //{
                //    mainClass.processedCount += filePaths.FindAll(p => p.ToLower().Contains("final")).Count;
                //    filePaths = filePaths.FindAll(p => !p.ToLower().Contains("final"));
                //}

                if (filePaths.Count > 0)
                {
                    var folderPath = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(filePaths[0])))));
                    //listBox1.Items.Add(folderPath);

                    Directory.CreateDirectory(path.Replace(mainClass.source, mainClass.destination));
                    foreach (var item in filePaths)
                    {

                        try
                        {
                            listBox1.Items.Add(item);

                            var fileName = System.IO.Path.GetFileName(item);
                            {
                                var destFile = System.IO.Path.Combine(path.Replace(mainClass.source, mainClass.destination), fileName);
                                System.IO.File.Copy(item, destFile, true);
                                var deleteDestinatio1n = System.IO.Path.Combine(path.Replace(mainClass.destination, mainClass.source), fileName);

                            }
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex);
                        }
                        finally
                        {
                            mainClass.processedCount++;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

        }
        private async  void button5_Click(object sender, EventArgs e)
        {
            //Publish
            try
            {
                //Start
                if (CheckValidation())
                {
                    //if (mainClass.Folder == null)
                    {
                        mainClass.filesCount = 0;
                        mainClass.foldersCount = 0;
                        mainClass.processedCount = 0;
                        listBox1.Items.Clear();
                        mainClass.Folder = await Preparing(mainClass.source, true);
                    }
                    _InfoLabel.Text = $"{mainClass.processedCount} Processed Files  of {mainClass.filesCount} Files, { mainClass.foldersCount} Folder";
                    await ProcessFolder(mainClass.Folder);

                    MessageBox.Show("Published..");
                }
                //listBox1.Items.Add(Directory.GetFiles(root, "*", SearchOption.AllDirectories).Length);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        //Clean And Build
        private void button2_Click(object sender, EventArgs e)
        {
            if (CheckValidation())
            {
                ExecuteCommand();
            }

        }
        void ExecuteCommand()
        {
            _InfoLabel.Text = "Cleaning & building Project";
            Directory.SetCurrentDirectory(mainClass.source);
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo(mainClass.source + "\\Build Services.bat");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                listBox1.Items.Add("output>>" + e.Data);
                this.Refresh();
                listBox1.TopIndex = listBox1.Items.Count - 1;
            };
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                listBox1.Items.Add("error>>" + e.Data);
                this.Refresh();
                listBox1.TopIndex = listBox1.Items.Count - 1;

            };
            process.BeginErrorReadLine();

            process.WaitForExit();

            listBox1.Items.Add("ExitCode: {0} " + process.ExitCode);
            process.Close();
            _InfoLabel.Text = "Cleaning & building Completed";

        }
        private bool CheckValidation()
        {
            if (string.IsNullOrEmpty(sourceTextBox.Text))
            {
                MessageBox.Show("Browse Source Folder");
                return false;
            }
            else
            {
                mainClass.source = sourceTextBox.Text;
            }
            if (string.IsNullOrEmpty(destinationTextBox.Text))
            {
                MessageBox.Show("Browse Source Folder");
                return false;
            }
            else
            {
                mainClass.destination = destinationTextBox.Text;
            }
            //if (string.IsNullOrEmpty(excelFileTextBox.Text))
            //{
            //    MessageBox.Show("Browse Excel File");
            //    return false;
            //}
            return true;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            var p6 = panel6.Size.Width;
            sourceTextBox.Size = new Size(p6 - 291, sourceTextBox.Size.Height);
            destinationTextBox.Size = new Size(p6 - 291, destinationTextBox.Size.Height);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                _InfoLabel.Text = "Publish Folder Loading Please Wait !";
                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    mainClass.destination = folderBrowserDialog1.SelectedPath;
                    destinationTextBox.Text = mainClass.destination;
                    _InfoLabel.Text = "Publish Folder Loaded Successfullys !";
                    listBox1.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void panel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            //Run Exe's

            try
            {
                //Start
                if (CheckValidation())
                {

                    listOfExes = new List<string>();
                    mainClass.filesCount = 0;
                    mainClass.foldersCount = 0;
                    mainClass.processedCount = 0;
                    listBox1.Items.Clear();
                    mainClass.Folder = await Preparing(mainClass.destination, false);
                    _InfoLabel.Text = $"Total Exe Fetched : {listOfExes.Count}";
                    listBox1.Items.Clear();
                    foreach (var item in listOfExes)
                    {
                        listBox1.Items.Add(item);
                    }
                    MessageBox.Show("Services Started..");
                }
                //listBox1.Items.Add(Directory.GetFiles(root, "*", SearchOption.AllDirectories).Length);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void panel13_Paint(object sender, PaintEventArgs e)
        {

        }

       
    }
}
