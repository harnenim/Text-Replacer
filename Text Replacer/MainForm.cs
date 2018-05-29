using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using Newtonsoft.Json.Linq;

namespace Text_Replacer
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]

    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            AllowTransparency = true;

            string localURL = Path.Combine(Directory.GetCurrentDirectory(), "view/main.html");
            mainView.Url = new Uri(localURL, UriKind.Absolute);
            mainView.ObjectForScripting = this;
        }

        private void MainView_SizeChanged(object sender, EventArgs e)
        {
            mainView.Size = ((Control)sender).Size;
        }

        public void IsEternal() { }

        private string dragging = null;
        public void ShowDragging(string selector)
        {
            dragging = selector;
            string[] pos = mainView.Document.InvokeScript("getPositionOf", new object[] { selector }).ToString().Split(',');
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

        private void DragLeaveMain(object sender, EventArgs e)
        {
            HideDragging();
        }

        private void DragOverMain(object sender, DragEventArgs e)
        {
            switch (dragging)
            {
                case "#listFile":
                    e.Effect = DragDropEffects.All;
                    break;
            }
        }

        private void DragDropMain(object sender, DragEventArgs e)
        {
            switch (dragging)
            {
                case "#listFile":
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        string[] filters = mainView.Document.InvokeScript("getFilters").ToString().Split(',');
                        for (int i = 0; i < filters.Length; i++)
                        {
                            filters[i] = "^" + filters[i].Trim().Replace(".", "\\.").Replace("*", ".*") + "$";
                        }

                        LoadFileList((string[])e.Data.GetData(DataFormats.FileDrop), filters);
                    }
                    break;
            }
            HideDragging();
        }

        /**
         * 위쪽은 다른 프로젝트에도 쓰일 공통 메서드
         * 여기부터 전용 코드
         */

        private void LoadFileList(string[] strFiles, string[] filters)
        {
            foreach (string strFile in strFiles)
            {
                string[] fileExists = mainView.Document.InvokeScript("getFiles").ToString().Split('?');
                if (!fileExists.Contains(strFile))
                {
                    if (Directory.Exists(strFile))
                    {
                        DirectoryInfo[] dirInfos = new DirectoryInfo(strFile).GetDirectories();
                        string[] innerDirs = new string[dirInfos.Length];
                        for (int i = 0; i < dirInfos.Length; i++)
                        {
                            innerDirs[i] = dirInfos[i].FullName;
                        }
                        LoadFileList(innerDirs, filters);

                        FileInfo[] fileInfos = new DirectoryInfo(strFile).GetFiles();
                        string[] innerFiles = new string[fileInfos.Length];
                        for (int i=0; i<fileInfos.Length; i++)
                        {
                            innerFiles[i] = fileInfos[i].FullName;
                        }
                        LoadFileList(innerFiles, filters);
                    }
                    else
                    {
                        string[] filenames = strFile.Split('\\');
                        string filename = filenames[filenames.Length - 1];
                        bool isMatched = false;
                        foreach(string filter in filters)
                        {
                            if (isMatched = System.Text.RegularExpressions.Regex.IsMatch(filename, filter, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                break;
                            }
                        }
                        if (isMatched)
                        {
                            mainView.Document.InvokeScript("addFile", new object[] { strFile });
                        }
                    }
                }
            }
        }

        public string[][] JsonReplacersToArray(string json)
        {
            //return new string[][] { };
            JArray jarr = JArray.Parse(json);
            string[][] replacers = new string[jarr.Count][];
            for (int i = 0; i < jarr.Count; i++)
            {
                JArray jarr1 = (JArray)jarr[i];
                replacers[i] = new string[] { jarr1[0].ToString(), jarr1[1].ToString() };
            }
            return replacers;
        }

        private static string defaultReplacersFile = "conf/replacers.json";

        public void LoadDefaultReplacers()
        {
            string json = "[]";
            try
            {
                StreamReader sr = new StreamReader(defaultReplacersFile, Encoding.UTF8);
                json = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }

            string[][] replacers = JsonReplacersToArray(json);
            foreach (string[] replacer in replacers)
            {
                mainView.Document.InvokeScript("addReplacer", new object[] { replacer[0], replacer[1] });
            }
            /*
            mainView.Document.InvokeScript("addReplacer", new object[] { "다시 한번", "다시 한 번" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "그리고 보니", "그러고 보니" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "뒤쳐", "뒤처" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "제 정신", "제정신" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "스탠드 얼론", "스탠드얼론" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "멘테넌스", "메인터넌스" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "뒷처리", "뒤처리" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "스탭도", "스태프도" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "등 져선", "등져선" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "타코이즈", "터쿼이즈" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "쓰레드", "스레드" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "져버리지", "저버리지" });
            mainView.Document.InvokeScript("addReplacer", new object[] { "글러먹", "글러 먹" });
            */
        }
        public void SaveDefaultReplacers(string json)
        {
            StreamWriter sw = new StreamWriter(defaultReplacersFile);
            sw.WriteLine(json, Encoding.UTF8);
            sw.Close();
        }

        public string GetReplacersJson()
        {
            return mainView.Document.InvokeScript("getReplacers").ToString();
        }
        public string[][] GetReplacers(string json)
        {
            return JsonReplacersToArray(json);
        }
        public string[] GetReplaced(string file, string[][] replacers)
        {
            return GetReplaced(file, Common.DetectEncoding(file), replacers);
        }
        public string[] GetReplaced(string file, Encoding encoding, string[][] replacers)
        {
            string source = null, sourcePreview = null, replaced = null, replacedPreview = null;

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(file, encoding);
                source = sourcePreview = replaced = replacedPreview = sr.ReadToEnd();
            }
            catch
            {

            }
            finally
            {
                if (sr != null) sr.Close();
            }

            sourcePreview = System.Security.SecurityElement.Escape(sourcePreview);
            replacedPreview = System.Security.SecurityElement.Escape(replacedPreview);

            foreach (string[] replacer in replacers)
            {
                sourcePreview = sourcePreview.Replace(replacer[0], "<span class='highlight'>" + replacer[0] + "</span>");
                replaced = replaced.Replace(replacer[0], replacer[1]);
                replacedPreview = replacedPreview.Replace(replacer[0], "<span class='highlight'>" + replacer[1] + "</span>");
            }

            return new string[] { source, sourcePreview, replaced, replacedPreview };
        }
        public void OpenSource(string file)
        {
            string[] replaced = GetReplaced(file, GetReplacers(GetReplacersJson()));
            mainView.Document.InvokeScript("showPreview", new object[] { replaced[1], replaced[3] });
        }
        public void DoReplace()
        {
            string replacersJson = GetReplacersJson();
            string[][] replacers = GetReplacers(replacersJson);
            string filesString = mainView.Document.InvokeScript("getFiles").ToString();
            if (filesString.Length == 0)
            {
                mainView.Document.InvokeScript("alert", new object[] { "파일이 없습니다." });
                return;
            }
            string[] files = filesString.Split('?');
            foreach (string file in files)
            {
                Encoding encoding = Common.DetectEncoding(file);
                string[] replaced = GetReplaced(file, encoding, replacers);
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(file, false, encoding);
                    sw.Write(replaced[2]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    if (sw != null) sw.Close();
                }
            }
            SaveDefaultReplacers(replacersJson);
            mainView.Document.InvokeScript("alert", new object[] { "작업이 완료됐습니다." });
        }
    }
}
