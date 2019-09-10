using System;

namespace LTWAudioDownload.Entitys
{
    public class t_bookchapter
    {
        public string chapterid { get; set; }
        public string bookid { get; set; }
        /// <summary>
        /// 下载时间
        /// </summary>
        public string time { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
