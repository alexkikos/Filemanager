using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using NLog;



namespace Core
{
	public sealed class Manager
	{

		delegate void Dallbuttons();
		/// <summary>
		/// делега собирает все кнопки для основного меню
		/// </summary>
		Dallbuttons dallbuttons;
		delegate void DallbuttonsSearch();
		/// <summary>
		/// делегать собирает все кнопки для поискового меню
		/// </summary>	
		Dallbuttons dallbuttonssearch;
		/// <summary>
		/// логирование ошибок
		/// </summary>
		static Logger logger;
		/// <summary>
		/// статический таймер, для информирования юзера что происходит и как долго
		/// </summary>
		static Timer aTimer;
		/// <summary>
		/// ивент для обработки ошибок
		/// </summary>
		event ErrorEventHandler EError;
		/// <summary>
		/// уведомление о том что мы что то нажали или сделали
		/// </summary>		
		event EventHandler EChangepos;
		/// <summary>
		/// уведомление о вызове кнопки + обработка сообщений кнопок
		/// </summary>
		event EventHandler ECallButton;
		/// <summary>
		/// ивент добавления/удаления в/из буфепра
		/// </summary>
		event EventHandler Buffer;
		/// <summary>
		/// переменная для перемещения и навигации в консоле
		/// </summary>
		int current_position = 0;
		/// <summary>
		/// инкапсулирю девайсы при старте сразу
		/// </summary>
		DriveInfo[] disks;
		/// <summary>
		/// сохраняю текущие пути для навигации
		/// </summary>
		List<string> current_folder;
		/// <summary>
		/// время, хранится для работы с папками/файлами, чтобы каждый раз не пересоздавать елемент
		/// </summary>
		DateTime dateTime;
		/// <summary>
		/// кнопки в консоле
		/// </summary>
		Button Bfind, Bsearch, BSearchCurrent, BbackSpace, Bdelete, BfindSize, BfindLastChange, BfindAccess, Bfindtxt;
		Button Bcopy, Bcut, Bpaste, Bclear, BPAnalyseAdd, BPStartAnalyze, Btext, Bfindwords;
		/// <summary>
		/// тут помещаем результаты любых поисков
		/// </summary>
		List<string> searching_results;
		/// <summary>
		/// массив информации которую хотим скопировать
		/// </summary>
		List<string> buffer;
		/// <summary>
		/// массив информации для перемещения
		/// </summary>
		List<string> cut_buffer;
		/// <summary>
		/// добавляю файлы для анализа
		/// </summary>
		List<string> files_for_analyze;
		/// <summary>
		/// логическая переменная, которая отвечает за вывод результата текущего состояния, или мы ищем или нет
		/// </summary>

		bool searching = false;
		/// <summary>
		/// регулярные выражения для обработки пользовательского вввода
		/// </summary>
		Regex Rdate, RangeBytes, Regex_mask;
		/// <summary>
		/// массив для считывания все текущий каталогов и файлов
		/// </summary>
		string[] all_files_entries;
		/// <summary>
		/// переменная, отслеживающая на каком листе мы в текущей момент 
		/// </summary>
		int curent_list = 1;
		/// <summary>
		/// количество файлов на странице для отображение, под минимальное разрешение экрана 27 == оптимальное
		/// </summary>
		int total_files_on_lists = 27;
		/// <summary>
		/// лог.переменная для отслеживания состояния кнопок, отображены или нет
		/// </summary>
		bool show_button = false;




		void ShowsButton()
		{
			if (!show_button && current_folder.Count > 0)
			{
				show_button = true;
				dallbuttons();
			}
			if (searching && !show_button)
			{
				show_button = true;
				dallbuttons();
			}
		}




		public void Menu()
		{
			ConsoleKey consoleKey;
			ConsoleKeyInfo a;
			UpdateInfo();
			do
			{
				a = Console.ReadKey(true);
				consoleKey = a.Key;
				switch (consoleKey)
				{
					case ConsoleKey.Backspace: BackCatalog(); break;
					case ConsoleKey.UpArrow: current_position--; UpdateInfo(); EChangepos(ConsoleKey.DownArrow, null); break;
					case ConsoleKey.DownArrow: current_position++; UpdateInfo(); EChangepos(ConsoleKey.DownArrow, null); break;
					case ConsoleKey.W: current_position--; UpdateInfo(); EChangepos(ConsoleKey.DownArrow, null); break;
					case ConsoleKey.S: current_position++; UpdateInfo(); EChangepos(ConsoleKey.DownArrow, null); break;
					case ConsoleKey.A: if (searching_results.Count > 0 || current_folder.Count > 0) ListDown(); break;
					case ConsoleKey.D: if (searching_results.Count > 0 || current_folder.Count > 0) ListUP(); break;
					case ConsoleKey.Enter: NextCatalog(); break;
					case ConsoleKey.F1: if (searching_results.Count > 0 || current_folder.Count > 0) MoveToButton(); break;
					case ConsoleKey.F2: if (searching_results.Count > 0 || current_folder.Count > 0) SearchButton("date"); break;
					case ConsoleKey.F3: if (searching_results.Count > 0 || current_folder.Count > 0) SearchButton("text"); break;
					case ConsoleKey.F4: if (searching_results.Count > 0 || current_folder.Count > 0) SearchButton("size"); break;
					case ConsoleKey.F5: if (searching_results.Count > 0 || current_folder.Count > 0) SearchButton("change"); break;
					case ConsoleKey.F6: if (searching_results.Count > 0 || current_folder.Count > 0) SearchButton("access"); break;
					case ConsoleKey.F7: if (searching_results.Count > 0 || current_folder.Count > 0) SearchButton("searchtxt"); break;
					case ConsoleKey.F8: if (searching_results.Count > 0 || current_folder.Count > 0) CopyButton(); break;
					case ConsoleKey.F9: if (searching_results.Count > 0 || current_folder.Count > 0) СutButton(); break;
					case ConsoleKey.F10: if (searching_results.Count > 0 || current_folder.Count > 0) PasteButton(); break;
					case ConsoleKey.F12: if (searching_results.Count > 0 || current_folder.Count > 0) ClearButton(); break;
					case ConsoleKey.Delete: if (searching_results.Count > 0 || current_folder.Count > 0) DeleButton(); break;
					case ConsoleKey.D1: if (searching_results.Count > 0 || current_folder.Count > 0) AddAnalyze(); break;
					case ConsoleKey.D2: if (searching_results.Count > 0 || current_folder.Count > 0) StartAnalyze(); break;
					case ConsoleKey.D3: if (searching_results.Count > 0 || current_folder.Count > 0) SearchButton("word"); break;
					case ConsoleKey.OemComma: if (searching_results.Count > 0 || current_folder.Count > 0) ListDown(); break;
					case ConsoleKey.OemPeriod: if (searching_results.Count > 0 || current_folder.Count > 0) ListUP(); break;
					case ConsoleKey.Add: if (searching_results.Count > 0 || current_folder.Count > 0) ListUP(); break;
					case ConsoleKey.Subtract: if (searching_results.Count > 0 || current_folder.Count > 0) ListDown(); break;
					case ConsoleKey.RightArrow: if (searching_results.Count > 0 || current_folder.Count > 0) ListUP(); break;
					case ConsoleKey.LeftArrow: if (searching_results.Count > 0 || current_folder.Count > 0) ListDown(); break;

				}

			} while (consoleKey != ConsoleKey.Escape);
		}



