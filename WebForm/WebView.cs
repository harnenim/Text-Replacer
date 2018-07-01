using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WebForm
{
    public class WebView : WebBrowser
    {
        #region 스크립트 핸들러

        delegate string ScriptHandler(string name, object[] args);
        public string Script(string name) { return Script(name, null); }
        public string Script(string name, object[] args)
        {
            object result = null;

            try
            {
                if (InvokeRequired)
                    result = Invoke(new ScriptHandler(Script), new object[] { name, args });
                else
                {
                    int len = (args == null) ? 0 : Math.Min(10, args.Length);
                    object[] param = new object[len + 1];
                    param[0] = name;
                    for (int i = 0; i < len; i++)
                        param[i + 1] = args[i];
                    result = Document.InvokeScript("call", param);
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


        #region 클릭 이벤트

        public void SetClickEvent(string id, string action)
        {
            Script("setClickEvent", new object[] { id, action });
        }
        public void SetChangeEvent(string id, string action)
        {
            Script("setChangeEvent", new object[] { id, action });
        }

        #endregion


        #region 파일 열기 대화상자

        public delegate bool OpenFileDelegate(string path);
        public Dictionary<string, OpenFileDelegate> openFileActions = new Dictionary<string, OpenFileDelegate>();

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
        public void SetOpenFileEvent(string id, OpenFileDelegate action)
        {
            openFileActions.Add(id, action);
        }

        #endregion
    }
}
