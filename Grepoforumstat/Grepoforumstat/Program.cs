using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grepoforumstat
{
    class Program
    {
        static List<param> par; //список параметров
        static request req; //объект класса работы с форумом
		
		/**
        @function Main
        Основная функция
        @param in args - список параметров командной строки
        @return 0 в случае успешного завершения работы, 1 иначе
        */
        static int Main(string[] args)
        {
            par = new List<param>();
			//разбор параметров командной строки
			int res=Parse(args, -1);
			while (true)
			{
				//завершение работы если поступила команда завершения работы
				if (res == 1)
					return 0;
				if (res == 0)
					par.Clear();
				//чтение данных из командной строки
				string ss=Console.ReadLine();
				//разбор полученных из командной строки данных
				string[] ar=ss.Split (' ');
				res=Parse(ar, res);
			}
			return 1;
		}
		
		/**
        @function Parse
        Функция разбора полученных данных командной строки
        @param in args - список параметров командной строки
        @param in res - результат выполнения предыдущего разбора командной строки
        @return 0 в случае успешного завершения работы, 1 иначе
        */
		static int Parse(string[] args, int res)
		{
			int j=0;
			for (int i = 0; i < args.GetLength(0); ++i)
			{
				if (args[i].ToCharArray()[0]=='-')
				{
					//получена командна
					param p= new param();
					p.c = args[i];
                    p.p = new List<string>();
                    par.Add(p);
					j++;
				}
				else if (j == 0)
					//параметр без команды
					Console.WriteLine ("unknown parameter: " + args[i]);
				else
					//параметр команды
					par[j-1].p.Add(args[i]);
			}
			//выполнение полученных команд
			return ParseCommands(par, res);
		}
		
		/**
        @function ParseCommands
        Функция разбора полученных данных командной строки
        @param in par - список команд и параметров
        @param in res - результат выполнения предыдущих команд
        @return -1 если недостаточно данных для входа, 1 если поступила команда завершения работы, 0 иначе
        */
		static int ParseCommands(List<param> par, int res)
		{
			string l="", p="", w="";
			//вход в игру, если еще не осуществлен
			if (res < 0)
			{
				for (int i = 0; i < par.Count; ++i)
				{
					//пароль
					if (par[i].c.CompareTo("-l") == 0)
					{
						for (int j = 0; j < par[i].p.Count; ++j)
							l += par[i].p[j]+ " ";
					}
					//пароль
					else if (par[i].c.CompareTo("-p") == 0)
						p = par[i].p[0];
					//мир
					else if (par[i].c.CompareTo("-w") == 0)
						w = par[i].p[0];
				}
				//не хватает данных для входа
				if (l.Length==0 || p.Length==0 || w.Length==0)
				{
					Console.WriteLine ("Enter login (-l), password (-p) and world (-w)!!!");
					return -1;
				}
				req = new request(l, p, w);
			}
			//обработка команд после входа
			for (int i = 0; i < par.Count; ++i)
			{
				//смена мира
				if (par[i].c.CompareTo("-w") == 0 && res>=0)
					req.ChangeWorld(par[i].p[0]);
				//перезапуск
				else if (par[i].c.CompareTo("-r") == 0)
					req.Reboot();
				//поиск по ключевому слову
				else if (par[i].c.CompareTo("-f") == 0)
				{
					List <string> str = new List<string>();
					for (int j = 1; j < par[i].p.Count; ++j)
						str.Add (par[i].p[j]);
					req.PrintThemesFind(str, par[i].p[0]);
				}
				//получение списка тем
				else if (par[i].c.CompareTo("-t") == 0)
				{
					List <string> str = new List<string>();
					for (int j = 0; j < par[i].p.Count; ++j)
						str.Add (par[i].p[j]);
					req.PrintThemes(str);
				}
				//получение списка вкладок
				else if (par[i].c.CompareTo("-m") == 0)
					req.PrintMenu();
				//получение списка тем с удаленными отчетами
				else if (par[i].c.CompareTo("-d") == 0)
				{
					List <string> str = new List<string>();
					for (int j = 0; j < par[i].p.Count; ++j)
						str.Add (par[i].p[j]);
					req.PrintThemesDeleted(str);
				}
				//получение списка отчетов
				else if (par[i].c.CompareTo("-a") == 0)
				{
					bool r = false, tous=false, our=false, def=false;
					int pr = 0;
					//фильтр "кто нападает"
					char c = par[i].p[0].ToCharArray()[0];
					switch (c){
					case 'p':
						break;
					case 's':
						pr += 2;
						break;
					case 'h':
						pr += 4;
						break;
					case 'a':
						pr += 13;
						break;
					default:
						pr += 13;
						Console.WriteLine ("Unknown parameter from: " + c);
                        break;
					}
					//фильтр "на кого нападают"
                    c = par[i].p[0].ToCharArray()[1];
					switch (c){
					case 'p':
						break;
					case 's':
						pr += 1;
						break;
					case 'h':
						pr += 7;
						break;
					case 'a':
						pr += 8;
						break;
					default:
						pr += 8;
						Console.WriteLine ("Unknown parameter to : " + c);
                        break;
					}
					//остальные фильтры
					string s = par[i].p[1];
					for (int j = 0; j < s.Length; ++j)
					{
						if (s.ToCharArray()[j] == 'r')
							r = true;
						else if (s.ToCharArray()[j] == 'o')
							our = true;
						else if (s.ToCharArray()[j] == 't')
							tous = true;
						else if (s.ToCharArray()[j] == 'd')
							def = true;
					}
					List<string> str = new List<string>();
					for (int j = 2; j < par[i].p.Count; ++j)
						str.Add (par[i].p[j]);
					req.PrintReports(str, pr,r, tous, our, def);
				}
				//выход
				else if (par[i].c.CompareTo("-e") == 0)
					return 1;
				//неизвестная команда
				else if (par[i].c.CompareTo("-l") != 0 && par[i].c.CompareTo("-p") != 0 && par[i].c.CompareTo("-w5") != 0)
					Console.WriteLine ("Unknown command: " + par[i].c);
			}
			return 0;
		}
    }
}
