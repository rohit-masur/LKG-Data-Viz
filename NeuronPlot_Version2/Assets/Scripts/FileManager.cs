using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class FileManager : MonoBehaviour {

	string path;
	private List<List<Dictionary<string, object>>> data;
	private List<string> stringList = new List<string>();

	void Awake() {
		data = new List<List<Dictionary<string, object>>>();
//		if (control == null) {
//			DontDestroyOnLoad (gameObject);
//			control = this;
//
//		} else if (control != this) {
//			Destroy (gameObject);
//		}
	}

	public void  OpenExplorer() {
		path = EditorUtility.OpenFilePanel ("Read CSV file", "", "csv");
		Debug.Log (path);

//		data.Add(CSVReader.Read(path));
		ReadFile(path);
		Debug.Log("finished loading the file");
	}

	void ReadFile(string filePath) {
		StreamReader sReader = new StreamReader (filePath);
		while (!sReader.EndOfStream) {
			string line = sReader.ReadLine ();
			stringList.Add (line);
		}
		Debug.Log(stringList.Count);
	}
}
