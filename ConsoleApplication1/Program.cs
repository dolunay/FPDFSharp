using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FPDFSharp;

namespace ConsoleApplication1
{
    public class tut2 : PDF
    {
        public override void Header()
        {
            this.Image("D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\logo.png", 10, 6, 30);
            this.SetFont("Arial", "B", 15);
            this.Cell(80);
            this.Cell(30, 10, "Title", "1", 0, "C");
            this.Ln(20);
        }

        public override void Footer()
        {
            this.SetY(-15);
            this.SetFont("Arial", "I", 8);
            this.Cell(0, 10, "Page " + this.PageNo() + "/{nb}", "0", 0, "C");
        }
    }

    public class tut3 : PDF
    {
        public override void Header()
        {
            this.SetFont("Arial", "B", 15);
            var w = this.GetStringWidth(this.Title) + 6;
            this.SetX((210-w)/2);
            this.SetDrawColor(0,80,180);
            this.SetFillColor(230,230,0);
            this.SetTextColor(220,50,50);
            this.SetLineWidth(1);
            this.Cell(w, 9, Title, "1", 1, "C", true);
            this.Ln(10);
        }

        public override void Footer()
        {
            SetY(-15);
            SetFont("Arial", "I", 8);
            SetTextColor(128);
            Cell(0,10,"Page " + this.PageNo(), "0", 0, "C");
        }

        public void ChapterTitle(int pageno, string label)
        {
            SetFont("Arial", "", 12);
            SetFillColor(200,220,255);
            Cell(0,6,"Chapter " + pageno + " : " + label, "0", 1, "L", true);
            Ln(4);
        }

        public void ChapterBody(string file)
        {
            //var txt = "sadjfhs kjdhfks jdhfk jshdjkf hskljdfs  wef.";
            var txt = System.IO.File.ReadAllText(file);
            SetFont("Times", "", 12);
            MultiCell(0, 5, txt);
            Ln();
            SetFont("", "I");
            Cell(0,5,"(end of excerpt)");
        }

        public void PrintChapter(int num, string title, string file)
        {
            AddPage();
            ChapterTitle(num, title);
            ChapterBody(file);
        }
    }

    public class tut4 : PDF
    {
        private int _col = 0;
        private double y0;

        public override void Header()
        {
            this.SetFont("Arial", "B", 15);
            var w = this.GetStringWidth(this.Title) + 6;
            this.SetX((210-w)/2);
            this.SetDrawColor(0,80,180);
            this.SetFillColor(230,230,0);
            this.SetTextColor(220,50,50);
            this.SetLineWidth(1);
            this.Cell(w,9,this.Title,"1",1, "C", true);
            this.Ln(10);
            this.y0 = this.GetY();
        }

        public override void Footer()
        {
            this.SetY(-15);
            this.SetFont("Arial", "I", 8);
            this.SetTextColor(128);
            this.Cell(0,10,"Page " + this.PageNo(),"0",0,"C");
        }

        public void SetCol(int col)
        {
            this._col = col;
            var x = 10 + col*65;
            this.SetLeftMargin(x);
            this.SetX(x);
        }

        public override bool AcceptPageBreak()
        {
            if (this._col < 2)
            {
                this.SetCol(this._col + 1);
                this.SetY(this.y0);
                return false;
            }
            else
            {
                this.SetCol(0);
                return true;
            }
        }

        public void ChapterTitle(int num, string label)
        {
            this.SetFont("Arial", "", 12);
            this.SetFillColor(200,220,255);
            this.Cell(0, 6, "Chapter " + num + " : " + label, "0", 1, "L", true);
            this.Ln(4);
            this.y0 = this.GetY();
        }

        public void ChapterBody(string file)
        {
            var txt = System.IO.File.ReadAllText(file);
            SetFont("Times", "", 12);
            MultiCell(60, 5, txt);
            Ln();
            SetFont("", "I");
            Cell(0, 5, "(end of excerpt)");
            this.SetCol(0);
        }

        public void PrintChapter(int num, string title, string file)
        {
            this.AddPage();
            this.ChapterTitle(num, title);
            this.ChapterBody(file);
        }
    }

    public class tut5 : PDF
    {
        public List<string[]> LoadData(string file)
        {
            var fileStream = new System.IO.StreamReader(file);
            var data = new List<string[]>();
            string line;
            while ((line = fileStream.ReadLine()) != null)
            {
                data.Add(line.Trim().Split(';'));
            }
            return data;
        }

        public void BasicTable(List<string> header, List<string[]> data)
        {
            foreach (var col in header)
            {
                this.Cell(40,7,col,"1");
            }
            this.Ln();
            foreach (var row in data)
            {
                foreach (var col in row)
                {
                    this.Cell(40,6,col,"1");
                }
                this.Ln();
            }
        }