		/// <summary>
		/// метод находит файлы в которых есть наборы слов (поиск выполняется по активному каталогу/диску и в соответствии с веденной маской поиска файлов)
		/// </summary>
		void FindWords(string[] words_for_find, string path, string[] mask)
		{

			try
			{
				foreach (var dir in Directory.GetFileSystemEntries(path))
				{
					//если это файл анализируем, если папка копаем дальше
					if (File.Exists(dir))
					{
						foreach (var maska in mask)
						{
							if (dir.Contains(maska))
							{
								FileStream fileStream = new FileStream(dir, FileMode.Open, FileAccess.Read, FileShare.Read);
								StreamReader binaryReader = new StreamReader(fileStream);
								string text = binaryReader.ReadToEnd();
								foreach (var word in words_for_find)
								{
									//если нашли соответствие
									if (text.Contains(word))
									{
										//добавляем в массив вывода инфы							
										searching_results.Add(dir);
									}
								}
								binaryReader.Close();
							}
						}
					}
					else FindWords(words_for_find, dir, mask);
				}
			}
			catch (Exception e)
			{
				logger.Debug("===========FINDWORDS==============");
				logger.Debug(e);
			}

		}








		void ListUP()
		{
			int length;
			if (searching) length = searching_results.Count;
			else length = all_files_entries.Length;
			if (length > total_files_on_lists * curent_list)
			{
				curent_list++;
				current_position = 1;
				Console.Clear();
				show_button = false;
				UpdateInfo();
			}
		}

		void ListDown()
		{
			if (curent_list > 1)
			{
				curent_list--;
				current_position = 1;
				Console.Clear();
				show_button = false;
				UpdateInfo();
			}
		}




		/// <summary>
		/// начинаем разбирать тект который был выбран для анализа
		/// </summary>
		void StartAnalyze()
		{
			try
			{
				if (files_for_analyze.Count > 0)
				{
					Console.SetCursorPosition(79, 4);
					Console.Write("Enter text for search(x-return) (if range then a,b) : ");
					string text = Console.ReadLine();
					text = text.ToLower();
					if (text == "x")
					{
						Console.Clear();
						show_button = false;
						UpdateInfo();
						return;
					}
					string[] text_search = text.Split((" ',' ,  ' ', '.' , '/' ").ToArray(), StringSplitOptions.RemoveEmptyEntries);
					Console.SetCursorPosition(79, 5);
					Console.Write("Enter text for replace (x-return): ");
					string text_for_replace = Console.ReadLine();
					if (text_for_replace == "x")
					{
						Console.Clear();
						show_button = false;
						UpdateInfo();
						return;
					}
					foreach (var item in files_for_analyze)
					{
						if (File.Exists(item))
						{
							FileStream fileStream = new FileStream(item, FileMode.Open, FileAccess.Read);
							StreamReader streamReader = new StreamReader(fileStream);
							string temp = streamReader.ReadToEnd();
							streamReader.Close();
							foreach (var item2 in text_search)
							{
								if (temp.Contains(item2))
								{
									temp = temp.Replace(item2, text_for_replace);
								}
							}
							fileStream = new FileStream(item, FileMode.Truncate, FileAccess.Write);
							StreamWriter streamWriter = new StreamWriter(fileStream);
							streamWriter.Write(temp);
							streamWriter.Close();
						}
					}
					files_for_analyze.Clear();
					UpdateInfo();
					Console.SetCursorPosition(79, 4);
					Console.Write("All done!");

				}
				else EError("U dont have files for analyze!", null);
			}
			catch (Exception e)
			{
				logger.Debug("===========StartAnalyzeError===");
				logger.Debug(e);
			}
		}


		/// <summary>
		/// метод добавления текста в анализируемый
		/// </summary>
		void AddAnalyze()
		{
			try
			{
				string path;
				int a = ((curent_list - 1) * total_files_on_lists) + current_position - 2;
				ECallButton(BPAnalyseAdd, null);
				if (!searching)
				{
					path = all_files_entries[a];
					//если это файл то добавляем на анализ				
				}
				else
				{
					path = searching_results[a];
				}
				if (File.Exists(path)) files_for_analyze.Add(path);
				else EError("U cant add dis: " + path, null);

			}
			catch (Exception e)
			{
				logger.Debug(e);
			}

		}



		/// <summary>
		/// очистка буфера
		/// </summary>
		void ClearButton()
		{
			if (buffer.Count > 0) buffer.Clear();
			if (cut_buffer.Count > 0) cut_buffer.Clear();
			if (files_for_analyze.Count > 0) files_for_analyze.Clear();
			ECallButton(Bclear, null);
		}



		/// <summary>
		/// вставляем все из текущих буферов в текущий каталог
		/// </summary>
		void PasteButton()
		{
			string path_new = null;
			try
			{

				if (buffer.Count > 0)
				{
					foreach (var item in buffer)
					{
						if (File.Exists(item))
						{
							FileInfo fileInfo = new FileInfo(item);
							path_new = current_folder.Last() + @"\" + fileInfo.Name;
							if (!File.Exists(path_new)) fileInfo.CopyTo(path_new);

						}
						else
						{
							DirectoryInfo directory = new DirectoryInfo(item);
							path_new = current_folder.Last() + @"\" + directory.Name;
							if (!Directory.Exists(path_new)) DirectoryCopy(item, path_new, true);

						}
					}
					buffer.Clear();
				}
				if (cut_buffer.Count > 0)
				{
					foreach (var item in cut_buffer)
					{
						if (File.Exists(item))
						{
							FileInfo fileInfo = new FileInfo(item);
							path_new = current_folder.Last() + @"\" + fileInfo.Name;
							if (!File.Exists(path_new)) File.Move(item, path_new);
						}
						else
						{
							DirectoryInfo directory = new DirectoryInfo(item);
							if (directory.Exists)
							{
								path_new = current_folder.Last() + @"\" + directory.Name + @"\";
								if (!Directory.Exists(path_new)) Directory.Move(item, path_new);
							}

						}
					}
					cut_buffer.Clear();
				}
			}
			catch (Exception e)
			{
				logger.Error("PasteButton error: " + path_new);
				logger.Error(e);

			}
			Console.Clear();
			show_button = false;
			UpdateInfo();
		}

