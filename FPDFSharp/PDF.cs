using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;

namespace FPDFSharp
{
    public class Image
    {
        public int w { get; set; }
        public int h { get; set; }
        public string cs { get; set; }
        public int bpc { get; set; }
        public string f { get; set; }
        public string dp { get; set; }
        public string pal { get; set; }
        public List<int> trns { get; set; }
        public string data { get; set; }
        public int i { get; set; }
        public int n { get; set; }
    }
    public class PdfFont
    {
        public string Font { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Up { get; set; }
        public int Ut { get; set; }
        public Dictionary<char, int> cw { get; set; }
        public int i { get; set; }
        public int n { get; set; }
    }

    public class PageLink
    {
        public double x { get; set; }
        public double y { get; set; }
        public double w { get; set; }
        public double h { get; set; }
        public string Link { get; set; }
    }

    public class Link
    {
        public int Page { get; set; }
        public double Y { get; set; }
    }

    public class PDF
    {
        private const string FPDF_VERSION = "1.7";
        private int Page { get; set; }
        private int n { get; set; }
        private Dictionary<int, int> Offsets { get; set; }
        private string Buffer { get; set; }
        private Dictionary<int, string> Pages { get; set; }
        private int State { get; set; }
        public bool Compress { get; set; }
        private double k { get; set; }
        private Orientation DefOrientation { get; set; }
        private Orientation CurOrientation { get; set; }
        private Dictionary<PageSize, PageDimensions> StdPageSizes { get; set; }
        private PageDimensions DefPageSize { get; set; }
        private PageDimensions CurPageSize { get; set; }
        private List<PageDimensions> PageSizes { get; set; } // TODO replace this with height and width to avoid lonside shortside problem
        private double wPt { get; set; }
        private double hPt { get; set; }
        private double w { get; set; }
        private double h { get; set; }
        private double lMargin { get; set; }
        private double tMargin { get; set; }
        private double rMargin { get; set; }
        private double bMargin { get; set; }
        private double cMargin { get; set; }
        private double x { get; set; }
        private double y { get; set; }
        private int lasth { get; set; }
        private double LineWidth { get; set; }
        private string fontpath { get; set; }
        private List<string> CoreFonts { get; set; }
        private Dictionary<string, PdfFont> fonts { get; set; }
        private Dictionary<string, PdfFont> FontFiles { get; set; }
        private List<int> diffs { get; set; }
        private string FontFamily { get; set; }
        private string FontStyle { get; set; }
        private bool Underline { get; set; }
        private PdfFont CurrentFont { get; set; }
        private int FontSizePt { get; set; }
        private float FontSize { get; set; }
        private string DrawColor { get; set; }
        private string FillColor { get; set; }
        private string TextColor { get; set; }
        private bool ColorFlag { get; set; }
        private float ws { get; set; }
        private Dictionary<string, ImageInformation> images { get; set; }
        private Dictionary<int, PageLink> PageLinks { get; set; }
        private List<Link> links { get; set; }
        private bool AutoPageBreak { get; set; }
        private double PageBreakTrigger { get; set; }
        private bool InHeader { get; set; }
        private bool InFooter { get; set; }
        private ZoomLevel ZoomMode { get; set; }
        private Layout LayoutMode { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Author { get; set; }
        public string Keywords { get; set; }
        public string Creator { get; set; }
        private string AliasNbPages { get; set; }
        private double PDFVersion { get; set; }

        public enum Unit
        {
            Points = 1,
            Millimetres = 2,
            Centimetres = 3,
            Inches = 4
        }

        public enum Orientation{
            Portrait = 1,
            Landscape = 2
        }

        public enum PageSize
        {
            A3 = 1,
            A4 = 2,
            A5 = 3,
            Letter = 4,
            Legal = 5
        }

        public enum ZoomLevel
        {
            FullPage = 1,
            FullWidth = 2,
            Real = 3,
            Default = 4
        }

        public enum Layout
        {
            Single = 1,
            Continuous = 2,
            Two = 3,
            Default = 4
        }

        private void Reset()
        {
            this.Page = 0;
            this.n = 2;
            this.Buffer = "";
            this.Pages = new Dictionary<int, string>();
            this.PageSizes = new List<PageDimensions>();
            this.InHeader = false;
            this.InFooter = false;
            this.fonts = new Dictionary<string, PdfFont>();
            this.lasth = 0;
            this.FontFamily = "";
            this.FontStyle = "";
            this.FontSizePt = 12;
            this.FontFiles = new Dictionary<string, PdfFont>();
            this.Offsets = new Dictionary<int, int>();
            this.images = new Dictionary<string, ImageInformation>();
            this.PageLinks = new Dictionary<int, PageLink>();
            this.links = new List<Link>();
            this.Underline = false;
            this.DrawColor = "0 G";
            this.FillColor = "0 g";
            this.TextColor = "0 g";
            this.ColorFlag = false;
            this.ws = 0;
            this.diffs = new List<int>();
            this.StdPageSizes = new Dictionary<PageSize, PageDimensions>
            {
                {PageSize.A3, new PageDimensions{ ShortSide = 841.89, LongSide = 1190.55 }},
                {PageSize.A4, new PageDimensions{ ShortSide = 595.28, LongSide = 841.89 }},
                {PageSize.A5, new PageDimensions{ ShortSide = 420.94, LongSide = 595.28 }},
                {PageSize.Letter, new PageDimensions{ ShortSide = 612, LongSide = 792 }},
                {PageSize.Legal, new PageDimensions{ ShortSide = 612, LongSide = 1008 }}
            };
            this.CoreFonts = new List<string>
            {
                "courier",
                "helvetica",
                "times",
                "symbol",
                "zapfdingbats"
            };
        }

        public class PageDimensions
        {
            public double LongSide { get; set; }
            public double ShortSide { get; set; }
        }

        public PDF(Orientation orientation = Orientation.Portrait, Unit unit = Unit.Millimetres,
            PageSize pageSize = PageSize.A4)
        {
            this.Reset();
            switch (unit)
            {
                case Unit.Points:
                    this.k = 1;
                    break;
                case Unit.Millimetres:
                    this.k = (72 / 25.4);
                    break;
                case Unit.Centimetres:
                    this.k = (72 / 2.54);
                    break;
                case Unit.Inches:
                    this.k = 72;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unit");
            }
            var size = this.getPageSize(pageSize);
            this.DefPageSize = size;
            this.CurPageSize = size;
            this.DefOrientation = orientation;
            switch (orientation)
            {
                case Orientation.Portrait:
                    this.w = size.ShortSide;
                    this.h = size.LongSide;
                    break;
                case Orientation.Landscape:
                    this.w = size.LongSide;
                    this.h = size.ShortSide;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orientation");
            }
            this.CurOrientation = this.DefOrientation;
            this.wPt = this.w * this.k;
            this.hPt = this.h * this.k;
            double margin = (28.35 / this.k);
            this.SetMargins(margin, margin);
            this.cMargin = (margin / 10);
            this.LineWidth = (0.567 / this.k);
            this.SetAutopPageBreak(true, 2 * margin);
            this.Compress = true;
            this.PDFVersion = 1.3;
        }

        private PageDimensions getPageSize(PageSize pageSize)
        {
            if(!this.StdPageSizes.ContainsKey(pageSize))
                throw new Exception("Uknown page size: " + pageSize);
            var size = this.StdPageSizes[pageSize];
            return new PageDimensions
            {
                LongSide = size.LongSide/this.k,
                ShortSide = size.ShortSide/this.k
            };
        }

        public void SetMargins(double left, double top, double right = -1)
        {
            this.lMargin = left;
            this.tMargin = top;
            if (right == -1)
                right = left;
            this.rMargin = right;
        }

        public void SetTopMargin(double margin)
        {
            this.tMargin = margin;
        }

        public void SetLeftMargin(double margin)
        {
            this.lMargin = margin;
            if (this.Page > 0 && this.x < margin)
                this.x = margin;
        }

        public void SetRightMargin(double margin)
        {
            this.rMargin = margin;
        }

        public void SetAutopPageBreak(bool auto, double margin)
        {
            this.AutoPageBreak = auto;
            this.bMargin = margin;
            this.PageBreakTrigger = this.h - margin;
        }

        public void SetDisplayMode(ZoomLevel zoom, Layout layout = Layout.Default)
        {
            ZoomMode = zoom;
            LayoutMode = layout;
        }

        public void SetTitle(string title, bool isUtf8 = false)
        {
            if (isUtf8)
                title = System.Text.Encoding.UTF8.GetString(GetBytes(title));
            Title = title;
        }
        public void SetSubject(string subject, bool isUtf8 = false)
        {
            if (isUtf8)
                subject = System.Text.Encoding.UTF8.GetString(GetBytes(subject));
            Subject = subject;
        }
        public void SetAuthor(string author, bool isUtf8 = false)
        {
            if (isUtf8)
                author = System.Text.Encoding.UTF8.GetString(GetBytes(author));
            Author = author;
        }
        public void SetKeywords(string keywords, bool isUtf8 = false)
        {
            if (isUtf8)
                keywords = System.Text.Encoding.UTF8.GetString(GetBytes(keywords));
            Keywords = keywords;
        }
        public void SetCreator(string creator, bool isUtf8 = false)
        {
            if (isUtf8)
                creator = System.Text.Encoding.UTF8.GetString(GetBytes(creator));
            Creator = creator;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public void AliasNBPages(string alias = "{nb}")
        {
            AliasNbPages = alias;
        }

        public void Open()
        {
            State = 1;
        }

        public void Close()
        {
            if (State == 3)
                return;
            if (Page == 0)
                AddPage();
            InFooter = true;
            Footer();
            InFooter = false;
            EndPage();
            EndDoc();
        }

        public void AddPage(Orientation orientation = Orientation.Portrait, PageDimensions size = null)
        {
           if(State == 0)
               Open();
            var family = FontFamily;
            var style = FontStyle + (Underline ? "u" : "");
            var fontSize = FontSizePt;
            var lw = LineWidth;
            var dc = DrawColor;
            var fc = FillColor;
            var tc = TextColor;
            var cf = ColorFlag;
            if (Page > 0)
            {
                InFooter = true;
                Footer();
                InFooter = false;
                EndPage();
            }
            BeginPage(orientation, size);
            Out("2 J");
            LineWidth = lw;
            Out(string.Format("{0:0.00} w", lw * k));
            if (family != "")
                SetFont(family, style, fontSize);
            DrawColor = dc;
            if (dc != "0 G")
                Out(dc);
            FillColor = fc;
            if (fc != "0 g")
                Out(fc);
            TextColor = tc;
            ColorFlag = cf;
            InHeader = true;
            Header();
            InHeader = false;
            if (LineWidth != lw)
            {
                LineWidth = lw;
                Out(string.Format("{0:0.00} w", lw * k));
            }
            if (family != "")
                SetFont(family, style, fontSize);
            if (DrawColor != dc)
            {
                DrawColor = dc;
                Out(dc);
            }
            if (FillColor != fc)
            {
                FillColor = fc;
                Out(fc);
            }
            TextColor = tc;
            ColorFlag = cf;
        }

        public virtual void Header() { }
        public virtual void Footer() { }

        public int PageNo()
        {
            return Page;
        }

        public void SetDrawColor(double r, double g = -1, double b = -1)
        {
            if ((r == 0 && g == 0 && b == 0) || g == -1)
            {
                DrawColor = string.Format("{0:0.000}", r / 255);
            }
            else
            {
                DrawColor = string.Format("{0:0.000} {1:0.000} {2:0.000} RG", r / 255, g / 255, b / 255);
            }
            if(Page > 0)
                Out(DrawColor);
        }

        public void SetFillColor(double r, double g = -1, double b = -1)
        {
            if ((r == 0 && g == 0 && b == 0) || g == -1)
            {
                FillColor = string.Format("{0:0.000}", r / 255);
            }
            else
            {
                FillColor = string.Format("{0:0.000} {1:0.000} {2:0.000} rg", r / 255, g / 255, b / 255);
            }
            this.ColorFlag = (this.FillColor != this.TextColor);
            if (Page > 0)
                Out(FillColor);
        }

        public void SetTextColor(double r, double g = -1, double b = -1)
        {
            if ((r == 0 && g == 0 && b == 0) || g == -1)
            {
                TextColor = string.Format("{0:0.000} g", r / 255);
            }
            else
            {
                TextColor = string.Format("{0:0.000} {1:0.000} {2:0.000} rg", r / 255, g / 255, b / 255);
            }
            //if (Page > 0)
            //    Out(TextColor);
            this.ColorFlag = FillColor != TextColor;
        }

        public float GetStringWidth(string s)
        {
            var cw = CurrentFont.cw;
            var w = 0;
            var l = s.Length;
            for (var i = 0; i < s.Length; i++)
            {
                w += cw.ContainsKey(s[i]) ? cw[s[i]] : cw['?'];
            }
            return w*FontSize/1000;
        }

        public void SetLineWidth(float width)
        {
            LineWidth = width;
            if (Page > 0)
                Out(string.Format("{0:0.00} w", width * k));
        }

        public void Line(double x1, double y1, double x2, double y2)
        {
            Out(string.Format("{0:0.00} {1:0.00} m {2:0.00} {3:0.00} l S", x1 * k, (h - y1) * k, x2 * k, (h - y2) * k));
        }

        public void Rect(double x, double y, double w, double h, string style = "")
        {
            var op = "S";
            if (style == "F")
                op = "f";
            else if (style == "FD" || style == "DF")
                op = "B";
            Out(string.Format("{0:0.00} {1:0.00} {2:0.00} {3:0.00} re {4}", x * k, (h - y) * k, w * k, (0 - h) * k, op));
        }

        public void AddFont(string family, string style = "", string file = "")
        {
            family = family.ToLower();
            //if (string.IsNullOrEmpty(file))
            //    file = family.Replace(" ", "") + style.ToLower() + ".php";
            style = style.ToUpper();
            if (style == "IB")
                style = "BI";
            var fontKey = (family + style).ToLower();
            if(fonts.ContainsKey(fontKey))
                return;
            var info = LoadFont(fontKey);
            info.i = this.fonts.Count + 1;
            fonts.Add(fontKey, info);
            //info["i"] = fonts.count + 1;
            //if(info.con)
        }

        public void SetFont(string family, string style = "", double size = 0)
        {
            if (family == "")
                family = FontFamily;
            else
            {
                family = family.ToLower();
            }
            style = style.ToUpper();
            if (style.Contains("U"))
            {
                Underline = true;
                style = style.Replace("U", "");
            }
            else
            {
                Underline = false;
            }
            if (style == "IB")
                style = "BI";
            if (size == 0)
                size = FontSizePt;
            if (FontFamily == family && FontStyle == style && FontSizePt == size)
                return;
            var fontKey = (family + style).ToLower();
            if (!fonts.ContainsKey(fontKey))
            {
                if (family == "arial")
                    family = "helvetica";
                if (CoreFonts.Contains(family))
                {
                    if (family == "symbol" || family == "zapfdingbats")
                        style = "";
                    fontKey = (family + style).ToLower();
                    if (!fonts.ContainsKey(fontKey))
                    {
                        AddFont(family, style);
                    }

                }
                else
                {
                    throw new Exception("Undefined font: " + family + " " + style);
                }
            }
            FontFamily = family;
            FontStyle = style;
            FontSizePt = (int) size;
            FontSize = (float) (size/k);
            CurrentFont = fonts[fontKey];
            if (Page > 0)
                Out(string.Format("BT /F{0} {1:0.00} Tf ET", CurrentFont.i, FontSizePt));
        }

        public void SetFontSize(double size)
        {
            if (FontSizePt == size)
            {
                return;
            }
            FontSizePt = (int) size;
            FontSize = (int) (size/k);
            if (Page > 0)
                Out(string.Format("BT /F{0} {1:0.00} Tf ET", CurrentFont.i, FontSizePt));
        }

        public int AddLink()
        {
            
            links.Add(new Link
            {
                Page = 0,
                Y = 0
            });
            return links.Count - 1;
        }

        public void SetLink(int linkIndex, double y = 0, int page = -1)
        {
            if (y == -1)
            {
                y = this.y;
            }
            if (page == -1)
            {
                page = this.Page;
            }
            this.links[linkIndex].Page = page;
            this.links[linkIndex].Y = y;
        }

        public void Link(double _x, double _y, double _w, double _h, string link)
        {
            this.PageLinks.Add(this.Page, new PageLink
            {
                x = (float) (_x*this.k),
                y = (float) (this.hPt - _y * this.k),
                w = (float) (_w * this.k),
                h = (float) (_h*this.k),
                Link = link
            });
            throw new NotImplementedException();
        }

        public void Text(double x, double y, string txt)
        {
            var s = string.Format("BT {0:0.00} {1:0.00} Td ({2}) Tj ET", x * k, (h - y) * k, Escape(txt));
        }

        public bool AcceptPageBreak()
        {
            return AutoPageBreak;
        }

        public void Cell(double _w, double _h = 0, string txt = "", string border = "0", double ln = 0, string align = "",
            bool fill = false, string link = "")
        {
            var _k = k;
            double _x;
            double _y;
            if ((y + _h) > PageBreakTrigger && !InHeader && !InFooter && AcceptPageBreak())
            {
                _x = x;
                var _ws = ws;
                if (_ws > 0)
                {
                    ws = 0;
                    Out("0 Tw");
                }
                AddPage(CurOrientation, CurPageSize);
                x = _x;
                if (_ws > 0)
                {
                    ws = _ws;
                    Out(string.Format("{0:0.000} Tw", _ws*_k));
                }
            }
            if (_w == 0)
                _w = w - rMargin - x;
            var s = "";
            if (fill || border == "1")
            {
                string op;
                if (fill)
                    op = (border == "1") ? "B" : "f";
                else
                {
                    op = "S";
                }
                s = string.Format("{0:0.00} {1:0.00} {2:0.00} {3:0.00} re {4} ", x * _k, (h - y) * _k, _w * _k, (0 - _h) * k, op);
            }

            
            
            if (border != "0" && border != "1")
            {
                _x = this.x;
                _y = this.y;
                if (border.Contains("L"))
                    s += string.Format("{0:0.00} {1:0.00} m {2:0.00} {3:0.00} 1 S ", _x*_k, (this.h - _y)*_k, _x*_k,
                        (this.h - (_y + _h))*_k);
                if (border.Contains("T"))
                    s += string.Format("{0:0.00} {1:0.00} m {2:0.00} {3:0.00} 1 S ", _x*_k, (this.h - _y)*_k,
                        (_x + _w)*_k, (this.h - _y)*_k);
                if (border.Contains("R"))
                    s += string.Format("{0:0.00} {1:0.00} m {2:0.00} {3:0.00} 1 S ", (_x + _w)*_k, (this.h - _y)*_k,
                        (_x + _w)*_k, (this.h - (_y + _h))*_k);
                if (border.Contains("B"))
                    s += string.Format("{0:0.00} {1:0.00} m {2:0.00} {3:0.00} 1 S ", _x*_k, (this.h - (_y + _h))*_k,
                        (_x + _w)*_k, (this.h - (_y + _h))*_k);
            }
            if (txt != "")
            {
                double dx;
                if (align == "R")
                    dx = _w - cMargin - GetStringWidth(txt);
                else if (align == "C")
                    dx = (_w - GetStringWidth(txt))/2;
                else
                {
                    dx = cMargin;
                }
                if (ColorFlag)
                    s += "q " + TextColor + " ";
                var txt2 = txt.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
                s += string.Format("BT {0:0.00} {1:0.00} Td ({2}) Tj ET", (x + dx) * _k, (h - (y + 0.5 * _h + 0.3 * FontSize)) * _k, txt2);
                if (Underline)
                    s += " " + DoUnderline(x + dx, y + 0.5*_h + 0.3*FontSize, txt);
                if (ColorFlag)
                    s += " Q";
                if (!string.IsNullOrEmpty(link))
                {
                    Link(x+dx, y+0.5*FontSize, GetStringWidth(txt), FontSize, link);
                }
            }
            if (!string.IsNullOrEmpty(s))
            {
                Out(s);
            }
            lasth = (int) _h;
            if (ln > 0)
            {
                y += _h;
                if (ln == 1)
                    x = lMargin;
            }
            else
            {
                x += _w;
            }
        }

        public void Output(string filename)
        {
            if(this.State < 3)
                this.Close();
            System.IO.File.WriteAllText(filename, this.Buffer);
        }

        public void MultiCell(double _w, double _h, string txt, string border = "0", string align = "J", bool fill = false)
        {
            var cw = this.CurrentFont.cw;
            var b2 = "";
            var ls = 0;
            if (_w == 0)
                _w = (this.w - this.rMargin - this.x);
            var wmax = (_w - 2 * this.cMargin) * 1000 / this.FontSize;
            var s = txt.Replace("\r", "");
            var nb = s.Length;
            if (nb > 0 && s[nb - 1] == '\n')
            {
                nb--;
            }
            var b = "0";
            if (!String.IsNullOrEmpty(border) && border != "0")
            {
                
                if (border == "1")
                {
                    border = "LTRB";
                    b = "LRT";
                    b2 = "LR";
                }
                else
                {
                    b2 = "";
                    if (border.Contains("L"))
                        b2 += "L";
                    if (border.Contains("R"))
                        b2 += "R";
                    b = border.Contains("T") ? b2 + "T" : b2;
                }
            }
            var sep = -1;
            var i = 0;
            var j = 0;
            var l = 0;
            var ns = 0;
            var nl = 1;
            while (i < nb)
            {
                var c = s[i];
                if (c == '\n')
                {
                    if (ws > 0)
                    {
                        ws = 0;
                        Out("0 Tw");
                    }
                    Cell(_w, _h, s.Substring(j, i-j), b, 2, align, fill);
                    i++;
                    sep = -1;
                    j = i;
                    l = 0;
                    ns = 0;
                    nl++;
                    if (!string.IsNullOrEmpty(border) && border != "0" && nl == 2)
                        b = b2;
                    continue;
                }
                
                if (c == ' ')
                {
                    sep = i;
                    ls = l;
                    ns++;
                }
                l += cw.ContainsKey(c) ? cw[c] : cw['?'];
                if (l > wmax)
                {
                    if (sep == -1)
                    {
                        if (i == j)
                            i++;
                        if (ws > 0)
                        {
                            ws = 0;
                            Out("0 Tw");
                        }
                        Cell(_w, _h, s.Substring(j, i - j), b, 2, align, fill);
                    }
                    else
                    {
                        if (align == "J")
                        {
                            ws = (float) ((ns > 1) ? (wmax - ls)/1000*FontSize/(ns - 1) : 0);
                            Out(string.Format("{0:0.000} Tw", ws*k));
                        }
                        Cell(_w, _h, s.Substring(j, sep-j), b, 2, align, fill);
                        i = sep + 1;
                    }
                    sep = -1;
                    j = i;
                    l = 0;
                    ns = 0;
                    nl++;
                    if (!string.IsNullOrEmpty(border) && border != "0" && nl == 2)
                    {
                        b = b2;
                    }
                }
                else
                {
                    i++;
                }
            }
            if (ws > 0)
            {
                ws = 0;
                Out("0 Tw");
            }
            if (!string.IsNullOrEmpty(border) && border.Contains("B"))
            {
                b += "B";
            }
            Cell(_w, _h, s.Substring(j, i-j), b, 2, align, fill);
            x = lMargin;
        }

        public void Write(double _h, string txt, string link = "")
        {
            var cw = CurrentFont.cw;
            var _w = w - rMargin - x;
            var wmax = (_w - 2*cMargin)*1000/FontSize;
            var s = txt.Replace("\r", "");
            var nb = s.Length;
            var sep = -1;
            var i = 0;
            var j = 0;
            var l = 0;
            var nl = 1;
            while (i < nb)
            {
                var c = s[i];
                if (c == 'n')
                {
                    Cell(_w, _h, s.Substring(j, i - j), "", 2, "", false, link);
                    i++;
                    sep = -1;
                    j = i;
                    l = 0;
                    if (nl == 1)
                    {
                        x = lMargin;
                        _w = w - rMargin - x;
                        wmax = (_w - 2*cMargin)*1000/FontSize;
                    }
                    nl++;
                    continue;
                }
                if (c == ' ')
                    sep = i;
                l += cw[c];
                if (l > wmax)
                {
                    if (sep == -1)
                    {
                        if (x > lMargin)
                        {
                            x = lMargin;
                            y += _h;
                            _w = w - rMargin - x;
                            wmax = (_w - 2*cMargin)*1000/FontSize;
                            i++;
                            nl++;
                            continue;
                        }
                        if (i == j)
                            i++;
                        Cell(_w, _h, s.Substring(j, i - j), "", 2, "", false, link);
                    }
                    else
                    {
                        Cell(_w, _h, s.Substring(j, sep - j), "", 2, "", false, link);
                        i = sep + 1;
                    }
                    sep = -1;
                    j = i;
                    l = 0;
                    if (nl == 1)
                    {
                        x = lMargin;
                        _w = w - rMargin - x;
                        wmax = (_w - 2*cMargin)*1000/FontSize;
                    }
                    nl++;


                }
                else
                {
                    nl++;
                }
            }
            if(i!=j)
                Cell(l/1000*FontSize, _h, s.Substring(j), "", 0, "", false, link);
        }

        public void Ln(double _h = -1)
        {
            x = lMargin;
            if (_h == -1)
                y += lasth;
            else
            {
                y += _h;
            }
        }

        public void Image(string file, double _x = -1, double _y = -1, double _w = 0, double _h = 0, string type = "",
            string link = "")
        {
            ImageInformation imgInfo;
            if (!images.ContainsKey(file))
            {
                if (type == "")
                {
                    var pos = file.LastIndexOf('.');
                    if (pos == -1)
                    {
                        throw new Exception("Image file has no extension and no type was specified: " + file);
                    }
                    type = file.Substring(pos + 1);
                }
                type = type.ToLower();
                if (type == "jpeg")
                    type = "jpg";
                
                switch (type)
                {
                    case "jpg":
                        imgInfo = ParseJpg(file);
                        break;
                    case "gif":
                        throw new NotImplementedException();
                        break;
                    case "png":
                        imgInfo = ParsePng(file);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                imgInfo.i = this.images.Count + 1;
                this.images.Add(file, imgInfo);

            }
            else
            {
                imgInfo = this.images[file];
            }

            if (_w == 0 && _h == 0)
            {
                _w = -96;
                _h = -96;
            }
            if (_w < 0)
                _w = -imgInfo.Width*72/_w/this.k;
            if (_h < 0)
                _h = -imgInfo.Height*72/_h/this.k;
            if (_w == 0)
                _w = _h*imgInfo.Width/imgInfo.Height;
            if (_h == 0)
                _h = _w*imgInfo.Height/imgInfo.Width;

            if (_y == -1)
            {
                if (this.y + _h > this.PageBreakTrigger && !this.InHeader && !this.InFooter && this.AcceptPageBreak())
                {
                    var x2 = this.x;
                    this.AddPage(this.CurOrientation, this.CurPageSize);
                    this.x = x2;
                }
                _y = this.y;
                this.y = _h;
            }
            if (_x == -1)
            {
                _x = this.x;
            }
            this.Out(string.Format("q {0:0.00} 0 0 {1:0.00} {2:0.00} {3:0.00} cm /I{4} Do Q", _w*this.k, _h*this.k, _x * this.k, (this.h-(_y+_h))*this.k, imgInfo.i));
            if(!string.IsNullOrEmpty(link))
                this.Link(_x, _y, _w, _h, link);

        }

        public double GetX()
        {
            return x;
        }

        public void SetX(double _x)
        {
            if (_x >= 0)
                x = _x;
            else
            {
                cMargin = (w + _x);
            }
        }

        public double GetY()
        {
            return y;
        }

        public void SetY(double _y)
        {
            x = lMargin;
            if (_y >= 0)
            {
                y = _y;
            }
            else
            {
                y = h + _y;
            }
        }

        public void SetXY(double x, double y)
        {
            SetY(y);
            SetX(x);
        }


        //TODO need to do output function


        private void CheckOutput()
        {
            
        }


        private void BeginPage(Orientation orientation, PageDimensions pageSize)
        {
            Page++;
            Pages.Add(Page,"");
            State = 2;
            x = lMargin;
            y = tMargin;
            FontFamily = "";
            if (pageSize == null)
            {
                pageSize = this.DefPageSize;
            }
            if (orientation != this.CurOrientation || pageSize.ShortSide != this.CurPageSize.ShortSide ||
                pageSize.LongSide != this.CurPageSize.LongSide)
            {
                if (orientation == Orientation.Portrait)
                {
                    this.w = pageSize.ShortSide;
                    this.h = pageSize.LongSide;
                }
                else
                {
                    this.w = pageSize.LongSide;
                    this.h = pageSize.ShortSide;
                }
                this.wPt = this.w*this.k;
                this.hPt = this.h*this.k;
                this.PageBreakTrigger = this.h - this.bMargin;
                this.CurOrientation = orientation;
                this.CurPageSize = pageSize;
            }
            if (orientation != this.DefOrientation || pageSize.LongSide != this.DefPageSize.LongSide ||
                pageSize.ShortSide != this.DefPageSize.ShortSide)
            {
                this.PageSizes[this.Page] = new PageDimensions
                {
                    LongSide = wPt,
                    ShortSide = hPt
                };
            }
        }

        private void EndPage()
        {
            State = 1;
        }
        
        private PdfFont LoadFont(string font)
        {
            var f = new PdfFont();
            f.cw = new Dictionary<char, int>();
            switch (font)
            {
                case "courier":
                    f.Type = "Core";
                    f.Name = "Courier";
                    f.Up = -100;
                    f.Ut = 50;
                    for (var i = 0; i <= 255; i++)
                    {
                        f.cw.Add((char)i, 600);
                    }
                    break;
                case "courierb":
                    f.Type = "Core";
                    f.Name = "Courier-Bold";
                    f.Up = -100;
                    f.Ut = 50;
                    for (var i = 0; i <= 255; i++)
                    {
                        f.cw.Add((char)i, 600);
                    }
                    break;
                case "courierbi":
                    f.Type = "Core";
                    f.Name = "Courier-BoldOblique";
                    f.Up = -100;
                    f.Ut = 50;
                    for (var i = 0; i <= 255; i++)
                    {
                        f.cw.Add((char)i, 600);
                    }
                    break;
                case "courieri":
                    f.Type = "Core";
                    f.Name = "Courier-Oblique";
                    f.Up = -100;
                    f.Ut = 50;
                    for (var i = 0; i <= 255; i++)
                    {
                        f.cw.Add((char)i, 600);
                    }
                    break;
                case "helvetica":
                    f.Type = "Core";
                    f.Name = "Helvetica";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 278);
                    f.cw.Add((char)1, 278);
                    f.cw.Add((char)2, 278);
                    f.cw.Add((char)3, 278);
                    f.cw.Add((char)4, 278);
                    f.cw.Add((char)5, 278);
                    f.cw.Add((char)6, 278);
                    f.cw.Add((char)7, 278);
                    f.cw.Add((char)8, 278);
                    f.cw.Add((char)9, 278);
                    f.cw.Add((char)10, 278);
                    f.cw.Add((char)11, 278);
                    f.cw.Add((char)12, 278);
                    f.cw.Add((char)13, 278);
                    f.cw.Add((char)14, 278);
                    f.cw.Add((char)15, 278);
                    f.cw.Add((char)16, 278);
                    f.cw.Add((char)17, 278);
                    f.cw.Add((char)18, 278);
                    f.cw.Add((char)19, 278);
                    f.cw.Add((char)20, 278);
                    f.cw.Add((char)21, 278);
                    f.cw.Add((char)22, 278);
                    f.cw.Add((char)23, 278);
                    f.cw.Add((char)24, 278);
                    f.cw.Add((char)25, 278);
                    f.cw.Add((char)26, 278);
                    f.cw.Add((char)27, 278);
                    f.cw.Add((char)28, 278);
                    f.cw.Add((char)29, 278);
                    f.cw.Add((char)30, 278);
                    f.cw.Add((char)31, 278);
                    f.cw.Add(' ', 278);
                    f.cw.Add('!', 278);
                    f.cw.Add('"', 355);
                    f.cw.Add('#', 556);
                    f.cw.Add('$', 556);
                    f.cw.Add('%', 889);
                    f.cw.Add('&', 667);
                    f.cw.Add('\'', 191);
                    f.cw.Add('(', 333);
                    f.cw.Add(')', 333);
                    f.cw.Add('*', 389);
                    f.cw.Add('+', 584);
                    f.cw.Add(',', 278);
                    f.cw.Add('-', 333);
                    f.cw.Add('.', 278);
                    f.cw.Add('/', 278);
                    f.cw.Add('0', 556);
                    f.cw.Add('1', 556);
                    f.cw.Add('2', 556);
                    f.cw.Add('3', 556);
                    f.cw.Add('4', 556);
                    f.cw.Add('5', 556);
                    f.cw.Add('6', 556);
                    f.cw.Add('7', 556);
                    f.cw.Add('8', 556);
                    f.cw.Add('9', 556);
                    f.cw.Add(':', 278);
                    f.cw.Add(';', 278);
                    f.cw.Add('<', 584);
                    f.cw.Add('=', 584);
                    f.cw.Add('>', 584);
                    f.cw.Add('?', 556);
                    f.cw.Add('@', 1015);
                    f.cw.Add('A', 667);
                    f.cw.Add('B', 667);
                    f.cw.Add('C', 722);
                    f.cw.Add('D', 722);
                    f.cw.Add('E', 667);
                    f.cw.Add('F', 611);
                    f.cw.Add('G', 778);
                    f.cw.Add('H', 722);
                    f.cw.Add('I', 278);
                    f.cw.Add('J', 500);
                    f.cw.Add('K', 667);
                    f.cw.Add('L', 556);
                    f.cw.Add('M', 833);
                    f.cw.Add('N', 722);
                    f.cw.Add('O', 778);
                    f.cw.Add('P', 667);
                    f.cw.Add('Q', 778);
                    f.cw.Add('R', 722);
                    f.cw.Add('S', 667);
                    f.cw.Add('T', 611);
                    f.cw.Add('U', 722);
                    f.cw.Add('V', 667);
                    f.cw.Add('W', 944);
                    f.cw.Add('X', 667);
                    f.cw.Add('Y', 667);
                    f.cw.Add('Z', 611);
                    f.cw.Add('[', 278);
                    f.cw.Add('\\', 278);
                    f.cw.Add(']', 278);
                    f.cw.Add('^', 469);
                    f.cw.Add('_', 556);
                    f.cw.Add('`', 333);
                    f.cw.Add('a', 556);
                    f.cw.Add('b', 556);
                    f.cw.Add('c', 500);
                    f.cw.Add('d', 556);
                    f.cw.Add('e', 556);
                    f.cw.Add('f', 278);
                    f.cw.Add('g', 556);
                    f.cw.Add('h', 556);
                    f.cw.Add('i', 222);
                    f.cw.Add('j', 222);
                    f.cw.Add('k', 500);
                    f.cw.Add('l', 222);
                    f.cw.Add('m', 833);
                    f.cw.Add('n', 556);
                    f.cw.Add('o', 556);
                    f.cw.Add('p', 556);
                    f.cw.Add('q', 556);
                    f.cw.Add('r', 333);
                    f.cw.Add('s', 500);
                    f.cw.Add('t', 278);
                    f.cw.Add('u', 556);
                    f.cw.Add('v', 500);
                    f.cw.Add('w', 722);
                    f.cw.Add('x', 500);
                    f.cw.Add('y', 500);
                    f.cw.Add('z', 500);
                    f.cw.Add('{', 334);
                    f.cw.Add('|', 260);
                    f.cw.Add('}', 334);
                    f.cw.Add('~', 584);
                    f.cw.Add((char)127, 350);
                    f.cw.Add((char)128, 556);
                    f.cw.Add((char)129, 350);
                    f.cw.Add((char)130, 222);
                    f.cw.Add((char)131, 556);
                    f.cw.Add((char)132, 333);
                    f.cw.Add((char)133, 1000);
                    f.cw.Add((char)134, 556);
                    f.cw.Add((char)135, 556);
                    f.cw.Add((char)136, 333);
                    f.cw.Add((char)137, 1000);
                    f.cw.Add((char)138, 667);
                    f.cw.Add((char)139, 333);
                    f.cw.Add((char)140, 1000);
                    f.cw.Add((char)141, 350);
                    f.cw.Add((char)142, 611);
                    f.cw.Add((char)143, 350);
                    f.cw.Add((char)144, 350);
                    f.cw.Add((char)145, 222);
                    f.cw.Add((char)146, 222);
                    f.cw.Add((char)147, 333);
                    f.cw.Add((char)148, 333);
                    f.cw.Add((char)149, 350);
                    f.cw.Add((char)150, 556);
                    f.cw.Add((char)151, 1000);
                    f.cw.Add((char)152, 333);
                    f.cw.Add((char)153, 1000);
                    f.cw.Add((char)154, 500);
                    f.cw.Add((char)155, 333);
                    f.cw.Add((char)156, 944);
                    f.cw.Add((char)157, 350);
                    f.cw.Add((char)158, 500);
                    f.cw.Add((char)159, 667);
                    f.cw.Add((char)160, 278);
                    f.cw.Add((char)161, 333);
                    f.cw.Add((char)162, 556);
                    f.cw.Add((char)163, 556);
                    f.cw.Add((char)164, 556);
                    f.cw.Add((char)165, 556);
                    f.cw.Add((char)166, 260);
                    f.cw.Add((char)167, 556);
                    f.cw.Add((char)168, 333);
                    f.cw.Add((char)169, 737);
                    f.cw.Add((char)170, 370);
                    f.cw.Add((char)171, 556);
                    f.cw.Add((char)172, 584);
                    f.cw.Add((char)173, 333);
                    f.cw.Add((char)174, 737);
                    f.cw.Add((char)175, 333);
                    f.cw.Add((char)176, 400);
                    f.cw.Add((char)177, 584);
                    f.cw.Add((char)178, 333);
                    f.cw.Add((char)179, 333);
                    f.cw.Add((char)180, 333);
                    f.cw.Add((char)181, 556);
                    f.cw.Add((char)182, 537);
                    f.cw.Add((char)183, 278);
                    f.cw.Add((char)184, 333);
                    f.cw.Add((char)185, 333);
                    f.cw.Add((char)186, 365);
                    f.cw.Add((char)187, 556);
                    f.cw.Add((char)188, 834);
                    f.cw.Add((char)189, 834);
                    f.cw.Add((char)190, 834);
                    f.cw.Add((char)191, 611);
                    f.cw.Add((char)192, 667);
                    f.cw.Add((char)193, 667);
                    f.cw.Add((char)194, 667);
                    f.cw.Add((char)195, 667);
                    f.cw.Add((char)196, 667);
                    f.cw.Add((char)197, 667);
                    f.cw.Add((char)198, 1000);
                    f.cw.Add((char)199, 722);
                    f.cw.Add((char)200, 667);
                    f.cw.Add((char)201, 667);
                    f.cw.Add((char)202, 667);
                    f.cw.Add((char)203, 667);
                    f.cw.Add((char)204, 278);
                    f.cw.Add((char)205, 278);
                    f.cw.Add((char)206, 278);
                    f.cw.Add((char)207, 278);
                    f.cw.Add((char)208, 722);
                    f.cw.Add((char)209, 722);
                    f.cw.Add((char)210, 778);
                    f.cw.Add((char)211, 778);
                    f.cw.Add((char)212, 778);
                    f.cw.Add((char)213, 778);
                    f.cw.Add((char)214, 778);
                    f.cw.Add((char)215, 584);
                    f.cw.Add((char)216, 778);
                    f.cw.Add((char)217, 722);
                    f.cw.Add((char)218, 722);
                    f.cw.Add((char)219, 722);
                    f.cw.Add((char)220, 722);
                    f.cw.Add((char)221, 667);
                    f.cw.Add((char)222, 667);
                    f.cw.Add((char)223, 611);
                    f.cw.Add((char)224, 556);
                    f.cw.Add((char)225, 556);
                    f.cw.Add((char)226, 556);
                    f.cw.Add((char)227, 556);
                    f.cw.Add((char)228, 556);
                    f.cw.Add((char)229, 556);
                    f.cw.Add((char)230, 889);
                    f.cw.Add((char)231, 500);
                    f.cw.Add((char)232, 556);
                    f.cw.Add((char)233, 556);
                    f.cw.Add((char)234, 556);
                    f.cw.Add((char)235, 556);
                    f.cw.Add((char)236, 278);
                    f.cw.Add((char)237, 278);
                    f.cw.Add((char)238, 278);
                    f.cw.Add((char)239, 278);
                    f.cw.Add((char)240, 556);
                    f.cw.Add((char)241, 556);
                    f.cw.Add((char)242, 556);
                    f.cw.Add((char)243, 556);
                    f.cw.Add((char)244, 556);
                    f.cw.Add((char)245, 556);
                    f.cw.Add((char)246, 556);
                    f.cw.Add((char)247, 584);
                    f.cw.Add((char)248, 611);
                    f.cw.Add((char)249, 556);
                    f.cw.Add((char)250, 556);
                    f.cw.Add((char)251, 556);
                    f.cw.Add((char)252, 556);
                    f.cw.Add((char)253, 500);
                    f.cw.Add((char)254, 556);
                    f.cw.Add((char)255, 500);

                    break;
                case "helveticab":
                    f.Type = "Core";
                    f.Name = "Helvetica-Bold";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 278);
                    f.cw.Add((char)1, 278);
                    f.cw.Add((char)2, 278);
                    f.cw.Add((char)3, 278);
                    f.cw.Add((char)4, 278);
                    f.cw.Add((char)5, 278);
                    f.cw.Add((char)6, 278);
                    f.cw.Add((char)7, 278);
                    f.cw.Add((char)8, 278);
                    f.cw.Add((char)9, 278);
                    f.cw.Add((char)10, 278);
                    f.cw.Add((char)11, 278);
                    f.cw.Add((char)12, 278);
                    f.cw.Add((char)13, 278);
                    f.cw.Add((char)14, 278);
                    f.cw.Add((char)15, 278);
                    f.cw.Add((char)16, 278);
                    f.cw.Add((char)17, 278);
                    f.cw.Add((char)18, 278);
                    f.cw.Add((char)19, 278);
                    f.cw.Add((char)20, 278);
                    f.cw.Add((char)21, 278);
                    f.cw.Add((char)22, 278);
                    f.cw.Add((char)23, 278);
                    f.cw.Add((char)24, 278);
                    f.cw.Add((char)25, 278);
                    f.cw.Add((char)26, 278);
                    f.cw.Add((char)27, 278);
                    f.cw.Add((char)28, 278);
                    f.cw.Add((char)29, 278);
                    f.cw.Add((char)30, 278);
                    f.cw.Add((char)31, 278);
                    f.cw.Add(' ', 278);
                    f.cw.Add('!', 333);
                    f.cw.Add('"', 474);
                    f.cw.Add('#', 556);
                    f.cw.Add('$', 556);
                    f.cw.Add('%', 889);
                    f.cw.Add('&', 722);
                    f.cw.Add('\'', 238);
                    f.cw.Add('(', 333);
                    f.cw.Add(')', 333);
                    f.cw.Add('*', 389);
                    f.cw.Add('+', 584);
                    f.cw.Add(',', 278);
                    f.cw.Add('-', 333);
                    f.cw.Add('.', 278);
                    f.cw.Add('/', 278);
                    f.cw.Add('0', 556);
                    f.cw.Add('1', 556);
                    f.cw.Add('2', 556);
                    f.cw.Add('3', 556);
                    f.cw.Add('4', 556);
                    f.cw.Add('5', 556);
                    f.cw.Add('6', 556);
                    f.cw.Add('7', 556);
                    f.cw.Add('8', 556);
                    f.cw.Add('9', 556);
                    f.cw.Add(':', 333);
                    f.cw.Add(';', 333);
                    f.cw.Add('<', 584);
                    f.cw.Add('=', 584);
                    f.cw.Add('>', 584);
                    f.cw.Add('?', 611);
                    f.cw.Add('@', 975);
                    f.cw.Add('A', 722);
                    f.cw.Add('B', 722);
                    f.cw.Add('C', 722);
                    f.cw.Add('D', 722);
                    f.cw.Add('E', 667);
                    f.cw.Add('F', 611);
                    f.cw.Add('G', 778);
                    f.cw.Add('H', 722);
                    f.cw.Add('I', 278);
                    f.cw.Add('J', 556);
                    f.cw.Add('K', 722);
                    f.cw.Add('L', 611);
                    f.cw.Add('M', 833);
                    f.cw.Add('N', 722);
                    f.cw.Add('O', 778);
                    f.cw.Add('P', 667);
                    f.cw.Add('Q', 778);
                    f.cw.Add('R', 722);
                    f.cw.Add('S', 667);
                    f.cw.Add('T', 611);
                    f.cw.Add('U', 722);
                    f.cw.Add('V', 667);
                    f.cw.Add('W', 944);
                    f.cw.Add('X', 667);
                    f.cw.Add('Y', 667);
                    f.cw.Add('Z', 611);
                    f.cw.Add('[', 333);
                    f.cw.Add('\\', 278);
                    f.cw.Add(']', 333);
                    f.cw.Add('^', 584);
                    f.cw.Add('_', 556);
                    f.cw.Add('`', 333);
                    f.cw.Add('a', 556);
                    f.cw.Add('b', 611);
                    f.cw.Add('c', 556);
                    f.cw.Add('d', 611);
                    f.cw.Add('e', 556);
                    f.cw.Add('f', 333);
                    f.cw.Add('g', 611);
                    f.cw.Add('h', 611);
                    f.cw.Add('i', 278);
                    f.cw.Add('j', 278);
                    f.cw.Add('k', 556);
                    f.cw.Add('l', 278);
                    f.cw.Add('m', 889);
                    f.cw.Add('n', 611);
                    f.cw.Add('o', 611);
                    f.cw.Add('p', 611);
                    f.cw.Add('q', 611);
                    f.cw.Add('r', 389);
                    f.cw.Add('s', 556);
                    f.cw.Add('t', 333);
                    f.cw.Add('u', 611);
                    f.cw.Add('v', 556);
                    f.cw.Add('w', 778);
                    f.cw.Add('x', 556);
                    f.cw.Add('y', 556);
                    f.cw.Add('z', 500);
                    f.cw.Add('{', 389);
                    f.cw.Add('|', 280);
                    f.cw.Add('}', 389);
                    f.cw.Add('~', 584);
                    f.cw.Add((char)127, 350);
                    f.cw.Add((char)128, 556);
                    f.cw.Add((char)129, 350);
                    f.cw.Add((char)130, 278);
                    f.cw.Add((char)131, 556);
                    f.cw.Add((char)132, 500);
                    f.cw.Add((char)133, 1000);
                    f.cw.Add((char)134, 556);
                    f.cw.Add((char)135, 556);
                    f.cw.Add((char)136, 333);
                    f.cw.Add((char)137, 1000);
                    f.cw.Add((char)138, 667);
                    f.cw.Add((char)139, 333);
                    f.cw.Add((char)140, 1000);
                    f.cw.Add((char)141, 350);
                    f.cw.Add((char)142, 611);
                    f.cw.Add((char)143, 350);
                    f.cw.Add((char)144, 350);
                    f.cw.Add((char)145, 278);
                    f.cw.Add((char)146, 278);
                    f.cw.Add((char)147, 500);
                    f.cw.Add((char)148, 500);
                    f.cw.Add((char)149, 350);
                    f.cw.Add((char)150, 556);
                    f.cw.Add((char)151, 1000);
                    f.cw.Add((char)152, 333);
                    f.cw.Add((char)153, 1000);
                    f.cw.Add((char)154, 556);
                    f.cw.Add((char)155, 333);
                    f.cw.Add((char)156, 944);
                    f.cw.Add((char)157, 350);
                    f.cw.Add((char)158, 500);
                    f.cw.Add((char)159, 667);
                    f.cw.Add((char)160, 278);
                    f.cw.Add((char)161, 333);
                    f.cw.Add((char)162, 556);
                    f.cw.Add((char)163, 556);
                    f.cw.Add((char)164, 556);
                    f.cw.Add((char)165, 556);
                    f.cw.Add((char)166, 280);
                    f.cw.Add((char)167, 556);
                    f.cw.Add((char)168, 333);
                    f.cw.Add((char)169, 737);
                    f.cw.Add((char)170, 370);
                    f.cw.Add((char)171, 556);
                    f.cw.Add((char)172, 584);
                    f.cw.Add((char)173, 333);
                    f.cw.Add((char)174, 737);
                    f.cw.Add((char)175, 333);
                    f.cw.Add((char)176, 400);
                    f.cw.Add((char)177, 584);
                    f.cw.Add((char)178, 333);
                    f.cw.Add((char)179, 333);
                    f.cw.Add((char)180, 333);
                    f.cw.Add((char)181, 611);
                    f.cw.Add((char)182, 556);
                    f.cw.Add((char)183, 278);
                    f.cw.Add((char)184, 333);
                    f.cw.Add((char)185, 333);
                    f.cw.Add((char)186, 365);
                    f.cw.Add((char)187, 556);
                    f.cw.Add((char)188, 834);
                    f.cw.Add((char)189, 834);
                    f.cw.Add((char)190, 834);
                    f.cw.Add((char)191, 611);
                    f.cw.Add((char)192, 722);
                    f.cw.Add((char)193, 722);
                    f.cw.Add((char)194, 722);
                    f.cw.Add((char)195, 722);
                    f.cw.Add((char)196, 722);
                    f.cw.Add((char)197, 722);
                    f.cw.Add((char)198, 1000);
                    f.cw.Add((char)199, 722);
                    f.cw.Add((char)200, 667);
                    f.cw.Add((char)201, 667);
                    f.cw.Add((char)202, 667);
                    f.cw.Add((char)203, 667);
                    f.cw.Add((char)204, 278);
                    f.cw.Add((char)205, 278);
                    f.cw.Add((char)206, 278);
                    f.cw.Add((char)207, 278);
                    f.cw.Add((char)208, 722);
                    f.cw.Add((char)209, 722);
                    f.cw.Add((char)210, 778);
                    f.cw.Add((char)211, 778);
                    f.cw.Add((char)212, 778);
                    f.cw.Add((char)213, 778);
                    f.cw.Add((char)214, 778);
                    f.cw.Add((char)215, 584);
                    f.cw.Add((char)216, 778);
                    f.cw.Add((char)217, 722);
                    f.cw.Add((char)218, 722);
                    f.cw.Add((char)219, 722);
                    f.cw.Add((char)220, 722);
                    f.cw.Add((char)221, 667);
                    f.cw.Add((char)222, 667);
                    f.cw.Add((char)223, 611);
                    f.cw.Add((char)224, 556);
                    f.cw.Add((char)225, 556);
                    f.cw.Add((char)226, 556);
                    f.cw.Add((char)227, 556);
                    f.cw.Add((char)228, 556);
                    f.cw.Add((char)229, 556);
                    f.cw.Add((char)230, 889);
                    f.cw.Add((char)231, 556);
                    f.cw.Add((char)232, 556);
                    f.cw.Add((char)233, 556);
                    f.cw.Add((char)234, 556);
                    f.cw.Add((char)235, 556);
                    f.cw.Add((char)236, 278);
                    f.cw.Add((char)237, 278);
                    f.cw.Add((char)238, 278);
                    f.cw.Add((char)239, 278);
                    f.cw.Add((char)240, 611);
                    f.cw.Add((char)241, 611);
                    f.cw.Add((char)242, 611);
                    f.cw.Add((char)243, 611);
                    f.cw.Add((char)244, 611);
                    f.cw.Add((char)245, 611);
                    f.cw.Add((char)246, 611);
                    f.cw.Add((char)247, 584);
                    f.cw.Add((char)248, 611);
                    f.cw.Add((char)249, 611);
                    f.cw.Add((char)250, 611);
                    f.cw.Add((char)251, 611);
                    f.cw.Add((char)252, 611);
                    f.cw.Add((char)253, 556);
                    f.cw.Add((char)254, 611);
                    f.cw.Add((char)255, 556);
                    break;
                case "helveticabi":
                    f.Type = "Core";
                    f.Name = "Helvetica-BoldOblique";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 278);
                    f.cw.Add((char)1, 278);
                    f.cw.Add((char)2, 278);
                    f.cw.Add((char)3, 278);
                    f.cw.Add((char)4, 278);
                    f.cw.Add((char)5, 278);
                    f.cw.Add((char)6, 278);
                    f.cw.Add((char)7, 278);
                    f.cw.Add((char)8, 278);
                    f.cw.Add((char)9, 278);
                    f.cw.Add((char)10, 278);
                    f.cw.Add((char)11, 278);
                    f.cw.Add((char)12, 278);
                    f.cw.Add((char)13, 278);
                    f.cw.Add((char)14, 278);
                    f.cw.Add((char)15, 278);
                    f.cw.Add((char)16, 278);
                    f.cw.Add((char)17, 278);
                    f.cw.Add((char)18, 278);
                    f.cw.Add((char)19, 278);
                    f.cw.Add((char)20, 278);
                    f.cw.Add((char)21, 278);
                    f.cw.Add((char)22, 278);
                    f.cw.Add((char)23, 278);
                    f.cw.Add((char)24, 278);
                    f.cw.Add((char)25, 278);
                    f.cw.Add((char)26, 278);
                    f.cw.Add((char)27, 278);
                    f.cw.Add((char)28, 278);
                    f.cw.Add((char)29, 278);
                    f.cw.Add((char)30, 278);
                    f.cw.Add((char)31, 278);
                    f.cw.Add(' ', 278);
                    f.cw.Add('!', 333);
                    f.cw.Add('"', 474);
                    f.cw.Add('#', 556);
                    f.cw.Add('$', 556);
                    f.cw.Add('%', 889);
                    f.cw.Add('&', 722);
                    f.cw.Add('\'', 238);
                    f.cw.Add('(', 333);
                    f.cw.Add(')', 333);
                    f.cw.Add('*', 389);
                    f.cw.Add('+', 584);
                    f.cw.Add(',', 278);
                    f.cw.Add('-', 333);
                    f.cw.Add('.', 278);
                    f.cw.Add('/', 278);
                    f.cw.Add('0', 556);
                    f.cw.Add('1', 556);
                    f.cw.Add('2', 556);
                    f.cw.Add('3', 556);
                    f.cw.Add('4', 556);
                    f.cw.Add('5', 556);
                    f.cw.Add('6', 556);
                    f.cw.Add('7', 556);
                    f.cw.Add('8', 556);
                    f.cw.Add('9', 556);
                    f.cw.Add(':', 333);
                    f.cw.Add(';', 333);
                    f.cw.Add('<', 584);
                    f.cw.Add('=', 584);
                    f.cw.Add('>', 584);
                    f.cw.Add('?', 611);
                    f.cw.Add('@', 975);
                    f.cw.Add('A', 722);
                    f.cw.Add('B', 722);
                    f.cw.Add('C', 722);
                    f.cw.Add('D', 722);
                    f.cw.Add('E', 667);
                    f.cw.Add('F', 611);
                    f.cw.Add('G', 778);
                    f.cw.Add('H', 722);
                    f.cw.Add('I', 278);
                    f.cw.Add('J', 556);
                    f.cw.Add('K', 722);
                    f.cw.Add('L', 611);
                    f.cw.Add('M', 833);
                    f.cw.Add('N', 722);
                    f.cw.Add('O', 778);
                    f.cw.Add('P', 667);
                    f.cw.Add('Q', 778);
                    f.cw.Add('R', 722);
                    f.cw.Add('S', 667);
                    f.cw.Add('T', 611);
                    f.cw.Add('U', 722);
                    f.cw.Add('V', 667);
                    f.cw.Add('W', 944);
                    f.cw.Add('X', 667);
                    f.cw.Add('Y', 667);
                    f.cw.Add('Z', 611);
                    f.cw.Add('[', 333);
                    f.cw.Add('\\', 278);
                    f.cw.Add(']', 333);
                    f.cw.Add('^', 584);
                    f.cw.Add('_', 556);
                    f.cw.Add('`', 333);
                    f.cw.Add('a', 556);
                    f.cw.Add('b', 611);
                    f.cw.Add('c', 556);
                    f.cw.Add('d', 611);
                    f.cw.Add('e', 556);
                    f.cw.Add('f', 333);
                    f.cw.Add('g', 611);
                    f.cw.Add('h', 611);
                    f.cw.Add('i', 278);
                    f.cw.Add('j', 278);
                    f.cw.Add('k', 556);
                    f.cw.Add('l', 278);
                    f.cw.Add('m', 889);
                    f.cw.Add('n', 611);
                    f.cw.Add('o', 611);
                    f.cw.Add('p', 611);
                    f.cw.Add('q', 611);
                    f.cw.Add('r', 389);
                    f.cw.Add('s', 556);
                    f.cw.Add('t', 333);
                    f.cw.Add('u', 611);
                    f.cw.Add('v', 556);
                    f.cw.Add('w', 778);
                    f.cw.Add('x', 556);
                    f.cw.Add('y', 556);
                    f.cw.Add('z', 500);
                    f.cw.Add('{', 389);
                    f.cw.Add('|', 280);
                    f.cw.Add('}', 389);
                    f.cw.Add('~', 584);
                    f.cw.Add((char)127, 350);
                    f.cw.Add((char)128, 556);
                    f.cw.Add((char)129, 350);
                    f.cw.Add((char)130, 278);
                    f.cw.Add((char)131, 556);
                    f.cw.Add((char)132, 500);
                    f.cw.Add((char)133, 1000);
                    f.cw.Add((char)134, 556);
                    f.cw.Add((char)135, 556);
                    f.cw.Add((char)136, 333);
                    f.cw.Add((char)137, 1000);
                    f.cw.Add((char)138, 667);
                    f.cw.Add((char)139, 333);
                    f.cw.Add((char)140, 1000);
                    f.cw.Add((char)141, 350);
                    f.cw.Add((char)142, 611);
                    f.cw.Add((char)143, 350);
                    f.cw.Add((char)144, 350);
                    f.cw.Add((char)145, 278);
                    f.cw.Add((char)146, 278);
                    f.cw.Add((char)147, 500);
                    f.cw.Add((char)148, 500);
                    f.cw.Add((char)149, 350);
                    f.cw.Add((char)150, 556);
                    f.cw.Add((char)151, 1000);
                    f.cw.Add((char)152, 333);
                    f.cw.Add((char)153, 1000);
                    f.cw.Add((char)154, 556);
                    f.cw.Add((char)155, 333);
                    f.cw.Add((char)156, 944);
                    f.cw.Add((char)157, 350);
                    f.cw.Add((char)158, 500);
                    f.cw.Add((char)159, 667);
                    f.cw.Add((char)160, 278);
                    f.cw.Add((char)161, 333);
                    f.cw.Add((char)162, 556);
                    f.cw.Add((char)163, 556);
                    f.cw.Add((char)164, 556);
                    f.cw.Add((char)165, 556);
                    f.cw.Add((char)166, 280);
                    f.cw.Add((char)167, 556);
                    f.cw.Add((char)168, 333);
                    f.cw.Add((char)169, 737);
                    f.cw.Add((char)170, 370);
                    f.cw.Add((char)171, 556);
                    f.cw.Add((char)172, 584);
                    f.cw.Add((char)173, 333);
                    f.cw.Add((char)174, 737);
                    f.cw.Add((char)175, 333);
                    f.cw.Add((char)176, 400);
                    f.cw.Add((char)177, 584);
                    f.cw.Add((char)178, 333);
                    f.cw.Add((char)179, 333);
                    f.cw.Add((char)180, 333);
                    f.cw.Add((char)181, 611);
                    f.cw.Add((char)182, 556);
                    f.cw.Add((char)183, 278);
                    f.cw.Add((char)184, 333);
                    f.cw.Add((char)185, 333);
                    f.cw.Add((char)186, 365);
                    f.cw.Add((char)187, 556);
                    f.cw.Add((char)188, 834);
                    f.cw.Add((char)189, 834);
                    f.cw.Add((char)190, 834);
                    f.cw.Add((char)191, 611);
                    f.cw.Add((char)192, 722);
                    f.cw.Add((char)193, 722);
                    f.cw.Add((char)194, 722);
                    f.cw.Add((char)195, 722);
                    f.cw.Add((char)196, 722);
                    f.cw.Add((char)197, 722);
                    f.cw.Add((char)198, 1000);
                    f.cw.Add((char)199, 722);
                    f.cw.Add((char)200, 667);
                    f.cw.Add((char)201, 667);
                    f.cw.Add((char)202, 667);
                    f.cw.Add((char)203, 667);
                    f.cw.Add((char)204, 278);
                    f.cw.Add((char)205, 278);
                    f.cw.Add((char)206, 278);
                    f.cw.Add((char)207, 278);
                    f.cw.Add((char)208, 722);
                    f.cw.Add((char)209, 722);
                    f.cw.Add((char)210, 778);
                    f.cw.Add((char)211, 778);
                    f.cw.Add((char)212, 778);
                    f.cw.Add((char)213, 778);
                    f.cw.Add((char)214, 778);
                    f.cw.Add((char)215, 584);
                    f.cw.Add((char)216, 778);
                    f.cw.Add((char)217, 722);
                    f.cw.Add((char)218, 722);
                    f.cw.Add((char)219, 722);
                    f.cw.Add((char)220, 722);
                    f.cw.Add((char)221, 667);
                    f.cw.Add((char)222, 667);
                    f.cw.Add((char)223, 611);
                    f.cw.Add((char)224, 556);
                    f.cw.Add((char)225, 556);
                    f.cw.Add((char)226, 556);
                    f.cw.Add((char)227, 556);
                    f.cw.Add((char)228, 556);
                    f.cw.Add((char)229, 556);
                    f.cw.Add((char)230, 889);
                    f.cw.Add((char)231, 556);
                    f.cw.Add((char)232, 556);
                    f.cw.Add((char)233, 556);
                    f.cw.Add((char)234, 556);
                    f.cw.Add((char)235, 556);
                    f.cw.Add((char)236, 278);
                    f.cw.Add((char)237, 278);
                    f.cw.Add((char)238, 278);
                    f.cw.Add((char)239, 278);
                    f.cw.Add((char)240, 611);
                    f.cw.Add((char)241, 611);
                    f.cw.Add((char)242, 611);
                    f.cw.Add((char)243, 611);
                    f.cw.Add((char)244, 611);
                    f.cw.Add((char)245, 611);
                    f.cw.Add((char)246, 611);
                    f.cw.Add((char)247, 584);
                    f.cw.Add((char)248, 611);
                    f.cw.Add((char)249, 611);
                    f.cw.Add((char)250, 611);
                    f.cw.Add((char)251, 611);
                    f.cw.Add((char)252, 611);
                    f.cw.Add((char)253, 556);
                    f.cw.Add((char)254, 611);
                    f.cw.Add((char)255, 556);


                    break;
                case "helveticai":

                    f.Type = "Core";
                    f.Name = "Helvetica-Oblique";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 278);
                    f.cw.Add((char)1, 278);
                    f.cw.Add((char)2, 278);
                    f.cw.Add((char)3, 278);
                    f.cw.Add((char)4, 278);
                    f.cw.Add((char)5, 278);
                    f.cw.Add((char)6, 278);
                    f.cw.Add((char)7, 278);
                    f.cw.Add((char)8, 278);
                    f.cw.Add((char)9, 278);
                    f.cw.Add((char)10, 278);
                    f.cw.Add((char)11, 278);
                    f.cw.Add((char)12, 278);
                    f.cw.Add((char)13, 278);
                    f.cw.Add((char)14, 278);
                    f.cw.Add((char)15, 278);
                    f.cw.Add((char)16, 278);
                    f.cw.Add((char)17, 278);
                    f.cw.Add((char)18, 278);
                    f.cw.Add((char)19, 278);
                    f.cw.Add((char)20, 278);
                    f.cw.Add((char)21, 278);
                    f.cw.Add((char)22, 278);
                    f.cw.Add((char)23, 278);
                    f.cw.Add((char)24, 278);
                    f.cw.Add((char)25, 278);
                    f.cw.Add((char)26, 278);
                    f.cw.Add((char)27, 278);
                    f.cw.Add((char)28, 278);
                    f.cw.Add((char)29, 278);
                    f.cw.Add((char)30, 278);
                    f.cw.Add((char)31, 278);
                    f.cw.Add(' ', 278);
                    f.cw.Add('!', 278);
                    f.cw.Add('"', 355);
                    f.cw.Add('#', 556);
                    f.cw.Add('$', 556);
                    f.cw.Add('%', 889);
                    f.cw.Add('&', 667);
                    f.cw.Add('\'', 191);
                    f.cw.Add('(', 333);
                    f.cw.Add(')', 333);
                    f.cw.Add('*', 389);
                    f.cw.Add('+', 584);
                    f.cw.Add(',', 278);
                    f.cw.Add('-', 333);
                    f.cw.Add('.', 278);
                    f.cw.Add('/', 278);
                    f.cw.Add('0', 556);
                    f.cw.Add('1', 556);
                    f.cw.Add('2', 556);
                    f.cw.Add('3', 556);
                    f.cw.Add('4', 556);
                    f.cw.Add('5', 556);
                    f.cw.Add('6', 556);
                    f.cw.Add('7', 556);
                    f.cw.Add('8', 556);
                    f.cw.Add('9', 556);
                    f.cw.Add(':', 278);
                    f.cw.Add(';', 278);
                    f.cw.Add('<', 584);
                    f.cw.Add('=', 584);
                    f.cw.Add('>', 584);
                    f.cw.Add('?', 556);
                    f.cw.Add('@', 1015);
                    f.cw.Add('A', 667);
                    f.cw.Add('B', 667);
                    f.cw.Add('C', 722);
                    f.cw.Add('D', 722);
                    f.cw.Add('E', 667);
                    f.cw.Add('F', 611);
                    f.cw.Add('G', 778);
                    f.cw.Add('H', 722);
                    f.cw.Add('I', 278);
                    f.cw.Add('J', 500);
                    f.cw.Add('K', 667);
                    f.cw.Add('L', 556);
                    f.cw.Add('M', 833);
                    f.cw.Add('N', 722);
                    f.cw.Add('O', 778);
                    f.cw.Add('P', 667);
                    f.cw.Add('Q', 778);
                    f.cw.Add('R', 722);
                    f.cw.Add('S', 667);
                    f.cw.Add('T', 611);
                    f.cw.Add('U', 722);
                    f.cw.Add('V', 667);
                    f.cw.Add('W', 944);
                    f.cw.Add('X', 667);
                    f.cw.Add('Y', 667);
                    f.cw.Add('Z', 611);
                    f.cw.Add('[', 278);
                    f.cw.Add('\\', 278);
                    f.cw.Add(']', 278);
                    f.cw.Add('^', 469);
                    f.cw.Add('_', 556);
                    f.cw.Add('`', 333);
                    f.cw.Add('a', 556);
                    f.cw.Add('b', 556);
                    f.cw.Add('c', 500);
                    f.cw.Add('d', 556);
                    f.cw.Add('e', 556);
                    f.cw.Add('f', 278);
                    f.cw.Add('g', 556);
                    f.cw.Add('h', 556);
                    f.cw.Add('i', 222);
                    f.cw.Add('j', 222);
                    f.cw.Add('k', 500);
                    f.cw.Add('l', 222);
                    f.cw.Add('m', 833);
                    f.cw.Add('n', 556);
                    f.cw.Add('o', 556);
                    f.cw.Add('p', 556);
                    f.cw.Add('q', 556);
                    f.cw.Add('r', 333);
                    f.cw.Add('s', 500);
                    f.cw.Add('t', 278);
                    f.cw.Add('u', 556);
                    f.cw.Add('v', 500);
                    f.cw.Add('w', 722);
                    f.cw.Add('x', 500);
                    f.cw.Add('y', 500);
                    f.cw.Add('z', 500);
                    f.cw.Add('{', 334);
                    f.cw.Add('|', 260);
                    f.cw.Add('}', 334);
                    f.cw.Add('~', 584);
                    f.cw.Add((char)127, 350);
                    f.cw.Add((char)128, 556);
                    f.cw.Add((char)129, 350);
                    f.cw.Add((char)130, 222);
                    f.cw.Add((char)131, 556);
                    f.cw.Add((char)132, 333);
                    f.cw.Add((char)133, 1000);
                    f.cw.Add((char)134, 556);
                    f.cw.Add((char)135, 556);
                    f.cw.Add((char)136, 333);
                    f.cw.Add((char)137, 1000);
                    f.cw.Add((char)138, 667);
                    f.cw.Add((char)139, 333);
                    f.cw.Add((char)140, 1000);
                    f.cw.Add((char)141, 350);
                    f.cw.Add((char)142, 611);
                    f.cw.Add((char)143, 350);
                    f.cw.Add((char)144, 350);
                    f.cw.Add((char)145, 222);
                    f.cw.Add((char)146, 222);
                    f.cw.Add((char)147, 333);
                    f.cw.Add((char)148, 333);
                    f.cw.Add((char)149, 350);
                    f.cw.Add((char)150, 556);
                    f.cw.Add((char)151, 1000);
                    f.cw.Add((char)152, 333);
                    f.cw.Add((char)153, 1000);
                    f.cw.Add((char)154, 500);
                    f.cw.Add((char)155, 333);
                    f.cw.Add((char)156, 944);
                    f.cw.Add((char)157, 350);
                    f.cw.Add((char)158, 500);
                    f.cw.Add((char)159, 667);
                    f.cw.Add((char)160, 278);
                    f.cw.Add((char)161, 333);
                    f.cw.Add((char)162, 556);
                    f.cw.Add((char)163, 556);
                    f.cw.Add((char)164, 556);
                    f.cw.Add((char)165, 556);
                    f.cw.Add((char)166, 260);
                    f.cw.Add((char)167, 556);
                    f.cw.Add((char)168, 333);
                    f.cw.Add((char)169, 737);
                    f.cw.Add((char)170, 370);
                    f.cw.Add((char)171, 556);
                    f.cw.Add((char)172, 584);
                    f.cw.Add((char)173, 333);
                    f.cw.Add((char)174, 737);
                    f.cw.Add((char)175, 333);
                    f.cw.Add((char)176, 400);
                    f.cw.Add((char)177, 584);
                    f.cw.Add((char)178, 333);
                    f.cw.Add((char)179, 333);
                    f.cw.Add((char)180, 333);
                    f.cw.Add((char)181, 556);
                    f.cw.Add((char)182, 537);
                    f.cw.Add((char)183, 278);
                    f.cw.Add((char)184, 333);
                    f.cw.Add((char)185, 333);
                    f.cw.Add((char)186, 365);
                    f.cw.Add((char)187, 556);
                    f.cw.Add((char)188, 834);
                    f.cw.Add((char)189, 834);
                    f.cw.Add((char)190, 834);
                    f.cw.Add((char)191, 611);
                    f.cw.Add((char)192, 667);
                    f.cw.Add((char)193, 667);
                    f.cw.Add((char)194, 667);
                    f.cw.Add((char)195, 667);
                    f.cw.Add((char)196, 667);
                    f.cw.Add((char)197, 667);
                    f.cw.Add((char)198, 1000);
                    f.cw.Add((char)199, 722);
                    f.cw.Add((char)200, 667);
                    f.cw.Add((char)201, 667);
                    f.cw.Add((char)202, 667);
                    f.cw.Add((char)203, 667);
                    f.cw.Add((char)204, 278);
                    f.cw.Add((char)205, 278);
                    f.cw.Add((char)206, 278);
                    f.cw.Add((char)207, 278);
                    f.cw.Add((char)208, 722);
                    f.cw.Add((char)209, 722);
                    f.cw.Add((char)210, 778);
                    f.cw.Add((char)211, 778);
                    f.cw.Add((char)212, 778);
                    f.cw.Add((char)213, 778);
                    f.cw.Add((char)214, 778);
                    f.cw.Add((char)215, 584);
                    f.cw.Add((char)216, 778);
                    f.cw.Add((char)217, 722);
                    f.cw.Add((char)218, 722);
                    f.cw.Add((char)219, 722);
                    f.cw.Add((char)220, 722);
                    f.cw.Add((char)221, 667);
                    f.cw.Add((char)222, 667);
                    f.cw.Add((char)223, 611);
                    f.cw.Add((char)224, 556);
                    f.cw.Add((char)225, 556);
                    f.cw.Add((char)226, 556);
                    f.cw.Add((char)227, 556);
                    f.cw.Add((char)228, 556);
                    f.cw.Add((char)229, 556);
                    f.cw.Add((char)230, 889);
                    f.cw.Add((char)231, 500);
                    f.cw.Add((char)232, 556);
                    f.cw.Add((char)233, 556);
                    f.cw.Add((char)234, 556);
                    f.cw.Add((char)235, 556);
                    f.cw.Add((char)236, 278);
                    f.cw.Add((char)237, 278);
                    f.cw.Add((char)238, 278);
                    f.cw.Add((char)239, 278);
                    f.cw.Add((char)240, 556);
                    f.cw.Add((char)241, 556);
                    f.cw.Add((char)242, 556);
                    f.cw.Add((char)243, 556);
                    f.cw.Add((char)244, 556);
                    f.cw.Add((char)245, 556);
                    f.cw.Add((char)246, 556);
                    f.cw.Add((char)247, 584);
                    f.cw.Add((char)248, 611);
                    f.cw.Add((char)249, 556);
                    f.cw.Add((char)250, 556);
                    f.cw.Add((char)251, 556);
                    f.cw.Add((char)252, 556);
                    f.cw.Add((char)253, 500);
                    f.cw.Add((char)254, 556);
                    f.cw.Add((char)255, 500);

                    break;
                case "symbol":
                    f.Type = "Core";
                    f.Name = "Symbol";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 250);
                    f.cw.Add((char)1, 250);
                    f.cw.Add((char)2, 250);
                    f.cw.Add((char)3, 250);
                    f.cw.Add((char)4, 250);
                    f.cw.Add((char)5, 250);
                    f.cw.Add((char)6, 250);
                    f.cw.Add((char)7, 250);
                    f.cw.Add((char)8, 250);
                    f.cw.Add((char)9, 250);
                    f.cw.Add((char)10, 250);
                    f.cw.Add((char)11, 250);
                    f.cw.Add((char)12, 250);
                    f.cw.Add((char)13, 250);
                    f.cw.Add((char)14, 250);
                    f.cw.Add((char)15, 250);
                    f.cw.Add((char)16, 250);
                    f.cw.Add((char)17, 250);
                    f.cw.Add((char)18, 250);
                    f.cw.Add((char)19, 250);
                    f.cw.Add((char)20, 250);
                    f.cw.Add((char)21, 250);
                    f.cw.Add((char)22, 250);
                    f.cw.Add((char)23, 250);
                    f.cw.Add((char)24, 250);
                    f.cw.Add((char)25, 250);
                    f.cw.Add((char)26, 250);
                    f.cw.Add((char)27, 250);
                    f.cw.Add((char)28, 250);
                    f.cw.Add((char)29, 250);
                    f.cw.Add((char)30, 250);
                    f.cw.Add((char)31, 250);
                    f.cw.Add(' ', 250);
                    f.cw.Add('!', 333);
                    f.cw.Add('"', 713);
                    f.cw.Add('#', 500);
                    f.cw.Add('$', 549);
                    f.cw.Add('%', 833);
                    f.cw.Add('&', 778);
                    f.cw.Add('\'', 439);
                    f.cw.Add('(', 333);
                    f.cw.Add(')', 333);
                    f.cw.Add('*', 500);
                    f.cw.Add('+', 549);
                    f.cw.Add(',', 250);
                    f.cw.Add('-', 549);
                    f.cw.Add('.', 250);
                    f.cw.Add('/', 278);
                    f.cw.Add('0', 500);
                    f.cw.Add('1', 500);
                    f.cw.Add('2', 500);
                    f.cw.Add('3', 500);
                    f.cw.Add('4', 500);
                    f.cw.Add('5', 500);
                    f.cw.Add('6', 500);
                    f.cw.Add('7', 500);
                    f.cw.Add('8', 500);
                    f.cw.Add('9', 500);
                    f.cw.Add(':', 278);
                    f.cw.Add(';', 278);
                    f.cw.Add('<', 549);
                    f.cw.Add('=', 549);
                    f.cw.Add('>', 549);
                    f.cw.Add('?', 444);
                    f.cw.Add('@', 549);
                    f.cw.Add('A', 722);
                    f.cw.Add('B', 667);
                    f.cw.Add('C', 722);
                    f.cw.Add('D', 612);
                    f.cw.Add('E', 611);
                    f.cw.Add('F', 763);
                    f.cw.Add('G', 603);
                    f.cw.Add('H', 722);
                    f.cw.Add('I', 333);
                    f.cw.Add('J', 631);
                    f.cw.Add('K', 722);
                    f.cw.Add('L', 686);
                    f.cw.Add('M', 889);
                    f.cw.Add('N', 722);
                    f.cw.Add('O', 722);
                    f.cw.Add('P', 768);
                    f.cw.Add('Q', 741);
                    f.cw.Add('R', 556);
                    f.cw.Add('S', 592);
                    f.cw.Add('T', 611);
                    f.cw.Add('U', 690);
                    f.cw.Add('V', 439);
                    f.cw.Add('W', 768);
                    f.cw.Add('X', 645);
                    f.cw.Add('Y', 795);
                    f.cw.Add('Z', 611);
                    f.cw.Add('[', 333);
                    f.cw.Add('\\', 863);
                    f.cw.Add(']', 333);
                    f.cw.Add('^', 658);
                    f.cw.Add('_', 500);
                    f.cw.Add('`', 500);
                    f.cw.Add('a', 631);
                    f.cw.Add('b', 549);
                    f.cw.Add('c', 549);
                    f.cw.Add('d', 494);
                    f.cw.Add('e', 439);
                    f.cw.Add('f', 521);
                    f.cw.Add('g', 411);
                    f.cw.Add('h', 603);
                    f.cw.Add('i', 329);
                    f.cw.Add('j', 603);
                    f.cw.Add('k', 549);
                    f.cw.Add('l', 549);
                    f.cw.Add('m', 576);
                    f.cw.Add('n', 521);
                    f.cw.Add('o', 549);
                    f.cw.Add('p', 549);
                    f.cw.Add('q', 521);
                    f.cw.Add('r', 549);
                    f.cw.Add('s', 603);
                    f.cw.Add('t', 439);
                    f.cw.Add('u', 576);
                    f.cw.Add('v', 713);
                    f.cw.Add('w', 686);
                    f.cw.Add('x', 493);
                    f.cw.Add('y', 686);
                    f.cw.Add('z', 494);
                    f.cw.Add('{', 480);
                    f.cw.Add('|', 200);
                    f.cw.Add('}', 480);
                    f.cw.Add('~', 549);
                    f.cw.Add((char)127, 0);
                    f.cw.Add((char)128, 0);
                    f.cw.Add((char)129, 0);
                    f.cw.Add((char)130, 0);
                    f.cw.Add((char)131, 0);
                    f.cw.Add((char)132, 0);
                    f.cw.Add((char)133, 0);
                    f.cw.Add((char)134, 0);
                    f.cw.Add((char)135, 0);
                    f.cw.Add((char)136, 0);
                    f.cw.Add((char)137, 0);
                    f.cw.Add((char)138, 0);
                    f.cw.Add((char)139, 0);
                    f.cw.Add((char)140, 0);
                    f.cw.Add((char)141, 0);
                    f.cw.Add((char)142, 0);
                    f.cw.Add((char)143, 0);
                    f.cw.Add((char)144, 0);
                    f.cw.Add((char)145, 0);
                    f.cw.Add((char)146, 0);
                    f.cw.Add((char)147, 0);
                    f.cw.Add((char)148, 0);
                    f.cw.Add((char)149, 0);
                    f.cw.Add((char)150, 0);
                    f.cw.Add((char)151, 0);
                    f.cw.Add((char)152, 0);
                    f.cw.Add((char)153, 0);
                    f.cw.Add((char)154, 0);
                    f.cw.Add((char)155, 0);
                    f.cw.Add((char)156, 0);
                    f.cw.Add((char)157, 0);
                    f.cw.Add((char)158, 0);
                    f.cw.Add((char)159, 0);
                    f.cw.Add((char)160, 750);
                    f.cw.Add((char)161, 620);
                    f.cw.Add((char)162, 247);
                    f.cw.Add((char)163, 549);
                    f.cw.Add((char)164, 167);
                    f.cw.Add((char)165, 713);
                    f.cw.Add((char)166, 500);
                    f.cw.Add((char)167, 753);
                    f.cw.Add((char)168, 753);
                    f.cw.Add((char)169, 753);
                    f.cw.Add((char)170, 753);
                    f.cw.Add((char)171, 1042);
                    f.cw.Add((char)172, 987);
                    f.cw.Add((char)173, 603);
                    f.cw.Add((char)174, 987);
                    f.cw.Add((char)175, 603);
                    f.cw.Add((char)176, 400);
                    f.cw.Add((char)177, 549);
                    f.cw.Add((char)178, 411);
                    f.cw.Add((char)179, 549);
                    f.cw.Add((char)180, 549);
                    f.cw.Add((char)181, 713);
                    f.cw.Add((char)182, 494);
                    f.cw.Add((char)183, 460);
                    f.cw.Add((char)184, 549);
                    f.cw.Add((char)185, 549);
                    f.cw.Add((char)186, 549);
                    f.cw.Add((char)187, 549);
                    f.cw.Add((char)188, 1000);
                    f.cw.Add((char)189, 603);
                    f.cw.Add((char)190, 1000);
                    f.cw.Add((char)191, 658);
                    f.cw.Add((char)192, 823);
                    f.cw.Add((char)193, 686);
                    f.cw.Add((char)194, 795);
                    f.cw.Add((char)195, 987);
                    f.cw.Add((char)196, 768);
                    f.cw.Add((char)197, 768);
                    f.cw.Add((char)198, 823);
                    f.cw.Add((char)199, 768);
                    f.cw.Add((char)200, 768);
                    f.cw.Add((char)201, 713);
                    f.cw.Add((char)202, 713);
                    f.cw.Add((char)203, 713);
                    f.cw.Add((char)204, 713);
                    f.cw.Add((char)205, 713);
                    f.cw.Add((char)206, 713);
                    f.cw.Add((char)207, 713);
                    f.cw.Add((char)208, 768);
                    f.cw.Add((char)209, 713);
                    f.cw.Add((char)210, 790);
                    f.cw.Add((char)211, 790);
                    f.cw.Add((char)212, 890);
                    f.cw.Add((char)213, 823);
                    f.cw.Add((char)214, 549);
                    f.cw.Add((char)215, 250);
                    f.cw.Add((char)216, 713);
                    f.cw.Add((char)217, 603);
                    f.cw.Add((char)218, 603);
                    f.cw.Add((char)219, 1042);
                    f.cw.Add((char)220, 987);
                    f.cw.Add((char)221, 603);
                    f.cw.Add((char)222, 987);
                    f.cw.Add((char)223, 603);
                    f.cw.Add((char)224, 494);
                    f.cw.Add((char)225, 329);
                    f.cw.Add((char)226, 790);
                    f.cw.Add((char)227, 790);
                    f.cw.Add((char)228, 786);
                    f.cw.Add((char)229, 713);
                    f.cw.Add((char)230, 384);
                    f.cw.Add((char)231, 384);
                    f.cw.Add((char)232, 384);
                    f.cw.Add((char)233, 384);
                    f.cw.Add((char)234, 384);
                    f.cw.Add((char)235, 384);
                    f.cw.Add((char)236, 494);
                    f.cw.Add((char)237, 494);
                    f.cw.Add((char)238, 494);
                    f.cw.Add((char)239, 494);
                    f.cw.Add((char)240, 0);
                    f.cw.Add((char)241, 329);
                    f.cw.Add((char)242, 274);
                    f.cw.Add((char)243, 686);
                    f.cw.Add((char)244, 686);
                    f.cw.Add((char)245, 686);
                    f.cw.Add((char)246, 384);
                    f.cw.Add((char)247, 384);
                    f.cw.Add((char)248, 384);
                    f.cw.Add((char)249, 384);
                    f.cw.Add((char)250, 384);
                    f.cw.Add((char)251, 384);
                    f.cw.Add((char)252, 494);
                    f.cw.Add((char)253, 494);
                    f.cw.Add((char)254, 494);
                    f.cw.Add((char)255, 0);


