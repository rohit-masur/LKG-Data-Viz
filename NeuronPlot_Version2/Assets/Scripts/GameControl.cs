using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameControl : MonoBehaviour {

	public static GameControl control;

	string path;
	private List<List<Dictionary<string, object>>> data;
	public float health;
	public float experience;

	void Awake() {
		if (control == null) {
			DontDestroyOnLoad (gameObject);
			control = this;
			data = new List<List<Dictionary<string, object>>>();
		} else if (control != this) {
			Destroy (gameObject);
		}
	}

	void OnGUI() {
		GUI.Label (new Rect (10, 10, 100, 30), "Health: " + health);
		GUI.Label (new Rect (10, 10, 100, 30), "Experience: " + experience);
	}

	public void  OpenExplorer() {
		path = EditorUtility.OpenFilePanel ("Read CSV file", "", "csv");
		Debug.Log (path);


		data.Add(CSVReader.Read(path));
		Debug.Log("finished loading the file");
	}
}
