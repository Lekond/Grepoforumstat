using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Grepoforumstat
{
    /**
    @class request
    основной класс для фомирования запросов и обработки данных
    */
    class request
    {
        string world; //мир
        string login; //имя учетной записи
        string pass; //пароль
        connection c; //объекта класса соединения с сервером
        CookieCollection cookies; //куки
		string forum_url; //адрес форума
		CookieContainer forum_cook; //куки форума
		string forum_ref; //Refer форума
		string tok; //токен
		List<forum_menu> forum; //список вкладок форума
		Dictionary<string, List<theme>> themes; //список тем форума
		Dictionary<string, string> messages; //список сообщений тем форума
		Dictionary<string, List<report>> reports; //список отчетов на форуме


        /**
        @function request
        Конструктор
        @param in l - имя учетной записи
        @param in p - пароль от учетной записи
        @param in w - мир
        */
        public request(string l, string p, string w)
        {
            //инициализация глобальных переменных
            world = w;
            login = l;
            pass = p;
            cookies = new CookieCollection();
            //создание объекта для работы с форумом
            c = new connection();
            //вход в игру
            Login();
            //вход в мир
            ChangeWorld(world);
			forum=new List<forum_menu>();
			themes=new Dictionary<string, List<theme>>();
			messages= new Dictionary<string, string>();
			reports=new Dictionary<string, List<report>>();
        }

        /**
	    @function Login
	    Вход в игру
	    */
        void Login()
        {
            //формирование и отправка запроса
            string serv = world.Substring(0, 2);
            string url = "https://" + serv + ".grepolis.com/start/index?action=login_from_start_page";
            string re = "https://" + serv + ".grepolis.com/start";
            string req = "json=%7B%22name%22%3A%22" + login + "%22%2C%22password%22%3A%22" + pass + "%22%2C%22passwordhash%22%3A%22%22%2C%22autologin%22%3Afalse%2C%22window_size%22%3A%221903x946%22%7D";
            string cook;
            string h;
            c.SetResponce(url, serv, req, null, true, re, out cook, out h);
            //получение кук
            GetCookies(cook, "PHPSESSID");
            GetCookies(cook, "cid");
            GetCookies(cook, "pid");
            Console.WriteLine("Logined");
        }

        /**
	    @function GetCookies
	    Получение кук из заголовка
	    @param in c - строка с куками
	    @param in name - имя кук
	    */
        void GetCookies(string c, string name)
        {
            int n = c.IndexOf(name);
            if (n>=0)
            {
                int m = c.IndexOf(";", n);
                string co = c.Substring(n + name.Length + 1, m - n - name.Length - 1);
                cookies.Add(new Cookie(name, co, "/", "grepolis.com"));
            }
        }

        /**
	    @function ~request
	    Деструктор
	    */
        ~request()
        {
            //выход из игры
            Logout();
        }

        /**
	    @function Logout
	    Выход из игры
	    */
        void Logout()
        {
            //формирование и отправка запроса
            string serv = world.Substring(0, 2);
            string url = "http://" + serv + ".grepolis.com/start/index?action=ajax_logout";
            string req = "json=%7B%7D";
            string cook, h;
            CookieContainer cc = new CookieContainer();
            cc.Add(cookies["cid"]);
            cc.Add(cookies["pid"]);
            cc.Add(cookies["PHPSESSID"]);
            c.SetResponce(url, serv, req, cc, true, null, out cook, out h);
			Console.WriteLine("Logouted");
        }

        /**
        @function GetForum
        Получение списка вкладок форума
        */
		void GetForum()
		{
			//формирование и отправка запроса
			string rr = "json=%7B%22type%22%3A%22openIndex%22%2C%22separate%22%3Afalse%2C%22town_id%22%3A" + cookies["toid"].Value + "%2C%22nl_init%22%3Atrue%7D";
			forum_cook=new CookieContainer();
			forum_cook.Add(cookies["cid"]);
            forum_cook.Add(cookies["sid"]);
            forum_cook.Add(cookies["toid"]);	
			string cook, h;
			string req = c.SetResponce(forum_url, world, rr, forum_cook, true, forum_ref, out cook, out h);
			int i = 0;
			int n = 0;
            int m = 0;
			forum=new List<forum_menu>();
			//формирование списка вкладок (идентификатор и имя)
			while (true)
			{
                n = req.IndexOf("menu_link", m);
				if (n >= 0)
				{
                    n= req.IndexOf("{", n);
                    m = req.IndexOf("}", n);
					string f=req.Substring(n, m - n+1);
					//удаление служебных символов
					f=f.Replace ("\n", "");
					f=f.Replace ("\t", "");
					f=f.Replace ("\\\"", "\"");
					f=f.Replace ("\\\\\"", "\\\"");
					j_forum_menu1 fr;
                    ParseJson(f, out fr);
					forum_menu ff=new forum_menu();
					ff.id=fr.id.Substring(9, fr.id.Length-9);
                    int i1 = fr.name.IndexOf(">");
                    int i2 = fr.name.IndexOf("<", i1);
                    ff.name=fr.name.Substring(i1+1, i2-i1-1);
					forum.Add (ff);
					++i;
				}
				else
					break;
			}
			//получение списка союзов для вкладки
			n = req.IndexOf("Forum.setData(", 0);
			int l=req.IndexOf ("})", n);
			//получение списков союзов
			int k=req.IndexOf ("\"", n);
			int p=req.IndexOf ("\"", k+1);
			string id=req.Substring (k+1, p-k-2);
			n= req.IndexOf("{", p);
			while (n<l && n>=0)
			{
            	m = req.IndexOf("}", n);
				string r = req.Substring(n, m - n+1);
            	//удаление служебных символов
            	r = r.Replace("\\", "");
    			j_forum_menu2 f;
                ParseJson(r, out f);
				//получение списка союзов, с которыми общая вкладка
				for (i = 0; i < forum.Count; ++i)
				{
					if (forum[i].id.CompareTo(id)==0)
					{
						forum[i].alliances=f.alliances;
						break;
					}
				}
				k=req.IndexOf ("\"", m);
				p=req.IndexOf ("\"", k+1);
				id=req.Substring (k+1, p-k-2);
				n= req.IndexOf("{", p);
			}	
        }

        /**
        @function ChangeWorld
        Смена игрового мира
        @param in w - код мира
        */
		public void ChangeWorld(string w)
		{
			//изменение глобальной переменной
			world = w;
			//формирование и отправка первого запроса
			string url, req;
			string serv = world.Substring(0, 2);
			url = "https://" + serv + ".grepolis.com/start?action=login_to_game_world";
			req = "world=" + world + "&facebook_session=&facebook_login=&portal_sid=&name="+login+"&password="+pass;
			string cook, h;
            CookieContainer cc = new CookieContainer();
            cc.Add(cookies["cid"]);
            cc.Add(cookies["pid"]);
            cc.Add(cookies["PHPSESSID"]);
            string re = "https://"+serv+".grepolis.com/start";
    		c.SetResponce(url, serv, req, cc, true, re, out cook, out h);
			//получение кук
			GetCookies(cook, "login_startup_time");
            GetCookies(cook, "logged_in");
            //формирование и отправка второго запроса
            url = h;
            cc.Add(cookies["login_startup_time"]);
            //cc.Add(new Cookie("logged_in", "false", "/", "grepolis.com"));
            forum_ref = h;
			c.SetResponce(url, world, null, cc, false, null, out cook, out h);
			//получение кук
			GetCookies(cook, "sid");
            //формирование и отправка третьего запроса
            url = "https://" + world + ".grepolis.com" + h;
            cc.Add(cookies["sid"]);
            forum_ref = "https://" + world + ".grepolis.com"+h;
            string s=c.SetResponce(url, world, null, cc, false, null, out cook, out h);
            //получение кук
            GetCookies(cook, "toid");
            //получение токена
            int n = s.IndexOf("\"csrfToken\"");
            int m = s.IndexOf(",", n + 15);
			tok=s.Substring(n + 13, m - n - 14);
            forum_url = "http://" + world + ".grepolis.com/game/alliance_forum?town_id=" + cookies["toid"].Value + "&action=forum" + "&h=" + tok;
            Console.WriteLine ("World changed: "+world);
        }

        /**
        @function ToCsv
        Сохранение данных в файл формата csv
        @param in v - данные для сохранения
        @param in names - имена столбцов
        @param in f - имя файла
        */
		void ToCsv(List<Dictionary<string, string>> v, List<string> names, string f_name)
		{
			//создание файла
			FileStream file = new FileStream(f_name+".csv", FileMode.Create);
  			StreamWriter writer = new StreamWriter(file);
			string s="";
			//печать заголовков
			for (int i=0; i<names.Count; ++i)
				s+=names[i]+";";
   			writer.WriteLine(s);
			//печать данных
			for (int j=0; j<v.Count; ++j)
			{
				s="";
                for (int i = 0; i < names.Count; ++i)
                {
					//конвертация специальных символов
                    v[j][names[i]]=v[j][names[i]].Replace("&amp;", "&");
                    v[j][names[i]] = v[j][names[i]].Replace("&quot;", "\"");
                    v[j][names[i]] = v[j][names[i]].Replace("&lt;", "<");
                    v[j][names[i]] = v[j][names[i]].Replace("&gt;", ">");
                    v[j][names[i]] = v[j][names[i]].Replace("&nbsp;", " ");
                    v[j][names[i]] = v[j][names[i]].Replace("&sect;", "§");
                    v[j][names[i]] = v[j][names[i]].Replace("&copy;", "©");
                    v[j][names[i]] = v[j][names[i]].Replace("&reg;", "®");
                    v[j][names[i]] = v[j][names[i]].Replace("&deg;", "°");
                    v[j][names[i]] = v[j][names[i]].Replace("&laquo;", "«");
                    v[j][names[i]] = v[j][names[i]].Replace("&raquo;", "»");
                    v[j][names[i]] = v[j][names[i]].Replace("&middot;", "·");
                    v[j][names[i]] = v[j][names[i]].Replace("&trade;", "™");
                    v[j][names[i]] = v[j][names[i]].Replace("&plusmn;", "±");
					v[j][names[i]] = v[j][names[i]].Replace("\\/", "/");
                    s += v[j][names[i]] + "; ";
                }
   				writer.WriteLine(s);
			}
			//закрытие файла
   			writer.Close();
		}

        /**
        @function Reboot
        Очистка данных, полученных с форума
        */
		public void Reboot()
		{
			forum.Clear();
			themes.Clear();
			messages.Clear();
			reports.Clear();
			Console.WriteLine ("Forum rebooted");
		}

        /**
        @function PrintMenu
        Вывод перечня вкладок
        */
        public void PrintMenu()
		{
			//получение списка вкладок
			if (forum.Count==0)
				GetForum ();
			List<Dictionary<string, string>> v=new List<Dictionary<string, string>>();
			//подготовка данных для сохраения в файл
			for (int i=0; i<forum.Count; ++i)
			{
				Dictionary<string, string> d=new Dictionary<string, string>();
				d.Add ("id", forum[i].id );
				d.Add ("name", forum[i].name );
				string s="";
                for (int j = 0; j < forum[i].alliances.Count; ++j)
                    s += forum[i].alliances[j] + ",";
                d.Add("alliances", s);
				v.Add (d);
			}
			List<string> names=new List<string>();
			//подготовка заголовков файла
			names.Add ("id");
			names.Add ("name");
			names.Add ("alliances");
			//сохранение в файл
			ToCsv (v, names, world+"_forum");
            Console.WriteLine("Menus downloaded");
		}

        /**
        @function DecodeFrom64
        Получение данных, закодированных base 64
        @param in encodedData - строка с закодированными данными
        @return строку с исходными данными
        */
        string DecodeFrom64(string encodedData)
   		{
      		byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
      		string returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);
      		return returnValue;
    	}

        /**
        @function ParseReports
        Получение всех отчетов темы
        @param in v - идентификатор темы
        */
		void ParseReports(string v)
		{
			//получение сообщений темы
			if (!messages.ContainsKey(v))
				GetMessages (v);
			string req = messages[v];
			int i = 0;
			//определение, есть ли в теме удаленные отчеты
			int n = req.IndexOf("published_report_header italic");
			if (n >= 0)
				for (int j = 0; j < forum.Count; ++j)
				{
                    if (themes.ContainsKey(forum[j].id))
                    {
                        for (int l = 0; l < themes[forum[j].id].Count; ++l)
                            if (themes[forum[j].id][l].id.CompareTo(v) == 0)
                                themes[forum[j].id][l].d = true;
                    }
				}
			n = 0;
			//получение списка отчетов темы
			List<report> lr=new List<report>();
			while (true)
			{
				n = req.IndexOf("published_report_header bold", n + 1);
				report rep=new report();
				rep.v=new List<info>();
				if (n >= 0)
				{
					//получение заголовка отчета
					int m = req.IndexOf("<span class=\\\"bold\\\">", n);
					int l = req.IndexOf("<span class=\\\"reports_date small\\\">", n);
					string str = req.Substring(m, l - m);
					int k = 0;
					int p = 0;
					//разбор заголовка отчета
					while (true)
					{
						k = str.IndexOf("<a", k);
						if (k < 0)
							break;
						k = str.IndexOf("\"", k);
						int j = str.IndexOf("\"", k + 10);
						string dec = DecodeFrom64(str.Substring(k + 2, j - k - 3));
						k = str.IndexOf("class=\\\"", k);
						j = str.IndexOf("\"", k + 8);
						string s = str.Substring(k + 8, j - k - 9);
						info inf=new info();
						if (s.CompareTo("gp_town_link") == 0)
						{
							//получение информации о городе
							rep.s += "g";
							j_town_info ti;
                            ParseJson(dec, out ti);
							inf.id=ti.id.ToString();
							inf.name=ti.name;
						}
						else
						{
							//получение информации об игроке
							rep.s += "p";
							j_player_info pi;
                            ParseJson(dec, out pi);
							inf.id=pi.id.ToString();
							inf.name=pi.name;
						}
						rep.v.Add(inf);
						k = str.IndexOf("a>", k);
						j = str.IndexOf("<", k);
						string ss = str.Substring(k + 2, j - k - 2);
						if (ss.IndexOf("(") >= 0)
							rep.s += "(";
						if (ss.IndexOf(")") >= 0)
							rep.s += ")";
						p++;
					}
					//отчет о шпионаже
					int b = req.IndexOf("espionage_report", l);
					if (b>0 && b < req.IndexOf("published_report_header bold", n + 1))
						rep.s = "---";
					//отчет с удавшимся бунтом
					if (req.IndexOf("<span class=\\\"bold\\\">", l) < req.IndexOf("published_report_header bold", n + 1))
						rep.r = true;
					else
						rep.r = false;
					//получение даты и времени отчета
					l = req.IndexOf("reports_date small", n);
					m = req.IndexOf(">", l);
					l = req.IndexOf("<", m);
					string d = req.Substring(m + 1, l - m - 1);
					d=d.Replace ("\\n", "");
					d=d.Replace ("\\t", "");
					d=d.Replace ("\\", "");
					rep.d=d;
					lr.Add (rep);
					++i;
				}
				else
					break;
			}
			reports.Add (v, lr);
		}

        /**
	    @function PrintThemes
	    Вывод перечня тем указанных вкладок
	    @param in v - список идентификаторов вкладок
	    */
		public void PrintThemes(List<string> v)
		{
			//получение списка вкладок
			if (forum.Count==0)
				GetForum ();
			//если все кладки - формирование списка вкладок
			if (v.Count==0)
			{
				List<string> v2=new List<string>();
				for (int i=0; i<forum.Count; ++i)
					v2.Add (forum[i].id);
				PrintThemes (v2);
				return;
			}
			//имя файла
			string f_name=world+"_themes";
			List<Dictionary<string, string>> l=new List<Dictionary<string, string>>();
			//подготовка списка тем для сохранения в файл
			for (int i=0; i<v.Count(); ++i)
			{
				//получение списка тем вкладки
				if (!themes.ContainsKey(v[i]))
					GetThemes (v[i]);
				for (int j=0; j<themes[v[i]].Count; ++j)
				{
					//подготовка списка тем вкладки для сохранения в файл
					Dictionary<string, string> d=new Dictionary<string, string>();
					d.Add ("theme", themes[v[i]][j].name);
					d.Add ("menu", FindName (v[i]));
					l.Add (d);
				}
				f_name+="_"+v[i];
			}
			//подготовка заголовков файла
			List<string> names=new List<string>();
			names.Add ("theme");
			names.Add ("menu");
			//сохранение в файл
			ToCsv (l, names, f_name);
			Console.WriteLine("Themes downloaded");
		}

        /**
        @function PrintThemesFind
        Вывод перечня тем указанных вкладок, в которых есть ключевое слово
        @param in m - список идентификаторов вкладок
        @param in f - ключевое слово для поиска
        */
		public void PrintThemesFind(List<string> v, string f)
		{
			//получение списка вкладок
			if (forum.Count==0)
				GetForum ();
			//если все кладки - формирование списка вкладок
			if (v.Count==0)
			{
				List<string> v2=new List<string>();
				for (int i=0; i<forum.Count; ++i)
					v2.Add (forum[i].id);
				PrintThemesFind (v2, f);
				return;
			}
			//имя файла
			string f_name=world+"_found_"+f;
			//подготовка списка тем для сохранения в файл
			List<Dictionary<string, string>> l=new List<Dictionary<string, string>>();
			for (int i=0; i<v.Count(); ++i)
			{
				//получение списка тем вкладки
				if (!themes.ContainsKey(v[i]))
					GetThemes (v[i]);
				for (int j=0; j<themes[v[i]].Count; ++j)
				{
					//получение сообщений темы
					if (!messages.ContainsKey(themes[v[i]][j].id))
						GetMessages (themes[v[i]][j].id);
					//подготовка данных для сохранения в файл если найдено ключевое слово
					if (messages[themes[v[i]][j].id].IndexOf(f)>=0)
					{
						Dictionary<string, string> d=new Dictionary<string, string>();
						d.Add ("theme", themes[v[i]][j].name);
						d.Add ("menu", FindName (v[i]));
						l.Add (d);
					}
				}
				f_name+="_"+v[i];
			}
			//подготовка заголовков файла
			List<string> names=new List<string>();
			names.Add ("theme");
			names.Add ("menu");
			//сохранение в файл
			ToCsv (l, names, f_name);
			Console.WriteLine("Themes with "+f+" downloaded");
		}

        /**
        @function FindName
        Поиск имени вкладки по идентификатору
        @param in id - идентификатор вкладки
        @return строка с именем вкладки
        */
		string FindName(string id)
		{
			for (int i=0; i<forum.Count; ++i)
				if (forum[i].id.CompareTo(id)==0)
					return forum[i].name;
            return "";
		}

        /**
        @function PrintThemesDeleted
        Вывод перечня тем указанных вкладок, в которых есть удаленные отчеты
        @param in v - список идентификаторов вкладок
        */
        public void PrintThemesDeleted(List<string> v)
		{
			//получение списка вкладок
			if (forum.Count==0)
				GetForum ();
			//если все кладки - формирование списка вкладок
			if (v.Count==0)
			{
				List<string> v2=new List<string>();
				for (int i=0; i<forum.Count; ++i)
					v2.Add (forum[i].id);
				PrintThemesDeleted (v2);
				return;
			}
			//имя файла
			string f_name=world+"_deleted";
			//подготовка списка тем для сохранения в файл
			List<Dictionary<string, string>> l=new List<Dictionary<string, string>>();
			for (int i=0; i<v.Count(); ++i)
			{
				//получение списка тем вкладки
				if (!themes.ContainsKey(v[i]))
					GetThemes (v[i]);
				for (int j=0; j<themes[v[i]].Count; ++j)
				{
					//получение списка отчетов вкладки
					if (!reports.ContainsKey(themes[v[i]][j].id))
						ParseReports (themes[v[i]][j].id);
					//подготовка данных для сохранения в файл если в теме есть удаленные отчеты
					if (themes[v[i]][j].d)
					{
						Dictionary<string, string> d=new Dictionary<string, string>();
						d.Add ("theme", themes[v[i]][j].name);
						d.Add ("menu", FindName (v[i]));
						l.Add (d);
					}
				}
				f_name+="_"+v[i];
			}
			//подготовка заголовков файла
			List<string> names=new List<string>();
			names.Add ("theme");
			names.Add ("menu");
			//сохранение в файл
			ToCsv (l, names, f_name);
			Console.WriteLine("Themes with deleted reports downloaded");
		}

        /**
        @function GetThemes
        Получение списка тем вкладки
        @param in v - идентификатор вкладки
        */
		void GetThemes(string v)
		{
			int page = 0;
			int pages = -1;
			int i = 0;
			//получение списка тем
			List<theme> lv=new List<theme>();
			while (page < pages || pages<0)
			{
				//формирование и отправка запроса
				string rr = "json=%7B%22type%22%3A%22go%22%2C%22separate%22%3Afalse%2C%22forum_id%22%3A%22" + v + "%22%2C%22page%22%3A" +(page+1).ToString() + "%2C%22town_id%22%3A" + cookies["toid"].Value + "%2C%22nl_init%22%3Atrue%7D";
				string cook, h;
				string req = c.SetResponce(forum_url, world, rr, forum_cook, true, forum_ref, out cook, out h);
				//получение количества страниц
				if (pages < 0)
				{
					int l = req.IndexOf("paginator_bg\\\">");
                    if (l < 0)
                        pages = 1;
                    else
                    {
                        int k = req.IndexOf("<", l);
                        Int32.TryParse(req.Substring(l + 15, k - l - 15), out pages);
                    }
				}
				int n = 0;
				//получение списка тем на странице
				while (true)
				{
					n = req.IndexOf("Forum.viewThread(", n + 1);
					if (n >= 0)
					{
						theme t=new theme();
						int m = req.IndexOf(")", n);
						t.id = req.Substring(n + 17, m - n - 17);
						n = req.IndexOf(">", n);
						m = req.IndexOf("<", n);
						t.name = req.Substring(n + 1, m - n - 1);
						t.d = false;
						lv.Add(t);
						n = req.IndexOf("Forum.viewThread(", n + 1);
						++i;
					}
					else
						break;
				}
				page++;
			}
			themes.Add (v, lv);
		}

        /**
        @function GetMessages
        Получение всех сообщений темы
        @param in v - идентификатор темы
        */
		void GetMessages(string v)
		{
			int page = 0;
			int pages = -1;
			string mes="";
			//получение сообщений
			while (page < pages || pages<0)
			{
				//формирование и отправка запроса
				string rr = "json=%7B%22type%22%3A%22go%22%2C%22separate%22%3Afalse%2C%22thread_id%22%3A"+v+"%2C%22page%22%3A"+ (page + 1).ToString() +"%2C%22town_id%22%3A"+cookies["toid"].Value+"%2C%22nl_init%22%3Atrue%7D";
				string cook, h;
				string req = c.SetResponce(forum_url, world, rr, forum_cook, true, forum_ref, out cook, out h);
				//получение количества страниц
				if (pages < 0)
				{
					int l = req.IndexOf("paginator_bg\\\">");
                    if (l < 0)
                        pages = 1;
                    else
                    {
                        int k = req.IndexOf("<", l);
                        Int32.TryParse(req.Substring(l + 15, k - l - 15), out pages);
                    }
				}
				//получение сообщений страницы
				mes += "\n"+req;
				page++;
			}
			messages.Add (v, mes);
		}

        /**
        @function GetTownInfo
        Получение информации о городе
        @param in id - идентификатор города
        @return структура с информацией о городе
        */
		town_info GetTownInfo(string id)
		{
			//формирование и отправка запроса
			string url = "http://" + world + ".grepolis.com/game/town_info?town_id=" + cookies["toid"].Value + "&action=info&h=" + tok + "&json=%7B%22id%22%3A" + id + "%2C%22town_id%22%3A" + cookies["toid"].Value + "%2C%22nl_init%22%3Atrue%7D";
			string cook, h;
			string str= c.SetResponce(url, world, null, forum_cook, false, forum_ref, out cook, out h);
			town_info v=new town_info();
			//игрок
			int n = str.IndexOf("gp_player_link");
			int m = str.IndexOf(">", n);
			int l = str.IndexOf("<", n);
			v.player = str.Substring(m + 1, l - m - 1);
			//союз
			n = str.IndexOf("allianceProfile");
			m = str.IndexOf(">", n);
			l = str.IndexOf("<", n);
			v.alliance = str.Substring(m + 1, l - m - 1);
			//океан
			n = str.IndexOf("Ocean:");
			m = str.IndexOf(":", n);
			l = str.IndexOf("(", n);
			v.ocean = str.Substring(m + 2, l - m - 3);
			return v;
		}

        /**
	    @function ParseDef
	    Получение данных об атаке на подкрепление
	    @param in v - информация об атаке из отчета
	    @return информация об атаке для печати
	    */
		Dictionary<string, string> ParseDef(List<info> v)//g(p)g(p), g(p)g()
		{
			Dictionary<string, string> res=new Dictionary<string, string>();
			res.Add ("bb_attack_town","[town]" + v[0].id + "[/town]");
			res.Add ("attack_town",v[0].name);
			town_info r = GetTownInfo(v[0].id);
			res.Add ("attack_town_ocean", r.ocean);
			res.Add ("attack_town_owner",r.player);
			res.Add ("attack_town_alliance",r.alliance);
			res.Add ("attacker", v[1].name);
			res.Add ("bb_defence_town","[town]" + v[2].id + "[/town]");
			res.Add ("defence_town", v[2].name);
			r = GetTownInfo(v[2].id);
			res.Add ("defence_town_ocean",r.ocean);
			res.Add ("defence_town_owner",r.player);
			res.Add ("defence_town_alliance", r.alliance);
			if (v.Count==4)
				res.Add ("defender", v[3].name);
			else
				res.Add ("defender", "");
			return res;
		}

        /**
        @function ParseOurAttack
        Получение данных о нашей атаке
        @param in v - информация об атаке из отчета
        @return информация об атаке для печати
        */
		Dictionary<string, string> ParseOurAttack(List<info> v)//gg(p), gg()
		{
			Dictionary<string, string> res=new Dictionary<string, string>();
			res.Add ("bb_attack_town","[town]" + v[0].id + "[/town]");
			res.Add ("attack_town",v[0].name);
			town_info r = GetTownInfo(v[0].id);
			res.Add ("attack_town_ocean", r.ocean);
			res.Add ("attack_town_owner",r.player);
			res.Add ("attack_town_alliance",r.alliance);
			res.Add ("attacker", "");
			res.Add ("bb_defence_town","[town]" + v[1].id + "[/town]");
			res.Add ("defence_town", v[1].name);
			r = GetTownInfo(v[1].id);
			res.Add ("defence_town_ocean",r.ocean);
			res.Add ("defence_town_owner",r.player);
			res.Add ("defence_town_alliance", r.alliance);
			if (v.Count==3)
				res.Add ("defender", v[2].name);
			else
				res.Add ("defender", "");
			return res;
		}
		
        /**
	    @function ParseAttackToUs
	    Получение данных об атаке на нас
	    @param in v - информация об атаке из отчета
	    @return информация об атаке для печати
	    */
		Dictionary<string, string> ParseAttackToUs(List<info> v)//g(p)g
		{
			Dictionary<string, string> res=new Dictionary<string, string>();
			res.Add ("bb_attack_town","[town]" + v[0].id + "[/town]");
			res.Add ("attack_town",v[0].name);
			town_info r = GetTownInfo(v[0].id);
			res.Add ("attack_town_ocean", r.ocean);
			res.Add ("attack_town_owner",r.player);
			res.Add ("attack_town_alliance",r.alliance);
			res.Add ("attacker", v[1].name);
			res.Add ("bb_defence_town","[town]" + v[2].id + "[/town]");
			res.Add ("defence_town", v[2].name);
			r = GetTownInfo(v[2].id);
			res.Add ("defence_town_ocean",r.ocean);
			res.Add ("defence_town_owner",r.player);
			res.Add ("defence_town_alliance", r.alliance);
			res.Add ("defender", "");
			return res;
		}

        /**
        @function PrintReports
        Вывод списка отчетов атак по указанным фильтрам
        @param in m - список идентификаторов вкладок
        @param in w - фильтр по нападающему/защищающемуся
        @param in r - фильтр только с бунтом
        @param in tous - фильтр атаки на нас
        @param in our - фильтр наши атаки
        @param in def - фильтр атаки на подкрепление
        */
		public void PrintReports(List<string>m, int w, bool r, bool tous, bool our, bool def)
		{
			//получение вкладок форума
			if (forum.Count==0)
				GetForum();
			//если все кладки - формирование списка вкладок
			if (m.Count==0)
			{
				List<string> v2=new List<string>();
				for (int i=0; i<forum.Count; ++i)
					v2.Add (forum[i].id);
				PrintReports (v2, w, r, tous, our, def);
				return;
			}
			//информация о текущем игроке
			town_info inf = GetTownInfo(cookies["toid"].Value);
			List<Dictionary<string,string>> v=new List<Dictionary<string, string>>();
			//имя файла
			string name=world;
			if (tous)
				name += "_tous";
			if (our)
				name += "_our";
			if (def)
				name += "_def";
			//подготовка данных для сохранения в файл
			for (int i = 0; i < m.Count; ++i)
			{
				//получение списка тем вкладки
				if (!themes.ContainsKey(m[i]))
					GetThemes(m[i]);
				for (int l = 0; l < themes[m[i]].Count; ++l)
				{
					//получение списка отчетов темы
					if (!reports.ContainsKey(themes[m[i]][l].id))
						ParseReports(themes[m[i]][l].id);
					for (int k = 0; k < reports[themes[m[i]][l].id].Count; ++k)
					{
						//подготовка данных отчета
						if (!r || reports[themes[m[i]][l].id][k].r)
						{
							bool fl = false;
							Dictionary<string,string> res=null;
							//подготовка данных по отчету о нападении на подкрепление
							if (tous && reports[themes[m[i]][l].id][k].s.CompareTo("g(p)g") == 0)
								res = ParseAttackToUs(reports[themes[m[i]][l].id][k].v);
							//подготовка данных по отчету об атаке
							if (our && (reports[themes[m[i]][l].id][k].s.CompareTo("gg(p)") == 0 || reports[themes[m[i]][l].id][k].s.CompareTo("gg()") == 0))
								res= ParseOurAttack(reports[themes[m[i]][l].id][k].v);
							//подготовка данных по отчету об обороне
							if (def && (reports[themes[m[i]][l].id][k].s.CompareTo("g(p)g(p)") == 0 || reports[themes[m[i]][l].id][k].s.CompareTo("g(p)g()") == 0))
								res = ParseDef(reports[themes[m[i]][l].id][k].v);
							//фильтрация отчетов по нападающему/обороняющемуся
							if (res!=null)
							{
								switch (w)
								{
									case 0://игрок на игрока
										if ((res["defender"].CompareTo(inf.player) == 0 || (res["defender"].Length==0 && res["defence_town_owner"].CompareTo(inf.player) == 0)) && (res["attacker"].CompareTo(inf.player) == 0 || (res["attacker"].Length==0 && res["attack_town_owner"].CompareTo(inf.player) == 0)))
											fl = true;
										break;
									case 1://игрок на союз
										if ((res["attacker"].CompareTo(inf.player) == 0 || (res["attacker"].Length==0 && res["attack_town_owner"].CompareTo(inf.player) == 0)) && PlayerAlliance(res["defence_town_alliance"], inf))
											fl = true;
										break;
									case 2://союз на игрока
										if (PlayerAlliance(res["attack_town_alliance"], inf) && (res["defender"].CompareTo(inf.player) == 0 || (res["defender"].Length==0 && res["defence_town_owner"].CompareTo(inf.player) == 0)))
											fl = true;
										break;	
									case 3://союз на союз
										if (PlayerAlliance(res["attack_town_alliance"], inf) && PlayerAlliance(res["defence_town_alliance"], inf))
											fl = true;
										break;
									case 4://альянс на игрока
										for (int b = 0; b < forum.Count; ++b)
										{
											if (forum[b].id.CompareTo(m[i]) == 0 && InAlliances(forum[b].alliances, res["attack_town_alliance"], inf) && (res["defender"].CompareTo(inf.player) == 0 || (res["defender"].Length==0 && res["defence_town_owner"].CompareTo(inf.player) == 0)))
											{
												fl=true;
												break;
											}
										}
										break;
									case 5://альянс на союз
										for (int b = 0; b < forum.Count; ++b)
										{
											if (forum[b].id.CompareTo(m[i]) == 0 && InAlliances(forum[b].alliances, res["attack_town_alliance"], inf) && PlayerAlliance(res["defence_town_alliance"], inf))
											{
												fl=true;
												break;
											}
										}
										break;
									case 7://игрок на альянс
										for (int b = 0; b < forum.Count; ++b)
										{
											if (forum[b].id.CompareTo(m[i]) == 0 && (res["attacker"].CompareTo(inf.player) == 0 || (res["attacker"].Length==0 && res["attack_town_owner"].CompareTo(inf.player) == 0)) && InAlliances(forum[b].alliances,res["defence_town_alliance"],inf))
											{
												fl=true;
												break;
											}
										}
										break;
									case 8://игрок на всех
										if (res["attacker"].CompareTo(inf.player) == 0 || (res["attacker"].Length==0 && res["attack_town_owner"].CompareTo(inf.player) == 0))
											fl = true;
										break;
									case 9://союз на альянс
										for (int b = 0; b < forum.Count; ++b)
										{
											if (forum[b].id.CompareTo(m[i]) == 0 && PlayerAlliance(res["attack_town_alliance"], inf) && InAlliances(forum[b].alliances, res["defence_town_alliance"], inf))
											{
												fl=true;
												break;
											}
										}
										break;
									case 10://союз на всех
										if (PlayerAlliance(res["attack_town_alliance"], inf))
											fl = true;
										break;
									case 11://альянс на альянс
										for (int b = 0; b < forum.Count; ++b)
										{
											if (forum[b].id.CompareTo(m[i]) == 0 && InAlliances(forum[b].alliances, res["attack_town_alliance"], inf) && InAlliances(forum[b].alliances, res["defence_town_alliance"], inf))
											{
												fl = true;
												break;
											}
										}
										break;
									case 12://альянс на всех
										for (int b = 0; b < forum.Count; ++b)
										{
											if (forum[b].id.CompareTo(m[i]) == 0 && InAlliances(forum[b].alliances, res["attack_town_alliance"], inf))
											{
												fl = true;
												break;
											}
										}
										break;
									case 13://все на игрока
										if (res["defender"].CompareTo(inf.player) == 0 || (res["defender"].Length==0 && res["defence_town_owner"].CompareTo(inf.player) == 0))
											fl = true;
										break;
									case 14://все на союз
										if (PlayerAlliance(res["defence_town_alliance"], inf))
											fl = true;
										break;
									case 20://все на альянс
										for (int b = 0; b < forum.Count; ++b)
										{
											if (forum[b].id.CompareTo(m[i]) == 0 && InAlliances(forum[b].alliances, res["defence_town_alliance"], inf))
											{
												fl = true;
												break;
											}
										}
										break;
									
									case 21://все на всех
										fl = true;
										break;
									}
								//если пройден фильтр - подготовка данных для сохранения
								if (fl)
								{
									res.Add ("datetime", reports[themes[m[i]][l].id][k].d);
									res.Add ("theme",themes[m[i]][l].name);
									res.Add ("menu", FindName(m[i]));
									v.Add (res);
								}
							}
						}
					}
				}
				name += "_" + m[i];
			}
			//подготовка заголовков файлов
			List<string> names=new List<string>();
			names.Add ("attack_town_ocean");
			names.Add ("bb_attack_town");
			names.Add ("attack_town");
			names.Add ("attack_town_owner");
			names.Add ("attack_town_alliance");
			names.Add ("attacker");
			names.Add ("defence_town_ocean");
			names.Add ("bb_defence_town");
			names.Add ("defence_town");
			names.Add ("defence_town_owner");
			names.Add ("defence_town_alliance");
			names.Add ("defender");
			names.Add ("datetime");
			names.Add ("theme");
			names.Add ("menu");
			//сохранение в файл
			ToCsv(v, names, name);
			Console.WriteLine ("Reports downloaded");
	    }

        /**
        @function InAlliances
        Проверка, является ли союз членом альянса
        @param in v - список союзов альянса
        @param in a - проверяемый союз
        @param in info - информация о городе, союз которого также в альянсе
        @return true если входит в альянс, false иначе
        */
		bool InAlliances(List<string> v, string a, town_info inf)
		{
			//союз - союз игрока?
			if (PlayerAlliance(a, inf))
				return true;
			//поиск среди союзов вкладки
			for (int i = 0; i < v.Count; ++i)
			{
				if (v[i].CompareTo(a) == 0)
					return true;
			}
			return false;
		}

        /**
        @function PlayerAlliance
        Проверка, является ли указанный союз союзом игрока
        @param in a - проверяемый союз
        @param in info - информация о городе игрока
        @return true если является, false иначе
        */
		bool PlayerAlliance(string a, town_info inf)
		{
			if (a.CompareTo(inf.alliance)==0)
				return true;
			return false;
		}

        /**
        @function ParseJson
        Создание объекта из строки с json-структурой
        @param in s - строка с json-структурой
        @param out ret - объект заданного класса
        */
        void ParseJson<T>(string s, out T ret)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
            ret = (T)json.ReadObject(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(s)));
        }  
	}
}