                    break;
                case "times":
                    f.Type = "Core";
                    f.Name = "Times-Roman";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 250);
                    f.cw.Add((char)1, 250);
                    f.cw.Add((char)2, 250);
                    f.cw.Add((char)3, 250);
                    f.cw.Add((char)4, 250);
                    f.cw.Add((char)5, 250);
                    f.cw.Add((char)6, 250);
                    f.cw.Add((char)7, 250);
                    f.cw.Add((char)8, 250);
                    f.cw.Add((char)9, 250);
                    f.cw.Add((char)10, 250);
                    f.cw.Add((char)11, 250);
                    f.cw.Add((char)12, 250);
                    f.cw.Add((char)13, 250);
                    f.cw.Add((char)14, 250);
                    f.cw.Add((char)15, 250);
                    f.cw.Add((char)16, 250);
                    f.cw.Add((char)17, 250);
                    f.cw.Add((char)18, 250);
                    f.cw.Add((char)19, 250);
                    f.cw.Add((char)20, 250);
                    f.cw.Add((char)21, 250);
                    f.cw.Add((char)22, 250);
                    f.cw.Add((char)23, 250);
                    f.cw.Add((char)24, 250);
                    f.cw.Add((char)25, 250);
                    f.cw.Add((char)26, 250);
                    f.cw.Add((char)27, 250);
                    f.cw.Add((char)28, 250);
                    f.cw.Add((char)29, 250);
                    f.cw.Add((char)30, 250);
                    f.cw.Add((char)31, 250);
                    f.cw.Add(' ', 250);
                    f.cw.Add('!', 333);
                    f.cw.Add('"', 408);
                    f.cw.Add('#', 500);
                    f.cw.Add('$', 500);
                    f.cw.Add('%', 833);
                    f.cw.Add('&', 778);
                    f.cw.Add('\'', 180);
                    f.cw.Add('(', 333);
                    f.cw.Add(')', 333);
                    f.cw.Add('*', 500);
                    f.cw.Add('+', 564);
                    f.cw.Add(',', 250);
                    f.cw.Add('-', 333);
                    f.cw.Add('.', 250);
                    f.cw.Add('/', 278);
                    f.cw.Add('0', 500);
                    f.cw.Add('1', 500);
                    f.cw.Add('2', 500);
                    f.cw.Add('3', 500);
                    f.cw.Add('4', 500);
                    f.cw.Add('5', 500);
                    f.cw.Add('6', 500);
                    f.cw.Add('7', 500);
                    f.cw.Add('8', 500);
                    f.cw.Add('9', 500);
                    f.cw.Add(':', 278);
                    f.cw.Add(';', 278);
                    f.cw.Add('<', 564);
                    f.cw.Add('=', 564);
                    f.cw.Add('>', 564);
                    f.cw.Add('?', 444);
                    f.cw.Add('@', 921);
                    f.cw.Add('A', 722);
                    f.cw.Add('B', 667);
                    f.cw.Add('C', 667);
                    f.cw.Add('D', 722);
                    f.cw.Add('E', 611);
                    f.cw.Add('F', 556);
                    f.cw.Add('G', 722);
                    f.cw.Add('H', 722);
                    f.cw.Add('I', 333);
                    f.cw.Add('J', 389);
                    f.cw.Add('K', 722);
                    f.cw.Add('L', 611);
                    f.cw.Add('M', 889);
                    f.cw.Add('N', 722);
                    f.cw.Add('O', 722);
                    f.cw.Add('P', 556);
                    f.cw.Add('Q', 722);
                    f.cw.Add('R', 667);
                    f.cw.Add('S', 556);
                    f.cw.Add('T', 611);
                    f.cw.Add('U', 722);
                    f.cw.Add('V', 722);
                    f.cw.Add('W', 944);
                    f.cw.Add('X', 722);
                    f.cw.Add('Y', 722);
                    f.cw.Add('Z', 611);
                    f.cw.Add('[', 333);
                    f.cw.Add('\\', 278);
                    f.cw.Add(']', 333);
                    f.cw.Add('^', 469);
                    f.cw.Add('_', 500);
                    f.cw.Add('`', 333);
                    f.cw.Add('a', 444);
                    f.cw.Add('b', 500);
                    f.cw.Add('c', 444);
                    f.cw.Add('d', 500);
                    f.cw.Add('e', 444);
                    f.cw.Add('f', 333);
                    f.cw.Add('g', 500);
                    f.cw.Add('h', 500);
                    f.cw.Add('i', 278);
                    f.cw.Add('j', 278);
                    f.cw.Add('k', 500);
                    f.cw.Add('l', 278);
                    f.cw.Add('m', 778);
                    f.cw.Add('n', 500);
                    f.cw.Add('o', 500);
                    f.cw.Add('p', 500);
                    f.cw.Add('q', 500);
                    f.cw.Add('r', 333);
                    f.cw.Add('s', 389);
                    f.cw.Add('t', 278);
                    f.cw.Add('u', 500);
                    f.cw.Add('v', 500);
                    f.cw.Add('w', 722);
                    f.cw.Add('x', 500);
                    f.cw.Add('y', 500);
                    f.cw.Add('z', 444);
                    f.cw.Add('{', 480);
                    f.cw.Add('|', 200);
                    f.cw.Add('}', 480);
                    f.cw.Add('~', 541);
                    f.cw.Add((char)127, 350);
                    f.cw.Add((char)128, 500);
                    f.cw.Add((char)129, 350);
                    f.cw.Add((char)130, 333);
                    f.cw.Add((char)131, 500);
                    f.cw.Add((char)132, 444);
                    f.cw.Add((char)133, 1000);
                    f.cw.Add((char)134, 500);
                    f.cw.Add((char)135, 500);
                    f.cw.Add((char)136, 333);
                    f.cw.Add((char)137, 1000);
                    f.cw.Add((char)138, 556);
                    f.cw.Add((char)139, 333);
                    f.cw.Add((char)140, 889);
                    f.cw.Add((char)141, 350);
                    f.cw.Add((char)142, 611);
                    f.cw.Add((char)143, 350);
                    f.cw.Add((char)144, 350);
                    f.cw.Add((char)145, 333);
                    f.cw.Add((char)146, 333);
                    f.cw.Add((char)147, 444);
                    f.cw.Add((char)148, 444);
                    f.cw.Add((char)149, 350);
                    f.cw.Add((char)150, 500);
                    f.cw.Add((char)151, 1000);
                    f.cw.Add((char)152, 333);
                    f.cw.Add((char)153, 980);
                    f.cw.Add((char)154, 389);
                    f.cw.Add((char)155, 333);
                    f.cw.Add((char)156, 722);
                    f.cw.Add((char)157, 350);
                    f.cw.Add((char)158, 444);
                    f.cw.Add((char)159, 722);
                    f.cw.Add((char)160, 250);
                    f.cw.Add((char)161, 333);
                    f.cw.Add((char)162, 500);
                    f.cw.Add((char)163, 500);
                    f.cw.Add((char)164, 500);
                    f.cw.Add((char)165, 500);
                    f.cw.Add((char)166, 200);
                    f.cw.Add((char)167, 500);
                    f.cw.Add((char)168, 333);
                    f.cw.Add((char)169, 760);
                    f.cw.Add((char)170, 276);
                    f.cw.Add((char)171, 500);
                    f.cw.Add((char)172, 564);
                    f.cw.Add((char)173, 333);
                    f.cw.Add((char)174, 760);
                    f.cw.Add((char)175, 333);
                    f.cw.Add((char)176, 400);
                    f.cw.Add((char)177, 564);
                    f.cw.Add((char)178, 300);
                    f.cw.Add((char)179, 300);
                    f.cw.Add((char)180, 333);
                    f.cw.Add((char)181, 500);
                    f.cw.Add((char)182, 453);
                    f.cw.Add((char)183, 250);
                    f.cw.Add((char)184, 333);
                    f.cw.Add((char)185, 300);
                    f.cw.Add((char)186, 310);
                    f.cw.Add((char)187, 500);
                    f.cw.Add((char)188, 750);
                    f.cw.Add((char)189, 750);
                    f.cw.Add((char)190, 750);
                    f.cw.Add((char)191, 444);
                    f.cw.Add((char)192, 722);
                    f.cw.Add((char)193, 722);
                    f.cw.Add((char)194, 722);
                    f.cw.Add((char)195, 722);
                    f.cw.Add((char)196, 722);
                    f.cw.Add((char)197, 722);
                    f.cw.Add((char)198, 889);
                    f.cw.Add((char)199, 667);
                    f.cw.Add((char)200, 611);
                    f.cw.Add((char)201, 611);
                    f.cw.Add((char)202, 611);
                    f.cw.Add((char)203, 611);
                    f.cw.Add((char)204, 333);
                    f.cw.Add((char)205, 333);
                    f.cw.Add((char)206, 333);
                    f.cw.Add((char)207, 333);
                    f.cw.Add((char)208, 722);
                    f.cw.Add((char)209, 722);
                    f.cw.Add((char)210, 722);
                    f.cw.Add((char)211, 722);
                    f.cw.Add((char)212, 722);
                    f.cw.Add((char)213, 722);
                    f.cw.Add((char)214, 722);
                    f.cw.Add((char)215, 564);
                    f.cw.Add((char)216, 722);
                    f.cw.Add((char)217, 722);
                    f.cw.Add((char)218, 722);
                    f.cw.Add((char)219, 722);
                    f.cw.Add((char)220, 722);
                    f.cw.Add((char)221, 722);
                    f.cw.Add((char)222, 556);
                    f.cw.Add((char)223, 500);
                    f.cw.Add((char)224, 444);
                    f.cw.Add((char)225, 444);
                    f.cw.Add((char)226, 444);
                    f.cw.Add((char)227, 444);
                    f.cw.Add((char)228, 444);
                    f.cw.Add((char)229, 444);
                    f.cw.Add((char)230, 667);
                    f.cw.Add((char)231, 444);
                    f.cw.Add((char)232, 444);
                    f.cw.Add((char)233, 444);
                    f.cw.Add((char)234, 444);
                    f.cw.Add((char)235, 444);
                    f.cw.Add((char)236, 278);
                    f.cw.Add((char)237, 278);
                    f.cw.Add((char)238, 278);
                    f.cw.Add((char)239, 278);
                    f.cw.Add((char)240, 500);
                    f.cw.Add((char)241, 500);
                    f.cw.Add((char)242, 500);
                    f.cw.Add((char)243, 500);
                    f.cw.Add((char)244, 500);
                    f.cw.Add((char)245, 500);
                    f.cw.Add((char)246, 500);
                    f.cw.Add((char)247, 564);
                    f.cw.Add((char)248, 500);
                    f.cw.Add((char)249, 500);
                    f.cw.Add((char)250, 500);
                    f.cw.Add((char)251, 500);
                    f.cw.Add((char)252, 500);
                    f.cw.Add((char)253, 500);
                    f.cw.Add((char)254, 500);
                    f.cw.Add((char)255, 500);

