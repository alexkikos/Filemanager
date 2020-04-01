using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

using System.IO;

//кнопка 2, сделать x для возврата++
//есть место для еще 1 кнопки, можно добавить поиск всех файлов в котором есть вхождение слов и вывод этих файлов++
//справка по ф1+++
//попробовать избавиться от мерцания

namespace FIlemanager_Lite_
{
	class Program
	{

		static void Main(string[] args)
		{
	
			try
			{

				Console.Title = "Filemanager_Lite";
				Console.SetWindowSize(165, 47);
				Console.SetBufferSize(170, 500);
				Manager manager = new Manager();
				manager.Menu();
			}
			catch (Exception e)
			{				
				Console.WriteLine(e.Message);
				Console.WriteLine("Minimum resolition for run 1366/768 and console size 165/47\nPress enter for continue");
			}
			Console.ReadKey();
		}
	}
}
