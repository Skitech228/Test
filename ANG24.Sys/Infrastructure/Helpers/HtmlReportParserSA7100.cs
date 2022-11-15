using HtmlAgilityPack;
using System.Diagnostics;

namespace ANG24.Sys.Infrastructure.Helpers
{
    public class HtmlReportParserSA7100
    {
        private List<SA7100Result> table;
        private HtmlDocument doc;
        public HtmlReportParserSA7100()
        {
        }

        public void Load(string path)
        {
            doc = new HtmlDocument();
            try
            {
                doc.Load(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.ToString()}: {ex.Message}");
            }
        }

        public List<SA7100Result> Parse()
        {
            table = new List<SA7100Result>();
            if (doc != null) GetInfoRows(doc.DocumentNode);
            return table;
        }
        private void GetInfoRows(HtmlNode doc)
        {
            if (doc.FirstChild == null) return;
            if (doc.FirstChild.Name == "td" && doc.FirstChild.InnerText != "" && doc.FirstChild.InnerText != "Датавремя ")
            {
                table.Add(new SA7100Result()
                {
                    Date = doc.ChildNodes[0].InnerText,
                    U = doc.ChildNodes[1].InnerText,
                    F = doc.ChildNodes[2].InnerText,
                    N = doc.ChildNodes[3].InnerText,
                    Scheme = doc.ChildNodes[4].InnerText,
                    Cx = doc.ChildNodes[5].InnerText,
                    tgd = doc.ChildNodes[6].InnerText,
                    Sko_cx = doc.ChildNodes[7].InnerText,
                    Sco_tg = doc.ChildNodes[8].InnerText,
                    R = doc.ChildNodes[9].InnerText,
                    T = doc.ChildNodes[10].InnerText,
                    CC = doc.ChildNodes[11].InnerText,
                    DeltaTg = doc.ChildNodes[12].InnerText,
                    Ka = doc.ChildNodes[13].InnerText,
                    R1 = doc.ChildNodes[14].InnerText,
                    R2 = doc.ChildNodes[15].InnerText,
                    Rzo = doc.ChildNodes[16].InnerText,
                    Rzx = doc.ChildNodes[17].InnerText
                });
            }
            else
                foreach (var child in doc.ChildNodes)
                    GetInfoRows(child);
        }

    }

    public class SA7100Result
    {
        public string Date { get; set; }
        public string U { get; set; }
        public string F { get; set; }
        public string N { get; set; }
        public string Scheme { get; set; }
        public string Cx { get; set; }
        public string tgd { get; set; }
        public string Sko_cx { get; set; }
        public string Sco_tg { get; set; }
        public string R { get; set; }
        public string T { get; set; } //???
        public string CC { get; set; }
        public string DeltaTg { get; set; }
        public string Ka { get; set; }
        public string R1 { get; set; }
        public string R2 { get; set; }
        public string Rzo { get; set; }
        public string Rzx { get; set; }
    }
}