                    break;
                case "timesb":
                    f.Type = "Core";
                    f.Name = "Times-Bold";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 250);
                    f.cw.Add((char)1, 250);
                    f.cw.Add((char)2, 250);
                    f.cw.Add((char)3, 250);
                    f.cw.Add((char)4, 250);
                    f.cw.Add((char)5, 250);
                    f.cw.Add((char)6, 250);
                    f.cw.Add((char)7, 250);
                    f.cw.Add((char)8, 250);
                    f.cw.Add((char)9, 250);
                    f.cw.Add((char)10, 250);
                    f.cw.Add((char)11, 250);
                    f.cw.Add((char)12, 250);
                    f.cw.Add((char)13, 250);
                    f.cw.Add((char)14, 250);
                    f.cw.Add((char)15, 250);
                    f.cw.Add((char)16, 250);
                    f.cw.Add((char)17, 250);
                    f.cw.Add((char)18, 250);
                    f.cw.Add((char)19, 250);
                    f.cw.Add((char)20, 250);
                    f.cw.Add((char)21, 250);
                    f.cw.Add((char)22, 250);
                    f.cw.Add((char)23, 250);
                    f.cw.Add((char)24, 250);
                    f.cw.Add((char)25, 250);
                    f.cw.Add((char)26, 250);
                    f.cw.Add((char)27, 250);
                    f.cw.Add((char)28, 250);
                    f.cw.Add((char)29, 250);
                    f.cw.Add((char)30, 250);
                    f.cw.Add((char)31, 250);
                    f.cw.Add(' ', 250);
                    f.cw.Add('!', 333);
                    f.cw.Add('"', 555);
                    f.cw.Add('#', 500);
                    f.cw.Add('$', 500);
                    f.cw.Add('%', 1000);
                    f.cw.Add('&', 833);
                    f.cw.Add('\'', 278);
                    f.cw.Add('(', 333);
                    f.cw.Add(')', 333);
                    f.cw.Add('*', 500);
                    f.cw.Add('+', 570);
                    f.cw.Add(',', 250);
                    f.cw.Add('-', 333);
                    f.cw.Add('.', 250);
                    f.cw.Add('/', 278);
                    f.cw.Add('0', 500);
                    f.cw.Add('1', 500);
                    f.cw.Add('2', 500);
                    f.cw.Add('3', 500);
                    f.cw.Add('4', 500);
                    f.cw.Add('5', 500);
                    f.cw.Add('6', 500);
                    f.cw.Add('7', 500);
                    f.cw.Add('8', 500);
                    f.cw.Add('9', 500);
                    f.cw.Add(':', 333);
                    f.cw.Add(';', 333);
                    f.cw.Add('<', 570);
                    f.cw.Add('=', 570);
                    f.cw.Add('>', 570);
                    f.cw.Add('?', 500);
                    f.cw.Add('@', 930);
                    f.cw.Add('A', 722);
                    f.cw.Add('B', 667);
                    f.cw.Add('C', 722);
                    f.cw.Add('D', 722);
                    f.cw.Add('E', 667);
                    f.cw.Add('F', 611);
                    f.cw.Add('G', 778);
                    f.cw.Add('H', 778);
                    f.cw.Add('I', 389);
                    f.cw.Add('J', 500);
                    f.cw.Add('K', 778);
                    f.cw.Add('L', 667);
                    f.cw.Add('M', 944);
                    f.cw.Add('N', 722);
                    f.cw.Add('O', 778);
                    f.cw.Add('P', 611);
                    f.cw.Add('Q', 778);
                    f.cw.Add('R', 722);
                    f.cw.Add('S', 556);
                    f.cw.Add('T', 667);
                    f.cw.Add('U', 722);
                    f.cw.Add('V', 722);
                    f.cw.Add('W', 1000);
                    f.cw.Add('X', 722);
                    f.cw.Add('Y', 722);
                    f.cw.Add('Z', 667);
                    f.cw.Add('[', 333);
                    f.cw.Add('\\', 278);
                    f.cw.Add(']', 333);
                    f.cw.Add('^', 581);
                    f.cw.Add('_', 500);
                    f.cw.Add('`', 333);
                    f.cw.Add('a', 500);
                    f.cw.Add('b', 556);
                    f.cw.Add('c', 444);
                    f.cw.Add('d', 556);
                    f.cw.Add('e', 444);
                    f.cw.Add('f', 333);
                    f.cw.Add('g', 500);
                    f.cw.Add('h', 556);
                    f.cw.Add('i', 278);
                    f.cw.Add('j', 333);
                    f.cw.Add('k', 556);
                    f.cw.Add('l', 278);
                    f.cw.Add('m', 833);
                    f.cw.Add('n', 556);
                    f.cw.Add('o', 500);
                    f.cw.Add('p', 556);
                    f.cw.Add('q', 556);
                    f.cw.Add('r', 444);
                    f.cw.Add('s', 389);
                    f.cw.Add('t', 333);
                    f.cw.Add('u', 556);
                    f.cw.Add('v', 500);
                    f.cw.Add('w', 722);
                    f.cw.Add('x', 500);
                    f.cw.Add('y', 500);
                    f.cw.Add('z', 444);
                    f.cw.Add('{', 394);
                    f.cw.Add('|', 220);
                    f.cw.Add('}', 394);
                    f.cw.Add('~', 520);
                    f.cw.Add((char)127, 350);
                    f.cw.Add((char)128, 500);
                    f.cw.Add((char)129, 350);
                    f.cw.Add((char)130, 333);
                    f.cw.Add((char)131, 500);
                    f.cw.Add((char)132, 500);
                    f.cw.Add((char)133, 1000);
                    f.cw.Add((char)134, 500);
                    f.cw.Add((char)135, 500);
                    f.cw.Add((char)136, 333);
                    f.cw.Add((char)137, 1000);
                    f.cw.Add((char)138, 556);
                    f.cw.Add((char)139, 333);
                    f.cw.Add((char)140, 1000);
                    f.cw.Add((char)141, 350);
                    f.cw.Add((char)142, 667);
                    f.cw.Add((char)143, 350);
                    f.cw.Add((char)144, 350);
                    f.cw.Add((char)145, 333);
                    f.cw.Add((char)146, 333);
                    f.cw.Add((char)147, 500);
                    f.cw.Add((char)148, 500);
                    f.cw.Add((char)149, 350);
                    f.cw.Add((char)150, 500);
                    f.cw.Add((char)151, 1000);
                    f.cw.Add((char)152, 333);
                    f.cw.Add((char)153, 1000);
                    f.cw.Add((char)154, 389);
                    f.cw.Add((char)155, 333);
                    f.cw.Add((char)156, 722);
                    f.cw.Add((char)157, 350);
                    f.cw.Add((char)158, 444);
                    f.cw.Add((char)159, 722);
                    f.cw.Add((char)160, 250);
                    f.cw.Add((char)161, 333);
                    f.cw.Add((char)162, 500);
                    f.cw.Add((char)163, 500);
                    f.cw.Add((char)164, 500);
                    f.cw.Add((char)165, 500);
                    f.cw.Add((char)166, 220);
                    f.cw.Add((char)167, 500);
                    f.cw.Add((char)168, 333);
                    f.cw.Add((char)169, 747);
                    f.cw.Add((char)170, 300);
                    f.cw.Add((char)171, 500);
                    f.cw.Add((char)172, 570);
                    f.cw.Add((char)173, 333);
                    f.cw.Add((char)174, 747);
                    f.cw.Add((char)175, 333);
                    f.cw.Add((char)176, 400);
                    f.cw.Add((char)177, 570);
                    f.cw.Add((char)178, 300);
                    f.cw.Add((char)179, 300);
                    f.cw.Add((char)180, 333);
                    f.cw.Add((char)181, 556);
                    f.cw.Add((char)182, 540);
                    f.cw.Add((char)183, 250);
                    f.cw.Add((char)184, 333);
                    f.cw.Add((char)185, 300);
                    f.cw.Add((char)186, 330);
                    f.cw.Add((char)187, 500);
                    f.cw.Add((char)188, 750);
                    f.cw.Add((char)189, 750);
                    f.cw.Add((char)190, 750);
                    f.cw.Add((char)191, 500);
                    f.cw.Add((char)192, 722);
                    f.cw.Add((char)193, 722);
                    f.cw.Add((char)194, 722);
                    f.cw.Add((char)195, 722);
                    f.cw.Add((char)196, 722);
                    f.cw.Add((char)197, 722);
                    f.cw.Add((char)198, 1000);
                    f.cw.Add((char)199, 722);
                    f.cw.Add((char)200, 667);
                    f.cw.Add((char)201, 667);
                    f.cw.Add((char)202, 667);
                    f.cw.Add((char)203, 667);
                    f.cw.Add((char)204, 389);
                    f.cw.Add((char)205, 389);
                    f.cw.Add((char)206, 389);
                    f.cw.Add((char)207, 389);
                    f.cw.Add((char)208, 722);
                    f.cw.Add((char)209, 722);
                    f.cw.Add((char)210, 778);
                    f.cw.Add((char)211, 778);
                    f.cw.Add((char)212, 778);
                    f.cw.Add((char)213, 778);
                    f.cw.Add((char)214, 778);
                    f.cw.Add((char)215, 570);
                    f.cw.Add((char)216, 778);
                    f.cw.Add((char)217, 722);
                    f.cw.Add((char)218, 722);
                    f.cw.Add((char)219, 722);
                    f.cw.Add((char)220, 722);
                    f.cw.Add((char)221, 722);
                    f.cw.Add((char)222, 611);
                    f.cw.Add((char)223, 556);
                    f.cw.Add((char)224, 500);
                    f.cw.Add((char)225, 500);
                    f.cw.Add((char)226, 500);
                    f.cw.Add((char)227, 500);
                    f.cw.Add((char)228, 500);
                    f.cw.Add((char)229, 500);
                    f.cw.Add((char)230, 722);
                    f.cw.Add((char)231, 444);
                    f.cw.Add((char)232, 444);
                    f.cw.Add((char)233, 444);
                    f.cw.Add((char)234, 444);
                    f.cw.Add((char)235, 444);
                    f.cw.Add((char)236, 278);
                    f.cw.Add((char)237, 278);
                    f.cw.Add((char)238, 278);
                    f.cw.Add((char)239, 278);
                    f.cw.Add((char)240, 500);
                    f.cw.Add((char)241, 556);
                    f.cw.Add((char)242, 500);
                    f.cw.Add((char)243, 500);
                    f.cw.Add((char)244, 500);
                    f.cw.Add((char)245, 500);
                    f.cw.Add((char)246, 500);
                    f.cw.Add((char)247, 570);
                    f.cw.Add((char)248, 500);
                    f.cw.Add((char)249, 556);
                    f.cw.Add((char)250, 556);
                    f.cw.Add((char)251, 556);
                    f.cw.Add((char)252, 556);
                    f.cw.Add((char)253, 500);
                    f.cw.Add((char)254, 556);
                    f.cw.Add((char)255, 500);

