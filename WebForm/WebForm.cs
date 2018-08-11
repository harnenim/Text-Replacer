using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WebForm
{
    public partial class WebForm : Form
    {
        public WebForm()
        {
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            AllowTransparency = true;

            string localURL = Path.Combine(Directory.GetCurrentDirectory(), "view/main.html");
            mainView.Url = new Uri(localURL, UriKind.Absolute);
            mainView.ObjectForScripting = this;

            FormClosed += new FormClosedEventHandler(WebFormClosed);
        }

        public void WebFormClosed(object sender, FormClosedEventArgs e)
        {
            // <textarea>에 커서 있거나 할 때 닫으면 상당한 비율로 정상종료 실패함
            // 이게 최선인가...
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public virtual void InitAfterLoad() { } // override용

        protected string Script(string name) { return mainView.Script(name, null); }
        protected string Script(string name, object[] args) { return mainView.Script(name, args); }
        protected void SetClickEvent(string id, string action) { mainView.SetClickEvent(id, action); }
        protected void SetChangeEvent(string id, string action) { mainView.SetChangeEvent(id, action); }


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
            layerForDrag.Left = mainView.Left + (int)Double.Parse(pos[0]);
            layerForDrag.Top = mainView.Top + (int)Double.Parse(pos[1]);
            layerForDrag.Width = (int)Double.Parse(pos[2]);
            layerForDrag.Height = (int)Double.Parse(pos[3]);
            Script("setShowDrag", new object[] { true });
        }
        public void HideDragging()
        {
            layerForDrag.Visible = false;
            Script("setShowDrag", new object[] { false });
        }

        protected void DragLeaveMain(object sender, EventArgs e) { HideDragging(); }
        protected void DragOverMain(object sender, DragEventArgs e) { e.Effect = dragEffects[dragging]; }
        protected void DragDropMain(object sender, DragEventArgs e) { dropActions[dragging]?.Invoke(e); HideDragging(); }
        protected void SetDragEvent(string id, DragDropEffects effect, DropActionDelegate action)
        {
            dragEffects.Add(id, effect);
            dropActions.Add(id, action);
            Script("setDroppable", new object[] { id });
        }

        #endregion
    }
}
