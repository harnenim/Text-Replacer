using System.Text;
using System.IO;

namespace TextFile
{
    class BOM
    {
        public char[] bom;
        public Encoding encoding;
        public BOM(Encoding encoding, char[] bom)
        {
            this.bom = bom;
            this.encoding = encoding;
        }

        public static char[] BOM_UTF7 = { (char)0x2b, (char)0x2f, (char)0x76 };
        public static char[] BOM_UTF8 = { (char)0xef, (char)0xbb, (char)0xbf };
        public static char[] BOM_UTF16LE = { (char)0xff, (char)0xfe };
        public static char[] BOM_UTF16BE = { (char)0xfe, (char)0xff };
        public static char[] BOM_UTF32 = { (char)0x00, (char)0x00, (char)0xfe, (char)0xff };

        private static BOM[] boms = {
                new BOM(Encoding.UTF7, BOM_UTF7)
            ,   new BOM(Encoding.UTF8, BOM_UTF8)
            ,   new BOM(Encoding.Unicode, BOM_UTF16LE)
            ,   new BOM(Encoding.BigEndianUnicode, BOM_UTF16BE)
            ,   new BOM(Encoding.UTF32, BOM_UTF32)
        };

        public static Encoding DetectEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                fs.Read(bom, 0, 4);
                fs.Close();
            }

            // Analyze the BOM
            foreach (BOM b in boms)
            {
                bool isFailed = false;
                for (int i = 0; i < b.bom.Length; i++)
                {
                    if (b.bom[i] != bom[i])
                    {
                        isFailed = true;
                        break;
                    }
                }
                if (!isFailed)
                    return b.encoding;
            }
            return Encoding.Default;
        }
    }
}