        public void ImprovedTable(List<string> header, List<string[]> data)
        {
            var w = new List<int> {40, 35, 40, 45};
            for (var i = 0; i < header.Count; i++)
            {
                this.Cell(w[i],7,header[i],"1", 0, "C");
            }
            this.Ln();
            foreach (var row in data)
            {
                this.Cell(w[0], 6, row[0], "LR");
                this.Cell(w[1], 6, row[1], "LR");
                this.Cell(w[2], 6, row[2], "LR", 0, "R");
                this.Cell(w[3], 6, row[3], "LR", 0, "R");
                this.Ln();
            }
            this.Cell(w.Sum(), 0, "", "T");
        }

        public void FancyTable(List<string> header, List<string[]> data)
        {
            this.SetFillColor(255,0,0);
            this.SetTextColor(255);
            this.SetDrawColor(128,0,0);
            this.SetLineWidth((float).3);
            this.SetFont("", "B");
            var w = new List<int> { 40, 35, 40, 45 };
            for (var i = 0; i < header.Count; i++)
            {
                this.Cell(w[i], 7, header[i], "1", 0, "C", true);
            }
            this.Ln();
            this.SetFillColor(224, 235, 255);
            this.SetTextColor(0);
            this.SetFont("");
            var fill = false;
            foreach (var row in data)
            {
                this.Cell(w[0], 6, row[0], "LR", 0, "L", fill);
                this.Cell(w[1], 6, row[1], "LR", 0, "L", fill);
                this.Cell(w[2], 6, row[2], "LR", 0, "R", fill);
                this.Cell(w[3], 6, row[3], "LR", 0, "R", fill);
                this.Ln();
                fill = !fill;
            }
            this.Cell(w.Sum(), 0, "", "T");
        }
    }

    public class tut6 : PDF
    {
        private Dictionary<string, int> styles = new Dictionary<string, int>{{"B", 0},{"I", 0}, {"U", 0}}; 
        private string HREF;

        public void WriteHTML(string html)
        {
            html = html.Replace('\n', ' ');
            var a = Regex.Split(html, @"/<(.*)>/U");
            for (var i = 0; i < a.Length; i++)
            {
                var e = a[i];
                if (i%2 == 0)
                {
                    if (!string.IsNullOrEmpty(this.HREF))
                    {
                        this.PutLink(this.HREF, e);
                    }
                    else
                    {
                        this.Write(5, e);
                    }
                }
                else
                {
                    if (e[0] == '/')
                    {
                        this.CloseTag(e.Substring(1).ToUpper());
                    }
                    else
                    {
                        var a2 = e.Split(' ').ToList();
                        var tag = a2[0];
                        a2.RemoveAt(0);
                        var attr = new Dictionary<string, string>();
                        foreach (var v in a2)
                        {
                            var matches = Regex.Matches(v, "/([^=]*)=[\"\\']?([^\"\\']*)/");
                            attr.Add(matches[1].Value, matches[2].Value);
                        }
                        this.OpenTag(tag, attr);
                    }
                }
            }
        }

        public void OpenTag(string tag, Dictionary<string, string> attr)
        {
            if (tag == "B" || tag == "I" || tag == "U")
            {
                this.SetStyle(tag, true);
            }
            if (tag == "A")
            {
                this.HREF = attr["HREF"];
            }
            if (tag == "BR")
            {
                this.Ln(5);
            }
        }

        public void CloseTag(string tag)
        {
            if (tag == "B" || tag == "I" || tag == "U")
            {
                this.SetStyle(tag, false);
            }
            if(tag == "A")
            {
                this.HREF = "";
            }
        }

        public void SetStyle(string tag, bool enable)
        {
            this.styles[tag] = (enable ? 1 : -1);
            var style = "";
            foreach (var s in new List<string>{"B", "I", "U"})
            {
                if (this.styles[s] > 0)
                    style += s;
            }
            this.SetFont("", style);
        }

