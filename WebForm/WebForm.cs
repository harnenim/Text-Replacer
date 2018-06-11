using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Security.Permissions;

namespace WebForm
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]

    public partial class WebForm : Form
    {
        public WebForm()
        {
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            AllowTransparency = true;

            string localURL = Path.Combine(Directory.GetCurrentDirectory(), "view/main.html");
            mainView.Url = new Uri(localURL, UriKind.Absolute);
            mainView.ObjectForScripting = this;
        }

        public virtual void InitAfterLoad() { } // override용

        private void MainView_SizeChanged(object sender, EventArgs e)
        {
            mainView.Size = ((Control)sender).Size;
        }

        public void IsEternal() { }


        #region 스크립트 핸들러

        delegate string ScriptHandler(string name, object[] args);
        protected string Script(string name) { return Script(name, null); }
        protected string Script(string name, object[] args)
        {
            object result = null;

            try
            {
                if (mainView.InvokeRequired)
                    result = Invoke(new ScriptHandler(Script), new object[] { name, args });
                else
                {
                    int len = (args == null) ? 0 : Math.Min(10, args.Length);
                    object[] param = new object[len + 1];
                    param[0] = name;
                    for (int i = 0; i < len; i++)
                        param[i + 1] = args[i];
                    result = mainView.Document.InvokeScript("call", param);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(0);
            }

            if (result == null) return null;
            return result.ToString();
        }

        #endregion


        #region 드래그 관련

        protected delegate void DropActionDelegate(DragEventArgs e);
        protected string dragging = null;
        protected Dictionary<string, DragDropEffects> dragEffects = new Dictionary<string, DragDropEffects>();
        protected Dictionary<string, DropActionDelegate> dropActions = new Dictionary<string, DropActionDelegate>();

        public void ShowDragging(string id)
        {
            dragging = id;
            string[] pos = Script("getPositionOf", new object[] { id }).Split(',');
            layerForDrag.Visible = true;
            layerForDrag.Left = mainView.Left + Int32.Parse(pos[0]);
            layerForDrag.Top = mainView.Top + Int32.Parse(pos[1]);
            layerForDrag.Width = Int32.Parse(pos[2]);
            layerForDrag.Height = Int32.Parse(pos[3]);
            mainView.Document.InvokeScript("setShowDrag", new object[] { true });
        }
        public void HideDragging()
        {
            layerForDrag.Visible = false;
            mainView.Document.InvokeScript("setShowDrag", new object[] { false });
        }

        private void DragLeaveMain(object sender, EventArgs e) { HideDragging(); }
        private void DragOverMain(object sender, DragEventArgs e) { e.Effect = dragEffects[dragging]; }
        private void DragDropMain(object sender, DragEventArgs e) { dropActions[dragging]?.Invoke(e); HideDragging(); }
        protected void SetDragEvent(string id, DragDropEffects effect, DropActionDelegate action)
        {
            dragEffects.Add(id, effect);
            dropActions.Add(id, action);
            Script("setDroppable", new object[] { id });
        }

        #endregion


        #region 클릭 이벤트

        protected void SetClickEvent(string id, string action)
        {
            Script("setClickEvent", new object[] { id, action });
        }
        protected void SetChangeEvent(string id, string action)
        {
            Script("setChangeEvent", new object[] { id, action });
        }

        #endregion


        #region 파일 열기 대화상자

        protected delegate bool OpenFileDelegate(string path);
        protected Dictionary<string, OpenFileDelegate> openFileActions = new Dictionary<string, OpenFileDelegate>();

        public void OpenFile(string id)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.FileName;
                if (OpenFile(id, path))
                    Script("setFile", new object[] { id, path });
            }
        }
        private bool OpenFile(string id, string path)
        {
            bool? result = openFileActions[id]?.Invoke(path);
            return result == null ? false : (bool)result;
        }
        protected void SetOpenFileEvent(string id, OpenFileDelegate action)
        {
            openFileActions.Add(id, action);
        }

        #endregion
    }
}
