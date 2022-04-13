using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("Data")]
public class DataManager : MonoBehaviour
{
	public static DataManager instance;
	public string path;

	private XmlSerializer serializer = new XmlSerializer(typeof(Data));
	Encoding encoding = Encoding.UTF8;
	private void Awake()
	{
		instance = this;
		path = Path.Combine(Application.persistentDataPath, "Data.xml");
	}

	public void Save(List<NeuralNetwork> net)
	{
		StreamWriter streamWriter = new StreamWriter(path, false, encoding);
		Data data = new Data{nets = net};
        
		serializer.Serialize(streamWriter, data);
	}

	public Data Load()
	{
		if (File.Exists(path))
		{
			FileStream fileStream = new FileStream(path, FileMode.Open);
			return serializer.Deserialize(fileStream) as Data;
		}

		return null;
	}
}