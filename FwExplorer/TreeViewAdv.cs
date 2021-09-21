using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;



namespace FuzzyByte.Forms
{
    public partial class TreeViewAdv : TreeView
    {
//        private Font underLineFnt;

        public TreeViewAdv()
        {
            InitializeComponent();
            this.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            this.DrawNode += new DrawTreeNodeEventHandler(TreeViewAdv_DrawNode);
            this.MouseClick += new MouseEventHandler(TreeViewAdv_MouseClick);
//            underLineFnt = new Font(Font.FontFamily, Font.Size, FontStyle.Underline);
        }



        public void BuildTreeView(string fwFileName, string path)
        {
            TreeNode root = Nodes.Add(fwFileName, fwFileName, 0, 0);
            BuildTreeView(root, path);
            root.Expand();
            SelectedNode = root;
        }

        private void BuildTreeView(TreeNode aParent, string path)
        {
            string[] dirList = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
            foreach (string aDir in dirList)
            {
                string dir = aDir;
                dir = dir.Replace(path, "");
                string[] dirs = dir.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                TreeNode parent = aParent;
                for (int i = 0; i < dirs.Length - 1; i++)
                {
                    TreeNode[] nodes = parent.Nodes.Find(dirs[i], false);
                    parent = nodes[0];
                }
                parent.Nodes.Add(dirs[dirs.Length - 1], dirs[dirs.Length - 1], 1, 2);
            }
        }

        public void UpdateTreeView(TreeNode aNode, string path)
        {
            aNode.Nodes.Clear();
            BuildTreeView(aNode, path);
            SelectedNode = aNode;
        }


        public string SelectedPath
        {
            get
            {
                string s="\\";
                TreeNode node = SelectedNode;
                if (node == null)
                    return s;
                while (node.Parent != null)
                {
                    s = PathSeparator + node.Text + s;
                    node = node.Parent;
                }
                return s;
            }
        }


        public void SelectChildNode(string findNode)
        {
            TreeNodeCollection childs = this.Nodes;
            if (SelectedNode != null)
                childs = SelectedNode.Nodes;

            findNode = findNode.ToLower();
            foreach (TreeNode aNode in childs)
            {
                if (aNode.Text.ToLower() == findNode)
                {
                    SelectedNode = aNode;
                    aNode.Parent.Expand();
                    return;
                }
            }
        }


        private TreeNode FindNode(TreeNodeCollection nodes, Point point)
        {
            if (nodes == null || nodes.Count == 0)
                return null;
            foreach (TreeNode aNode in nodes)
            {
                if (point.Y >= aNode.Bounds.Top && point.Y <= aNode.Bounds.Bottom)
                    return aNode;
                TreeNode found = FindNode(aNode.Nodes, point);
                if (found != null)
                    return found;
            }
            return null;
        }

        void TreeViewAdv_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = PointToClient(Cursor.Position);
            TreeNode foundNode = FindNode(Nodes, point);
            if (foundNode != null)
            {
                SelectedNode = foundNode;
            }
        }


        void TreeViewAdv_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true;
            e.Graphics.FillRectangle(Brushes.White, e.Node.Bounds);
            e.Graphics.FillRectangle(Brushes.White, e.Bounds);
            return;

/*            e.Graphics.FillRectangle(Brushes.White, e.Node.Bounds);
            e.Graphics.FillRectangle(Brushes.White, e.Bounds);
            Font fnt = Font;
            Point point = PointToClient(Cursor.Position);
            if (e.Node.Bounds.Contains(point))
                fnt = underLineFnt;

            Brush foreColor = SystemBrushes.ControlText;
            if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Node.Bounds);
                foreColor = SystemBrushes.HighlightText;
            }
            if (!Focused && e.Node == SelectedNode)
            {
                e.Graphics.FillRectangle(SystemBrushes.Control, e.Node.Bounds);
            }

            if (e.Node.Bounds.Width != 0)
                e.Graphics.DrawString(e.Node.Text, fnt, foreColor, e.Node.Bounds.Location);*/
        }


    }
}
