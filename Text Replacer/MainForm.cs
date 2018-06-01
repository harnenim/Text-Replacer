using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using Newtonsoft.Json.Linq;

namespace Web_Form
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]

    public partial class MainForm : WebForm
    {
        #region 초기화

        public MainForm()
        {
            ClientSize = new System.Drawing.Size(1008, 729);
            Name = "MainForm";
            Text = "Text Replacer";
        }

        public override void InitAfterLoad()
        {
            SetDragEvent("listFile", DragDropEffects.All, new DropActionDelegate(DropListFile));
            SetClickEvent("btnSubmit", "DoReplace");
        }

        #endregion

        #region 파일 드래그

        private void DropListFile(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filters = Script("getFilters").ToString().Split(',');
                for (int i = 0; i < filters.Length; i++)
                {
                    filters[i] = "^" + filters[i].Trim().Replace(".", "\\.").Replace("*", ".*") + "$";
                }

                LoadFileList((string[])e.Data.GetData(DataFormats.FileDrop), filters);
            }
        }

        private void LoadFileList(string[] strFiles, string[] filters)
        {
            foreach (string strFile in strFiles)
            {
                string[] fileExists = Script("getFiles").ToString().Split('?');
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
                            Script("addFile", new object[] { strFile });
                        }
                    }
                }
            }
        }

        #endregion


        // 이 프로그램에선 string[][] 형식 json으로만 주고받음
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
                Script("addReplacer", new object[] { replacer[0], replacer[1] });
            }
            /*
            Script("addReplacer", new object[] { "다시 한번", "다시 한 번" });
            Script("addReplacer", new object[] { "그리고 보니", "그러고 보니" });
            Script("addReplacer", new object[] { "뒤쳐", "뒤처" });
            Script("addReplacer", new object[] { "제 정신", "제정신" });
            Script("addReplacer", new object[] { "스탠드 얼론", "스탠드얼론" });
            Script("addReplacer", new object[] { "멘테넌스", "메인터넌스" });
            Script("addReplacer", new object[] { "뒷처리", "뒤처리" });
            Script("addReplacer", new object[] { "스탭도", "스태프도" });
            Script("addReplacer", new object[] { "등 져선", "등져선" });
            Script("addReplacer", new object[] { "타코이즈", "터쿼이즈" });
            Script("addReplacer", new object[] { "쓰레드", "스레드" });
            Script("addReplacer", new object[] { "져버리지", "저버리지" });
            Script("addReplacer", new object[] { "글러먹", "글러 먹" });
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
            return Script("getReplacers").ToString();
        }
        public string[][] GetReplacers(string json)
        {
            return JsonReplacersToArray(json);
        }
        public string[] GetReplaced(string file, string[][] replacers)
        {
            return GetReplaced(file, BomEncoding.DetectEncoding(file), replacers);
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
            Script("showPreview", new object[] { replaced[1], replaced[3] });
        }
        public void DoReplace()
        {
            string replacersJson = GetReplacersJson();
            string[][] replacers = GetReplacers(replacersJson);
            string filesString = Script("getFiles").ToString();
            if (filesString.Length == 0)
            {
                Script("alert", new object[] { "파일이 없습니다." });
                return;
            }
            string[] files = filesString.Split('?');
            foreach (string file in files)
            {
                Encoding encoding = BomEncoding.DetectEncoding(file);
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
            Script("alert", new object[] { "작업이 완료됐습니다." });
        }
    }
}
