using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using UnityEngine;
namespace Tools
{
	public class THttpDown
	{
		private static List<THttpDown> LoadingDown = new List<THttpDown>();
		public string url;
		public bool HaveError = false;
		public long maxLength = 0L;
		public byte[] data = null;
		public float progress = 0f;
		private string contentType = "application/x-www-form-urlencoded";
		private int requestTimeOut = 10000;
		private int readTimeOut = 1000;
		private CallBackArg<THttpDown> callback;
		private Thread RequestThread;
		private bool isGet = true;
		private string tag = "";
		private Dictionary<string, string> postParameters = null;
		public string Tag
		{
			get
			{
				return this.tag;
			}
			set
			{
				this.tag = value;
			}
		}
		public static THttpDown DoGet(string url, CallBackArg<THttpDown> callback)
		{
			bool flag = THttpDown.IsDown(url, "");
			THttpDown result;
			if (flag)
			{
				Debug.Log(url + "正在下载中，请不 要重复下载！");
				result = null;
			}
			else
			{
				THttpDown hNHttpDwon = new THttpDown(url, callback);
				THttpDown.AddDown(hNHttpDwon);
				result = hNHttpDwon;
			}
			return result;
		}
		public static THttpDown DoGet(string url, CallBackArg<THttpDown> callback, string contentType)
		{
			THttpDown hNHttpDwon = THttpDown.DoGet(url, callback);
			hNHttpDwon.SetContentType(contentType);
			return hNHttpDwon;
		}
		public static THttpDown DoPost(string url, CallBackArg<THttpDown> callback)
		{
			bool flag = THttpDown.IsDown(url, "");
			THttpDown result;
			if (flag)
			{
				Debug.Log(url + "正在下载中，请不 要重复下载！");
				result = null;
			}
			else
			{
				THttpDown hNHttpDwon = new THttpDown(url, callback);
				hNHttpDwon.isGet = false;
				THttpDown.AddDown(hNHttpDwon);
				result = hNHttpDwon;
			}
			return result;
		}
		public static THttpDown DoPost(string url, CallBackArg<THttpDown> callback, string contentType)
		{
			THttpDown hNHttpDwon = THttpDown.DoGet(url, callback);
			hNHttpDwon.SetContentType(contentType);
			hNHttpDwon.isGet = false;
			hNHttpDwon.postParameters = new Dictionary<string, string>();
			return hNHttpDwon;
		}
		public static THttpDown GetDown(string urlKey, string tag = "")
		{
			THttpDown result;
			int num;
			for (int i = 0; i < THttpDown.LoadingDown.Count; i = num + 1)
			{
				THttpDown hNHttpDwon = THttpDown.LoadingDown[i];
				bool flag = hNHttpDwon == null;
				if (!flag)
				{
					bool flag2 = hNHttpDwon.url == urlKey && hNHttpDwon.Tag == tag;
					if (flag2)
					{
						result = hNHttpDwon;
						return result;
					}
				}
				num = i;
			}
			result = null;
			return result;
		}
		public static bool IsDown(string urlKey, string tag = "")
		{
			bool result;
			int num;
			for (int i = 0; i < THttpDown.LoadingDown.Count; i = num + 1)
			{
				THttpDown hNHttpDwon = THttpDown.LoadingDown[i];
				bool flag = hNHttpDwon == null;
				if (!flag)
				{
					bool flag2 = hNHttpDwon.url == urlKey && hNHttpDwon.Tag == tag;
					if (flag2)
					{
						result = true;
						return result;
					}
				}
				num = i;
			}
			result = false;
			return result;
		}
		public static void AddDown(THttpDown down)
		{
			string text = down.url;
			string text2 = down.Tag;
			bool flag = THttpDown.IsDown(text, text2);
			if (flag)
			{
				Debug.Log(text + "正在下载中，请不 要重复下载！");
			}
			else
			{
				THttpDown.LoadingDown.Add(down);
			}
		}
		public THttpDown(string url, CallBackArg<THttpDown> callback)
		{
			this.url = url;
			this.callback = callback;
		}
		public void StartDown()
		{
			this.RequestThread = new Thread(new ThreadStart(this.Run));
			this.RequestThread.IsBackground = true;
			this.RequestThread.Priority = System.Threading.ThreadPriority.Lowest;
			this.RequestThread.Start();
		}
		public void SetContentType(string contentType)
		{
			this.contentType = contentType;
		}
		public void SetRequestTimeOut(int timeOut)
		{
			this.requestTimeOut = timeOut;
		}
		public void SetReadTimeOut(int timeOut)
		{
			this.readTimeOut = timeOut;
		}
		public void AddPostParam(string key, string value)
		{
			bool flag = this.postParameters == null;
			if (flag)
			{
				this.postParameters = new Dictionary<string, string>();
			}
			this.postParameters.Add(key, value);
		}
		private void OnComplete()
		{
			bool flag = this.callback != null;
			if (flag)
			{
				this.callback(this);
			}
			THttpDown.LoadingDown.Remove(this);
			TLogger.Log(this.url + "：下载完毕", null, null);
		}
		public string GetResponseText()
		{
			return Encoding.UTF8.GetString(this.data);
		}
		private void Run()
		{
			int num = 0;
			string content = string.Format("url：{0}, contentType:{1}", this.url, this.contentType);
            TLogger.Log(content, "HttpDwon", "Run");
			bool flag = !this.isGet;
			if (flag)
			{
				content = string.Format("post参数:" + this.GetRequestString(), new object[0]);
                TLogger.Log(content, "HttpDwon", "Run");
			}
			try
			{
				while (!this.DownComplete())
				{
					int num2 = num;
					num = num2 + 1;
					bool flag2 = num >= 3;
					if (flag2)
					{
						break;
					}
					this.progress = 0f;
					this.HaveError = false;
					content = string.Format("第{0}次重新下载", num);
                    TLogger.Log(content, "HttpDwon", "Run");
				}
				this.OnComplete();
				this.DestroySelf();
			}
			catch (Exception ex)
			{
                TLogger.Log(ex.Message + "/n/n" + ex.StackTrace, null, null);
			}
			finally
			{
				GC.Collect();
			}
		}
		private bool DownComplete()
		{
			Stream stream = null;
			HttpWebResponse httpWebResponse = null;
			HttpWebRequest httpWebRequest = null;
			TArrayBuffer<byte> tArrayBuffer = null;
			long num = 0L;
			bool result;
			try
			{
				httpWebRequest = (HttpWebRequest)WebRequest.Create(this.url);
				httpWebRequest.Timeout = this.requestTimeOut;
				httpWebRequest.ContentType = this.contentType;
				HttpRequestCachePolicy cachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
				httpWebRequest.CachePolicy = cachePolicy;
				bool flag = this.isGet;
				if (flag)
				{
					httpWebRequest.Method = "GET";
				}
				else
				{
					httpWebRequest.Method = "POST";
					byte[] requestBytes = this.GetRequestBytes();
					httpWebRequest.ContentType = "application/x-www-form-urlencoded";
					httpWebRequest.ContentLength = (long)requestBytes.Length;
					using (Stream requestStream = httpWebRequest.GetRequestStream())
					{
						requestStream.Write(requestBytes, 0, requestBytes.Length);
						requestStream.Close();
					}
				}
				httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				stream = httpWebResponse.GetResponseStream();
				stream.ReadTimeout = this.readTimeOut;
                tArrayBuffer = new TArrayBuffer<byte>();
				byte[] array = new byte[1024];
				int num2;
				while ((num2 = stream.Read(array, 0, array.Length)) > 0)
				{
                    tArrayBuffer.Write(array, 0, num2);
					num += (long)num2;
					this.progress = (float)((double)num / (double)this.maxLength);
				}
				byte[] array2 = tArrayBuffer.Read(tArrayBuffer.Available);
				int num3 = array2.Length;
				this.data = new byte[num3];
				Array.Copy(array2, this.data, num3);
				TLogger.Log("读取字节流长度:" + num3, "HttpDwon", "Run");
				bool flag2 = this.progress < 1f;
				if (flag2)
				{
					this.HaveError = true;
                    TLogger.ErrorInfo("下载错误：文件下载流已经读完数据，但是下载的文件长度小于服务器，这可能是由于本地网络突然中断导致！！", null, null);
					result = false;
				}
				else
				{
					this.HaveError = false;
					result = true;
				}
			}
			catch (WebException ex)
			{
				this.HaveError = true;
                TLogger.ErrorInfo(ex.ToString(), null, null);
                TLogger.ErrorInfo("webEx.Status = " + ex.Status, null, null);
				bool flag3 = ex.Status == WebExceptionStatus.ProtocolError;
				if (flag3)
				{
                    TLogger.ErrorInfo("response.StatusCode = " + ((HttpWebResponse)ex.Response).StatusCode, null, null);
                    TLogger.ErrorInfo("response.StatusDescription = " + ((HttpWebResponse)ex.Response).StatusDescription, null, null);
				}
				bool flag4 = ex.Status == WebExceptionStatus.Timeout;
				if (flag4)
				{
					result = false;
				}
				else
				{
					result = true;
				}
			}
			catch (IOException ex2)
			{
				this.HaveError = true;
                TLogger.ErrorInfo(ex2.ToString(), null, null);
				result = true;
			}
			catch (Exception ex3)
			{
				this.HaveError = true;
                TLogger.ErrorInfo(ex3.ToString(), null, null);
				result = true;
			}
			finally
			{
				bool flag5 = tArrayBuffer != null;
				if (flag5)
				{
                    tArrayBuffer.DestroySelf();
                    tArrayBuffer = null;
				}
				bool flag6 = stream != null;
				if (flag6)
				{
					stream.Close();
					stream = null;
				}
				bool flag7 = httpWebResponse != null;
				if (flag7)
				{
					httpWebResponse.Close();
					httpWebResponse = null;
				}
			}
			return result;
		}
		private byte[] GetRequestBytes()
		{
			bool flag = this.postParameters == null || this.postParameters.Count == 0;
			byte[] result;
			if (flag)
			{
				result = new byte[0];
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string current in this.postParameters.Keys)
				{
					stringBuilder.Append(current + "=" + this.postParameters[current] + "&");
				}
				stringBuilder.Length--;
				result = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			}
			return result;
		}
		private string GetRequestString()
		{
			bool flag = this.postParameters == null || this.postParameters.Count == 0;
			string result;
			if (flag)
			{
				result = "";
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string current in this.postParameters.Keys)
				{
					stringBuilder.Append(current + "=" + this.postParameters[current] + "&");
				}
				stringBuilder.Length--;
				result = stringBuilder.ToString();
			}
			return result;
		}
		public override bool Equals(object obj)
		{
			THttpDown hNHttpDwon = obj as THttpDown;
			bool flag = hNHttpDwon == null;
			return !flag && hNHttpDwon.url.Equals(this.url) && hNHttpDwon.Tag.Equals(this.tag);
		}
		public override int GetHashCode()
		{
			return this.url.GetHashCode() + this.tag.GetHashCode();
		}
		public void DestroySelf()
		{
			this.url = null;
			this.contentType = null;
			this.tag = null;
			this.callback = null;
			this.data = null;
			bool flag = this.postParameters != null;
			if (flag)
			{
				this.postParameters.Clear();
			}
			this.postParameters = null;
		}
	}
}
