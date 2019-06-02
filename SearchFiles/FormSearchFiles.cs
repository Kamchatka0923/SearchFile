using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace SearchFiles
{
    public partial class FormSearchFiles : Form
    {
        public FormSearchFiles()
        {
            InitializeComponent();
        }
        string path = string.Empty;
        string keyword = string.Empty;
        Queue<string> FileNames = new Queue<string>();
        void SearchFile(object path)
        {
            try
            {
                DirectoryInfo DI = new DirectoryInfo(path.ToString());
                FileInfo[] FIs = DI.GetFiles("*" + keyword + "*");
                DirectoryInfo[] FNs = DI.GetDirectories("*" + keyword + "*");
                DirectoryInfo[] DIs = DI.GetDirectories();
                lock (FileNames)
                {
                    foreach (var it in FIs)
                    {
                        FileNames.Enqueue(it.FullName);
                        SetListbox(it.FullName);
                    }
                    if (checkBox1.Checked)
                        foreach (var it in FNs)
                        {
                            FileNames.Enqueue(it.FullName);
                            SetListbox(it.FullName);
                        }
                }
                foreach (var it in DIs)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(SearchFile));
                    thread.Start(it.FullName);
                }
            }
            catch { }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            FileNames.Clear();
            keyword = textBox1.Text;
            if (path == "计算机")
                foreach (var it in Directory.GetLogicalDrives())
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(SearchFile));
                    thread.Start(it);
                }
            SearchFile(path);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(this,new EventArgs());
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            int index = listBox1.IndexFromPoint(((MouseEventArgs)e).Location);
            if(index!=ListBox.NoMatches)
            {
                try
                {
                    string filepath = listBox1.Items[listBox1.SelectedIndex].ToString();
                    if(File.Exists(filepath))
                        filepath = filepath.Remove(filepath.LastIndexOf(@"\"));
                    System.Diagnostics.Process.Start("explorer.exe", @filepath);
                }
                catch { MessageBox.Show("未选择要打开的文件路径"); }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            TreeNode computer = treeView1.Nodes.Add("计算机");
            foreach (var it in Directory.GetLogicalDrives())
                computer.Nodes.Add(it);
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            foreach(TreeNode node in e.Node.Nodes)
            {
                if(node.Nodes.Count==0)
                {
                    string nodepath = node.FullPath.Remove(0, 4);
                    DirectoryInfo dir = new DirectoryInfo(nodepath);
                    try
                    {
                        foreach (var it in dir.GetDirectories())
                        {
                            if (it.Name.Contains(@"$") || it.Name.Contains("System Volume Information"))
                                continue;
                            node.Nodes.Add(it.Name);
                        }
                    }
                    catch { continue; }
                }
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                path = e.Node.FullPath.Remove(0, 4);
                this.Text = "查找文件：" + path;
            }
            catch
            {
                path = e.Node.FullPath;
                this.Text = "查找文件：" + path;
            }
        }

        private delegate void SetListboxDelegate(string text);
        private void SetListbox(string text)
        {
            if(listBox1.InvokeRequired)
            {
                SetListboxDelegate d = new SetListboxDelegate(SetListbox);
                this.Invoke(d, text);
            }
            else
            {
                listBox1.Items.Add(text);
            }
        }
    }
}
