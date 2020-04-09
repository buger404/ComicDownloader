using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComicDownloader
{
    class Program
    {
        public static bool loaded = false;
        [STAThread]
        public static void Main(string[] args)
        {
        restart:
            Console.WriteLine("Comic downloader\n1.Mangabz\n2.动漫之家");
            Console.Write("Input:");
            string engine = Console.ReadLine();
            if (engine != "1" && engine != "2") { goto restart; }
            Console.Write("Input comic id:");
            string id = Console.ReadLine();
            Console.Write("Name for comic:");
            string name = Console.ReadLine();
            if (!Directory.Exists(@"D:\Comic\" + name)) { Directory.CreateDirectory(@"D:\Comic\" + name); }
            WebBrowser wb = new WebBrowser();
            wb.DocumentCompleted += Wb_DocumentCompleted;
            wb.Navigate("http://www.mangabz.com/" + id);
            Console.Write("Connecting " + "http://www.mangabz.com/" + id);
            loaded = false; do { Application.DoEvents(); } while (!loaded);
            List<string> links = new List<string>();
            foreach(HtmlElement he in wb.Document.GetElementsByTagName("a"))
            {
                string link = null;
                link = he.GetAttribute("href");
                if(link != null) 
                {
                    if (link.StartsWith("http://www.mangabz.com/m") == true && link.StartsWith("http://www.mangabz.com/ma") == false)
                    {
                        links.Add(link);
                    }
                }
            }
            Console.WriteLine("\n" + links.Count + " chapters avaliable .");
            int Ticks = 0;
            foreach(string link in links)
            {
                Console.WriteLine("fetching " + link);
                wb.Navigate(link);
                loaded = false; do { Application.DoEvents(); } while (!loaded);
                Console.WriteLine("fetched " + wb.DocumentTitle);
                string path = "D:\\Comic\\" + name + "\\" + wb.DocumentTitle;
                Ticks = 1;
                if (!Directory.Exists(path)) 
                { 
                    Directory.CreateDirectory(path);
                    do
                    {
                        FakeSleep(200);
                        WebClient wc = new WebClient();
                        string url = "";
                        string fname = Ticks.ToString();
                        if (fname.Length == 1) { fname = "0" + fname; }
                        int fail = 0;
                        retry:
                        foreach (HtmlElement he in wb.Document.GetElementsByTagName("img"))
                        {
                            url = he.GetAttribute("src");
                            if(url != null)
                            {
                                if (url.StartsWith("http://image.mangabz.com/") && url.IndexOf("&key=") >= 0) { break; }
                            }
                            url = "";
                        }
                        if (url == "") 
                        {
                            fail++;
                            if(fail < 10)
                            {
                                Console.WriteLine("Connection timed-out (" + fail + ") , retry ...");
                                FakeSleep(20);
                                goto retry;
                            }
                            else
                            {
                                Console.WriteLine("Failed !"); goto exitdo;
                            }
                        }
                        Console.WriteLine("get " + url);
                        wc.DownloadFile(url,
                                        path + "\\" + fname + ".jpg");
                        Console.WriteLine("Downloaded " + path + "\\" + fname + ".jpg");
                        wc.Dispose();
                        wb.Document.InvokeScript("ShowNext");
                        Ticks++;
                        foreach (HtmlElement he in wb.Document.GetElementsByTagName("div"))
                        {
                            if (he.InnerText != null) 
                            {
                                if (he.InnerText.IndexOf("当前章节已读完") >= 0) { goto exitdo; }
                            }
                        }
                    } while (true);
                exitdo:
                    Console.WriteLine("Read done !");
                }
                else
                {
                    Console.WriteLine("already exists , skiped .");
                }
            }
            wb.DocumentCompleted -= Wb_DocumentCompleted;
            wb.Dispose();
            goto restart;
        }
        private static void FakeSleep(int Tick)
        {
            for(int i = 1;i <= Tick; i++)
            {
                Thread.Sleep(16);
                Application.DoEvents();
            }
        }
        private static void Wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            loaded = true;
        }
    }
}
