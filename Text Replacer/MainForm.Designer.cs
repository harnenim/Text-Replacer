using System.Windows.Forms;
using System.Drawing;

namespace Text_Replacer
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.mainView = new System.Windows.Forms.WebBrowser();
            this.layerForDrag = new Text_Replacer.TransparentPanel();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // mainView
            // 
            this.mainView.AllowNavigation = false;
            this.mainView.AllowWebBrowserDrop = false;
            this.mainView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainView.Location = new System.Drawing.Point(0, 0);
            this.mainView.Margin = new System.Windows.Forms.Padding(0);
            this.mainView.MinimumSize = new System.Drawing.Size(20, 20);
            this.mainView.Name = "mainView";
            this.mainView.Size = new System.Drawing.Size(1008, 728);
            this.mainView.TabIndex = 5;
            this.mainView.SizeChanged += new System.EventHandler(this.MainView_SizeChanged);
            // 
            // layerForDrag
            // 
            this.layerForDrag.AllowDrop = true;
            this.layerForDrag.Location = new System.Drawing.Point(0, 0);
            this.layerForDrag.Name = "layerForDrag";
            this.layerForDrag.Size = new System.Drawing.Size(200, 100);
            this.layerForDrag.TabIndex = 7;
            this.layerForDrag.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragDropMain);
            this.layerForDrag.DragOver += new System.Windows.Forms.DragEventHandler(this.DragOverMain);
            this.layerForDrag.DragLeave += new System.EventHandler(this.DragLeaveMain);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.layerForDrag);
            this.Controls.Add(this.mainView);
            this.Name = "MainForm";
            this.Text = "Text Replacer";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ErrorProvider errorProvider1;
        private WebBrowser mainView;
        private TransparentPanel layerForDrag;
    }
}

