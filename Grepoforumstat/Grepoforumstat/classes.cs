using System;
using System.Collections.Generic;

namespace Grepoforumstat
{
	//вкладка форума из запроса
	public class j_forum_menu1
	{
    	public string id { get; set; }
    	public string onclick { get; set; }
    	public string className { get; set; }
    	public string name { get; set; }
	}
	
	//вкладка форума с союзами из запроса
	public class j_forum_menu2
	{
   		public string forum_content_shorten { get; set; }
    	public List<string> alliances { get; set; }
	}
	
	//вкладка форума
	public class forum_menu
	{
    	public string id { get; set; }
    	public string name { get; set; }
		public List<string> alliances { get; set; }
	}
	
	//информация о городе из запроса
	public class j_town_info
	{
    	public int id { get; set; }
    	public int ix { get; set; }
    	public int iy { get; set; }
    	public string tp { get; set; }
    	public string name { get; set; }
	}
	
	//информация об игроке из запроса
	public class j_player_info
	{
    	public string name { get; set; }
    	public int id { get; set; }
	}
	
	//тема
	public class theme
	{
    	public string id { get; set; }
    	public string name { get; set; }
		public bool d { get; set; }
	}
	
	//отчет
	public class report
	{
    	public string s { get; set; }
    	public List<info> v { get; set; }
    	public string d { get; set; }
		public bool r { get; set; }
	}
	
	//информация об игроке/городе
	public class info
	{
		public string id { get; set; }
    	public string name { get; set; }
	}
	
	//информация о городе
	public class town_info
	{
		public string player { get; set; }
		public string alliance { get; set; }
		public string ocean { get; set; }
	}
	
	//команда и параметры
	public class param
	{
		public string c { get; set; }
		public List<string> p { get; set; }
	}
}