		/// <summary>
		/// копирование папок и содержимого
		/// </summary>
		/// <param name="sourceDirName"></param>
		/// <param name="destDirName"></param>
		/// <param name="copySubDirs"></param>
		void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			try
			{

				DirectoryInfo dir = new DirectoryInfo(sourceDirName);
				DirectoryInfo[] dirs = dir.GetDirectories();
				if (!Directory.Exists(destDirName))
				{
					Directory.CreateDirectory(destDirName);
				}
				FileInfo[] files = dir.GetFiles();
				foreach (FileInfo file in files)
				{
					string temppath = Path.Combine(destDirName, file.Name);
					if (!File.Exists(temppath)) file.CopyTo(temppath, false);
				}
				if (copySubDirs)
				{
					foreach (DirectoryInfo subdir in dirs)
					{
						string temppath = Path.Combine(destDirName, subdir.Name);
						DirectoryCopy(subdir.FullName, temppath, copySubDirs);
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("DirectoryCopy error: " + sourceDirName + "; dist: " + destDirName);
				logger.Error(e);
				Console.WriteLine(e);
			}
		}


		/// <summary>
		/// метод, который добавляет выделенный файл в буфер для вырезания обьектов, а затем при  вставке обьекта совершает перемещение файла
		/// </summary>
		void СutButton()
		{

			if (current_position != 1)
			{
				string s;
				int a = ((curent_list - 1) * total_files_on_lists) + current_position - 2;
				if (searching)
				{
					s = searching_results[a];
				}
				else
					s = all_files_entries[a];
				//если мы хотим вырезать, проверяем нету ли данного файла уже в текущем буфере для вырезки + проверяем не помещен ли данный файл в буфер копирования
				if (!cut_buffer.Contains(s) && !buffer.Contains(s))
				{
					Console.SetCursorPosition(79, 4);
					string tmp = s;
					if (tmp.Length > 56) tmp = tmp.Remove(55);
					Console.Write("Confirm(y/n) to cut: (" + tmp + "): ");
					string s1 = Console.ReadLine();
					Console.SetCursorPosition(79, 4);
					for (int i = 0; i < 83; i++) Console.Write(" ");
					
					s1 = s1.ToLower();
					if (s1 == "n")
					{
						UpdateInfo();
						return;
					}			
					if (File.Exists(s))
					{
						//удаляю из листа найденной информации
						if (searching) searching_results.Remove(searching_results[a]);
					}
					else
					{
						//иначе это папка и удаляем ее
						DirectoryInfo s12 = new DirectoryInfo(s);
						//Directory.Delete(s, true);
						//удаляю все вхождения из нашего серч листа
						if (searching) searching_results.RemoveAll(x => x.Contains(s12.Name));
					}
					cut_buffer.Add(s);
					if (searching)
					{
						Console.Clear();
						show_button = false;
					}
					UpdateInfo();
					Buffer("File added to Cut Buffer", null);
					logger.Debug("Added to cut: " + cut_buffer.Last());
				}
				else Buffer("This position already exist", null);
			}
			else Buffer("U cant copy/cut this", null);

		}
		/// <summary>
		/// метод добавления необходимого елемента в буфер для копирования
		/// </summary>
		void CopyButton()
		{
			string[] b = Directory.GetFileSystemEntries(current_folder.Last());
			if (current_position != 1)
			{
				string s;
				if (searching)
				{
					int a = ((curent_list - 1) * total_files_on_lists) + current_position - 2;
					s = searching_results[a];
				}
				else
				{
					int a = ((curent_list - 1) * total_files_on_lists) + current_position - 2;
					s = all_files_entries[a];
				}
				if (!buffer.Contains(s) && !cut_buffer.Contains(s))
				{
					buffer.Add(s);
					Buffer("File added to Copy Buffer", null);
					logger.Debug("Added to copy: " + buffer.Last());
				}
				else Buffer("This position already exist", null);
			}
			else Buffer("U cant copy/cut this", null);
		}


		/// <summary>
		/// метод удаления файлов и папок
		/// </summary>
		void DeleButton()
		{
			try
			{
				if (current_position != 1)
				{
					int a = ((curent_list - 1) * total_files_on_lists) + current_position - 2;
					string s;
					if (searching)
					{
						s = searching_results[a];
					}
					else
						s = all_files_entries[a];

					Console.SetCursorPosition(79, 3);
					string tmp = s;
					if (tmp.Length > 50) tmp = tmp.Remove(49);
					Console.WriteLine("Confirm to delete file/folder (" + tmp + ")");
					Console.SetCursorPosition(79, 4);
					Console.Write("Choce(y/n): ");
					string u = Console.ReadLine();
					u = u.ToLower();
					if (u == "y")
					{
						FileAttributes atr = File.GetAttributes(s);
						//если это файл и он должен существовать	
						if (atr.HasFlag(FileAttributes.Directory))
						{

							//иначе это папка и удаляем ее
							DirectoryInfo s1 = new DirectoryInfo(s);
							logger.Debug("Delete dir: " + s1.Name);
							Directory.Delete(s, true);
							//удаляю все вхождения из нашего серч листа
							searching_results.RemoveAll(x => x.Contains(s1.Name));

						}
						else
						{
							logger.Debug("Delete file: " + s);
							File.Delete(s);
							if (searching) searching_results.Remove(searching_results[a]);

						}

					}
					Console.Clear();
					show_button = false;
					UpdateInfo();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				logger.Error("Deletefile error");
				logger.Error(e);
			}

		}

		/// <summary>
		/// таймер для поиска файлов в системе
		/// </summary>
		static void SetTimer()
		{
			// Create a timer with a ten second interval.
			aTimer = new System.Timers.Timer(10000);
			// Hook up the Elapsed event for the timer. 
			aTimer.Elapsed += OnTimedEvent;
			aTimer.AutoReset = true;
			aTimer.Enabled = false;

		}


		/// <summary>
		/// анализ длины текста при вводе в консоль
		/// </summary>
		/// <returns></returns>
		string LengthAnalyze()
		{
			int startx = Console.CursorLeft;
			int starty = Console.CursorTop;
			string pattern = "";
			ConsoleKey a = ConsoleKey.DownArrow;
			do
			{

				ConsoleKeyInfo b;
				b = Console.ReadKey();
				a = b.Key;
				if (a == ConsoleKey.Enter) break;
				else if (a == ConsoleKey.Backspace)
				{
					if (pattern.Length > 0)
						pattern = pattern.Remove(pattern.Length-1);

					if (pattern.Length > 0) Console.Write(" \b");
					else
					{
						Console.Write(" \b");
						Console.SetCursorPosition(startx, starty);
					}
				}
				else pattern += b.KeyChar;
				if (pattern.Length + 1 > 28)
				{
					Console.Clear();
					throw new Exception("To many symvols, max is 28");
				}
			} while (pattern.Length < 28);
			return pattern;
		}

		bool CheckUserInput(string x, string mask)
		{
			if (x == "x" || mask == "x")
			{
				show_button = false;
				Console.Clear();
				UpdateInfo();			
			}
			if (mask == "x") return true;
			else if (x == "x") return true;

			return false;
		}
		void SearchButton(string command)
		{
			try
			{
				ECallButton(Bsearch, null);
				string pattern = null, mask = null;		

				if (command == "date")
				{
					Console.SetCursorPosition(79, 3);
					Console.Write("Searching files at date creation!");
					Console.SetCursorPosition(79, 4);
					Console.Write("Enter range of dates (x-back): ");
				}
				else
					if (command == "change")
				{
					Console.SetCursorPosition(79, 3);
					Console.Write("Searching files at laste change!");
					Console.SetCursorPosition(79, 4);
					Console.Write("Enter range of dates (x-return): ");
				}
				else
					if (command == "access")
				{
					Console.SetCursorPosition(79, 3);
					Console.Write("Searching files at laste access!");
					Console.SetCursorPosition(79, 4);
					Console.Write("Enter range of dates (x-return): ");
				}
				else
					if (command == "text")
				{
					Console.SetCursorPosition(79, 3);
					Console.Write("Searching text or files at mask/text");
					Console.SetCursorPosition(79, 4);
					Console.Write("Enter text(t1,t2) for search  (x-return): ");
				}
				else
					if (command == "size")
				{
					Console.SetCursorPosition(79, 3);
					Console.Write("Searching files at size!");
					Console.SetCursorPosition(79, 4);
					Console.Write("Size for search (x-return): ");
				}
				else
					if (command == "searchtxt")
				{
					Console.SetCursorPosition(79, 3);
					Console.Write("Masks searcning txt,docx (x-return): ");
					mask = LengthAnalyze();
					if (CheckUserInput(pattern, mask)) return;
					Console.SetCursorPosition(79, 4);
					Console.Write("Word to search (x-return): ");

				}
				else
					if (command == "word")
				{
					Console.SetCursorPosition(79, 3);
					Console.Write("Enter mask for search (x-return): ");
					mask = LengthAnalyze();
					if (CheckUserInput(pattern, mask)) return;
					Console.SetCursorPosition(79, 4);
					Console.Write("Word to search (x-return): ");

				}
				pattern = LengthAnalyze();
				if (CheckUserInput(pattern, mask)) return;
				Console.Clear();
				searching = true;
				if (searching_results.Count > 0) searching_results.Clear();
				pattern = pattern.ToLower();

				Console.WriteLine("searching begin!");
				aTimer.Start();
				if (command == "date" || command == "change" || command == "access")
				{
					MatchCollection matchCollection = Rdate.Matches(pattern);
					int.TryParse(matchCollection[6].Value + matchCollection[7].Value, out int year2);
					int.TryParse(matchCollection[5].Value, out int month2);
					int.TryParse(matchCollection[4].Value, out int day2);
					int.TryParse(matchCollection[2].Value + matchCollection[3].Value, out int year);
					int.TryParse(matchCollection[1].Value, out int month);
					int.TryParse(matchCollection[0].Value, out int day);
					dateTime = new DateTime(year, month, day);
					DateTime dateTime2 = new DateTime(year2, month2, day2);
					if (command == "date") SearchDate(dateTime, dateTime2, current_folder.Last());
					else if (command == "change") SearchLastChange(dateTime, dateTime2, current_folder.Last());
					else if (command == "access") SearchLastAccess(dateTime, dateTime2, current_folder.Last());
				}
				else
				if (command == "text")
				{
					string[] words = pattern.Split(" ','".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					SearchCatalog(pattern, current_folder.Last(), words);
				}
				else
				if (command == "size")
				{
					MatchCollection matchCollection = RangeBytes.Matches(pattern);
					if (long.TryParse(matchCollection[0].Value, out long min))
					{
						if (long.TryParse(matchCollection[1].Value, out long max))
						{

							SearchAtSize(min, max, current_folder.Last());
						}
						else
						{
							Console.Write("Check input");
							searching = false;
						}
					}
					else
					{
						Console.Write("Check input");
						searching = false;
					}
				}
				else
				if (command == "searchtxt")
				{

					string[] s = mask.Split(" ','".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					SearchInsideTxt(pattern, current_folder.Last(), s);
				}
				else
				if (command == "word")
				{

					string[] s = pattern.Split(" ',', ' ' ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					string[] g = mask.Split(" ',' , ' ' ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					FindWords(s, current_folder.Last(), g);
				}
				Console.Clear();
				current_position = 1;
				show_button = false;
				UpdateInfo();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine("Press enter for continue");
				logger.Error("SearchButton error");
				logger.Error(e);
			}
			finally
			{
				aTimer.Stop();

			}
		}


		/// <summary>
		/// поиск текста в файлах
		/// </summary>
		/// <param name="text_for_search"></param>
		/// <param name="path"></param>
		/// <param name="mask"></param>
		void SearchInsideTxt(string text_for_search, string path, string[] mask)
		{
			try
			{
				foreach (string item in Directory.GetFileSystemEntries(path))
				{
					FileInfo a = new FileInfo(item);
					foreach (var item2 in mask)
					{
						if (a.Extension.EndsWith(item2))
						{
							FileStream fileStream = new FileStream(item, FileMode.Open, FileAccess.ReadWrite);
							StreamReader streamReader = new StreamReader(fileStream);
							string find_s = streamReader.ReadToEnd();
							find_s = find_s.ToLower();
							if (find_s.Contains(text_for_search))
							{
								searching_results.Add(item);
							}
							streamReader.Close();
						}
					}
					//если это папка, углубляемся дальше
					if (!File.Exists(item)) SearchInsideTxt(text_for_search, item, mask);
				}
			}

			catch (Exception e)
			{
				logger.Error("SearchInsideTxt error : " + path);
				logger.Error(e);
			}
		}



		/// <summary>
		/// поиск файлов по диапазону дат последнего использования
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="patch"></param>
		void SearchLastAccess(DateTime min, DateTime max, string patch)
		{
			try
			{
				foreach (string item in Directory.GetFileSystemEntries(patch))
				{
					//если это файл а не папка
					DateTime dateTime1 = File.GetLastAccessTime(item);
					if (dateTime1 >= min && dateTime1 <= max)
					{
						searching_results.Add(item);
					}
					if (!File.Exists(item)) SearchLastAccess(min, max, item);
				}
			}
			catch (Exception ex)
			{
				logger.Error("SearchLastAccess error : " + patch);
				logger.Error(ex);

			}
		}


		/// <summary>
		/// поиск файлов по диапахону дат изменений в файлах
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="patch"></param>
		void SearchLastChange(DateTime min, DateTime max, string patch)
		{
			try
			{
				//получаю список всех файлов и каталогов, и рекурсией углубляюсь в каждую папку
				foreach (string item in Directory.GetFileSystemEntries(patch))
				{
					DateTime dateTime1 = File.GetLastWriteTime(item);
					if (dateTime1 >= min && dateTime1 <= max)
					{
						searching_results.Add(item);
					}
					if (!File.Exists(item)) SearchLastChange(min, max, item);
				}
			}
			catch (Exception ex)
			{
				logger.Error("SearchLastChange error : " + patch);
				logger.Error(ex);

			}
		}



		/// <summary>
		/// производим поиск файлов в указанном промежутке
		/// </summary>
		/// <param name="min">минимальный размер байт</param>
		/// <param name="max">максимальный размер байт</param>
		/// <param name="path">путь</param>
		void SearchAtSize(long min, long max, string path)
		{
			try
			{

				foreach (var item in Directory.GetFileSystemEntries(path))
				{

					//если это файл а не дериктория
					if (File.Exists(item))
					{
						FileInfo a = new FileInfo(item);
						if (a.Length >= min && a.Length <= max)
						{
							searching_results.Add(item);
						}
					}
					else
						SearchAtSize(min, max, item);
				}

			}
			catch (Exception ex)
			{
				logger.Debug("SearchAtSize error : " + path);
				logger.Debug(ex);

			}
		}



		/// <summary>
		/// ищу все вхождения заданного поиска (папки/файлы и тд, повторения исключаются)
		/// </summary>
		/// <param name="pattern"></param>
		/// <param name="path"></param>
		void SearchCatalog(string pattern, string path, string[] words)
		{
			try
			{
				foreach (string item in Directory.GetFileSystemEntries(path))
				{
					foreach (var item2 in words)
					{
						if (item.ToLower().Contains(item2.ToLower()) && !searching_results.Contains(item2.ToLower()) && !searching_results.Contains(item)) searching_results.Add(item);
					}

					if (!File.Exists(item)) SearchCatalog(pattern, item, words);
				}
			}
			catch (Exception e)
			{
				logger.Error("Error in search catalog: " + path);
				logger.Error(e);

			}
		}



		/// <summary>
		/// поиск файлов по всей системе
		/// </summary>
		/// <param name="path"></param>
		/// <param name="pattern"></param>
		void SearchDate(DateTime min, DateTime max, string patch)
		{
			try
			{
				foreach (string item in Directory.GetFileSystemEntries(patch))
				{

					DateTime dateTime1 = File.GetCreationTime(item);
					if (dateTime1 >= min && dateTime1 <= max)
					{
						searching_results.Add(item);
					}
					//если это не файл ищем дальше, если файл его не трогаем
					if (!File.Exists(item)) SearchDate(min, max, item);
				}
			}
			catch (Exception ex)
			{

				logger.Debug("error in search date : " + patch.ToString());
				logger.Debug(ex);

			}
		}

		/// <summary>
		/// реализация кнопки навигации F1
		/// </summary>
		void MoveToButton()
		{
			int y = 3;
			Console.Clear();
			//Console.SetCursorPosition(65, y);			
			//Console.WriteLine("Справочная информация");
			//Console.SetCursorPosition(20, ++y);
			Btext = new Button("ИНФОРМАЦИЯ О ПРОГРАММЕ", '#', 45, 0, 1, 1, 3, 9, 12);
			Btext.ShowButton();

			Btext = new Button("F2 - Поиск файлов / каталогов по дате их создания.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Программа запрашивает период времени для поиска(формат 01.01.2001 или 01 01 2001).");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Поиск осуществляется в каталоге/диске где вы сейчас находитесь.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Результат работы - вывод информации которая была в промежутке указанных дат.");


			Btext = new Button("F3 - Поиск файлов/каталогов по их названию.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Программа запрашивает набор слов(или слово), по которым будет осуществлять поиск.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Поиск осуществляется в каталоге/диске где вы сейчас находитесь,");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("алгоритм работы - анализ каждого файла или каталога на наличие указанного набора слов.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Пример ввода: слово1 слово2,слово3,слво4 слово5.");



			Btext = new Button("F4 - Поиск файлов/каталогов по  их размеру(размер указывается в байтах).", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Программа запрашивает диапазон байт (от 0 до 100 - к примеру), после чего выполняет поиск согласно указанным критериям.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Формат ввода: 0 50000 или 0-5000 или 0,5000 или 0/5000 и тд.");


			Btext = new Button("F5 - Поиск файлов/каталогов по дате их изменения.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Программа запрашивает период времени для поиска(дата1 и дата2). Поиск осуществляется в каталоге/диске где вы сейчас находитесь.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Результат работы: вывод файлов/каталогов которые входят в промежуток указанных дат и условий.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Формат ввода: 01,01,2001 или 01 01 2001 или 01.01.2001.");


			Btext = new Button("F6 - Поиск файлов/каталогов по дате последнего обращения к файлу/папке.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Программа запрашивает период времени для поиска(дата1 и дата2). Поиск осуществляется в каталоге/диске где вы сейчас находитесь.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Результат работы: вывод файлов/каталогов которые входят в промежуток указанных дат и условий.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Формат ввода: 01,01,2001 или 01 01 2001 или 01.01.2001.");


			Btext = new Button("F7 - Поиск файлов по заданной маске.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Программа запрашивает набор \"масок\", по которым будет осуществляться поиск.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("К примеру, нужно найти все тектовые файлы и html файлы.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Вводим \"txt,html\" и программа найдет все файлы с этим расширением(поиск в активном каталоге - где вы сейчас находитесь).");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Если нужно найти файлы ТОЛЬКО с этим расширением, то следущий параметр ввода пропускаете(жмете ввод и пустую строчку оставляете),");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("но если вам в содержании этих файлов нужно найти определенную строку,указываете в param2, что нужно найти в этих файлах.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("К примеру Вам нужно найти следующие слова: alex,dima,skin - вы их точно также и вводите в параметры строки.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Формат ввода: строка1: txt,html");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Формат ввода: строка2: alex,timyr");



			Btext = new Button("F8 - Добавление файла/папки в буфер для копирования.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Вы можете сколько угодно добавить файлов/папок в буфер копирования.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Добавление осуществляется относительно выделенной позиции в менеджере файлов.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Чтобы добавить каталог/файл - нужно нажать на кнопку.");


			Btext = new Button("F9 - Добавление файла/папки в буфер для перемещения.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Вы можете сколько угодно добавить файлов/папок в буфер файлов/каталогов для перемещения.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Добавление осуществляется относительно выделенной позиции в менеджере файлов.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Чтобы добавить каталог/файл - нужно нажать на кнопку.");




			Btext = new Button("F10 - Кнопка вставки.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Выполняется копирование/перемещение всех файлов из соответствующих буферов.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Если у вас были файлы/папки в буфере на копировании - будут скопированы, если были в буфере на перемещение(Cut) - будут перемещены.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Действие выполняется сразу для всех буферов.");

			Btext = new Button("Backspace - возврат в предыдущий каталог.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();



			Btext = new Button("F12 - Очистка буфера для копирования/перемещения файлов/папок.", '#', 20, y += 3, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Программа выполнит сброс накопленной информации со всех буферов (очистит их).");




			Btext = new Button("1 - Добавление файлов на анализ текста.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Вы можете добавить сколько угодно файлов в буфер для анализирования файлов.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Каталоги не добавляются!.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Чтобы добавить файл - нужно нажать на кнопку.");




			Btext = new Button("2 - Анализирование файлов.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Метод выполняет поиск текста в выбранных файлах с последующей заменой нужного текста.");
			Console.SetCursorPosition(20, ++y);
			Console.WriteLine("Первой строкой программа запрашивает слово(набор слов) для поиска в отобранных ранее файлах.");
			Console.SetCursorPosition(20, ++y);
			Console.WriteLine("Второй строкой ввода, Вы указываете на какие наборы слов(слово) Вам нужно выполнить замену.");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Формат ввода: строка1(указываем что найти): alex,vadim");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Формат ввода: строка2(указываем на что заменить): alex3");


			Btext = new Button("3 - Поиск файлов по маске и содержанию файла.", '#', 20, y += 1, 1, 1, 3, 11, 3);
			Btext.ShowButton();
			Console.SetCursorPosition(20, y += 3);
			Console.WriteLine("Метод выполняет поиск текста(заданного вами) в выбранных файлах (согласно маске ввода).");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Параметры ввода, строка1: вводите через любой разделить в каких файлах нужно выполнить поиск(может быть набор файлов).");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Параметры ввода, строка2: вводите через любой разделить, что нужно искать в файле(может быть набор слов).");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Пример ввода, строка1: txt,ini,dat");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Пример ввода, строка2: alex,1213 dima valentin");
			Console.SetCursorPosition(20, y += 1);
			Console.WriteLine("Результат работы - программа находит все файлы которые соответствуют строке1, далее открывает файл и ищет соответствия строки2.");


			Btext = new Button("Выход-нажмите ВВод", '#', 20, y += 3, 1, 1, 3, 4, 7);
			Btext.ShowButton();
		}



		/// <summary>
		/// метод перемещений по каталогам
		/// </summary>
		void NextCatalog()
		{
			try
			{
				Console.Clear();
				show_button = false;
				if (current_folder.Count == 0)
				{
					for (int i = 0; i < disks.Length; i++)
					{
						if (i == current_position - 1 && disks[i].IsReady)
						{
							current_folder.Add(disks[i].Name);
							break;
						}
					}
				}
				else
				{
					int a = ((curent_list - 1) * total_files_on_lists) + current_position - 2;

					//если мы в позиции 1 == \..\ = возвращаемся назад
					if (current_position == 1)
					{
						BackCatalog();
						return;
					}
					else
					{
						if (!searching)
						{
							//если мы выбрали файл  то запустим его, если нет то это каталог - войдем в него
							if (File.Exists(all_files_entries[a]))
							{

								Process.Start(all_files_entries[a]);
							}
							else
							{
								current_folder.Add(all_files_entries[a]);
								curent_list = 1;
							}
						}
						else
						{
							if (File.Exists(searching_results[a]))
							{

								Process.Start(searching_results[a]);
							}
							else
							{
								current_folder.Add(searching_results[a] + @"\");
								searching = false;
								curent_list = 1;
							}

						}
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("====NEXT catalog error");
				logger.Error(e);
			}
			current_position = 1;

			UpdateInfo();
		}

		/// <summary>
		/// графическое отображение нашего перемещения
		/// </summary>
		/// <param name="curren_places_count"></param>
		/// <param name="in_dirs"></param>
		void Navagation(int curren_places_count, bool in_dirs = false)
		{

			if (current_position == curren_places_count)
			{
				Console.BackgroundColor = ConsoleColor.Magenta;
				Console.ForegroundColor = ConsoleColor.Yellow;
				return;
			}
			if (in_dirs || searching)
			{
				int length;
				if (searching) length = searching_results.Count;
				else length = all_files_entries.Length;

				if (current_position == curren_places_count)
				{
					Console.BackgroundColor = ConsoleColor.Magenta;
					Console.ForegroundColor = ConsoleColor.Yellow;
					return;
				}
				int delta = length - ((curent_list - 1) * total_files_on_lists);
				if (delta >= total_files_on_lists)
				{
					//когда лист заполнен полностью
					if (current_position > total_files_on_lists)
					{

						if (current_position - 1 > total_files_on_lists + 1)
						{
							current_position = 1;
							Console.BackgroundColor = ConsoleColor.Magenta;
							return;
						}
					}
					else
					{
						if (current_position < 1)
						{
							current_position = total_files_on_lists + 2;
							Console.BackgroundColor = ConsoleColor.Magenta;

						}
					}
					//когда лист заполнен частично
				}
				else
				{
					if (current_position > delta + 1)
					{
						current_position = 1;
						Console.BackgroundColor = ConsoleColor.Magenta;
						return;
					}
					else
					{
						if (current_position < 1)
						{
							current_position = delta + 1;
							Console.BackgroundColor = ConsoleColor.Magenta;

						}
					}
				}
			}
			else
			{

				if (current_position > disks.Count())
				{
					current_position = 1;
				}
				else
				{
					if (current_position < 1)
					{
						current_position = disks.Count();
					}
				}
			}

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.BackgroundColor = ConsoleColor.Black;
		}










		/// <summary>
		/// каждый шаг в консоли == обновление и перерисовка информации
		/// </summary>
		void UpdateInfo()
		{
			int count = 1;
			try
			{

				//навигация для списка поиска и списка каталогов немного отличается, поэтому через условия
				if (!searching)
				{
					if (current_folder.Count == 0)
					{
						Console.Clear();
						GetDevices();
						return;
					}
					all_files_entries = Directory.GetFileSystemEntries(current_folder.Last());			
					ShowsButton();
					Console.SetCursorPosition(0, 6);
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("===================NAMES============================SIZE======================ATTRIBUTES=================Date Creation=======Date Last Write=====Last Acess Time");
					Navagation(count, true);
					Console.WriteLine(@"\..\");
					count++;
					Navagation(count, true);
					//составляю лист для отображения
					int skips = 0;
					if (all_files_entries.Length > total_files_on_lists)
					{
						if (curent_list > 1) skips = total_files_on_lists * (curent_list - 1);
					}
					var obj_temp = all_files_entries.Skip(skips);
					int count2 = -1;
					Console.SetCursorPosition(0, 8);
					foreach (string item in obj_temp)
					{
						if (count2 == total_files_on_lists) break;
						count2++;
						DirectoryInfo directoryInfo = new DirectoryInfo(item);
						FileAttributes fileAttributes = directoryInfo.Attributes;
						//если это не папка
						if (!fileAttributes.HasFlag(FileAttributes.Directory))
						{
							FileInfo file = new FileInfo(item);
							string b = file.Name;
							if (b.Length > 40)
							{
								b = b.Remove(40);
								b += "...";
							}
							Console.Write(count + skips + ". " + b);
							dateTime = file.CreationTime;
							Console.SetCursorPosition(49, Console.CursorTop);
							Console.Write("|");
							Console.SetCursorPosition(50, Console.CursorTop);
							Console.ForegroundColor = ConsoleColor.Cyan;
							Console.Write(file.Length);
							Console.SetCursorPosition(65, Console.CursorTop);
							Console.Write("|");
							Console.SetCursorPosition(66, Console.CursorTop);
							Console.ForegroundColor = ConsoleColor.DarkGreen;
							Console.Write(file.Attributes);
							Console.SetCursorPosition(105, Console.CursorTop);
							Console.Write("|");
							Console.SetCursorPosition(106, Console.CursorTop);
							Console.ForegroundColor = ConsoleColor.DarkMagenta;
							Console.Write(dateTime.ToShortDateString());
							dateTime = file.LastWriteTime;
							Console.SetCursorPosition(125, Console.CursorTop);
							Console.Write("|");
							Console.ForegroundColor = ConsoleColor.Green;
							Console.Write(dateTime.ToShortDateString());
							dateTime = file.LastAccessTime;
							Console.SetCursorPosition(145, Console.CursorTop);
							Console.Write("|");
							Console.ForegroundColor = ConsoleColor.DarkYellow;
							Console.WriteLine(dateTime.ToShortDateString());
							count++;

						}
						else
						{

							string b = directoryInfo.Name;
							if (b.Length > 40)
							{
								b = b.Remove(40);
								b += "...";
							}
							Console.Write(count + skips + ". " + b);
							dateTime = directoryInfo.CreationTime;
							Console.SetCursorPosition(49, Console.CursorTop);
							Console.Write("|");
							Console.SetCursorPosition(50, Console.CursorTop);
							Console.ForegroundColor = ConsoleColor.Cyan;
							Console.Write("~");
							Console.SetCursorPosition(65, Console.CursorTop);
							Console.Write("|");
							Console.SetCursorPosition(66, Console.CursorTop);
							Console.ForegroundColor = ConsoleColor.DarkGreen;
							string a = directoryInfo.Attributes.ToString();
							if (a.Length > 38) a = a.Remove(38);
							Console.Write(a);
							Console.SetCursorPosition(105, Console.CursorTop);
							Console.Write("|");
							Console.SetCursorPosition(106, Console.CursorTop);
							Console.ForegroundColor = ConsoleColor.DarkMagenta;
							Console.Write(dateTime.ToShortDateString());
							dateTime = directoryInfo.LastWriteTime;
							Console.SetCursorPosition(125, Console.CursorTop);
							Console.Write("|");
							Console.ForegroundColor = ConsoleColor.Green;
							Console.Write(dateTime.ToShortDateString());
							dateTime = directoryInfo.LastAccessTime;
							Console.SetCursorPosition(145, Console.CursorTop);
							Console.Write("|");
							Console.ForegroundColor = ConsoleColor.DarkYellow;
							Console.WriteLine(dateTime.ToShortDateString());
							count++;

						}
						Navagation(count, true);
					}
					Console.SetCursorPosition(0, total_files_on_lists + 9);
					Console.Write("===========================================================================" + ("<" + curent_list + "/" + Total_lists + ">") + "=================================================================================");

					int y = 6;
					Console.ForegroundColor = ConsoleColor.Yellow;
					for (int i = 0; i < total_files_on_lists + 4; i++)
					{
						Console.SetCursorPosition(160, y++);
						Console.WriteLine("#");
					}
					Console.SetCursorPosition(0, total_files_on_lists + 10);
					Manager_EChangeDirectory(current_folder.Last(), null);

				}
				else
				{
					///если мы в поиске, отключаем не нужные кнопки и делаем вывод информации немного другой
			
					ShowsButton();
					Console.SetCursorPosition(0, 6);
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("===================NAMES============================SIZE======================ATTRIBUTES=================Date Creation=======Date Last Write=====Last Acess Time");
					Navagation(count, true);
					Console.WriteLine(@"\..\");
					count++;
					Navagation(count, true);
					if (searching_results.Count == 0)
					{
						Console.WriteLine("No result");
					}
					else
					{						
						//составляю лист для отображения
						int skips = 0;
						if (searching_results.Count() > total_files_on_lists)
						{
							if (curent_list > 1) skips = total_files_on_lists * (curent_list - 1);
						}
						var obj_temp = searching_results.Skip(skips);
						int count2 = -1;
						for (int i = 0; i < obj_temp.Count(); i++)
						{
							if (count2 == total_files_on_lists) break;
							Navagation(count, true);
							FileInfo a = new FileInfo(obj_temp.ElementAt(i));
							string b = a.FullName;
							if (b.Length > 40)
							{
								b = b.Remove(40);
								b += "...";
							}
							dateTime = a.CreationTime;
							Console.Write(count + skips + ". " + b);
							Console.SetCursorPosition(49, Console.CursorTop);
							Console.Write("|");
							Console.SetCursorPosition(50, Console.CursorTop);
							if (a.Exists) Console.Write(a.Length);
							Console.SetCursorPosition(65, Console.CursorTop);
							Console.Write("|");
							Console.SetCursorPosition(66, Console.CursorTop);
							Console.Write(a.Attributes);
							Console.SetCursorPosition(125, Console.CursorTop);
							Console.Write("|");
							Console.Write(dateTime.ToShortDateString());
							dateTime = a.LastWriteTime;
							Console.SetCursorPosition(125, Console.CursorTop);
							Console.Write("|");
							Console.Write(dateTime.ToShortDateString());
							dateTime = a.LastAccessTime;
							Console.SetCursorPosition(145, Console.CursorTop);
							Console.Write("|");
							Console.WriteLine(dateTime.ToShortDateString());
							count++;
							count2++;
						}
									
						
					}
					Navagation(count, true);
					Console.SetCursorPosition(0, total_files_on_lists + 9);
					Console.Write("===========================================================================" + ("<" + curent_list + "/" + ((searching_results.Count / total_files_on_lists) + 1) + ">") + "=================================================================================");

					int y = 6;
					Console.ForegroundColor = ConsoleColor.Yellow;
					for (int i = 0; i < total_files_on_lists + 4; i++)
					{
						Console.SetCursorPosition(160, y++);
						Console.WriteLine("#");
					}
					Console.SetCursorPosition(0, total_files_on_lists + 10);
					Manager_EChangeDirectory(current_folder.Last(), null);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error in table label, press enter");
				logger.Debug("Error in update info");
				logger.Debug(e);
			}
			finally
			{
				GC.Collect();
			}
		}

		/// <summary>
		/// возврат в предыдущий каталог
		/// </summary>
		void BackCatalog()
		{
			if (current_folder.Count > 0)
			{
				searching = false;
				current_folder.Remove(current_folder.Last());
				current_position = 1;
				curent_list = 1;
				if (current_folder.Count == 0) show_button = false;
				show_button = false;
				Console.Clear();
				UpdateInfo();
			}

		}


		/// <summary>
		/// получение дисков и вывод их
		/// </summary>
		void GetDevices()
		{
			//размерность дисков вычисляется в целых числах, иначе не смог бы сделать универсальную рамки(формы прямоугольные) для них
			int count = 1;
			Console.WriteLine("\n\n\n\n");
			Navagation(count);
			///повторно вызываю для того, если во время работы с менеджером, вставили флешку, и нам нужно ее увидеть
			disks = DriveInfo.GetDrives();
			try
			{
				foreach (var item in disks)
				{
					Navagation(count);
					for (int i = 0; i < 7; i++)
					{
						if (i == 0 || i == 6)
						{
							for (int c = 0; c < 30; c++)
							{
								Console.Write("=");
							}

						}
						else
						{
							for (int c = 0; c < 30; c++)
							{

								if (c == 1 && i == 1)
								{
									Console.Write(item.Name);
									int temp = c;
									c += item.Name.Length;
									while (c < 29)
									{
										Console.Write(" ");
										c += 1;
									}
									Console.Write("#");

								}
								else
								if (c == 1 && i == 2)
								{
									if (item.IsReady)
									{
										string p = item.VolumeLabel;
										if (p.Length > 15) p = p.Remove(15);
										Console.Write("Название: " + p);
										c += p.Length + 10;
										while (c < 29)
										{
											Console.Write(" ");
											c += 1;
										}
										Console.Write("#");
									}
									else
									{

										while (c < 29)
										{
											Console.Write(" ");
											c += 1;
										}
										Console.Write("#");
									}
								}
								else
								if (c == 1 && i == 3)
								{
									if (item.IsReady)
									{
										string p = item.DriveFormat;
										if (p.Length > 15) p = p.Remove(15);
										Console.Write("Система: " + p);
										c += p.Length + 9;
										while (c < 29)
										{
											Console.Write(" ");
											c += 1;
										}
										Console.Write("#");
									}
									else
									{
										while (c < 29)
										{
											Console.Write(" ");
											c += 1;
										}
										Console.Write("#");
									}
								}
								else
								if (c == 1 && i == 4)
								{
									if (item.IsReady)
									{
										int t;
										Console.Write("Свободно места: " + item.TotalFreeSpace / 1073741824 + " GB");
										long g = item.TotalFreeSpace / 1073741824;
										if (g == 0)
										{
											t = 1;
										}
										else t = (int)Math.Log10(g) + 1;
										c += 19;
										c += t;//сюда входит текст Свободно места

										while (c < 29)
										{
											Console.Write(" ");
											c += 1;
										}
										Console.Write("#");
									}
									else
									{

										while (c < 29)
										{
											Console.Write(" ");
											c += 1;
										}
										Console.Write("#");
									}
								}
								else
								if (c == 1 && i == 5)
								{
									if (item.IsReady)
									{
										Console.Write("Обьем диска: " + item.TotalSize / 1073741824 + " GB");
										long g = item.TotalSize / 1073741824;
										c += (int)Math.Log10(g) + 17;
										while (c < 29)
										{
											Console.Write(" ");
											c += 1;
										}
										Console.Write("#");
									}
									else
									{
										while (c < 29)
										{
											Console.Write(" ");
											c += 1;
										}
										Console.Write("#");
									}
								}
								else
									Console.Write(" ");

							}

						}
						Console.WriteLine();
					}
					count++;
				}
			}
			catch (Exception e)
			{

				logger.Error("Ошибка обработки девайсов", e);

			}
			Navagation(count);
		}




		#region Construct and property

		static Manager()
		{
			logger = LogManager.GetCurrentClassLogger();
		}
		public Manager()
		{

			logger.Info("Start programm");
			disks = DriveInfo.GetDrives();
			current_folder = new List<string>();
			buffer = new List<string>();
			cut_buffer = new List<string>();
			searching_results = new List<string>();
			files_for_analyze = new List<string>();
			dateTime = new DateTime();
			Console.ForegroundColor = ConsoleColor.Yellow;
			EChangepos += Manager_EChangepos;
			ECallButton += Bfind_EPressbutton;
			Buffer += Manager_Buffer;
			EError += Manager_EError;
			Bfind = new Button("Info(F1)", '#', 1, 0, 1, 1, 3, 13, 1);
			Bsearch = new Button("SDate(F2)", '#', 13, 0, 1, 1, 3, 7, 2);
			BSearchCurrent = new Button("Search(F3)", '#', 26, 0, 1, 1, 3, 3, 11);
			BfindSize = new Button("Find1(F4)", '#', 40, 0, 1, 1, 3, 10, 8);
			BfindLastChange = new Button("Find2(F5)", '#', 53, 0, 1, 1, 3, 7, 5);
			BfindAccess = new Button("Find3(F6)", '#', 66, 0, 1, 1, 3, 11, 3);
			Bfindtxt = new Button("Find4(F7)", '#', 79, 0, 1, 1, 3, 7, 9);
			Bdelete = new Button("Delete", '#', 92, 0, 1, 1, 3, 6, 13);
			Bcopy = new Button("Copy(F8)", '#', 102, 0, 1, 1, 3, 10, 13);
			Bcut = new Button("Cut(F9)", '#', 114, 0, 1, 1, 3, 4, 7);
			Bpaste = new Button("Paste(F10)", '#', 125, 0, 1, 1, 3, 3, 10);
			BbackSpace = new Button("Back(backspace)", '#', 139, 0, 1, 1, 3, 9, 12);
			Bclear = new Button("Clear Buffer(F12)", '#', 1, 3, 1, 1, 3, 3, 10);
			BPAnalyseAdd = new Button("Add to Analyze(1)", '#', 22, 3, 1, 1, 3, 7, 9);
			BPStartAnalyze = new Button("Analyze Files(2)", '#', 43, 3, 1, 1, 3, 14, 1);
			Bfindwords = new Button("FindWords(3)", '#', 63, 3, 1, 1, 3, 14, 8);

			//вытягиваю даты
			Rdate = new Regex(@"\d{2}", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
			//шаблон для поиска файлов по размеру, шаблон в виде ренжа 234234-234234
			RangeBytes = new Regex(@"\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
			Regex_mask = new Regex(@"\w+");
			SetTimer();
			dallbuttons += Bfind.ShowButton;
			dallbuttons += Bsearch.ShowButton;
			dallbuttons += BSearchCurrent.ShowButton;
			dallbuttons += BbackSpace.ShowButton;
			dallbuttons += Bdelete.ShowButton;
			dallbuttons += BfindSize.ShowButton;
			dallbuttons += BfindLastChange.ShowButton;
			dallbuttons += BfindAccess.ShowButton;
			dallbuttons += Bfindtxt.ShowButton;
			dallbuttons += Bcopy.ShowButton;
			dallbuttons += Bcut.ShowButton;
			dallbuttons += Bpaste.ShowButton;
			dallbuttons += Bfindwords.ShowButton;
			dallbuttons += BPStartAnalyze.ShowButton;
			dallbuttons += BPAnalyseAdd.ShowButton;
			dallbuttons += Bclear.ShowButton;

			dallbuttonssearch += BbackSpace.ShowButton;
			dallbuttonssearch += Bdelete.ShowButton;
			dallbuttonssearch += Bcopy.ShowButton;
			dallbuttonssearch += Bcut.ShowButton;
			dallbuttonssearch += Bpaste.ShowButton;
			dallbuttonssearch += Bclear.ShowButton;
			dallbuttonssearch += BPAnalyseAdd.ShowButton;
			dallbuttonssearch += BPStartAnalyze.ShowButton;
		}


		int Total_lists
		{
			get
			{
				if (all_files_entries.Count() > total_files_on_lists)
				{
					return (all_files_entries.Length / total_files_on_lists) + 1;
				}
				else return 1;
			}
		}

		#endregion


		#region EventManager

		private void Manager_EError(object sender, ErrorEventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("===================Error Logs===================");
			Console.WriteLine(sender.ToString());
			Console.WriteLine("================================================");
		}

		/// <summary>
		/// при добавлении файлов в буфер выводим евент сообщение
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Manager_Buffer(object sender, EventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("===================Event Logs===================");
			Console.WriteLine("Added to buffer: " + sender.ToString());
			Console.WriteLine("================================================");
			Console.ForegroundColor = ConsoleColor.Yellow;
		}



		/// <summary>
		/// евент на нажатие кнопок
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Bfind_EPressbutton(object sender, EventArgs e)
		{
			Button s = sender as Button;
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("===================Event Logs===================");
			Console.WriteLine("User push button: " + s.Text);
			Console.WriteLine("================================================");
			Console.ForegroundColor = ConsoleColor.Yellow;
		}

		/// <summary>
		/// отображение текущей локации
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Manager_EChangeDirectory(object sender, EventArgs e)
		{
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("===================Event Logs===================");
			Console.WriteLine("Current location: " + sender.ToString());
			Console.WriteLine("================================================");
			Console.ForegroundColor = ConsoleColor.Yellow;
		}
		/// <summary>
		/// евент на нажатие клавиш
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Manager_EChangepos(object sender, EventArgs e)
		{

			if (buffer.Count > 0 || cut_buffer.Count > 0 || files_for_analyze.Count > 0)
			{
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("==================BUFFER LOGS===================");
				if (files_for_analyze.Count > 0)
				{

					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("ATTENTION!!! Analyze buffer not empty!");
					Console.ForegroundColor = ConsoleColor.DarkGreen;
				}
				if (buffer.Count > 0 || cut_buffer.Count > 0)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("ATTENTION!!!Buffer memory have some info");
					Console.ForegroundColor = ConsoleColor.DarkGreen;
				}
				Console.WriteLine("================================================");
			}

		}

		/// <summary>
		/// евент обработки таймера при поиске, копировании
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			Console.WriteLine("Searching: " + e.SignalTime);

		}
		#endregion

	}
}