        public void PutLink(string url, string txt)
        {
            this.SetTextColor(0,0,255);
            this.SetStyle("U", true);
            this.Write(5, txt, url);
            this.SetStyle("U", false);
            this.SetTextColor(0);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //var tut1 = new FPDFSharp.PDF();
            //tut1.Compress = false;
            //tut1.AddPage();
            //tut1.SetFont("Arial", "B", 16);
            //tut1.Cell(40, 10, "Hello World!");
            //tut1.Output("D:\\fpdfsharptuts\\tut1.pdf");

            //TODO images
            //var tut2 = new tut2();
            //tut2.Compress = false;
            //tut2.AliasNBPages();
            //tut2.AddPage();
            //tut2.SetFont("Times", "", 12);
            //for (var i = 1; i <= 40; i++)
            //{
            //    tut2.Cell(0, 10, "Printing line number " + i, "0", 1);
            //}
            //tut2.Output("D:\\fpdfsharptuts\\tut2.pdf");

            //TODO possible encoding problem otherwise good
            //var tut3 = new tut3();
            //tut3.Compress = false;
            //var title = "20000 Leagues Under the Seas";
            ////tut3.SetTitle(title);
            ////tut3.SetAuthor("Jules Verne");
            ////tut3.PrintChapter(1, "A RUNAWAY REEF", "D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\20k_c1.txt");
            ////tut3.PrintChapter(2, "THE PROS AND CONS", "D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\20k_c2.txt");
            ////tut3.Output("D:\\fpdfsharptuts\\tut3.pdf");

            ////TODO broken columns
            //var tut4 = new tut4();
            //tut4.Compress = false;
            //tut4.SetTitle(title);
            //tut4.SetAuthor("Jules Verne");
            //tut4.PrintChapter(1, "A RUNAWAY REEF", "D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\20k_c1.txt");
            //tut4.PrintChapter(2, "THE PROS AND CONS", "D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\20k_c2.txt");
            //tut4.Output("D:\\fpdfsharptuts\\tut4.pdf");
            //System.Diagnostics.Process.Start("D:\\fpdfsharptuts\\tut4.pdf");
            //todo 
            var tut5 = new tut5();
            tut5.Compress = false;
            var header = new List<string> { "Country", "Capital", "Area (sq km)", "Pop. (thousands)" };
            var data = tut5.LoadData("D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\countries.txt");
            tut5.SetFont("Arial", "", 14);
            tut5.AddPage();
            tut5.BasicTable(header, data);
            tut5.AddPage();
            tut5.ImprovedTable(header, data);
            tut5.AddPage();
            tut5.FancyTable(header, data);
            tut5.Output("D:\\fpdfsharptuts\\tut5.pdf");
            System.Diagnostics.Process.Start("D:\\fpdfsharptuts\\tut5.pdf");
            ////todo link
            //var html = "You can now easily print text mixing different styles: <b>bold</b>, <i>italic</i>, <u>underlined</u>, or <b><i><u>all at once</u></i></b>!<br><br>You can also insert links on text, such as <a href=\"http://www.fpdf.org\">www.fpdf.org</a>, or on an image: click on the logo.";
            //var tut6 = new tut6();
            //tut6.Compress = false;
            //tut6.AddPage();
            //tut6.SetFont("Arial", "", 20);
            //tut6.Write(5, "To find out what's new in this tutorial, click ");
            //tut6.SetFont("", "U");
            //var link = tut6.AddLink();
            //tut6.Write(5, "here", link.ToString());
            //tut6.SetFont("");
            //tut6.AddPage();
            //tut6.SetLink(link);
            //tut6.Image("logo.png", 10, 12, 30, 0, "", "http://www.fpdf.org");
            //tut6.SetLeftMargin(45);
            //tut6.SetFontSize(14);
            //tut6.WriteHTML(html);
            //tut6.Output("D:\\fpdfsharptuts\\tut6.pdf");

            //var s = "D:\\Photos\\photos for big frame\\3.JPG";
            //var bmp = new Bitmap(s);
            //var props = bmp.PropertyItems;
            //var flags = (ImageFlags)bmp.Flags;
            //flags.HasFlag(ImageFlags.ColorSpaceRgb);


            //var pdf = new aPdf();
            //pdf.Compress = false;
            //pdf.AddPage();

            //pdf.SetFont("Arial", "B", 16);
            //pdf.Cell(40, 10, "Hello World!");
            //pdf.Output("D:\\trial1.pdf");







            //var s = "chr(0)=>278,chr(1)=>278,chr(2)=>278,chr(3)=>278,chr(4)=>278,chr(5)=>278,chr(6)=>278,chr(7)=>278,chr(8)=>278,chr(9)=>278,chr(10)=>278,chr(11)=>278,chr(12)=>278,chr(13)=>278,chr(14)=>278,chr(15)=>278,chr(16)=>278,chr(17)=>278,chr(18)=>278,chr(19)=>278,chr(20)=>278,chr(21)=>278,chr(22)=>278,chr(23)=>278,chr(24)=>278,chr(25)=>278,chr(26)=>278,chr(27)=>278,chr(28)=>278,chr(29)=>278,chr(30)=>278,chr(31)=>278,' '=>278,'!'=>278,'\"'=>355,'#'=>556,'$'=>556,'%'=>889,'&'=>667,'\''=>191,'('=>333,')'=>333,'*'=>389,'+'=>584,'comma'=>278,'-'=>333,'.'=>278,'/'=>278,'0'=>556,'1'=>556,'2'=>556,'3'=>556,'4'=>556,'5'=>556,'6'=>556,'7'=>556,'8'=>556,'9'=>556,':'=>278,';'=>278,'<'=>584,'='=>584,'>'=>584,'?'=>556,'@'=>1015,'A'=>667,'B'=>667,'C'=>722,'D'=>722,'E'=>667,'F'=>611,'G'=>778,'H'=>722,'I'=>278,'J'=>500,'K'=>667,'L'=>556,'M'=>833,'N'=>722,'O'=>778,'P'=>667,'Q'=>778,'R'=>722,'S'=>667,'T'=>611,'U'=>722,'V'=>667,'W'=>944,'X'=>667,'Y'=>667,'Z'=>611,'['=>278,'\\'=>278,']'=>278,'^'=>469,'_'=>556,'`'=>333,'a'=>556,'b'=>556,'c'=>500,'d'=>556,'e'=>556,'f'=>278,'g'=>556,'h'=>556,'i'=>222,'j'=>222,'k'=>500,'l'=>222,'m'=>833,'n'=>556,'o'=>556,'p'=>556,'q'=>556,'r'=>333,'s'=>500,'t'=>278,'u'=>556,'v'=>500,'w'=>722,'x'=>500,'y'=>500,'z'=>500,'{'=>334,'|'=>260,'}'=>334,'~'=>584,chr(127)=>350,chr(128)=>556,chr(129)=>350,chr(130)=>222,chr(131)=>556,chr(132)=>333,chr(133)=>1000,chr(134)=>556,chr(135)=>556,chr(136)=>333,chr(137)=>1000,chr(138)=>667,chr(139)=>333,chr(140)=>1000,chr(141)=>350,chr(142)=>611,chr(143)=>350,chr(144)=>350,chr(145)=>222,chr(146)=>222,chr(147)=>333,chr(148)=>333,chr(149)=>350,chr(150)=>556,chr(151)=>1000,chr(152)=>333,chr(153)=>1000,chr(154)=>500,chr(155)=>333,chr(156)=>944,chr(157)=>350,chr(158)=>500,chr(159)=>667,chr(160)=>278,chr(161)=>333,chr(162)=>556,chr(163)=>556,chr(164)=>556,chr(165)=>556,chr(166)=>260,chr(167)=>556,chr(168)=>333,chr(169)=>737,chr(170)=>370,chr(171)=>556,chr(172)=>584,chr(173)=>333,chr(174)=>737,chr(175)=>333,chr(176)=>400,chr(177)=>584,chr(178)=>333,chr(179)=>333,chr(180)=>333,chr(181)=>556,chr(182)=>537,chr(183)=>278,chr(184)=>333,chr(185)=>333,chr(186)=>365,chr(187)=>556,chr(188)=>834,chr(189)=>834,chr(190)=>834,chr(191)=>611,chr(192)=>667,chr(193)=>667,chr(194)=>667,chr(195)=>667,chr(196)=>667,chr(197)=>667,chr(198)=>1000,chr(199)=>722,chr(200)=>667,chr(201)=>667,chr(202)=>667,chr(203)=>667,chr(204)=>278,chr(205)=>278,chr(206)=>278,chr(207)=>278,chr(208)=>722,chr(209)=>722,chr(210)=>778,chr(211)=>778,chr(212)=>778,chr(213)=>778,chr(214)=>778,chr(215)=>584,chr(216)=>778,chr(217)=>722,chr(218)=>722,chr(219)=>722,chr(220)=>722,chr(221)=>667,chr(222)=>667,chr(223)=>611,chr(224)=>556,chr(225)=>556,chr(226)=>556,chr(227)=>556,chr(228)=>556,chr(229)=>556,chr(230)=>889,chr(231)=>500,chr(232)=>556,chr(233)=>556,chr(234)=>556,chr(235)=>556,chr(236)=>278,chr(237)=>278,chr(238)=>278,chr(239)=>278,chr(240)=>556,chr(241)=>556,chr(242)=>556,chr(243)=>556,chr(244)=>556,chr(245)=>556,chr(246)=>556,chr(247)=>584,chr(248)=>611,chr(249)=>556,chr(250)=>556,chr(251)=>556,chr(252)=>556,chr(253)=>500,chr(254)=>556,chr(255)=>500";
            //var entries = s.Split(',');
            //var sb = new StringBuilder();
            //foreach (var entry in entries)
            //{
            //    var bits = entry.Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
            //    var number = bits[1];
            //    var preNumber = bits[0];
            //    //f.cw.Add((char)0, 278);
            //    //f.cw.Add('$', 556);

            //    if (preNumber.Contains("chr"))
            //    {
            //        preNumber = preNumber.Replace(")", "").Replace("chr(", "(char)");
            //    }
            //    sb.AppendLine(string.Format("f.cw.Add({0}, {1});", preNumber, number));
            //}
            //System.IO.File.WriteAllText("D:\\helvetica.txt", sb.ToString());



            //Console.ReadLine();
        }
    }
}
