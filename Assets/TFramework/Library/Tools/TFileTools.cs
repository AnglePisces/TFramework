using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Tools
{
	public class HNFileTools : MonoBehaviour
	{
		private static HNFileTools _instance;
		public static HNFileTools instance
		{
			get
			{
				bool flag = HNFileTools._instance == null;
				if (flag)
				{
					HNFileTools._instance = new GameObject("HNFileTools").AddComponent<HNFileTools>();
				}
				return HNFileTools._instance;
			}
		}
		public static byte[] ReadFile(string path)
		{
			bool flag = !File.Exists(path);
			byte[] result;
			if (flag)
			{
				result = null;
			}
			else
			{
				using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					byte[] array = new byte[(int)fileStream.Length];
					fileStream.Read(array, 0, array.Length);
					fileStream.Close();
					result = array;
				}
			}
			return result;
		}
		public static void WriteFile(string path, byte[] data)
		{
			bool flag = File.Exists(path);
			if (flag)
			{
				File.Delete(path);
			}
			using (FileStream fileStream = new FileStream(path, FileMode.Create))
			{
				fileStream.Write(data, 0, data.Length);
				fileStream.Close();
			}
		}
		public static bool IsFileInUse(string fileName)
		{
			bool result = true;
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
				result = false;
			}
			catch
			{
			}
			finally
			{
				bool flag = fileStream != null;
				if (flag)
				{
					fileStream.Close();
				}
			}
			return result;
		}
		public static bool DelFile(string path)
		{
			bool flag = !File.Exists(path);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				File.Delete(path);
				result = true;
			}
			return result;
		}
		public void SaveLocalData(string path, byte[] data)
		{
			string path2 = path.Substring(0, path.LastIndexOf("/") + 1);
			bool flag = !Directory.Exists(path2);
			if (flag)
			{
				Directory.CreateDirectory(path2);
			}
			HNFileTools.WriteFile(path, data);
		}
	}
}
