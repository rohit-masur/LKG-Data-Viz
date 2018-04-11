using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class Graph : MonoBehaviour {

	public Transform pointPrefab;
	public Transform pointOneSpkiePrefab;
	public Transform pointTwoSpkiePrefab;
	public Transform pointThreeSpkiePrefab;
    public int dataCount;
	public Text dataStartText;  
	public Text dataEndText; 
	public Text dataSpikesText;
	public Text dataTimeInSeconds;
	public Text dataTimeInSecondsLow;
	public Text dataTimeInSecondsMedium;
	public Text dataTimeInSecondsHigh;
	public Text dataLow;
	public Text dataHigh;
	public Text dataMedium;
	public int maxRange;


	private int noOfSpikes=0;
	private int lowSpikes = 0;
	private int mediumSpikes = 0;
	private int highSpikes = 0;


	private float xval;
	private float yval;
	private float nspk;
	private float ntime = 0.0f;
	private float ntime2;
	private float ntime3;
	private float timeSec;
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
	private ArrayList timeOfSpike = new ArrayList();
	private ArrayList timeOfSpikeLow = new ArrayList();
	private ArrayList timeOfSpikeMedium = new ArrayList();
	private ArrayList timeOfSpikeHigh = new ArrayList();



	void showRange(int range, int spikes, int low, int high, int medium, string timesLow, string timesMedium, string timesHigh){
		
		int tillRange = range + maxRange -1;
		string startingPoint = range.ToString ();
		string end = tillRange.ToString ();
		dataStartText.text = "Datapoints: "+ startingPoint;
		dataEndText.text = " to " + end;
		dataSpikesText.text = "Total Spikes: "+ (low+medium+high).ToString();

		if (!(string.IsNullOrEmpty (timesLow))) {
			dataTimeInSecondsLow.text =  timesLow;
		} else {
			dataTimeInSecondsLow.text = string.Empty;
		}

		if (!(string.IsNullOrEmpty (timesMedium))) {
			dataTimeInSecondsMedium.text =  timesMedium;
		} else {
			dataTimeInSecondsMedium.text = string.Empty;
		}

		if (!(string.IsNullOrEmpty (timesHigh))) {
			dataTimeInSecondsHigh.text = timesHigh;
		} else {
			dataTimeInSecondsHigh.text = string.Empty;
		}
		dataLow.text = "L: "+low.ToString ();
		dataMedium.text = "M: "+medium.ToString ();
		dataHigh.text = "H: "+high.ToString ();


	}
		
    public  void Slider_Changed(float newCount)
      {
    //    Debug.Log("SliderData is " + newCount);

        int count = (int)newCount;
        //int count = newCount as int;
        DestroyGraph();
        VisGraph(count);
      }

	void Awake () {
        data = CSVReader.Read("brain_data");
        //	StartCoroutine (VisGraph());
        VisGraph(1);
	
	}

    public void DestroyGraph() {
        //Destroy(GameObject.FindWithTag("clone"));
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("clone");
        foreach (GameObject enemy in enemies)
            GameObject.Destroy(enemy);
    }

	public void DestroyTimeList(ArrayList arr){
		for (int i = 0; i < arr.Count; i++) {
			arr.RemoveAt (i);
		}
	}

	public string CreateStringOfTime(ArrayList arr){
	

		StringBuilder sb = new StringBuilder();
		char[] MyChar = { ',', ' ' };

		for (int i = 0; i < arr.Count; i++) {
		
			sb.Append (arr [i].ToString());
			sb.Append ("s, ");

		}
		string newString = sb.ToString().TrimEnd(MyChar);
		return newString;
	}
		

	public void  VisGraph(int sliderData){

   //     Debug.Log("In VisGraph_SliderData is "+ sliderData);

		for (int i = sliderData; i < sliderData +maxRange; i++) {

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
				
				noOfSpikes++;
				lowSpikes++;
				timeSec = ntime / 10000;
				//timeOfSpike.Add (timeSec);
				timeOfSpikeLow.Add (timeSec);
				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Instantiate (pointOneSpkiePrefab, vect3intensity, Quaternion.identity);
			}
			if (nspk == 2) {

				noOfSpikes+=2;
				mediumSpikes++;
		//		timeOfSpike.Add (ntime / 10000);
		//		timeOfSpike.Add (ntime2 / 10000);
				timeOfSpikeMedium.Add(((ntime+ntime2)/2)/10000);
		//		timeOfSpikeMedium.Add (ntime2 / 10000);
				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Instantiate (pointTwoSpkiePrefab, vect3intensity, Quaternion.identity);
			}

			if (nspk == 3) {

				noOfSpikes+=3;
				highSpikes++;
				timeOfSpikeHigh.Add (((ntime+ntime2+ntime3)/3)/10000);
		//		timeOfSpikeHigh.Add (ntime2 / 10000);
		//		timeOfSpikeHigh.Add (ntime3 / 10000);
				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Instantiate (pointThreeSpkiePrefab, vect3intensity, Quaternion.identity);
			}

		}

		string timesLow = CreateStringOfTime (timeOfSpikeLow);
		string timesMedium = CreateStringOfTime (timeOfSpikeMedium);
		string timesHigh = CreateStringOfTime (timeOfSpikeHigh);

		showRange(sliderData, noOfSpikes,lowSpikes,highSpikes,mediumSpikes,timesLow,timesMedium,timesHigh);
		DestroyTimeList (timeOfSpikeLow);
		DestroyTimeList (timeOfSpikeMedium);
		DestroyTimeList (timeOfSpikeHigh);
		noOfSpikes = 0;
		lowSpikes = 0;
		mediumSpikes = 0;
		highSpikes = 0;
	}
	

}
