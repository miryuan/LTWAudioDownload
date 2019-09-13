using LTWAudioDownload.Entitys;
using SQLiteSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace LTWAudioDownload
{
    public class DataHelper
    {
        static SqlSugarClient db = new SqlSugarClient(@"Data Source=D:\Download\db.sqlite;");

        /// <summary>
        /// 找到第一条符合的数据
        /// </summary>
        /// <param name="bookid"></param>
        /// <returns></returns>
        public static t_book FindBook(string bookid)
        {
            return db.Queryable<t_book>().Where(c => c.bookid == bookid).First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookid"></param>
        /// <returns></returns>
        public static bool CheckBook(string bookid)
        {
            int count = db.Queryable<t_book>().Where(c => c.bookid == bookid).Count();
            return count > 0 ? true : false;
        }

        /// <summary>
        /// 添加一条记录
        /// </summary>
        /// <param name="bookid"></param>
        /// <param name="booktitle"></param>
        public static void AddBook(string bookid, string booktitle)
        {
            var ret = db.Insert(new t_book() { bookid = bookid, booktitle = booktitle });
        }

        /// <summary>
        /// 查找章节数量
        /// </summary>
        /// <param name="bookid"></param>
        /// <returns></returns>
        public static int FindChapterCount(string bookid)
        {
            return db.Queryable<t_bookchapter>().Where(c => c.bookid == bookid).Count();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookid"></param>
        /// <param name="chapterid"></param>
        /// <returns></returns>
        public static bool CheckBookChapter(string bookid, string chapterid)
        {
            int count = db.Queryable<t_bookchapter>().Where(c => c.bookid == bookid && c.chapterid == chapterid).Count();
            return count > 0 ? true : false;
        }

        /// <summary>
        /// 添加一条记录
        /// </summary>
        /// <param name="bookid"></param>
        /// <param name="chapterid"></param>
        public static void AddBookChapter(string bookid, string chapterid)
        {
            var ret = db.Insert<t_bookchapter>(new t_bookchapter() { bookid = bookid, chapterid = chapterid, time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }
    }
}
