using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ToDataVizScreen() {
		Application.LoadLevel ("data_viz");
	}

	public void ToFileSelectionScreen() {
		Application.LoadLevel ("file_selection");
	}
		
}
