using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace fileManager2022
{
    internal class Manager
    {
        public string parentCat { get; internal set; } = DriveInfo.GetDrives()[0].Name;
        //internal FileSystemWatcher parentWatcher { get; set; }
        private DriveInfo[] driveInfo;
        private DirectoryInfo parentInfo;
        private DirectoryInfo[] catsInfo;
        private FileInfo[] fileInfo;
        private ListView _attachedLView;
        private ComboBox _attachedBox;
        internal LinkedList<DirectoryInfo> catBuffer { get; set; } = new LinkedList<DirectoryInfo>();
        internal LinkedList<FileInfo> fileBuffer { get; set; } = new LinkedList<FileInfo>();
        internal bool isCopying { get; set; } = true;

        public Manager() { }
        public Manager(ListView attachedLView, ComboBox attachedBox)
        {
            if (attachedLView != null) _attachedLView = attachedLView;
            if (attachedBox != null) _attachedBox = attachedBox;

            /*parentWatcher = new FileSystemWatcher(parentCat);
            parentWatcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;
            parentWatcher.Changed += new FileSystemEventHandler(OnFileSystemChanged);
            parentWatcher.Renamed += new RenamedEventHandler(OnFileSystemChanged);
            parentWatcher.Deleted += new FileSystemEventHandler(OnFileSystemChanged);
            parentWatcher.Created += new FileSystemEventHandler(OnFileSystemChanged);

            parentWatcher.Filter = "*.*";
            //parentWatcher.IncludeSubdirectories = true;
            parentWatcher.EnableRaisingEvents = false;*/
        }
        internal void RefreshCatalogue()
        {
            _attachedLView.Items.Clear();
            _attachedBox.Items.Clear();

            driveInfo = DriveInfo.GetDrives();
            parentInfo = new DirectoryInfo(parentCat); 
            if (parentInfo.Exists) // if can look into this directory??
            { 
                fileInfo = parentInfo.GetFiles();
                catsInfo = parentInfo.GetDirectories();                

                if (parentInfo.Parent != null) _attachedLView.Items.Add(new ListViewItem { Text = "[.]", Tag = parentInfo.Parent });

                _attachedBox.Text = parentCat;
                foreach (DriveInfo drive in driveInfo)
                {
                    _attachedBox.Items.Add(drive.Name);
                }
                foreach (DirectoryInfo cat in catsInfo)
                {
                    string[] catstring = new string[4] { "[" + cat.Name + "]", cat.Extension, "dir", cat.CreationTime.ToString() };
                    _attachedLView.Items.Add(new ListViewItem(catstring) { Tag = cat });
                }
                foreach (FileInfo file in fileInfo)
                {
                    string[] filestring = new string[4] { file.Name, file.Extension, file.Length.ToString(), file.CreationTime.ToString() };
                    _attachedLView.Items.Add(new ListViewItem(filestring) { Tag = file });
                }
            } // else ????
        }

        internal void Rename(FileSystemInfo info, string newpath) //change oldpath for item index
        {
            if (info is DirectoryInfo)
            {
                if (info.Exists) ((DirectoryInfo)info).MoveTo(newpath);
            } else if (info is FileInfo)
                if (info.Exists) ((FileInfo)info).MoveTo(newpath);

            RefreshCatalogue();
        }
        internal void Paste(string path) { }
        internal void Paste()
        {
            if (isCopying)
            {
                foreach (DirectoryInfo cat in catBuffer)                    
                    RecursiveCopy(cat, parentCat); // existance is checked in recursive function
                foreach (FileInfo file in fileBuffer)
                    if (file.Exists) {
                        int i = 0;
                        string newName = file.Name;
                        while (File.Exists(parentCat + @"\" + newName)) // if parentCat and file.DirectoryName aren't equal, suggest to replace?
                        {
                            i++;
                            newName = file.Name.Replace(file.Extension, "") + " - copy " + i.ToString() + file.Extension;
                        }

                        file.CopyTo(parentCat + @"\" + newName);
                    }
            } else
            {
                foreach (DirectoryInfo cat in catBuffer)
                    if (cat.Exists) cat.MoveTo(parentCat + @"\" + cat.Name);
                foreach (FileInfo file in fileBuffer)
                    if (file.Exists) file.MoveTo(parentCat + @"\" + file.Name);
            }

            catBuffer.Clear();
            fileBuffer.Clear();

            RefreshCatalogue();
        }

        private void RecursiveCopy(DirectoryInfo cat, string dest)
        {
            if (!cat.Exists) return;

            string newPath = dest + @"\" + cat.Name;
            Directory.CreateDirectory(newPath); // maybe there already exists?

            foreach (FileInfo file in cat.GetFiles())
            {
                file.CopyTo(newPath + @"\" + file.Name);
            }
            foreach (DirectoryInfo subcat in cat.GetDirectories())
            {
                RecursiveCopy(subcat, newPath);
            }
        }

        internal void Delete(LinkedList<FileSystemInfo> info)
        {
            foreach(FileSystemInfo catOrFile in info)
            {
                if (catOrFile is DirectoryInfo)
                {
                    if (catOrFile.Exists) ((DirectoryInfo)catOrFile).Delete(true);
                }
                else if (catOrFile is FileInfo)
                    if (catOrFile.Exists) ((FileInfo)catOrFile).Delete();
            }

            RefreshCatalogue();
        }
        internal void Compress(LinkedList<FileSystemInfo> info)
        {
            foreach (FileSystemInfo catOrFile in info)
            {
                if (catOrFile is FileInfo)
                {
                    if (catOrFile.Exists)
                    {
                        FileStream srcFile = File.OpenRead(catOrFile.FullName);
                        FileStream destFile = File.Create(catOrFile.FullName.Replace(catOrFile.Extension, "") + ".gz");

                        byte[] buffer = new byte[srcFile.Length];
                        srcFile.Read(buffer, 0, buffer.Length);

                        using (GZipStream gzstream = new GZipStream(destFile, CompressionMode.Compress))
                        {
                            gzstream.Write(buffer, 0, buffer.Length);
                        }

                        srcFile.Close();
                        destFile.Close();
                    }
                    //ZipFile.CreateFromDirectory(catOrFile.FullName, catOrFile.FullName.Replace(catOrFile.Extension, "") + ".gz", CompressionLevel.Optimal, true);
                } else
                {
                    if (catOrFile is DirectoryInfo)
                    {
                        if (catOrFile.Exists)
                        {
                            ZipFile.CreateFromDirectory(catOrFile.FullName, catOrFile.FullName + ".gz", CompressionLevel.Optimal, true);
                        }
                    }
                }
            }

            RefreshCatalogue();
        }
    }
}
