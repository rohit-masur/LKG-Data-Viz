using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour {

	public Transform pointPrefab;
	public Transform pointOneSpkiePrefab;
	public Transform pointTwoSpkiePrefab;
	public Transform pointThreeSpkiePrefab;
    public int dataCount;

	private float xval;
	private float yval;
	private float nspk;
	private float ntime;
	private float ntime2;
	private float ntime3;
	private Vector3 vect3xy;
	private Vector3 temp;
	private Vector3 vect3intensity;
	private float intensitysub;
	private float condensevalue = 0.4f;
	private float nspikeCondenseValue = 10.0f;
	private float nspikeVal;
	private float offsetvalue = 50;
	private float updivvalue = 10000;
    private List<Dictionary<string, object>> data;


    public  void Slider_Changed(float newCount)
      {
        Debug.Log("SliderData is " + newCount);

        int count = (int)newCount;
        //int count = newCount as int;
        DestroyGraph();
        VisGraph(count);
      }

	void Awake () {
        data = CSVReader.Read("brain_data");
        //	StartCoroutine (VisGraph());
        VisGraph(10);
	
	}

    public void DestroyGraph() {
        //Destroy(GameObject.FindWithTag("clone"));
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("clone");
        foreach (GameObject enemy in enemies)
            GameObject.Destroy(enemy);
    }

	public void  VisGraph(int sliderData){
        
		
		Debug.Log (data.Count);
        Debug.Log("SliderData is "+ sliderData);

        for (int i = 0; i < sliderData; i++) {
		//	Debug.Log ("before");
		
			Debug.Log ("after");

			xval = float.Parse(data[i]["xval"].ToString());
			yval = float.Parse(data[i]["yval"].ToString());
			nspk = float.Parse(data[i]["nspk"].ToString());

			if (nspk == 1) {
				ntime = float.Parse (data [i] ["ntime"].ToString ());
			}
			if (nspk == 2) {
				ntime = float.Parse (data [i] ["ntime"].ToString ());
				ntime2 = float.Parse (data [i] ["ntime2"].ToString ());
			}
			if (nspk == 3) {
				ntime = float.Parse (data [i] ["ntime"].ToString ());
				ntime2 = float.Parse (data [i] ["ntime2"].ToString ());
				ntime3 = float.Parse (data [i] ["ntime3"].ToString ());
			}


			xval = xval * condensevalue;
			yval = yval * condensevalue;

		//	yield return new WaitForSeconds (0.01f);
			Vector3 vect3xy = new Vector3(xval, transform.position.y, yval);

			if (xval > 0 && yval > 0)
			{

				Instantiate(pointPrefab, vect3xy, Quaternion.identity);
			}

			if (nspk == 1) {

				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Instantiate (pointOneSpkiePrefab, vect3intensity, Quaternion.identity);
			}
			if (nspk == 2) {

				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Instantiate (pointTwoSpkiePrefab, vect3intensity, Quaternion.identity);
			}

			if (nspk == 3) {

				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Instantiate (pointThreeSpkiePrefab, vect3intensity, Quaternion.identity);
			}

		}
	
	}
	

}
