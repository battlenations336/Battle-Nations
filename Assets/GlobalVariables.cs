using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables : MonoBehaviour {
	/* 
	 * Variables In here are the ones that change
	 * inBattle - Is player in battle
	 * pbsob - Player Battle Square Ocupied By
	 */

	public static bool inBattle;
	public static GameObject[] pbsob;

	void Start()
	{
		pbsob = new GameObject[13];
	}



}
