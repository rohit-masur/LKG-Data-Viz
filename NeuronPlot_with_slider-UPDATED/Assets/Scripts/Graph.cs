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
	public Transform spikePoint;
    public int dataCount;
	public Text dataStartText;  
	public Text dataEndText; 
	public Text dataSpikesText;
	public Text dataTimeInSeconds;
	public Text dataTimeInSecondsLow;
	public Text dataTimeInSecondsMedium;
	public Text dataTimeInSecondsHigh;
	public Text dataSelectedTime;
	public Text dataLow;
	public Text dataHigh;
	public Text dataMedium;
	public int maxRange;
	public float maxTimeInSeconds;
	public Text dataStartTimeInSeconds;
	public Text dataEndTimeInSeconds;



	private int noOfSpikes=0;
	private int lowSpikes = 0;
	private int mediumSpikes = 0;
	private int highSpikes = 0;
	private int startTimeSec =0;
	private int endTimeSec =0;

	private Slider timeFrameSlider;
	private float nstime;
	private float mstime;
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
    private List<Dictionary<string, object>> data;
	private ArrayList timeOfSpikeLow = new ArrayList();
	private ArrayList timeOfSpikeMedium = new ArrayList();
	private ArrayList timeOfSpikeHigh = new ArrayList();
	
	public Transform spikePrefab;


	void showRange(int range, int tillRange, int spikes, int low, int high, int medium, string timesLow, string timesMedium, string timesHigh){
		
	//	int tillRange = range + maxRange -1;
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
		
    public  void Slider_Changed(float newCount)
      {
   
        int count = (int)newCount;
        DestroyGraph();
		DestroySpikes ();
		ShowStaticGraph ();
        VisGraph(count);
		TimeSlider.setValue ();
      }

	public void TimeSlider_Changed(float newTime){

		TimeSlider.setMinMax (startTimeSec, endTimeSec);
		int val = TimeSlider.getValue ();
		dataSelectedTime.text = val.ToString ();

		int maxPoint = Mathf.FloorToInt((((val * 100) / 3)));
		int minPoint = Mathf.FloorToInt((((startTimeSec * 100) / 3)));


		DestroyGraph ();
		DestroySpikes ();
		DestroyStaticPoints ();

		VisGraph (minPoint, maxPoint);


	}
		

	void Awake () {
        data = CSVReader.Read("brain_data");

		ShowStaticGraph ();
        VisGraph(1);

		int end = (int)(maxRange * 0.03f);
		TimeSlider.setMinMax (0, end+1);
	
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
			xval = float.Parse (data [i] ["xval"].ToString ());
			yval = float.Parse (data [i] ["yval"].ToString ());
			nspk = float.Parse (data [i] ["nspk"].ToString ());

			xval = xval * condensevalue;
			yval = yval * condensevalue;
		
			Datapoint.setDataPointColor (0.3f);
			Vector3 vect3xy = new Vector3 (xval, 0, yval);
			Instantiate (staticPointPrefab, vect3xy, Quaternion.identity);
		}
	}
		
		

	public void  VisGraph(int sliderData){
		for (int i = sliderData; i < sliderData +maxRange; i++) {

			startTimeSec = (int)(sliderData * 0.03f);
		    endTimeSec = (int)((sliderData + maxRange) * 0.03f);

			dataStartTimeInSeconds.text = startTimeSec.ToString();
			dataEndTimeInSeconds.text = endTimeSec.ToString();

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

			Vector3 vect3xy = new Vector3 (xval, 0, yval);
			Datapoint.setDataPointColor ();
			Instantiate (staticPointPrefab, vect3xy, Quaternion.identity);


			if (nspk == 1) {
				
				noOfSpikes++;
				lowSpikes++;
				timeSec = ntime / 10000;
				timeOfSpikeLow.Add (timeSec);
				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Spike.setColor (0.1f);
				Instantiate (pointOneSpkiePrefab, vect3intensity, Quaternion.identity);
				CreateSpike (xval, yval, nspikeVal);
			}
			if (nspk == 2) {

				noOfSpikes+=2;
				mediumSpikes++;
				timeOfSpikeMedium.Add(((ntime+ntime2)/2)/10000);
				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Spike.setColor (0.1f);
				Instantiate (pointTwoSpkiePrefab, vect3intensity, Quaternion.identity);
				CreateSpike (xval, yval, nspikeVal);
			}

			if (nspk == 3) {

				noOfSpikes+=3;
				highSpikes++;
				timeOfSpikeHigh.Add (((ntime+ntime2+ntime3)/3)/10000);
				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Spike.setColor (0.1f);
				Instantiate (pointThreeSpkiePrefab, vect3intensity, Quaternion.identity);
				CreateSpike (xval, yval, nspikeVal);
			}

		}

		string timesLow = CreateStringOfTime (timeOfSpikeLow);
		string timesMedium = CreateStringOfTime (timeOfSpikeMedium);
		string timesHigh = CreateStringOfTime (timeOfSpikeHigh);

		int tillRange = sliderData + maxRange - 1;
		showRange(sliderData, tillRange, noOfSpikes,lowSpikes,highSpikes,mediumSpikes,timesLow,timesMedium,timesHigh);

		DestroyTimeList (timeOfSpikeLow);
		DestroyTimeList (timeOfSpikeMedium);
		DestroyTimeList (timeOfSpikeHigh);
		noOfSpikes = 0;
		lowSpikes = 0;
		mediumSpikes = 0;
		highSpikes = 0;
	}



	public void  VisGraph(int minTime, int maxtime){

		float alpha = 0.0f;
		bool flag = false;
		int midP = (maxtime - minTime) / 2;
		int mid = minTime + midP;
		int d = 0;
		float[] arr = new float[mid +1];
		int l = 0;



		for (int i = minTime; i < maxtime; i++) {
			d++;
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

			Vector3 vect3xy = new Vector3(xval, 0, yval);

			Datapoint.setDataPointColor ();

			if (alpha < 1 && flag == false) {
				alpha = (float)d / (float)midP;
				arr [l] = alpha;
			//	Debug.Log (alpha);
				l++;
			} 

			else {
				
				flag = true;
				alpha = arr [l];
				l--;
			//	Debug.Log (alpha);

			}

			Datapoint.setDataPointColor (alpha);

			Instantiate(staticPointPrefab, vect3xy, Quaternion.identity);

			if (nspk == 1) {

				noOfSpikes++;
				lowSpikes++;
				timeSec = ntime / 10000;
				timeOfSpikeLow.Add (timeSec);
				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Spike.setColor (alpha);
				Instantiate (pointOneSpkiePrefab, vect3intensity, Quaternion.identity);
				CreateSpike (xval, yval, nspikeVal);
			}
			if (nspk == 2) {

				noOfSpikes+=2;
				mediumSpikes++;
				timeOfSpikeMedium.Add(((ntime+ntime2)/2)/10000);
				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Spike.setColor (alpha);
				Instantiate (pointTwoSpkiePrefab, vect3intensity, Quaternion.identity);
				CreateSpike (xval, yval, nspikeVal);
			}

			if (nspk == 3) {

				noOfSpikes+=3;
				highSpikes++;
				timeOfSpikeHigh.Add (((ntime+ntime2+ntime3)/3)/10000);
				nspikeVal = nspk * nspikeCondenseValue;
				Vector3 vect3intensity = new Vector3(xval , nspikeVal, yval);
				Spike.setColor (alpha);
				Instantiate (pointThreeSpkiePrefab, vect3intensity, Quaternion.identity);
				CreateSpike (xval, yval, nspikeVal);
			}


		}

		string timesLow = CreateStringOfTime (timeOfSpikeLow);
		string timesMedium = CreateStringOfTime (timeOfSpikeMedium);
		string timesHigh = CreateStringOfTime (timeOfSpikeHigh);


		showRange(minTime, maxtime, noOfSpikes,lowSpikes,highSpikes,mediumSpikes,timesLow,timesMedium,timesHigh);



		DestroyTimeList (timeOfSpikeLow);
		DestroyTimeList (timeOfSpikeMedium);
		DestroyTimeList (timeOfSpikeHigh);
		noOfSpikes = 0;
		lowSpikes = 0;
		mediumSpikes = 0;
		highSpikes = 0;
	}


	private void CreateSpike (float x, float z, float y) 
	{
		int tempY = (int) y;
		Vector3 position;
		position.z = 0f;
		for (int i = 1; i <= tempY; i++) {
			Transform point = Instantiate (spikePrefab);
			position.x = x;
			position.z = z;
			position.y = i;
			point.localPosition = position;
		}
	}

	public void DestroySpikes() {
		GameObject[] spikes = GameObject.FindGameObjectsWithTag("spike");
		foreach (GameObject spike in spikes)
			GameObject.Destroy(spike);
	}

	public void DestroyStaticPoints() {
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("staticpoint");
		foreach (GameObject enemy in enemies)
			GameObject.Destroy(enemy);
	}
	

}