                    break;
                case "timesbi":
                    f.Type = "Core";
                    f.Name = "Times-BoldItalic";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 250);
                    f.cw.Add((char)1, 250);
                    f.cw.Add((char)2, 250);
                    f.cw.Add((char)3, 250);
                    f.cw.Add((char)4, 250);
                    f.cw.Add((char)5, 250);
                    f.cw.Add((char)6, 250);
                    f.cw.Add((char)7, 250);
                    f.cw.Add((char)8, 250);
                    f.cw.Add((char)9, 250);
                    f.cw.Add((char)10, 250);
                    f.cw.Add((char)11, 250);
                    f.cw.Add((char)12, 250);
                    f.cw.Add((char)13, 250);
                    f.cw.Add((char)14, 250);
                    f.cw.Add((char)15, 250);
                    f.cw.Add((char)16, 250);
                    f.cw.Add((char)17, 250);
                    f.cw.Add((char)18, 250);
                    f.cw.Add((char)19, 250);
                    f.cw.Add((char)20, 250);
                    f.cw.Add((char)21, 250);
                    f.cw.Add((char)22, 250);
                    f.cw.Add((char)23, 250);
                    f.cw.Add((char)24, 250);
                    f.cw.Add((char)25, 250);
                    f.cw.Add((char)26, 250);
                    f.cw.Add((char)27, 250);
                    f.cw.Add((char)28, 250);
                    f.cw.Add((char)29, 250);
                    f.cw.Add((char)30, 250);
                    f.cw.Add((char)31, 250);
                    f.cw.Add(' ', 250);
                    f.cw.Add('!', 389);
                    f.cw.Add('"', 555);
                    f.cw.Add('#', 500);
                    f.cw.Add('$', 500);
                    f.cw.Add('%', 833);
                    f.cw.Add('&', 778);
                    f.cw.Add('\'', 278);
                    f.cw.Add('(', 333);
                    f.cw.Add(')', 333);
                    f.cw.Add('*', 500);
                    f.cw.Add('+', 570);
                    f.cw.Add(',', 250);
                    f.cw.Add('-', 333);
                    f.cw.Add('.', 250);
                    f.cw.Add('/', 278);
                    f.cw.Add('0', 500);
                    f.cw.Add('1', 500);
                    f.cw.Add('2', 500);
                    f.cw.Add('3', 500);
                    f.cw.Add('4', 500);
                    f.cw.Add('5', 500);
                    f.cw.Add('6', 500);
                    f.cw.Add('7', 500);
                    f.cw.Add('8', 500);
                    f.cw.Add('9', 500);
                    f.cw.Add(':', 333);
                    f.cw.Add(';', 333);
                    f.cw.Add('<', 570);
                    f.cw.Add('=', 570);
                    f.cw.Add('>', 570);
                    f.cw.Add('?', 500);
                    f.cw.Add('@', 832);
                    f.cw.Add('A', 667);
                    f.cw.Add('B', 667);
                    f.cw.Add('C', 667);
                    f.cw.Add('D', 722);
                    f.cw.Add('E', 667);
                    f.cw.Add('F', 667);
                    f.cw.Add('G', 722);
                    f.cw.Add('H', 778);
                    f.cw.Add('I', 389);
                    f.cw.Add('J', 500);
                    f.cw.Add('K', 667);
                    f.cw.Add('L', 611);
                    f.cw.Add('M', 889);
                    f.cw.Add('N', 722);
                    f.cw.Add('O', 722);
                    f.cw.Add('P', 611);
                    f.cw.Add('Q', 722);
                    f.cw.Add('R', 667);
                    f.cw.Add('S', 556);
                    f.cw.Add('T', 611);
                    f.cw.Add('U', 722);
                    f.cw.Add('V', 667);
                    f.cw.Add('W', 889);
                    f.cw.Add('X', 667);
                    f.cw.Add('Y', 611);
                    f.cw.Add('Z', 611);
                    f.cw.Add('[', 333);
                    f.cw.Add('\\', 278);
                    f.cw.Add(']', 333);
                    f.cw.Add('^', 570);
                    f.cw.Add('_', 500);
                    f.cw.Add('`', 333);
                    f.cw.Add('a', 500);
                    f.cw.Add('b', 500);
                    f.cw.Add('c', 444);
                    f.cw.Add('d', 500);
                    f.cw.Add('e', 444);
                    f.cw.Add('f', 333);
                    f.cw.Add('g', 500);
                    f.cw.Add('h', 556);
                    f.cw.Add('i', 278);
                    f.cw.Add('j', 278);
                    f.cw.Add('k', 500);
                    f.cw.Add('l', 278);
                    f.cw.Add('m', 778);
                    f.cw.Add('n', 556);
                    f.cw.Add('o', 500);
                    f.cw.Add('p', 500);
                    f.cw.Add('q', 500);
                    f.cw.Add('r', 389);
                    f.cw.Add('s', 389);
                    f.cw.Add('t', 278);
                    f.cw.Add('u', 556);
                    f.cw.Add('v', 444);
                    f.cw.Add('w', 667);
                    f.cw.Add('x', 500);
                    f.cw.Add('y', 444);
                    f.cw.Add('z', 389);
                    f.cw.Add('{', 348);
                    f.cw.Add('|', 220);
                    f.cw.Add('}', 348);
                    f.cw.Add('~', 570);
                    f.cw.Add((char)127, 350);
                    f.cw.Add((char)128, 500);
                    f.cw.Add((char)129, 350);
                    f.cw.Add((char)130, 333);
                    f.cw.Add((char)131, 500);
                    f.cw.Add((char)132, 500);
                    f.cw.Add((char)133, 1000);
                    f.cw.Add((char)134, 500);
                    f.cw.Add((char)135, 500);
                    f.cw.Add((char)136, 333);
                    f.cw.Add((char)137, 1000);
                    f.cw.Add((char)138, 556);
                    f.cw.Add((char)139, 333);
                    f.cw.Add((char)140, 944);
                    f.cw.Add((char)141, 350);
                    f.cw.Add((char)142, 611);
                    f.cw.Add((char)143, 350);
                    f.cw.Add((char)144, 350);
                    f.cw.Add((char)145, 333);
                    f.cw.Add((char)146, 333);
                    f.cw.Add((char)147, 500);
                    f.cw.Add((char)148, 500);
                    f.cw.Add((char)149, 350);
                    f.cw.Add((char)150, 500);
                    f.cw.Add((char)151, 1000);
                    f.cw.Add((char)152, 333);
                    f.cw.Add((char)153, 1000);
                    f.cw.Add((char)154, 389);
                    f.cw.Add((char)155, 333);
                    f.cw.Add((char)156, 722);
                    f.cw.Add((char)157, 350);
                    f.cw.Add((char)158, 389);
                    f.cw.Add((char)159, 611);
                    f.cw.Add((char)160, 250);
                    f.cw.Add((char)161, 389);
                    f.cw.Add((char)162, 500);
                    f.cw.Add((char)163, 500);
                    f.cw.Add((char)164, 500);
                    f.cw.Add((char)165, 500);
                    f.cw.Add((char)166, 220);
                    f.cw.Add((char)167, 500);
                    f.cw.Add((char)168, 333);
                    f.cw.Add((char)169, 747);
                    f.cw.Add((char)170, 266);
                    f.cw.Add((char)171, 500);
                    f.cw.Add((char)172, 606);
                    f.cw.Add((char)173, 333);
                    f.cw.Add((char)174, 747);
                    f.cw.Add((char)175, 333);
                    f.cw.Add((char)176, 400);
                    f.cw.Add((char)177, 570);
                    f.cw.Add((char)178, 300);
                    f.cw.Add((char)179, 300);
                    f.cw.Add((char)180, 333);
                    f.cw.Add((char)181, 576);
                    f.cw.Add((char)182, 500);
                    f.cw.Add((char)183, 250);
                    f.cw.Add((char)184, 333);
                    f.cw.Add((char)185, 300);
                    f.cw.Add((char)186, 300);
                    f.cw.Add((char)187, 500);
                    f.cw.Add((char)188, 750);
                    f.cw.Add((char)189, 750);
                    f.cw.Add((char)190, 750);
                    f.cw.Add((char)191, 500);
                    f.cw.Add((char)192, 667);
                    f.cw.Add((char)193, 667);
                    f.cw.Add((char)194, 667);
                    f.cw.Add((char)195, 667);
                    f.cw.Add((char)196, 667);
                    f.cw.Add((char)197, 667);
                    f.cw.Add((char)198, 944);
                    f.cw.Add((char)199, 667);
                    f.cw.Add((char)200, 667);
                    f.cw.Add((char)201, 667);
                    f.cw.Add((char)202, 667);
                    f.cw.Add((char)203, 667);
                    f.cw.Add((char)204, 389);
                    f.cw.Add((char)205, 389);
                    f.cw.Add((char)206, 389);
                    f.cw.Add((char)207, 389);
                    f.cw.Add((char)208, 722);
                    f.cw.Add((char)209, 722);
                    f.cw.Add((char)210, 722);
                    f.cw.Add((char)211, 722);
                    f.cw.Add((char)212, 722);
                    f.cw.Add((char)213, 722);
                    f.cw.Add((char)214, 722);
                    f.cw.Add((char)215, 570);
                    f.cw.Add((char)216, 722);
                    f.cw.Add((char)217, 722);
                    f.cw.Add((char)218, 722);
                    f.cw.Add((char)219, 722);
                    f.cw.Add((char)220, 722);
                    f.cw.Add((char)221, 611);
                    f.cw.Add((char)222, 611);
                    f.cw.Add((char)223, 500);
                    f.cw.Add((char)224, 500);
                    f.cw.Add((char)225, 500);
                    f.cw.Add((char)226, 500);
                    f.cw.Add((char)227, 500);
                    f.cw.Add((char)228, 500);
                    f.cw.Add((char)229, 500);
                    f.cw.Add((char)230, 722);
                    f.cw.Add((char)231, 444);
                    f.cw.Add((char)232, 444);
                    f.cw.Add((char)233, 444);
                    f.cw.Add((char)234, 444);
                    f.cw.Add((char)235, 444);
                    f.cw.Add((char)236, 278);
                    f.cw.Add((char)237, 278);
                    f.cw.Add((char)238, 278);
                    f.cw.Add((char)239, 278);
                    f.cw.Add((char)240, 500);
                    f.cw.Add((char)241, 556);
                    f.cw.Add((char)242, 500);
                    f.cw.Add((char)243, 500);
                    f.cw.Add((char)244, 500);
                    f.cw.Add((char)245, 500);
                    f.cw.Add((char)246, 500);
                    f.cw.Add((char)247, 570);
                    f.cw.Add((char)248, 500);
                    f.cw.Add((char)249, 556);
                    f.cw.Add((char)250, 556);
                    f.cw.Add((char)251, 556);
                    f.cw.Add((char)252, 556);
                    f.cw.Add((char)253, 444);
                    f.cw.Add((char)254, 500);
                    f.cw.Add((char)255, 444);

                    break;
                case "timesi":
                    f.Type = "Core";
                    f.Name = "Times-Italic";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 250);
                    f.cw.Add((char)1, 250);
                    f.cw.Add((char)2, 250);
                    f.cw.Add((char)3, 250);
                    f.cw.Add((char)4, 250);
                    f.cw.Add((char)5, 250);
                    f.cw.Add((char)6, 250);
                    f.cw.Add((char)7, 250);
                    f.cw.Add((char)8, 250);
                    f.cw.Add((char)9, 250);
                    f.cw.Add((char)10, 250);
                    f.cw.Add((char)11, 250);
                    f.cw.Add((char)12, 250);
                    f.cw.Add((char)13, 250);
                    f.cw.Add((char)14, 250);
                    f.cw.Add((char)15, 250);
                    f.cw.Add((char)16, 250);
                    f.cw.Add((char)17, 250);
                    f.cw.Add((char)18, 250);
                    f.cw.Add((char)19, 250);
                    f.cw.Add((char)20, 250);
                    f.cw.Add((char)21, 250);
                    f.cw.Add((char)22, 250);
                    f.cw.Add((char)23, 250);
                    f.cw.Add((char)24, 250);
                    f.cw.Add((char)25, 250);
                    f.cw.Add((char)26, 250);
                    f.cw.Add((char)27, 250);
                    f.cw.Add((char)28, 250);
                    f.cw.Add((char)29, 250);
                    f.cw.Add((char)30, 250);
                    f.cw.Add((char)31, 250);
                    f.cw.Add(' ', 250);
                    f.cw.Add('!', 333);
                    f.cw.Add('"', 420);
                    f.cw.Add('#', 500);
                    f.cw.Add('$', 500);
                    f.cw.Add('%', 833);
                    f.cw.Add('&', 778);
                    f.cw.Add('\'', 214);
                    f.cw.Add('(', 333);
                    f.cw.Add(')', 333);
                    f.cw.Add('*', 500);
                    f.cw.Add('+', 675);
                    f.cw.Add(',', 250);
                    f.cw.Add('-', 333);
                    f.cw.Add('.', 250);
                    f.cw.Add('/', 278);
                    f.cw.Add('0', 500);
                    f.cw.Add('1', 500);
                    f.cw.Add('2', 500);
                    f.cw.Add('3', 500);
                    f.cw.Add('4', 500);
                    f.cw.Add('5', 500);
                    f.cw.Add('6', 500);
                    f.cw.Add('7', 500);
                    f.cw.Add('8', 500);
                    f.cw.Add('9', 500);
                    f.cw.Add(':', 333);
                    f.cw.Add(';', 333);
                    f.cw.Add('<', 675);
                    f.cw.Add('=', 675);
                    f.cw.Add('>', 675);
                    f.cw.Add('?', 500);
                    f.cw.Add('@', 920);
                    f.cw.Add('A', 611);
                    f.cw.Add('B', 611);
                    f.cw.Add('C', 667);
                    f.cw.Add('D', 722);
                    f.cw.Add('E', 611);
                    f.cw.Add('F', 611);
                    f.cw.Add('G', 722);
                    f.cw.Add('H', 722);
                    f.cw.Add('I', 333);
                    f.cw.Add('J', 444);
                    f.cw.Add('K', 667);
                    f.cw.Add('L', 556);
                    f.cw.Add('M', 833);
                    f.cw.Add('N', 667);
                    f.cw.Add('O', 722);
                    f.cw.Add('P', 611);
                    f.cw.Add('Q', 722);
                    f.cw.Add('R', 611);
                    f.cw.Add('S', 500);
                    f.cw.Add('T', 556);
                    f.cw.Add('U', 722);
                    f.cw.Add('V', 611);
                    f.cw.Add('W', 833);
                    f.cw.Add('X', 611);
                    f.cw.Add('Y', 556);
                    f.cw.Add('Z', 556);
                    f.cw.Add('[', 389);
                    f.cw.Add('\\', 278);
                    f.cw.Add(']', 389);
                    f.cw.Add('^', 422);
                    f.cw.Add('_', 500);
                    f.cw.Add('`', 333);
                    f.cw.Add('a', 500);
                    f.cw.Add('b', 500);
                    f.cw.Add('c', 444);
                    f.cw.Add('d', 500);
                    f.cw.Add('e', 444);
                    f.cw.Add('f', 278);
                    f.cw.Add('g', 500);
                    f.cw.Add('h', 500);
                    f.cw.Add('i', 278);
                    f.cw.Add('j', 278);
                    f.cw.Add('k', 444);
                    f.cw.Add('l', 278);
                    f.cw.Add('m', 722);
                    f.cw.Add('n', 500);
                    f.cw.Add('o', 500);
                    f.cw.Add('p', 500);
                    f.cw.Add('q', 500);
                    f.cw.Add('r', 389);
                    f.cw.Add('s', 389);
                    f.cw.Add('t', 278);
                    f.cw.Add('u', 500);
                    f.cw.Add('v', 444);
                    f.cw.Add('w', 667);
                    f.cw.Add('x', 444);
                    f.cw.Add('y', 444);
                    f.cw.Add('z', 389);
                    f.cw.Add('{', 400);
                    f.cw.Add('|', 275);
                    f.cw.Add('}', 400);
                    f.cw.Add('~', 541);
                    f.cw.Add((char)127, 350);
                    f.cw.Add((char)128, 500);
                    f.cw.Add((char)129, 350);
                    f.cw.Add((char)130, 333);
                    f.cw.Add((char)131, 500);
                    f.cw.Add((char)132, 556);
                    f.cw.Add((char)133, 889);
                    f.cw.Add((char)134, 500);
                    f.cw.Add((char)135, 500);
                    f.cw.Add((char)136, 333);
                    f.cw.Add((char)137, 1000);
                    f.cw.Add((char)138, 500);
                    f.cw.Add((char)139, 333);
                    f.cw.Add((char)140, 944);
                    f.cw.Add((char)141, 350);
                    f.cw.Add((char)142, 556);
                    f.cw.Add((char)143, 350);
                    f.cw.Add((char)144, 350);
                    f.cw.Add((char)145, 333);
                    f.cw.Add((char)146, 333);
                    f.cw.Add((char)147, 556);
                    f.cw.Add((char)148, 556);
                    f.cw.Add((char)149, 350);
                    f.cw.Add((char)150, 500);
                    f.cw.Add((char)151, 889);
                    f.cw.Add((char)152, 333);
                    f.cw.Add((char)153, 980);
                    f.cw.Add((char)154, 389);
                    f.cw.Add((char)155, 333);
                    f.cw.Add((char)156, 667);
                    f.cw.Add((char)157, 350);
                    f.cw.Add((char)158, 389);
                    f.cw.Add((char)159, 556);
                    f.cw.Add((char)160, 250);
                    f.cw.Add((char)161, 389);
                    f.cw.Add((char)162, 500);
                    f.cw.Add((char)163, 500);
                    f.cw.Add((char)164, 500);
                    f.cw.Add((char)165, 500);
                    f.cw.Add((char)166, 275);
                    f.cw.Add((char)167, 500);
                    f.cw.Add((char)168, 333);
                    f.cw.Add((char)169, 760);
                    f.cw.Add((char)170, 276);
                    f.cw.Add((char)171, 500);
                    f.cw.Add((char)172, 675);
                    f.cw.Add((char)173, 333);
                    f.cw.Add((char)174, 760);
                    f.cw.Add((char)175, 333);
                    f.cw.Add((char)176, 400);
                    f.cw.Add((char)177, 675);
                    f.cw.Add((char)178, 300);
                    f.cw.Add((char)179, 300);
                    f.cw.Add((char)180, 333);
                    f.cw.Add((char)181, 500);
                    f.cw.Add((char)182, 523);
                    f.cw.Add((char)183, 250);
                    f.cw.Add((char)184, 333);
                    f.cw.Add((char)185, 300);
                    f.cw.Add((char)186, 310);
                    f.cw.Add((char)187, 500);
                    f.cw.Add((char)188, 750);
                    f.cw.Add((char)189, 750);
                    f.cw.Add((char)190, 750);
                    f.cw.Add((char)191, 500);
                    f.cw.Add((char)192, 611);
                    f.cw.Add((char)193, 611);
                    f.cw.Add((char)194, 611);
                    f.cw.Add((char)195, 611);
                    f.cw.Add((char)196, 611);
                    f.cw.Add((char)197, 611);
                    f.cw.Add((char)198, 889);
                    f.cw.Add((char)199, 667);
                    f.cw.Add((char)200, 611);
                    f.cw.Add((char)201, 611);
                    f.cw.Add((char)202, 611);
                    f.cw.Add((char)203, 611);
                    f.cw.Add((char)204, 333);
                    f.cw.Add((char)205, 333);
                    f.cw.Add((char)206, 333);
                    f.cw.Add((char)207, 333);
                    f.cw.Add((char)208, 722);
                    f.cw.Add((char)209, 667);
                    f.cw.Add((char)210, 722);
                    f.cw.Add((char)211, 722);
                    f.cw.Add((char)212, 722);
                    f.cw.Add((char)213, 722);
                    f.cw.Add((char)214, 722);
                    f.cw.Add((char)215, 675);
                    f.cw.Add((char)216, 722);
                    f.cw.Add((char)217, 722);
                    f.cw.Add((char)218, 722);
                    f.cw.Add((char)219, 722);
                    f.cw.Add((char)220, 722);
                    f.cw.Add((char)221, 556);
                    f.cw.Add((char)222, 611);
                    f.cw.Add((char)223, 500);
                    f.cw.Add((char)224, 500);
                    f.cw.Add((char)225, 500);
                    f.cw.Add((char)226, 500);
                    f.cw.Add((char)227, 500);
                    f.cw.Add((char)228, 500);
                    f.cw.Add((char)229, 500);
                    f.cw.Add((char)230, 667);
                    f.cw.Add((char)231, 444);
                    f.cw.Add((char)232, 444);
                    f.cw.Add((char)233, 444);
                    f.cw.Add((char)234, 444);
                    f.cw.Add((char)235, 444);
                    f.cw.Add((char)236, 278);
                    f.cw.Add((char)237, 278);
                    f.cw.Add((char)238, 278);
                    f.cw.Add((char)239, 278);
                    f.cw.Add((char)240, 500);
                    f.cw.Add((char)241, 500);
                    f.cw.Add((char)242, 500);
                    f.cw.Add((char)243, 500);
                    f.cw.Add((char)244, 500);
                    f.cw.Add((char)245, 500);
                    f.cw.Add((char)246, 500);
                    f.cw.Add((char)247, 675);
                    f.cw.Add((char)248, 500);
                    f.cw.Add((char)249, 500);
                    f.cw.Add((char)250, 500);
                    f.cw.Add((char)251, 500);
                    f.cw.Add((char)252, 500);
                    f.cw.Add((char)253, 444);
                    f.cw.Add((char)254, 500);
                    f.cw.Add((char)255, 444);

                    break;
                case "zapfdingbats":
                    f.Type = "Core";
                    f.Name = "ZapfDingbats";
                    f.Up = -100;
                    f.Ut = 50;
                    f.cw.Add((char)0, 0);
                    f.cw.Add((char)1, 0);
                    f.cw.Add((char)2, 0);
                    f.cw.Add((char)3, 0);
                    f.cw.Add((char)4, 0);
                    f.cw.Add((char)5, 0);
                    f.cw.Add((char)6, 0);
                    f.cw.Add((char)7, 0);
                    f.cw.Add((char)8, 0);
                    f.cw.Add((char)9, 0);
                    f.cw.Add((char)10, 0);
                    f.cw.Add((char)11, 0);
                    f.cw.Add((char)12, 0);
                    f.cw.Add((char)13, 0);
                    f.cw.Add((char)14, 0);
                    f.cw.Add((char)15, 0);
                    f.cw.Add((char)16, 0);
                    f.cw.Add((char)17, 0);
                    f.cw.Add((char)18, 0);
                    f.cw.Add((char)19, 0);
                    f.cw.Add((char)20, 0);
                    f.cw.Add((char)21, 0);
                    f.cw.Add((char)22, 0);
                    f.cw.Add((char)23, 0);
                    f.cw.Add((char)24, 0);
                    f.cw.Add((char)25, 0);
                    f.cw.Add((char)26, 0);
                    f.cw.Add((char)27, 0);
                    f.cw.Add((char)28, 0);
                    f.cw.Add((char)29, 0);
                    f.cw.Add((char)30, 0);
                    f.cw.Add((char)31, 0);
                    f.cw.Add(' ', 278);
                    f.cw.Add('!', 974);
                    f.cw.Add('"', 961);
                    f.cw.Add('#', 974);
                    f.cw.Add('$', 980);
                    f.cw.Add('%', 719);
                    f.cw.Add('&', 789);
                    f.cw.Add('\'', 790);
                    f.cw.Add('(', 791);
                    f.cw.Add(')', 690);
                    f.cw.Add('*', 960);
                    f.cw.Add('+', 939);
                    f.cw.Add(',', 549);
                    f.cw.Add('-', 855);
                    f.cw.Add('.', 911);
                    f.cw.Add('/', 933);
                    f.cw.Add('0', 911);
                    f.cw.Add('1', 945);
                    f.cw.Add('2', 974);
                    f.cw.Add('3', 755);
                    f.cw.Add('4', 846);
                    f.cw.Add('5', 762);
                    f.cw.Add('6', 761);
                    f.cw.Add('7', 571);
                    f.cw.Add('8', 677);
                    f.cw.Add('9', 763);
                    f.cw.Add(':', 760);
                    f.cw.Add(';', 759);
                    f.cw.Add('<', 754);
                    f.cw.Add('=', 494);
                    f.cw.Add('>', 552);
                    f.cw.Add('?', 537);
                    f.cw.Add('@', 577);
                    f.cw.Add('A', 692);
                    f.cw.Add('B', 786);
                    f.cw.Add('C', 788);
                    f.cw.Add('D', 788);
                    f.cw.Add('E', 790);
                    f.cw.Add('F', 793);
                    f.cw.Add('G', 794);
                    f.cw.Add('H', 816);
                    f.cw.Add('I', 823);
                    f.cw.Add('J', 789);
                    f.cw.Add('K', 841);
                    f.cw.Add('L', 823);
                    f.cw.Add('M', 833);
                    f.cw.Add('N', 816);
                    f.cw.Add('O', 831);
                    f.cw.Add('P', 923);
                    f.cw.Add('Q', 744);
                    f.cw.Add('R', 723);
                    f.cw.Add('S', 749);
                    f.cw.Add('T', 790);
                    f.cw.Add('U', 792);
                    f.cw.Add('V', 695);
                    f.cw.Add('W', 776);
                    f.cw.Add('X', 768);
                    f.cw.Add('Y', 792);
                    f.cw.Add('Z', 759);
                    f.cw.Add('[', 707);
                    f.cw.Add('\\', 708);
                    f.cw.Add(']', 682);
                    f.cw.Add('^', 701);
                    f.cw.Add('_', 826);
                    f.cw.Add('`', 815);
                    f.cw.Add('a', 789);
                    f.cw.Add('b', 789);
                    f.cw.Add('c', 707);
                    f.cw.Add('d', 687);
                    f.cw.Add('e', 696);
                    f.cw.Add('f', 689);
                    f.cw.Add('g', 786);
                    f.cw.Add('h', 787);
                    f.cw.Add('i', 713);
                    f.cw.Add('j', 791);
                    f.cw.Add('k', 785);
                    f.cw.Add('l', 791);
                    f.cw.Add('m', 873);
                    f.cw.Add('n', 761);
                    f.cw.Add('o', 762);
                    f.cw.Add('p', 762);
                    f.cw.Add('q', 759);
                    f.cw.Add('r', 759);
                    f.cw.Add('s', 892);
                    f.cw.Add('t', 892);
                    f.cw.Add('u', 788);
                    f.cw.Add('v', 784);
                    f.cw.Add('w', 438);
                    f.cw.Add('x', 138);
                    f.cw.Add('y', 277);
                    f.cw.Add('z', 415);
                    f.cw.Add('{', 392);
                    f.cw.Add('|', 392);
                    f.cw.Add('}', 668);
                    f.cw.Add('~', 668);
                    f.cw.Add((char)127, 0);
                    f.cw.Add((char)128, 390);
                    f.cw.Add((char)129, 390);
                    f.cw.Add((char)130, 317);
                    f.cw.Add((char)131, 317);
                    f.cw.Add((char)132, 276);
                    f.cw.Add((char)133, 276);
                    f.cw.Add((char)134, 509);
                    f.cw.Add((char)135, 509);
                    f.cw.Add((char)136, 410);
                    f.cw.Add((char)137, 410);
                    f.cw.Add((char)138, 234);
                    f.cw.Add((char)139, 234);
                    f.cw.Add((char)140, 334);
                    f.cw.Add((char)141, 334);
                    f.cw.Add((char)142, 0);
                    f.cw.Add((char)143, 0);
                    f.cw.Add((char)144, 0);
                    f.cw.Add((char)145, 0);
                    f.cw.Add((char)146, 0);
                    f.cw.Add((char)147, 0);
                    f.cw.Add((char)148, 0);
                    f.cw.Add((char)149, 0);
                    f.cw.Add((char)150, 0);
                    f.cw.Add((char)151, 0);
                    f.cw.Add((char)152, 0);
                    f.cw.Add((char)153, 0);
                    f.cw.Add((char)154, 0);
                    f.cw.Add((char)155, 0);
                    f.cw.Add((char)156, 0);
                    f.cw.Add((char)157, 0);
                    f.cw.Add((char)158, 0);
                    f.cw.Add((char)159, 0);
                    f.cw.Add((char)160, 0);
                    f.cw.Add((char)161, 732);
                    f.cw.Add((char)162, 544);
                    f.cw.Add((char)163, 544);
                    f.cw.Add((char)164, 910);
                    f.cw.Add((char)165, 667);
                    f.cw.Add((char)166, 760);
                    f.cw.Add((char)167, 760);
                    f.cw.Add((char)168, 776);
                    f.cw.Add((char)169, 595);
                    f.cw.Add((char)170, 694);
                    f.cw.Add((char)171, 626);
                    f.cw.Add((char)172, 788);
                    f.cw.Add((char)173, 788);
                    f.cw.Add((char)174, 788);
                    f.cw.Add((char)175, 788);
                    f.cw.Add((char)176, 788);
                    f.cw.Add((char)177, 788);
                    f.cw.Add((char)178, 788);
                    f.cw.Add((char)179, 788);
                    f.cw.Add((char)180, 788);
                    f.cw.Add((char)181, 788);
                    f.cw.Add((char)182, 788);
                    f.cw.Add((char)183, 788);
                    f.cw.Add((char)184, 788);
                    f.cw.Add((char)185, 788);
                    f.cw.Add((char)186, 788);
                    f.cw.Add((char)187, 788);
                    f.cw.Add((char)188, 788);
                    f.cw.Add((char)189, 788);
                    f.cw.Add((char)190, 788);
                    f.cw.Add((char)191, 788);
                    f.cw.Add((char)192, 788);
                    f.cw.Add((char)193, 788);
                    f.cw.Add((char)194, 788);
                    f.cw.Add((char)195, 788);
                    f.cw.Add((char)196, 788);
                    f.cw.Add((char)197, 788);
                    f.cw.Add((char)198, 788);
                    f.cw.Add((char)199, 788);
                    f.cw.Add((char)200, 788);
                    f.cw.Add((char)201, 788);
                    f.cw.Add((char)202, 788);
                    f.cw.Add((char)203, 788);
                    f.cw.Add((char)204, 788);
                    f.cw.Add((char)205, 788);
                    f.cw.Add((char)206, 788);
                    f.cw.Add((char)207, 788);
                    f.cw.Add((char)208, 788);
                    f.cw.Add((char)209, 788);
                    f.cw.Add((char)210, 788);
                    f.cw.Add((char)211, 788);
                    f.cw.Add((char)212, 894);
                    f.cw.Add((char)213, 838);
                    f.cw.Add((char)214, 1016);
                    f.cw.Add((char)215, 458);
                    f.cw.Add((char)216, 748);
                    f.cw.Add((char)217, 924);
                    f.cw.Add((char)218, 748);
                    f.cw.Add((char)219, 918);
                    f.cw.Add((char)220, 927);
                    f.cw.Add((char)221, 928);
                    f.cw.Add((char)222, 928);
                    f.cw.Add((char)223, 834);
                    f.cw.Add((char)224, 873);
                    f.cw.Add((char)225, 828);
                    f.cw.Add((char)226, 924);
                    f.cw.Add((char)227, 924);
                    f.cw.Add((char)228, 917);
                    f.cw.Add((char)229, 930);
                    f.cw.Add((char)230, 931);
                    f.cw.Add((char)231, 463);
                    f.cw.Add((char)232, 883);
                    f.cw.Add((char)233, 836);
                    f.cw.Add((char)234, 836);
                    f.cw.Add((char)235, 867);
                    f.cw.Add((char)236, 867);
                    f.cw.Add((char)237, 696);
                    f.cw.Add((char)238, 696);
                    f.cw.Add((char)239, 874);
                    f.cw.Add((char)240, 0);
                    f.cw.Add((char)241, 874);
                    f.cw.Add((char)242, 760);
                    f.cw.Add((char)243, 946);
                    f.cw.Add((char)244, 771);
                    f.cw.Add((char)245, 865);
                    f.cw.Add((char)246, 771);
                    f.cw.Add((char)247, 888);
                    f.cw.Add((char)248, 967);
                    f.cw.Add((char)249, 888);
                    f.cw.Add((char)250, 831);
                    f.cw.Add((char)251, 873);
                    f.cw.Add((char)252, 927);
                    f.cw.Add((char)253, 970);
                    f.cw.Add((char)254, 918);
                    f.cw.Add((char)255, 0);

                    break;
            }
            return f;
        }


        private string Escape(string s)
        {
            s = s.Replace("\\", "\\\\");
            s = s.Replace("(", "\\(");
            s = s.Replace(")", "\\)");
            s = s.Replace("\r", "\\r");
            return s;
        }

        private string TextString(string s)
        {
            return "(" + Escape(s) + ")";
        }

        public class ImageInformation
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public string Channels { get; set; }
            public int Bits { get; set; }
            public string F { get; set; }
            public string Data { get; set; }
            public string dp { get; set; }
            public string pal { get; set; }
            public List<int> trns { get; set; }
            public string smask { get; set; }
            public int i { get; set; }
            public int n { get; set; }
        }

        private ImageInformation ParsePng(string file)
        {
            var f = File.OpenRead(file);
            //var qweret = new System.Windows.Media.Imaging.PngBitmapDecoder(f, BitmapCreateOptions.PreservePixelFormat,
            //    BitmapCacheOption.Default);
            var info = this.ParsePngStream(f, file);
            f.Close();
            return info;
        }

        private ImageInformation ParsePngStream(FileStream f, string file)
        {
            var firstChars = this.ReadStream(ref f, 8);
            if (firstChars != (char)137 + "PNG" + (char)13 + (char)10 + (char)26 + (char)10)
            {
                throw new Exception("Not a PNG file: " + file);
            }

            //read header chunk
            this.ReadStream(ref f, 4);
            if (this.ReadStream(ref f, 4) != "IHDR")
                throw new Exception("Incorrect PNG file: " + file);
            var _w = this.ReadInt(ref f);
            var _h = this.ReadInt(ref f);
            var bpc = Ord(ReadStream(ref f, 1));
            if(bpc > 8)
                throw new Exception("16-bit depth not supported: " + file);
            var ct = Ord(ReadStream(ref f, 1));
            var colspace = "";
            if (ct == 0 || ct == 4)
                colspace = "DeviceGray";
            else if (ct == 2 || ct == 6)
                colspace = "DeviceRGB";
            else if (ct == 3)
                colspace = "Indexed";
            else
            {
                throw new Exception("Unknown color type: " + file);
            }
            if (Ord(this.ReadStream(ref f, 1)) != 0)
                throw new Exception("Unknown compression method: " + file);
            if (Ord(this.ReadStream(ref f, 1)) != 0)
                throw new Exception("Unknown filter method: " + file);
            if (Ord(this.ReadStream(ref f, 1)) != 0)
                throw new Exception("Interlacing not supported: " + file);
            this.ReadStream(ref f, 4);
            var dp = "/Predictor 15 /Colors " + (colspace == "DeviceRGB" ? 3 : 1) + " /BitsPerComponent " + bpc +
                     " /Columns " + _w;


            // scan chunks looking for palette, transparency and image data
            var pal = "";
            var trns = new List<int>();
            var data = "";
            var _n = -1;
            do
            {
                _n = this.ReadInt(ref f);
                var type = this.ReadStream(ref f, 4);
                if (type == "PLTE")
                {
                    //read palette
                    pal = this.ReadStream(ref f, _n, true);
                    this.ReadStream(ref f, 4);
                }
                else if (type == "tRNS")
                {
                    var _t = this.ReadStream(ref f, _n);
                    if (ct == 0)
                        trns.Add(Ord(_t.Substring(1, 1)));
                    else if (ct == 2)
                    {
                        trns.Add(Ord(_t.Substring(1, 1)));
                        trns.Add(Ord(_t.Substring(3, 1)));
                        trns.Add(Ord(_t.Substring(5, 1)));
                    }
                    else
                    {
                        var pos = _t.IndexOf((char) 0);
                        if (pos > -1)
                            trns.Add(pos);
                    }
                    this.ReadStream(ref f, 4);
                }
                else if (type == "IDAT")
                {
                    // read image data block
                    data += this.ReadStream(ref f, _n, true);
                    this.ReadStream(ref f, 4);
                }
                else if (type == "IEND")
                    break;
                else
                {
                    this.ReadStream(ref f, _n + 4);
                }
            } while (_n > -1);

            if (colspace == "Indexed" && pal == string.Empty)
            {
                throw new Exception("Missing pallette in " + file);
            }

            var info = new ImageInformation
            {
                Width = _w,
                Height = _h,
                Channels = colspace,
                Bits = bpc,
                F = "FlateDecode",
                dp = dp,
                pal = pal,
                trns = trns
            };
            if (ct > 4)
            {
                data = UnZip(data);
                var color = "";
                var alpha = "";
                if (ct == 4)
                {
                    //grey image
                    var len = 2*_w;
                    for (var i = 0; i < _h; i++)
                    {
                        var pos = (1 + len)*i;
                        color += data[pos];
                        alpha += data[pos];
                        var line = data.Substring(pos + 1, len);
                        color += Regex.Replace(line, "/(.)./s", "$1");
                        alpha += Regex.Replace(line, "/.(.)/s", "$1");
                    }
                }
                else
                {
                    // RGB Image
                    var len = 4*_w;
                    for (var i = 0; i < _h; i++)
                    {
                        var pos = (1 + len)*i;
                        color += data[pos];
                        alpha += data[pos];
                        var line = data.Substring(pos + 1, len);
                        color += Regex.Replace(line, "/(.{3})./s", "$1");
                        alpha += Regex.Replace(line, "/.{3}(.)/s", "$1");
                    }
                }
                data = null;
                data = Zip(color);
                info.smask = Zip(alpha);
                if (this.PDFVersion < 1.4)
                    this.PDFVersion = 1.4;
            }
            info.Data = data;
            return info;
        }

        private int Ord(string s)
        {
            return (int) s[0];
        }

        private int ReadInt(ref FileStream f)
        {
            var buffer = new byte[4];
            f.Read(buffer, 0, 4);
            buffer = buffer.Reverse().ToArray();
            return BitConverter.ToInt16(buffer, 0);
        }

        private ImageInformation ParseJpg(string file)
        {
            var bmp = new Bitmap(file);
            var props = bmp.PropertyItems;
            var flags = (ImageFlags)bmp.Flags;
            var colSpace = "DeviceRGB";
            if (flags.HasFlag(ImageFlags.ColorSpaceCmyk))
                colSpace = "DeviceCMYK";
            else if (flags.HasFlag(ImageFlags.ColorSpaceGray))
                colSpace = "DeviceGray";
            var memStream = new MemoryStream();
            bmp.Save(memStream, ImageFormat.Bmp);
            return new ImageInformation
            {
             
                Height = bmp.Height,
                Width = bmp.Width,
                Channels = colSpace,
                Bits = 8,
                F = "DCTDecode",
                Data = Convert.ToBase64String(memStream.ToArray())
            };

        }


        private string ReadStream(ref FileStream f, int n, bool parseToUtf8 = false)
        {
            var s = "";
            var buffer = new byte[n];
            var bytesRead = f.Read(buffer, 0, n);
            if (parseToUtf8 && false)
            {
                var s2 = System.Text.Encoding.UTF8.GetString(buffer);
                return s2;
            }
            foreach (var b in buffer)
            {
                s += (char) b;
            }
            return s;
        }

        private void NewObj()
        {
            n++;
            if (this.Offsets.ContainsKey(n))
            {
                Offsets[n] = this.Buffer.Length;
            }
            else
            {
                Offsets.Add(n, this.Buffer.Length);
            }
            Out(n + " 0 obj");
        }

        private void PutStream(string s)
        {
            Out("stream");
            Out(s);
            Out("endstream");
        }

        private void Out(string s)
        {
            if (State == 2)
                Pages[Page] += s + "\n";
            else
            {
                Buffer += s + "\n";
            }
        }

        private void PutPages()
        {
            var nb = Page;
            if (this.AliasNbPages != null)
            {
                for (var n = 1; n <= nb; n++)
                {
                    this.Pages[n] = this.Pages[n].Replace(this.AliasNbPages, nb.ToString());
                }
            }
            double _wPt;
            double _hPt;
            if (this.DefOrientation == Orientation.Portrait)
            {
                _wPt = this.DefPageSize.ShortSide*this.k;
                _hPt = this.DefPageSize.LongSide*this.k;
            }
            else
            {
                _wPt = this.DefPageSize.LongSide*this.k;
                _hPt = this.DefPageSize.ShortSide*this.k;
            }
            var filter = (this.Compress) ? "/Filter /FlateDecode " : "";
            for (var n = 1; n <= nb; n++)
            {
                this.NewObj();
                this.Out("<</Type /Page");
                this.Out("/Parent 1 0 R");
                if (this.PageSizes.Count > n)
                {
                    this.Out(string.Format("/MediaBox [0 0 {0:0.00} {1:0.00}]", this.PageSizes[n].ShortSide, this.PageSizes[n].LongSide));
                }
                this.Out("/Resources 2 0 R");
                if (this.PageLinks.ContainsKey(n))
                {
                    var annots = "/Annots [";
                    foreach (var plk in this.PageLinks.Keys)
                    {
                        var pl = this.PageLinks[plk];
                        var rect = string.Format("{0:0.00} {1:0.00} {2:0.00} {3:0.00}", pl.x, pl.y, pl.x + pl.w, pl.y - pl.h);
                        annots += "<</Type /Annot /Subtype /Link /Rect [" + rect + "] /Border [0 0 0] ";
                        annots += "/A <</S /URI /URI " + this.TextString(pl.Link) + ">>>>";

                    }
                    this.Out(annots + "]");
                }



                if (this.PDFVersion > 1.3)
                    this.Out("/Group <</Type /Group /S /Transparency /CS /DeviceRGB>>");
                this.Out("/Contents " + (this.n + 1) + " 0 R>>");
                this.Out("endobj");
                //Page content
                var p = this.Compress ? Zip(this.Pages[n]) : this.Pages[n];
                //var p = this.Pages[n];
                this.NewObj();
                this.Out("<<" + filter + "/Length " + p.Length + ">>");
                this.PutStream(p);
                this.Out("endobj");
            }
            if (Offsets.ContainsKey(1))
            {
                this.Offsets[1] = this.Buffer.Length;
            }
            else
            {
                this.Offsets.Add(1, this.Buffer.Length);
            }
            
            this.Out("1 0 obj");
            this.Out("<</Type /Pages");
            var kids = "/Kids [";
            for (var i = 0; i < nb; i++)
            {
                kids += (3 + 2*i) + " 0 R ";
            }
            this.Out(kids + "]");
            this.Out("/Count " + nb);
            this.Out(string.Format("/MediaBox [0 0 {0:0.00} {1:0.00}]", _wPt, _hPt));
            this.Out(">>");
            this.Out("endobj");
        }
        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static string UnZip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Decompress))
                {
                    CopyTo(msi, gs);
                }
                return Encoding.UTF8.GetString(mso.ToArray(), 0, mso.ToArray().Length);
            }
        }

        public static string Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return Encoding.UTF8.GetString(mso.ToArray(), 0, mso.ToArray().Length);
            }
        }
        private string DoUnderline(double _x, double _y, string txt)
        {
            var up = CurrentFont.Up;
            var ut = CurrentFont.Ut;
            var _w = GetStringWidth(txt) + ws*txt.Count(i => i == ' ');
            return string.Format("{0:0.00} {1:0.00} {2:0.00} {3:0.00} re f", _x * k, (h - (_y - up / 1000 * FontSize)) * k, _w * k,
                (0 - ut)/1000*FontSizePt);
        }

        private void Putfonts()
        {
            var nf = this.n;
            foreach (var diff in this.diffs)
            {
                this.NewObj();
                this.Out("<</Type /Encoding /BaseEncoding /WinAnsiEncoding /Differences [" + diff + "]>>");
                this.Out("endobj");
            }
            foreach (var k in this.FontFiles.Keys)
            {
                var file = this.FontFiles[k];
                var f = new System.IO.FileInfo(k);
                this.NewObj();
                file.n = this.n;
                var font = System.IO.File.ReadAllLines(this.fontpath + file);
                if (!string.IsNullOrEmpty(k))
                {
                    throw new Exception("Font file not found " + file);
                }
                var compressed = k.Substring(-2) == ".z";
                //todo dont know what all this info stuff is.....
                this.Out("<</Length " + k.Length);
                if(compressed)
                    this.Out("/Filter /FlatDecode");
               //TODO this.Out("/Length1 " + InFooter.en);
                this.Out(">>");
                this.PutStream(k);
                this.Out("endobj");
            }
            foreach (var font in this.fonts)
            {
                font.Value.n = this.n + 1;
                var type = font.Value.Type;
                var name = font.Value.Name;
                if (type == "Core")
                {
                    this.NewObj();
                    this.Out("<</Type /Font");
                    this.Out("/BaseFont /" + name);
                    this.Out("/Subtype /Type1");
                    if(name != "Symbol" && name != "ZapfDingbats")
                        this.Out("/Encoding /WinAnsiEncoding");
                    this.Out(">>");
                    this.Out("endobj");
                }
                else if (type == "Type1" || type == "TrueType")
                {
                    this.NewObj();
                    this.Out("<</Type /Font");
                    this.Out("/BaseFont /" + name);
                    this.Out("/Subtype /" + type);
                    this.Out("/FirstChar 32 /LastChar 255");
                    this.Out("/Widths " + (this.n + 1) + " 0 R");
                    this.Out("/FontDescriptor " + (this.n + 2) + " 0 R");
                    //TODO font.diffn
                    this.Out("/Encoding /WinAnsiEncoding");
                    this.Out(">>");
                    this.Out("endobj");
                    this.NewObj();
                    var cw = font.Value.cw;
                    var s = "[";
                    for (var i = 32; i <= 255; i++)
                    {
                        s += cw[(char) i] + " ";
                    }
                    this.Out(s + "]");
                    this.Out("endobj");
                    this.NewObj();
                    s = "<</Type /FontDescriptor /FontName /" + name;
                    // TODO font.desc ??
                    //foreach (d VARIABLE in font.Value.)
                    //{

                    //}
                    //TODO dont.file
                    this.Out(s + ">>");
                    this.Out("enobj");
                }
                else
                {
                    //TODO allow for additional types
                    var mtd = "_put" + type.ToLower();

                }
            }
        }


        private void PutXObjectDict()
        {
            foreach (var k in this.images.Keys)
            {
                var image = images[k];
                this.Out("/I" + image.i + " " + image.n + " 0 R");
            }
        }

        private void PutResourceDict()
        {
            this.Out("/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]");
            this.Out("/Font <<");
            foreach (var k in this.fonts.Keys)
            {
                var font = this.fonts[k];
                this.Out("/F" + font.i + " " + font.n + " 0 R");
            }
            this.Out(">>");
            this.Out("/XObject <<");
            this.PutXObjectDict();
            this.Out(">>");
        }

        private void PutResources()
        {
            this.Putfonts();
            this.PutImages();
            if (this.Offsets.ContainsKey(2))
            {
                this.Offsets[2] = this.Buffer.Length;
            }
            else
            {
                this.Offsets.Add(2, this.Buffer.Length);
            }
            this.Out("2 0 obj");
            this.Out("<<");
            this.PutResourceDict();
            this.Out(">>");
            this.Out("endobj");
        }

        private void PutImages()
        {
            foreach (var image in this.images)
            {
                this.PutImage(image.Value);
                image.Value.Data = null;
                image.Value.smask = null;
            }
        }

        private void PutImage(ImageInformation info)
        {
            this.NewObj();
            info.n = this.n;
            this.Out("<</Type /XObject");
            this.Out("/Subtype /Image");
            this.Out("/Width " + info.Width);
            this.Out("/Height " + info.Height);
            if (info.Channels == "Indexed")
            {
                this.Out("/ColorSpace [/Indexed /DeviceRGB " + (info.pal.Length/3 - 1) + " " + (this.n + 1) + " 0 R]");
            }
            else
            {
                this.Out("/ColorSpace /" + info.Channels);
                if(info.Channels == "DeviceCMYK")
                    this.Out("/Decode [1 0 1 0 1 0 1 0]");
            }
            this.Out("/BitsPerComponent " + info.Bits);
            if(info.F != null)
                this.Out("/Filter /" + info.F);
            if(info.dp != null)
                this.Out("/DecodeParms <<" + info.dp + ">>");
            if (info.trns != null)
            {
                var trns = "";
                for (var i = 0; i < info.trns.Count; i++)
                    trns += info.trns[i] + " " + info.trns[i] + " ";
                this.Out("/Mask [" + trns + "]");
            }
            if (info.smask != null)
            {
                this.Out("/SMask " + (this.n + 1) + " 0 R");
            }
            this.Out("/Length " + info.Data.Length + ">>");
            this.PutStream(info.Data);
            this.Out("endobj");
            if (info.smask != null)
            {
                var dp = "/Predictor 15 /Colors 1 /BitsPerComponent 8 /Columns " + info.Width;
                var smask = new ImageInformation
                {
                    Width = info.Width,
                    Height = info.Height,
                    Channels = "DeviceGray",
                    Bits = 8,
                    F = info.F,
                    dp = dp,
                    Data = info.smask
                };
                this.PutImage(smask);
            }
            if (info.Channels == "Indexed")
            {
                var filter = this.Compress ? "/Filter /FlatDecode " : "";
                var pal = this.Compress ? "TODO COMPRESS" : info.pal;
                this.NewObj();
                this.Out("<<" + filter + "/Length " + pal.Length + ">>");
                this.PutStream(pal);
                this.Out("endobj");
            }
        }

        private void PutInfo()
        {
            this.Out("/Producer " + this.TextString("FPDF " + FPDF_VERSION));
            if(!string.IsNullOrEmpty(this.Title))
                this.Out("/Title " + this.TextString(this.Title));
            if(!string.IsNullOrEmpty(this.Subject))
                this.Out("/Subject " + this.TextString(this.Subject));
            if(!string.IsNullOrEmpty(this.Author))
                this.Out("/Author " + this.TextString(this.Author));
            if(!string.IsNullOrEmpty(this.Keywords))
                this.Out("/Keywords " + this.TextString(this.Keywords));
            if(!string.IsNullOrEmpty(this.Creator))
                this.Out("/Creator " + this.TextString(this.Creator));
            this.Out("/CreationDate " + this.TextString("D:" + DateTime.Now.ToString("yyyyMMddHHmmss")));
        }

        private void PutCatalog()
        {
            this.Out("/Type /Catalog");
            this.Out("/Pages 1 0 R");
            if(this.ZoomMode == ZoomLevel.FullPage)
                this.Out("/OpenAction [3 0 R /Fit]");
            else if(this.ZoomMode == ZoomLevel.FullWidth)
                this.Out("/OpenAction [3 0 R /FitH null]");
            else if(this.ZoomMode == ZoomLevel.Real)
                this.Out("/OpenAction [3 0 R /XYZ null null 1]");
            if(this.LayoutMode == Layout.Single)
                this.Out("/PageLayout /SinglePage");
            else if(this.LayoutMode == Layout.Continuous)
                this.Out("/PageLayout /OneColumn");
            else if(this.LayoutMode == Layout.Two)
                this.Out("/PageLayout /TwoColumnLeft");
        }

        private void PutHeader()
        {
            this.Out("%PDF-" + this.PDFVersion);
        }

        private void PutTrailer()
        {
            this.Out("/Size " + (this.n + 1));
            this.Out("/Root " + this.n + " 0 R");
            this.Out("/Info " + (this.n - 1) + " 0 R");
        }

        private void EndDoc()
        {
            this.PutHeader();
            this.PutPages();
            this.PutResources();
            //Info
            this.NewObj();
            this.Out("<<");
            this.PutInfo();
            this.Out(">>");
            this.Out("endobj");
            //Catalog
            this.NewObj();
            this.Out("<<");
            this.PutCatalog();
            this.Out(">>");
            this.Out("endobj");
            //Cross-ref
            var o = this.Buffer.Length;
            this.Out("xref");
            this.Out("0 " + (this.n + 1));
            this.Out("0000000000 65535 f ");
            for (var i = 1; i <= this.n; i++)
            {
                this.Out(string.Format("{0} 00000 n ", this.Offsets[i].ToString("0000000000")));
            }
            //Trailer
            this.Out("trailer");
            this.Out("<<");
            this.PutTrailer();
            this.Out(">>");
            this.Out("startxref");
            this.Out(o.ToString());
            this.Out("%%EOF");
            this.State = 3;

        }
    }
}
