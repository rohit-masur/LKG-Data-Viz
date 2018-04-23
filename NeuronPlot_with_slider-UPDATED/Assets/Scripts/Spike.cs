using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;


public class Spike : MonoBehaviour { 

	private static Color color;
	private static Material material;

	void Awake() {
		material = GetComponent<Renderer>().material;
	//	Debug.Log (material);
	}


	public static void setColor(float alpha) {
		if (material != null) {
			Color newcolor = material.GetColor ("_Color");
			newcolor.a = alpha;
			material.SetColor ("_Color", newcolor);
		}
	}

}

