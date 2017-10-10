using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
namespace Tools
{
	public class TXmlHelper
	{
		public static T ReadXML<T>(string path)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			StreamReader textReader = new StreamReader(path);
			return (T)((object)xmlSerializer.Deserialize(textReader));
		}
		public static string WriteXML<T>(T item, string xmlPath)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(item.GetType());
			try
			{
				FileStream fileStream = File.Create(xmlPath);
				fileStream.Close();
				TextWriter textWriter = new StreamWriter(xmlPath, false, Encoding.UTF8);
				XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
				xmlSerializerNamespaces.Add(string.Empty, string.Empty);
				xmlSerializer.Serialize(textWriter, item, xmlSerializerNamespaces);
				textWriter.Flush();
				textWriter.Close();
			}
			catch (Exception e)
			{
				TLogger.LogException(e);
			}
			return TXmlHelper.SerializeToXmlStr<T>(item, true);
		}
		public static string SerializeToXmlStr<T>(T obj, bool omitXmlDeclaration = true)
		{
			return TXmlHelper.XmlSerialize<T>(obj, omitXmlDeclaration);
		}
		public static string XmlSerialize<T>(T obj, bool omitXmlDeclaration = true)
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.OmitXmlDeclaration = omitXmlDeclaration;
			xmlWriterSettings.Encoding = new UTF8Encoding(false);
			MemoryStream memoryStream = new MemoryStream();
			XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
			XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
			xmlSerializerNamespaces.Add(string.Empty, string.Empty);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
			return Encoding.UTF8.GetString(memoryStream.ToArray());
		}
		public static void XmlSerialize<T>(string path, T obj, bool omitXmlDeclaration, bool removeDefaultNamespace)
		{
			using (XmlWriter xmlWriter = XmlWriter.Create(path, new XmlWriterSettings
			{
				OmitXmlDeclaration = omitXmlDeclaration
			}))
			{
				XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
				if (removeDefaultNamespace)
				{
					xmlSerializerNamespaces.Add(string.Empty, string.Empty);
				}
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
				xmlSerializer.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
			}
		}
		public static T XmlDeserialize<T>(string xmlOfObject) where T : class
		{
			XmlReader xmlReader = XmlReader.Create(new StringReader(xmlOfObject), new XmlReaderSettings());
			return (T)((object)new XmlSerializer(typeof(T)).Deserialize(xmlReader));
		}
	}
}
