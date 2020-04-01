using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	internal sealed class Button
	{
	
		int xpos;
		int ypos;
		int width;
		int length;
		string text;
		char button_symvol;
		int color_text;
		int text_x_indent;
		int text_y_indent;
		int color_background;



		internal int Xpos { get => xpos; set => xpos=value; }
		internal int Ypos { get => ypos; set => ypos=value; }
		internal int Width { get => width; }
		internal int Length { get => length;  }
		internal string Text { get => text; set => text = value; }

		internal void ShowButton()
		{
		
			Console.SetCursorPosition(xpos, ypos);
			int tempx = xpos;
			int tempy = ypos;
			for (int i = 0; i < width; i++)
			{
				for (int c = 0; c < length; c++)
				{
					if (i == 0 || i + 1 == width || c == 0 || c + 1 == length)
					{

						Console.ForegroundColor = (ConsoleColor)color_text;
						Console.BackgroundColor = ConsoleColor.Black;
						Console.Write(button_symvol);
					}
					else
					if (i == text_y_indent && c == text_x_indent)
					{
						Console.ForegroundColor = (ConsoleColor)color_text;
						Console.BackgroundColor = (ConsoleColor)color_background;
						Console.Write(text);
						int a = text.Length - 1;
						c += a;
					}
					else
					{
						Console.ForegroundColor = (ConsoleColor)color_text;
						Console.BackgroundColor = (ConsoleColor)color_background;
						Console.Write(" ");
					}
				}
				Console.WriteLine();
				Console.SetCursorPosition(xpos, tempy += 1);
			}
			
			Console.ResetColor();
	
		}

		internal Button(string text, char button_symvol, int xpos, int ypos, int text_x_indent, int text_y_indent, int width, int color_text, int color_background)
		{
			this.text = text;
			this.button_symvol = button_symvol;
			this.xpos = xpos;
			this.ypos = ypos;
			this.width = width;
			this.length = text.Length+2;
			this.color_text = color_text;
			this.color_background = color_background;
			this.text_x_indent = text_x_indent;
			this.text_y_indent = text_y_indent;

		}

	}
}

