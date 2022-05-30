using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace fileManager2022
{
    public partial class Form1 : Form
    {
        private Manager manager1;
        private Environment environment;
        public Form1()
        {
            InitializeComponent();
            MyInitializeComponent();
        }
        private void MyInitializeComponent()
        {
            SuspendLayout();

            manager1 = new Manager(listView1, comboBox1);
            manager1.RefreshCatalogue();

            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                Stream fileStream = new FileStream("envirData.dat", FileMode.Open, FileAccess.Read, FileShare.None);
                environment = (Environment)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();
            } catch (Exception ex)
            {
                environment = new Environment();
            }

            toolStripComboBox1.Items.AddRange(environment.fontCollection);
            for (int i = 0; i < toolStripComboBox1.Items.Count; i++)
                if (toolStripComboBox1.Items[i].Equals(environment.currFont))
                {
                    toolStripComboBox1.SelectedIndex = i;
                    break;
                }

            this.BackColor = environment.currBgCol;

            ResumeLayout(false);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1) // if only one item selected
            {
                if (listView1.SelectedItems[0].Tag is DirectoryInfo) // if it's catalogue
                {
                    manager1.parentCat = ((DirectoryInfo)listView1.SelectedItems[0].Tag).FullName; // new parent path
                    manager1.RefreshCatalogue();
                } else
                {
                    if (listView1.SelectedItems[0].Tag is FileInfo) // if it's file
                        if (((FileInfo)listView1.SelectedItems[0].Tag).Exists) Process.Start(((FileInfo)listView1.SelectedItems[0].Tag).FullName); 
                        // if it exists => start
                }
            }
        }

        private void popupMenuClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0) // if something is selected
            {
                if ((sender as ToolStripMenuItem).Name.Equals("renameToolStripMenuItem"))
                {
                    listView1.LabelEdit = true;                    
                    listView1.SelectedItems[0].Text = ((FileSystemInfo)listView1.SelectedItems[0].Tag).Name; // delete [] brackets from item text
                    listView1.SelectedItems[0].BeginEdit();
                }
                if ((sender as ToolStripMenuItem).Name.Equals("moveToToolStripMenuItem")||
                    (sender as ToolStripMenuItem).Name.Equals("copyToToolStripMenuItem"))
                {
                    manager1.catBuffer.Clear();
                    manager1.fileBuffer.Clear();

                    foreach (ListViewItem item in listView1.SelectedItems)
                    {
                        if (item.Tag is DirectoryInfo)
                            manager1.catBuffer.AddLast(item.Tag as DirectoryInfo);
                        else
                            if (item.Tag is FileInfo)
                                manager1.fileBuffer.AddLast(item.Tag as FileInfo);

                        item.ForeColor = System.Drawing.SystemColors.InactiveCaption;
                    }

                    manager1.isCopying = (sender as ToolStripMenuItem).Name.Equals("copyToToolStripMenuItem");
                    pasteToolStripMenuItem.Enabled = true;
                }
                if ((sender as ToolStripMenuItem).Name.Equals("deleteToolStripMenuItem"))
                {
                    LinkedList<FileSystemInfo> tmpInfo = new LinkedList<FileSystemInfo>();
                    foreach (ListViewItem item in listView1.SelectedItems)
                        tmpInfo.AddLast(item.Tag as FileSystemInfo);

                    manager1.Delete(tmpInfo); // unite compression and deleteion
                }
                if ((sender as ToolStripMenuItem).Name.Equals("compressToolStripMenuItem"))
                {
                    LinkedList<FileSystemInfo> tmpInfo = new LinkedList<FileSystemInfo>();
                    foreach (ListViewItem item in listView1.SelectedItems)
                        tmpInfo.AddLast(item.Tag as FileSystemInfo);

                    manager1.Compress(tmpInfo);               
                }
            } else // if nothing is selected (to paste correctly)
            {
                if ((sender as ToolStripMenuItem).Name.Equals("pasteToolStripMenuItem"))
                {
                    manager1.Paste();

                    pasteToolStripMenuItem.Enabled = false;
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DirectoryInfo newDrive = new DirectoryInfo(comboBox1.SelectedItem.ToString());
            if (newDrive.Exists)
            {
                manager1.parentCat = newDrive.FullName;
                manager1.RefreshCatalogue();
            }
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            listView1.LabelEdit = false;
            listView1.SelectedItems[0].Focused = false;

            if (e.Label == null) // if nothing changed in the label
            {
                if (listView1.SelectedItems[0].Tag is DirectoryInfo)
                {
                    listView1.SelectedItems[0].Text = "[" + listView1.SelectedItems[0].Text/*((DirectoryInfo)listView1.SelectedItems[0].Tag).Name*/ + "]";
                     // add these brackets back if it's catalogue
                }
            } else
            {
                if (!e.Label.Equals("")) // if smth changed and if the path name is appropriate
                {
                    if (listView1.SelectedItems[0].Tag is DirectoryInfo) // if catalogue is renamed
                    {
                        string newPath = ((DirectoryInfo)listView1.SelectedItems[0].Tag).Parent.FullName + @"\" + e.Label;
                        manager1.Rename(listView1.SelectedItems[0].Tag as DirectoryInfo, newPath);
                        /*listView1.Items[e.Item].Text = "[" + e.Label + "]]]]";
                        listView1.Items[e.Item].Tag = new DirectoryInfo(newPath);
                        listView1.Items[e.Item].Text = "[" + e.Label + "]]]]";*/
                    }
                    else
                    {
                        if (listView1.Items[e.Item].Tag is FileInfo) // if file is renamed
                        {
                            string newPath = ((FileInfo)listView1.Items[e.Item].Tag).Directory.FullName + @"\" + e.Label;
                            manager1.Rename(listView1.Items[e.Item].Tag as FileInfo, newPath);
                        }
                    }
                }
            }
            listView1.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            manager1.RefreshCatalogue();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            Stream stream = new FileStream("envirData.dat", FileMode.Create, FileAccess.Write, FileShare.None);
            binaryFormatter.Serialize(stream, environment);
            stream.Close();

            Application.Exit();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            environment.currFont = (string)toolStripComboBox1.SelectedItem;

            listView1.Font = new System.Drawing.Font(environment.currFont, 9.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            button2.Font = new System.Drawing.Font(environment.currFont, 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            comboBox1.Font = new System.Drawing.Font(environment.currFont, 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            menuStrip1.Font = new System.Drawing.Font(environment.currFont, 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            contextMenuStrip1.Font = new System.Drawing.Font(environment.currFont, 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            environment.currBgCol = colorDialog1.Color;
            this.BackColor = environment.currBgCol;
        }
    }
}
