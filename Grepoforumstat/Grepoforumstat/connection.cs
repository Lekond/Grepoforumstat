using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Grepoforumstat
{
    /**
    @class connection
    класс для отправки запросов по сети
    */
    class connection
    {
        /**
	    @function connection
	    Конструктор
	    */
        public connection()
        { }

        /**
	    @function ~connection
	    Деструктор
	    */
        ~connection()
        { }

        /**
	    @function SetResponce
	    Функция отправки запроса на сервер
	    @param in url - адрес запроса
	    @param in world - мир
	    @param in request - текст запроса
	    @param in cookies - передаваемые куки (если есть)
	    @param in post - является ли запрос запросом post
	    @param in refer - заголовок Reference, если не по умолчанию
        @param out cook - куки ответа
        @param out head - заголовок Location ответа
	    @return строка, содержащая ответ сервера
	    */
        public string SetResponce(string url, string world, string request, CookieContainer cookies, bool post, string refer, out string cook, out string head)
        {
            //создание объекта
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            //заголовки
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.ContentType = "application/x-www-form-urlencoded";
            if (refer==null)
                req.Referer = "http://" + world.Substring(0, 2) + ".grepolis.com/";
            else
                req.Referer = refer;
            req.Host = world + ".grepolis.com";
            req.UserAgent = "Mozilla/5.0(Windows NT 6.3; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
            req.Headers.Add("Accept-Language: ru-RU, ru; q=0.8, en-US; q=0.5, en; q=0.3");
            req.Headers.Add("X-Requested-With: XMLHttpRequest");
            req.AllowAutoRedirect = false;
            //метод
            if (post)
                req.Method = "POST";
            else
                req.Method = "GET";
            //куки в запрос
            req.CookieContainer = cookies;
            //запрос
            if (request != null)
            {
                byte[] ByteArr = System.Text.Encoding.GetEncoding(65001).GetBytes(request);
                req.ContentLength = ByteArr.Length;
                req.GetRequestStream().Write(ByteArr, 0, ByteArr.Length);
            }
            //ответ
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            //получение заголовка Location
            head = resp.Headers.Get("Location");
            //получение кук
            cook = resp.Headers.Get("Set-Cookie");
            //вывод ошибки
            string ret = reader.ReadToEnd();
            int n = ret.IndexOf("error");
            if (n >= 0)
            {
                int m = ret.IndexOf("\"", n + 10);
                Console.WriteLine(ret.Substring(n + 8, m - n - 8));
            }
            return ret;
        }
    }
}
