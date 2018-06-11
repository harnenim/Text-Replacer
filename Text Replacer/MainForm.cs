using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using Newtonsoft.Json.Linq;
using TextFile;

namespace TextReplacer
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]

    public partial class MainForm : WebForm.WebForm
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


        #region 기본값 관련

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
            catch (Exception e)
            {
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
                string[] fileExists = Script("getFiles")?.Split('?');
                if (fileExists == null || !fileExists.Contains(strFile))
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
                        Console.WriteLine(strFile);
                        string[] filenames = strFile.Split('\\');
                        string filename = filenames[filenames.Length - 1];
                        bool isMatched = false;
                        foreach(string filter in filters)
                        {
                            Console.WriteLine(filter);
                            if (isMatched = System.Text.RegularExpressions.Regex.IsMatch(filename, filter, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                break;
                            }
                        }
                        Console.WriteLine(isMatched);
                        if (isMatched)
                        {
                            Console.WriteLine("addFile: " + strFile);
                            Script("addFile", new object[] { strFile });
                        }
                    }
                }
            }
        }

        #endregion

        // html 뷰에서 문자열 쌍(json) 가져오기
        public string GetReplacersJson()
        {
            return Script("getReplacers");
        }
        // json 문자열 쌍을 이중배열로 변환(string[][2]로 나와야 함)
        public string[][] GetReplacers(string json)
        {
            return JsonReplacersToArray(json);
        }

        /*
         * string file: 파일 경로
         * string[][2] replacers: 문자열 쌍
         * return string[4]: 원본 / 원본 미리보기 / 결과 / 결과 미리보기
         * ... 사실 string 4개짜리 class 만들기 귀찮아서 string[4]로 처리한 거임
         */
        public string[] GetReplaced(string file, string[][] replacers)
        {
            return GetReplaced(file, BOM.DetectEncoding(file), replacers);
        }
        /*
         * string file: 파일 경로
         * Encoding encoding: 파일 문자열 인코딩
         * string[][2] replacers: 문자열 쌍
         * return string[4]: 원본 / 원본 미리보기 / 결과 / 결과 미리보기
         */
        public string[] GetReplaced(string file, Encoding encoding, string[][] replacers)
        {
            // 원본 / 원본 미리보기 / 결과 / 결과 미리보기
            string source = null, sourcePreview = null, replaced = null, replacedPreview = null;

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(file, encoding);

                // 모두 불러온 대로 초기화
                source = sourcePreview = replaced = replacedPreview = sr.ReadToEnd();
            }
            catch { }
            finally { if (sr != null) sr.Close(); }

            // 미리보기 html escape
            sourcePreview   = System.Security.SecurityElement.Escape(sourcePreview  );
            replacedPreview = System.Security.SecurityElement.Escape(replacedPreview);

            // 각각의 변환 대상 문자열 쌍에 대해 변환
            foreach (string[] replacer in replacers)
            {
                // 원본 미리보기(하이라이트 태그만 씌움) / 결과 / 결과 미리보기 각각 변환
                sourcePreview = sourcePreview.Replace(replacer[0], "<span class='highlight'>" + replacer[0] + "</span>");
                replaced = replaced.Replace(replacer[0], replacer[1]);
                replacedPreview = replacedPreview.Replace(replacer[0], "<span class='highlight'>" + replacer[1] + "</span>");
                // 태그가 붙은 replacedPreview는 2번째 이후 변환값에 대해 문제가 생길 가능성이 있어 보임
                // 뜯어고쳐야 함... 이런 식으로 하지 말고 모든 변환 위치를 기억해둬야 할 것 같음...
                // 아니 근데 솔직히 변환 기능이랑 별개로 그냥 미리보기 하나 만들자고 구조 뜯어고치는 것도 뻘짓 같은데
            }

            // return string[4]: 원본 / 원본 미리보기 / 결과 / 결과 미리보기
            return new string[] { source, sourcePreview, replaced, replacedPreview };
        }

        // 선택한 파일 미리보기
        public void ShowPreview(string file)
        {
            string[] replaced = GetReplaced(file, GetReplacers(GetReplacersJson()));
            Script("showPreview", new object[] { replaced[1], replaced[3] });
        }

        // 변환 및 저장
        public void DoReplace()
        {
            string replacersJson = GetReplacersJson();
            string[][] replacers = GetReplacers(replacersJson);
            string filesString = Script("getFiles");
            if (filesString == null)
            {
                Script("alert", new object[] { "파일이 없습니다." });
                return;
            }
            string[] files = filesString.Split('?');
            foreach (string file in files)
            {
                Encoding encoding = BOM.DetectEncoding(file);
                string[] replaced = GetReplaced(file, encoding, replacers);
                StreamWriter sw = null;
                try
                {
                    // 원본 파일의 인코딩대로 저장
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

            // 변환했으면 변환 문자열 쌍을 기억해둠
            SaveDefaultReplacers(replacersJson);

            Script("alert", new object[] { "작업이 완료됐습니다." });
        }
    }
}
