using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;


public class Graph : MonoBehaviour {

	public Transform pointPrefab;
	public Transform staticPointPrefab;
	public Transform pointOneSpkiePrefab;
	public Transform pointTwoSpkiePrefab;
	public Transform pointThreeSpkiePrefab;
	public Transform pointZeroSpkiePrefab;
	public Transform spikePoint;
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
	public float maxTimeInSeconds;
	public Slider mySlider;


	private int noOfSpikes=0;
	private int lowSpikes = 0;
	private int mediumSpikes = 0;
	private int highSpikes = 0;

	private float nstime;
	private float mstime;
	private float xval;
	private float yval;
	private float[] nspk = new float[21];
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
    private List<List<Dictionary<string, object>>> data;
	private ArrayList timeOfSpikeLow = new ArrayList();
	private ArrayList timeOfSpikeMedium = new ArrayList();
	private ArrayList timeOfSpikeHigh = new ArrayList();
	
	public Transform spikePrefab;


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
		dataLow.text = "L: "+ low.ToString ();
		dataMedium.text = "M: "+ medium.ToString ();
		dataHigh.text = "H: "+ high.ToString ();


	}


	public void Slider_Value(){
	
	}

		
    public  void Slider_Changed(float newCount)
      {
   
        int count = (int)newCount;
		Debug.Log("normal");
        DestroyGraph();
		DestroySpikes ();
        VisGraph(count);
      }

	public void TimeSlider_Changed(float newTime){

		Debug.Log (newTime);
		Debug.Log (nstime);
		Debug.Log (mstime);

		if(newTime < nstime || newTime <  mstime){
		//	DestroyGraph ();
		//	DestroySpikes ();
		//	VisGraph (newTime, newTime + maxTimeInSeconds);

		}

	}

	void Awake () {
		data = new List<List<Dictionary<string, object>>>();
//		data.Add(CSVReader.Read("neuron_1"));
//		data.Add(CSVReader.Read("rat2"));
//		data.Add(CSVReader.Read("rat3"));
//		data.Add(CSVReader.Read("rat4"));
		for (int i = 1; i <= 21; i++) {
			Debug.Log ("neurons/rat" + i.ToString ());
			data.Add(CSVReader.Read("neurons/rat" + i.ToString()));
		}
//		data = CSVReader.Read("neuron_2");
//		data = CSVReader.Read("neuron_3");
        //	StartCoroutine (VisGraph());
//		for(int i = 0 ; i < 10 ; i++) {
//			Debug.Log(data[0][i] ["xval"].ToString ());
//		}
		ShowStaticGraph ();
        VisGraph(1);
	
	}

    public void DestroyGraph() {
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

	public void ShowStaticGraph () {
		for (int i = 0; i < 4000; i++) {
			xval = float.Parse (data[0] [i] ["xval"].ToString ());
			yval = float.Parse (data[0] [i] ["yval"].ToString ());

			xval = xval * condensevalue;
			yval = yval * condensevalue;
		
			Datapoint.setDataPointColor (0.3f);
			Vector3 vect3xy = new Vector3 (xval, 0, yval);
			Instantiate (staticPointPrefab, vect3xy, Quaternion.identity);
		}
	}
		
		

	public void  VisGraph(int sliderData){
		for (int i = sliderData; i < sliderData +maxRange; i++) {

			xval = float.Parse(data[0][i]["xval"].ToString());
			yval = float.Parse(data[0][i]["yval"].ToString());
			for (int m = 0; m < data.Count; m++) {
				nspk[m] = (float.Parse (data [m] [i] ["nspk"].ToString ()));
			}

//			if (nspk == 1) {
//				ntime = float.Parse (data [i] ["ntime"].ToString ());
//
//			}
//			if (nspk == 2) {
//				ntime = float.Parse (data [i] ["ntime"].ToString ());
//				ntime2 = float.Parse (data [i] ["ntime2"].ToString ());
//
//			}
//			if (nspk == 3) {
//				ntime = float.Parse (data [i] ["ntime"].ToString ());
//				ntime2 = float.Parse (data [i] ["ntime2"].ToString ());
//				ntime3 = float.Parse (data [i] ["ntime3"].ToString ());
//
//			}
				
			xval = xval * condensevalue;
			yval = yval * condensevalue;

//			Vector3 vect3xy = new Vector3 (xval, 0, yval);
//			Datapoint.setDataPointColor ();
//			Instantiate (staticPointPrefab, vect3xy, Quaternion.identity);

			CreateSpike (xval, yval, nspk);

//			if (nspk == 1) {
//				
//				noOfSpikes++;
//				lowSpikes++;
//				timeSec = ntime / 10000;
//				timeOfSpikeLow.Add (timeSec);
//				nspikeVal = nspk * nspikeCondenseValue;
//				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
//				Spike.setColor (0.1f);
//				Instantiate (pointOneSpkiePrefab, vect3intensity, Quaternion.identity);
//				CreateSpike (xval, yval, nspikeVal);
//			}
//			if (nspk == 2) {
//
//				noOfSpikes+=2;
//				mediumSpikes++;
//				timeOfSpikeMedium.Add(((ntime+ntime2)/2)/10000);
//				nspikeVal = nspk * nspikeCondenseValue;
//				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
//				Spike.setColor (0.1f);
//				Instantiate (pointTwoSpkiePrefab, vect3intensity, Quaternion.identity);
//				CreateSpike (xval, yval, nspikeVal);
//			}
//
//			if (nspk == 3) {
//
//				noOfSpikes+=3;
//				highSpikes++;
//				timeOfSpikeHigh.Add (((ntime+ntime2+ntime3)/3)/10000);
//				nspikeVal = nspk * nspikeCondenseValue;
//				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
//				Spike.setColor (0.1f);
//				Instantiate (pointThreeSpkiePrefab, vect3intensity, Quaternion.identity);
//				CreateSpike (xval, yval, nspikeVal);
//			}

		}

//		string timesLow = CreateStringOfTime (timeOfSpikeLow);
//		string timesMedium = CreateStringOfTime (timeOfSpikeMedium);
//		string timesHigh = CreateStringOfTime (timeOfSpikeHigh);
//
//		showRange(sliderData, noOfSpikes,lowSpikes,highSpikes,mediumSpikes,timesLow,timesMedium,timesHigh);
//
//		DestroyTimeList (timeOfSpikeLow);
//		DestroyTimeList (timeOfSpikeMedium);
//		DestroyTimeList (timeOfSpikeHigh);
		noOfSpikes = 0;
		lowSpikes = 0;
		mediumSpikes = 0;
		highSpikes = 0;
	}

//	public void  VisGraph(float minTime, float maxtime){
//
//		int startTime = (int)minTime;
//		int endTime = (int)maxtime;
//
//		for (int i = startTime; i < endTime; i++) {
//
//			xval = float.Parse(data[0][i]["xval"].ToString());
//			yval = float.Parse(data[0][i]["yval"].ToString());
//			nspk = float.Parse(data[i]["nspk"].ToString());
//
//			if (nspk == 1) {
//				ntime = float.Parse (data [i] ["ntime"].ToString ());
//
//			}
//			if (nspk == 2) {
//				ntime = float.Parse (data [i] ["ntime"].ToString ());
//				ntime2 = float.Parse (data [i] ["ntime2"].ToString ());
//
//			}
//			if (nspk == 3) {
//				ntime = float.Parse (data [i] ["ntime"].ToString ());
//				ntime2 = float.Parse (data [i] ["ntime2"].ToString ());
//				ntime3 = float.Parse (data [i] ["ntime3"].ToString ());
//
//			}
//
//
//			xval = xval * condensevalue;
//			yval = yval * condensevalue;
//
//			Vector3 vect3xy = new Vector3(xval, 0, yval);
//
//
//			Instantiate(pointPrefab, vect3xy, Quaternion.identity);
//
//			if (nspk == 1) {
//
//				noOfSpikes++;
//				lowSpikes++;
//				timeSec = ntime / 10000;
//				timeOfSpikeLow.Add (timeSec);
//				nspikeVal = nspk * nspikeCondenseValue;
//				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
//				Instantiate (pointOneSpkiePrefab, vect3intensity, Quaternion.identity);
//				CreateSpike (xval, yval, nspikeVal);
//			}
//			if (nspk == 2) {
//
//				noOfSpikes+=2;
//				mediumSpikes++;
//				timeOfSpikeMedium.Add(((ntime+ntime2)/2)/10000);
//				nspikeVal = nspk * nspikeCondenseValue;
//				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
//				Instantiate (pointTwoSpkiePrefab, vect3intensity, Quaternion.identity);
//				CreateSpike (xval, yval, nspikeVal);
//			}
//
//			if (nspk == 3) {
//
//				noOfSpikes+=3;
//				highSpikes++;
//				timeOfSpikeHigh.Add (((ntime+ntime2+ntime3)/3)/10000);
//				nspikeVal = nspk * nspikeCondenseValue;
//				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
//				Instantiate (pointThreeSpkiePrefab, vect3intensity, Quaternion.identity);
//				CreateSpike (xval, yval, nspikeVal);
//			}
//
//		}
//
//		string timesLow = CreateStringOfTime (timeOfSpikeLow);
//		string timesMedium = CreateStringOfTime (timeOfSpikeMedium);
////		string timesHigh = CreateStringOfTime (timeOfSpikeHigh);
//
//	//	showRange(startTime	, noOfSpikes,lowSpikes,highSpikes,mediumSpikes,timesLow,timesMedium,timesHigh);
//
//		/*
//		GameObject temp = GameObject.Find("Slider Time");
//		if (temp != null) {
//			// Get the Slider Component
//			mySlider = temp.GetComponent<Slider> ();
//
//			if (mySlider != null) {
//				mySlider.minValue = sliderData * 0.03f;
//				mySlider.maxValue = (sliderData+maxRange) * 0.03f;
//			}
//		}
//		*/
//
//		DestroyTimeList (timeOfSpikeLow);
//		DestroyTimeList (timeOfSpikeMedium);
//		DestroyTimeList (timeOfSpikeHigh);
//		noOfSpikes = 0;
//		lowSpikes = 0;
//		mediumSpikes = 0;
//		highSpikes = 0;
//	}


	private void CreateSpike (float x, float z, float[] points) 
	{	
//		int tempY = (int) y;
		Vector3 position;
		position.z = 0f;
		Transform point;
//		for (int i = 1; i <= tempY; i++) {
//			point = Instantiate (spikePrefab);
//			position.x = x;
//			position.z = z;
//			position.y = i;
//			point.localPosition = position;
//		}

		for (int i = 1; i <= points.Length; i++) {
			if (points[i - 1] == 1) {
				point = Instantiate (pointOneSpkiePrefab);
			} else if (points [i - 1] == 2) {
				point = Instantiate (pointTwoSpkiePrefab);
			} else if (points [i - 1] == 3) {
				point = Instantiate (pointThreeSpkiePrefab);
			} else {
				point = Instantiate (pointZeroSpkiePrefab);
			}

			position.x = x;
			position.z = z;
			position.y = (i*4.3f - 2);
			point.localPosition = position;
		}
	}

	private void DestroySpikes() {
		GameObject[] spikes = GameObject.FindGameObjectsWithTag("spike");
		foreach (GameObject spike in spikes)
			GameObject.Destroy(spike);
	}
	

}
