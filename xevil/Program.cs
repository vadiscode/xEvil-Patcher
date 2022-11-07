using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace xevil
{
    internal sealed class Program
	{
		private static void ConsoleHeader()
		{
			Console.Clear();
			Console.WriteLine("-------------------------------------------------------");
			Console.WriteLine("|                   xEvil Patcher                     |");
			Console.WriteLine("-------------------------------------------------------");
			Console.WriteLine("");
		}

		private static void Main()
		{
			var foundxevil = false;
			var makepatch1 = false;
			var makepatch2 = false;
			var iter = 1;
			var once = false;
			var flag = false;
			var fullpath = "";
			var fullpid = 0u;
            var num = 0;

			ConsoleHeader();

			while (true)
			{
				makepatch1 = false;
				makepatch2 = false;
				if (fullpath != "")
				{
					var directoryName = Path.GetDirectoryName(fullpath);
                    if (Directory.Exists(directoryName))
					{
						var path = directoryName + "\\log.txt";
						if (File.Exists(path))
						{
							num = Enumerable.Count(Enumerable.ToList(File.ReadAllLines(path)));
							Console.Title = "xEvil Patcher | Captchas: " + num;
							if (num > 600)
							{
								File.Delete(path);
								if (fullpid != 0)
								{
									Console.WriteLine("Killing process!");
									KillTask(fullpid);
								}
								fullpid = 0u;
							}
						}
					}
				}
				Utilities.EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
				{
					var windowClass = GetWindowClass(hWnd);
                    if (windowClass.ToLower().Contains("xevilmain"))
					{
						foundxevil = true;
						if (!once)
						{
							fullpath = GetFullName(hWnd, out fullpid);
							Console.WriteLine("Path: " + fullpath + "; PId: " + fullpid);
						}
					}
					if (windowClass.ToLower().Contains("formgen"))
					{
						var window = Utilities.GetWindow(hWnd, 4u);
                        if (window != IntPtr.Zero && GetWindowClass(window).ToLower().Contains("xevilmain"))
						{
							Utilities.EnumChildWindows(hWnd, delegate (IntPtr chWnd, IntPtr clParam)
							{
								if (GetWindowClass(chWnd).ToLower().Contains("tbutton"))
								{
									if (!makepatch1)
									{
										Console.WriteLine(DateTime.Now.ToString() + " [Patching xEvil] #" + iter);
									}
									makepatch1 = true;
									Utilities.ShowWindowAsync(hWnd, 0);
									Utilities.SendMessage(chWnd, 10u, 0, 0);
									Utilities.SendMessage(chWnd, 7u, 0, 0);
									Utilities.SendMessage(chWnd, 6u, 0, 0);
									Utilities.SendMessage(chWnd, 245u, 0, 0);
								}
								return true;
							}, IntPtr.Zero);
						}
					}
					if (windowClass.ToLower().Contains("#32770"))
					{
						var push = false;
                        var bpush = IntPtr.Zero;
                        Utilities.EnumChildWindows(hWnd, delegate (IntPtr chWnd, IntPtr clParam)
						{
							if (GetWindowText(chWnd).ToLower().Contains("xevil"))
							{
								push = true;
							}
							if (GetWindowClass(chWnd).ToLower().Contains("button"))
							{
								bpush = chWnd;
							}
							return true;
						}, IntPtr.Zero);
						if (push && bpush != IntPtr.Zero)
						{
							var intPtr = bpush;
                            if (!makepatch2)
							{
								Console.WriteLine(DateTime.Now.ToString() + " [Patching xEvil (2)]");
							}
							makepatch2 = true;
							Utilities.ShowWindowAsync(hWnd, 0);
							Utilities.EnableWindow(intPtr, true);
							Utilities.SendMessage(intPtr, 245u, 0, 0);
						}
					}
					return true;
				}, IntPtr.Zero);
				if (foundxevil)
				{
					if (!once)
					{
						Console.WriteLine(DateTime.Now.ToString() + " XEvil is found! Monitoring...");
						Console.WriteLine("Path: " + fullpath + "; PId=" + fullpid);
						once = true;
					}
					if (makepatch1 || makepatch2)
					{
						if (makepatch1)
						{
							iter++;
						}
						Thread.Sleep(5000);
						flag = false;
					}
					else
					{
						if (!flag)
						{
							ConsoleHeader();
							Console.WriteLine(DateTime.Now.ToString() + " Patched! Monitoring...");
							flag = true;
							Console.WriteLine("Path: " + fullpath);
						}
						iter = 1;
						Thread.Sleep(1000);
					}
					if (iter > 100)
					{
						break;
					}
				}
				else
				{
					Console.WriteLine(DateTime.Now.ToString() + " XEvil was not found.");
					once = false;
					if (!foundxevil && fullpath != "" && num > 600)
                    {
						Console.WriteLine("Starting the process!");
						var processStartInfo = new ProcessStartInfo(fullpath)
                        {
                            WindowStyle = ProcessWindowStyle.Minimized
                        };
                        Process.Start(processStartInfo);
                        Thread.Sleep(3000);
                    }
                    else
					{
						Thread.Sleep(30000);
					}
				}
				foundxevil = false;
			}
			Console.WriteLine(DateTime.Now.ToString() + " Too many operations... exit.");
			Console.ReadLine();
		}

		public static string GetFullName(IntPtr hwnd, out uint pid)
		{
            Utilities.GetWindowThreadProcessId(hwnd, out pid);
			return Process.GetProcessById((int)pid).MainModule.FileName;
		}

		public static void KillTask(uint pid)
		{
			Process.GetProcessById((int)pid).Kill();
		}

		private static string GetWindowText(IntPtr hWnd)
		{
			var num = Utilities.GetWindowTextLength(hWnd) + 1;
            var stringBuilder = new StringBuilder(num);
            num = Utilities.GetWindowText(hWnd, stringBuilder, num);
			return stringBuilder.ToString(0, num);
		}

		private static string GetWindowClass(IntPtr hWnd)
		{
			var num = 260;
			var stringBuilder = new StringBuilder(num);
            num = Utilities.GetClassName(hWnd, stringBuilder, num);
			return stringBuilder.ToString(0, num);
		}
	}
}
