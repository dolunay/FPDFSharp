using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DiffMatchPatch;
using FluentAssertions;
using FPDFSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FPDFSharpTests
{
    [TestClass]
    public class TutorialTests
    {

        private static readonly string BaseDirectory = "D:\\Projects\\FPDFSharp\\FPDFSharpTests";

        [TestMethod]
        public void Tutorial1()
        {
           
            var PHPWrittenContentFile = Path.Combine(BaseDirectory, "fixtures", "tutorials", "fpdf", "tutorial1.pdf");

            var expectedPDFContents = DateFix(File.ReadAllText(PHPWrittenContentFile));
            
            var outputFile = Path.GetTempFileName();
            
            var tut1 = new FPDFSharp.PDF();
            tut1.Compress = false;
            tut1.AddPage();
            tut1.SetFont("Arial", "B", 16);
            tut1.Cell(40, 10, "Hello World!");
            tut1.Output(outputFile);
            OutputDiff(DateFix(File.ReadAllText(outputFile).Trim()), expectedPDFContents.Trim(), "tut1");
            DateFix(File.ReadAllText(outputFile).Trim()).Should().Be(expectedPDFContents.Trim());
        }

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
                this.SetX((210 - w) / 2);
                this.SetDrawColor(0, 80, 180);
                this.SetFillColor(230, 230, 0);
                this.SetTextColor(220, 50, 50);
                this.SetLineWidth(1);
                this.Cell(w, 9, Title, "1", 1, "C", true);
                this.Ln(10);
            }

            public override void Footer()
            {
                SetY(-15);
                SetFont("Arial", "I", 8);
                SetTextColor(128);
                Cell(0, 10, "Page " + this.PageNo(), "0", 0, "C");
            }

            public void ChapterTitle(int pageno, string label)
            {
                SetFont("Arial", "", 12);
                SetFillColor(200, 220, 255);
                Cell(0, 6, "Chapter " + pageno + " : " + label, "0", 1, "L", true);
                Ln(4);
            }

            public void ChapterBody(string file)
            {
                var txt = System.IO.File.ReadAllText(file);
                SetFont("Times", "", 12);
                MultiCell(0, 5, txt);
                Ln();
                SetFont("", "I");
                Cell(0, 5, "(end of excerpt)");
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
                this.SetX((210 - w) / 2);
                this.SetDrawColor(0, 80, 180);
                this.SetFillColor(230, 230, 0);
                this.SetTextColor(220, 50, 50);
                this.SetLineWidth(1);
                this.Cell(w, 9, this.Title, "1", 1, "C", true);
                this.Ln(10);
                this.y0 = this.GetY();
            }

            public override void Footer()
            {
                this.SetY(-15);
                this.SetFont("Arial", "I", 8);
                this.SetTextColor(128);
                this.Cell(0, 10, "Page " + this.PageNo(), "0", 0, "C");
            }

            public void SetCol(int col)
            {
                this._col = col;
                var x = 10 + col * 65;
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
                this.SetFillColor(200, 220, 255);
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
                    this.Cell(40, 7, col, "1");
                }
                this.Ln();
                foreach (var row in data)
                {
                    foreach (var col in row)
                    {
                        this.Cell(40, 6, col, "1");
                    }
                    this.Ln();
                }
            }

            public void ImprovedTable(List<string> header, List<string[]> data)
            {
                var w = new List<int> { 40, 35, 40, 45 };
                for (var i = 0; i < header.Count; i++)
                {
                    this.Cell(w[i], 7, header[i], "1", 0, "C");
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
                this.SetFillColor(255, 0, 0);
                this.SetTextColor(255);
                this.SetDrawColor(128, 0, 0);
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
            private Dictionary<string, int> styles = new Dictionary<string, int> { { "B", 0 }, { "I", 0 }, { "U", 0 } };
            private string HREF;

            public void WriteHTML(string html)
            {
                html = html.Replace('\n', ' ');
                var a = Regex.Split(html, @"/<(.*)>/U");
                for (var i = 0; i < a.Length; i++)
                {
                    var e = a[i];
                    if (i % 2 == 0)
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
                if (tag == "A")
                {
                    this.HREF = "";
                }
            }

            public void SetStyle(string tag, bool enable)
            {
                this.styles[tag] = (enable ? 1 : -1);
                var style = "";
                foreach (var s in new List<string> { "B", "I", "U" })
                {
                    if (this.styles[s] > 0)
                        style += s;
                }
                this.SetFont("", style);
            }

            public void PutLink(string url, string txt)
            {
                this.SetTextColor(0, 0, 255);
                this.SetStyle("U", true);
                this.Write(5, txt, url);
                this.SetStyle("U", false);
                this.SetTextColor(0);
            }
        }

        [TestMethod]
        public void Tutorial2()
        {
            var PHPWrittenContentFile = Path.Combine(BaseDirectory, "fixtures", "tutorials", "fpdf", "tutorial2.pdf");

            var expectedPDFContents = DateFix(File.ReadAllText(PHPWrittenContentFile));

            var outputFile = Path.GetTempFileName();

            var tut2 = new tut2();
            tut2.Compress = false;
            tut2.AliasNBPages();
            tut2.AddPage();
            tut2.SetFont("Times", "", 12);
            for (var i = 1; i <= 40; i++)
            {
                tut2.Cell(0, 10, "Printing line number " + i, "0", 1);
            }
            tut2.Output(outputFile);
            OutputDiff(DateFix(File.ReadAllText(outputFile).Trim()), expectedPDFContents.Trim(), "tut2");
            DateFix(File.ReadAllText(outputFile).Trim()).Should().Be(expectedPDFContents.Trim());
        }


        [TestMethod]
        public void Tutorial3()
        {
            var PHPWrittenContentFile = Path.Combine(BaseDirectory, "fixtures", "tutorials", "fpdf", "tutorial3.pdf");

            var expectedPDFContents = DateFix(File.ReadAllText(PHPWrittenContentFile));

            var outputFile = Path.GetTempFileName();

            var tut3 = new tut3();
            tut3.Compress = false;
            var title = "20000 Leagues Under the Seas";
            tut3.SetTitle(title);
            tut3.SetAuthor("Jules Verne");
            tut3.PrintChapter(1, "A RUNAWAY REEF", "D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\20k_c1.txt");
            tut3.PrintChapter(2, "THE PROS AND CONS", "D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\20k_c2.txt");
            tut3.Output(outputFile);
            OutputDiff(DateFix(File.ReadAllText(outputFile).Trim()), expectedPDFContents.Trim(), "tut3");
            DateFix(File.ReadAllText(outputFile).Trim()).Should().Be(expectedPDFContents.Trim());
        }


        [TestMethod]
        public void Tutorial4()
        {
            var PHPWrittenContentFile = Path.Combine(BaseDirectory, "fixtures", "tutorials", "fpdf", "tutorial4.pdf");

            var expectedPDFContents = DateFix(File.ReadAllText(PHPWrittenContentFile));

            var outputFile = Path.GetTempFileName() + ".pdf";

            var tut4 = new tut4();
            tut4.Compress = false;
            var title = "20000 Leagues Under the Seas";
            tut4.SetTitle(title);
            tut4.SetAuthor("Jules Verne");
            tut4.PrintChapter(1, "A RUNAWAY REEF", "D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\20k_c1.txt");
            tut4.PrintChapter(2, "THE PROS AND CONS", "D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\20k_c2.txt");
            tut4.Output(outputFile);
            //OutputDiff(DateFix(File.ReadAllText(outputFile).Trim()), expectedPDFContents.Trim(), "tut4");
            //System.Diagnostics.Process.Start(outputFile);
            DateFix(File.ReadAllText(outputFile).Trim()).Should().Be(expectedPDFContents.Trim());
        }


        [TestMethod]
        public void Tutorial5()
        {
            var PHPWrittenContentFile = Path.Combine(BaseDirectory, "fixtures", "tutorials", "fpdf", "tutorial5.pdf");

            var expectedPDFContents = DateFix(File.ReadAllText(PHPWrittenContentFile));

            var outputFile = Path.GetTempFileName();

            var tut5 = new tut5();

            var header = new List<string>
            {
                "Country",
                "Capital",
                "Area (sq km)",
                "Pop. (thousands)"
            };

            tut5.Compress = false;
            var data = tut5.LoadData("D:\\Projects\\FPHPTest\\FPHPTest\\tutorial\\countries.txt");
            tut5.SetFont("Arial", "", 14);
            tut5.AddPage();
            tut5.BasicTable(header, data);
            tut5.AddPage();
            tut5.ImprovedTable(header, data);
            tut5.AddPage();
            tut5.FancyTable(header, data);
            tut5.Output(outputFile);
            //OutputDiff(DateFix(File.ReadAllText(outputFile).Trim()), expectedPDFContents.Trim(), "tut5");
            //System.Diagnostics.Process.Start(outputFile);
            DateFix(File.ReadAllText(outputFile).Trim()).Should().Be(expectedPDFContents.Trim());
        }

        [TestMethod]
        public void Tutorial6()
        {
            var PHPWrittenContentFile = Path.Combine(BaseDirectory, "fixtures", "tutorials", "fpdf", "tutorial6.pdf");

            var expectedPDFContents = DateFix(File.ReadAllText(PHPWrittenContentFile));

            var outputFile = Path.GetTempFileName();

            var tut6 = new tut6();

            var html = "You can now easily print text mixing different styles: <b>bold</b>, <i>italic</i>, <u>underlined</u>, or <b><i><u>all at once</u></i></b>!<br><br>You can also insert links on text, such as <a href=\"http://www.fpdf.org\">www.fpdf.org</a>, or on an image: click on the logo.";

            tut6.Compress = false;
            tut6.AddPage();
            tut6.SetFont("Arial", "", 20);
            tut6.Write(5, "To find out what's new in this tutorial, click ");
            tut6.SetFont("", "U");
            var link = tut6.AddLink();
            tut6.Write(5, "here", link.ToString());
            tut6.SetFont("");
            tut6.AddPage();
            tut6.SetLink(link);
            tut6.Image("logo.png", 10,12,30,0,"", "http://www.fpdf.org");
            tut6.SetLeftMargin(45);
            tut6.SetFontSize(14);
            tut6.WriteHTML(html);
            tut6.Output(outputFile);
            OutputDiff(DateFix(File.ReadAllText(outputFile).Trim()), expectedPDFContents.Trim(), "tut6");
            DateFix(File.ReadAllText(outputFile).Trim()).Should().Be(expectedPDFContents.Trim());
        }

        //this method replaces the date field which will nessacerily be different from the stored PDFs
        private string DateFix(string s)
        {
            var dateRegex = new Regex("D:[0-9]*");
            return dateRegex.Replace(s, "XXX");
        }

        private void OutputDiff(string a, string b, string title)
        {
            //var diff = new DiffMatchPatch.diff_match_patch();
            //var diffs = diff.diff_main(a, b);
            var html = a + Environment.NewLine + "=========================================" + Environment.NewLine + b;
            //D:\\fpdfsharptuts\\outputs\\
            File.WriteAllText("D:\\fpdfsharptuts\\outputs\\" + title + ".txt", html);
        }
    }
}
