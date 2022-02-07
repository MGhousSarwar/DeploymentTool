using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeploymentToolForServices
{
    [Serializable]
    public class Folder
    {
        public string FolderPath { get; set; }
        public List<Folder> SubFolders { get; set; }
        public List<string> Files { get; set; }
        public bool IsProcessed { get; set; }
        public bool InProcess { get; set; }
        public Folder(string path)
        {
            FolderPath = path;
            SubFolders = new List<Folder>();
            Files = new List<string>();
            IsProcessed = false;
            InProcess = false;
        }
    }
    [Serializable]
    public class MainClass
    {
        public Folder Folder { get; set; }
        public long foldersCount { get; set; }
        public long filesCount { get; set; }
        public long processedCount { get; set; }
        public string source { get; set; }
        public string destination { get; set; }
        public List<string> probihitedPath = new List<string>();
        public List<string> errorList = new List<string>();
        public string excellPath { get; set; }
        public MainClass()
        {
            Folder = null;
            foldersCount = 0;
            filesCount = 0;
            processedCount = 0;

        }
        public bool deleteCheck { get; set; }
        public bool fileNameV0 { get; set; }
        public bool lastSaveDate { get; set; }
        public bool fileNameQA { get; set; }
        public bool FilePathFinal { get; set; }
        public bool exceptPath { get; set; }
    }
}
