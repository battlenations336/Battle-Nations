using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcticTrooperStats : MonoBehaviour {

	public static GameObject arcticTrooperPlayer;
	public static Vector2[] xyPos;

	// Use this for initialization
	void Start () {

		arcticTrooperPlayer = GameObject.Find("arcticTrooperPlayer");
		Vector2[] xyPos = new Vector2[13];
		xyPos[0] = new Vector2(10.001f, 101.22f);


	}
}
