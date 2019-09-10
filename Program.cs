using HtmlAgilityPack;
using LTWAudioDownload.Entitys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace LTWAudioDownload
{
    class Program
    {
        static Dictionary<string, string> headers = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3897.0 Safari/537.36 Edg/78.0.272.0");
            headers.Add("Accept-Encoding", "gzip, deflate, br");
            headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");

            bool isFindBook = false;
            do
            {
                string bookid = ReadBookID();
                string urlbase = "https://ting55.com/book/" + bookid;
                var webGet = new HtmlWeb();
                var doc = webGet.Load(urlbase);//book page
                if (doc == null || doc.DocumentNode == null)
                {
                    Console.WriteLine("找不到页面.");
                    continue;
                }
                string booktitle = doc.DocumentNode.SelectSingleNode(".//div[@class='binfo']/h1").InnerText + "[" + bookid + "]";
                string localPath = CreateDirectory(booktitle);

                if (!DataHelper.CheckBook(bookid))
                    DataHelper.AddBook(bookid, booktitle);

                var audioPageNums = FindAudioPages(doc);
                Console.WriteLine("找到了" + audioPageNums.Count + "个文件.");
                foreach (var num in audioPageNums)
                {
                    try
                    {
                        if (DataHelper.CheckBookChapter(bookid, num))
                            continue;

                        //真实的地址
                        var audioFileResult = GetAudioRealUrl(bookid, num);
                        Console.WriteLine("开始下载[" + booktitle + "][" + num + "]文件.");
                        var flag = Download(num, audioFileResult, localPath);
                        if (flag)
                        {
                            Console.WriteLine("下载文件成功,文件已经保存在" + localPath + "中.");
                            DataHelper.AddBookChapter(bookid, num);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("下载文件失败:" + e.Message);
                    }
                }
            } while (!isFindBook);

            while (true)
            {
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="booktitle"></param>
        static string CreateDirectory(string booktitle)
        {
            string path = @"Download\" + booktitle + "\\";

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// 从输入端获取书编号
        /// </summary>
        /// <returns></returns>
        static string ReadBookID()
        {
            string bookID = "";
            do
            {
                Console.Write("复制编号到这里:");
                bookID = Console.ReadLine();
            } while (bookID.Trim().Length == 0);
            return bookID;
        }



        //https://ting55.com/book/4316
        /// <summary>
        /// 音频列表
        /// </summary>
        /// <param name="page">页面Html</param>
        /// <returns></returns>
        static List<string> FindAudioPages(HtmlAgilityPack.HtmlDocument page)
        {
            List<string> pageid = new List<string>();
            var meat_property_List = page.DocumentNode.SelectNodes(".//div[@class='plist']/ul/li");
            foreach (var item in meat_property_List)
            {
                pageid.Add(item.InnerText);
            }
            return pageid;
        }



        /// <summary>
        /// 获取真实的文件地址
        /// </summary>
        /// <param name="bookid"></param>
        /// <param name="pageNum"></param>
        static AudioFileResult GetAudioRealUrl(string bookid, string pageNum)
        {
            string json = HttpHelper.HttpPostAsync(
                string.Format("https://ting55.com/glink?bookId={0}&isPay=0&page={1}&ha=t&mid=12500", bookid, pageNum), headers: headers
                ).Result;

            return Newtonsoft.Json.JsonConvert.DeserializeObject<AudioFileResult>(json);
        }

        /// <summary>
        /// Http方式下载文件
        /// </summary>
        /// <param name="chapterid">章节编号</param>
        /// <param name="AudioFileResult">文件信息</param>
        /// <param name="localPath">本地文件夹</param>
        /// <returns></returns>
        static bool Download(string chapterid, AudioFileResult audioInfo, string localPath)
        {
            string netUrl = "";
            if (audioInfo.ourl.Trim().Length > 0)
                netUrl = audioInfo.ourl;
            else if (audioInfo.plink.Trim().Length > 0)
                netUrl = audioInfo.plink;
            else
                netUrl = audioInfo.url;

            bool flag = false;
            long startPosition = 0; // 上次下载的文件起始位置
            FileStream writeStream = null; // 写入本地文件流对象
            var file = new System.IO.FileInfo(netUrl);
            string newFileName = chapterid + file.Extension;
            string newFilePath = localPath + newFileName;

            // 判断要下载的文件夹是否存在
            if (!File.Exists(newFilePath))
            {
                writeStream = new FileStream(newFilePath, FileMode.Create);// 文件不保存创建一个文件
                startPosition = 0;
            }

            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(netUrl);// 打开网络连接
                if (startPosition > 0)
                {
                    myRequest.AddRange((int)startPosition);// 设置Range值,与上面的writeStream.Seek用意相同,是为了定义远程文件读取位置
                }

                Stream readStream = myRequest.GetResponse().GetResponseStream();// 向服务器请求,获得服务器的回应数据流

                byte[] btArray = new byte[512];// 定义一个字节数据,用来向readStream读取内容和向writeStream写入内容
                int contentSize = readStream.Read(btArray, 0, btArray.Length);// 向远程文件读第一次

                while (contentSize > 0)// 如果读取长度大于零则继续读
                {
                    writeStream.Write(btArray, 0, contentSize);// 写入本地文件
                    contentSize = readStream.Read(btArray, 0, btArray.Length);// 继续向远程文件读取
                }

                //关闭流
                writeStream.Close();
                readStream.Close();

                flag = true;        //返回true下载成功
            }
            catch (Exception)
            {
                writeStream.Close();
                flag = false;       //返回false下载失败
            }

            return flag;
        }
    }
}
