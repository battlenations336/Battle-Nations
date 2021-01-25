using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Variables in here do not change and are clickable
 * pbsquare - player battle square
 * 
 */

public class ClickManager : MonoBehaviour {

	public static GameObject pbsquare;
	public static GameObject pbsquare1;
	public static GameObject pbsquare2;
	public static GameObject pbsquare3;
	public static GameObject pbsquare4;
	public static GameObject pbsquare5;
	public static GameObject pbsquare6;
	public static GameObject pbsquare7;
	public static GameObject pbsquare8;
	public static GameObject pbsquare9;
	public static GameObject pbsquare10;
	public static GameObject pbsquare11;
	public static GameObject pbsquare12;
	public static GameObject arcticTrooperIcon;

	void Start()
	{
		pbsquare = GameObject.Find("playerbattleSquare");
		pbsquare1 = GameObject.Find("playerbattleSquare1");
		pbsquare2 = GameObject.Find("playerbattleSquare2");
		pbsquare3 = GameObject.Find("playerbattleSquare3");
		pbsquare4 = GameObject.Find("playerbattleSquare4");
		pbsquare5 = GameObject.Find("playerbattleSquare5");
		pbsquare6 = GameObject.Find("playerbattleSquare6");
		pbsquare7 = GameObject.Find("playerbattleSquare7");
		pbsquare8 = GameObject.Find("playerbattleSquare8");
		pbsquare9 = GameObject.Find("playerbattleSquare9");
		pbsquare10 = GameObject.Find("playerbattleSquare10");
		pbsquare11 = GameObject.Find("playerbattleSquare11");
		pbsquare12 = GameObject.Find("playerbattleSquare12");
		arcticTrooperIcon = GameObject.Find("s_arctic_trooper_icon@2x");
	}


	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonDown(0))
		{

			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

			if (hit.collider != null)
			{

				if (hit.collider.gameObject.Equals(pbsquare))
				{
					if (GlobalVariables.inBattle == true)
					{
						Debug.Log("pbsquare hit");
					}

					else
					{

					}
				}

				// CHARACTER PLACER ICONS

				if (hit.collider.gameObject.Equals(arcticTrooperIcon))
				{
					//Check for non-occupied square
					for (int squareIndex = 0; squareIndex < 13; squareIndex++)
					{
						if (GlobalVariables.pbsob[squareIndex] == null)
						{
							var xyposArray = ArcticTrooperStats.xyPos;
							Debug.Log("Found a square" + squareIndex);
							ArcticTrooperStats.arcticTrooperPlayer.GetComponent<Transform>().position = xyposArray[0];
							break;
						}
						else
						{
							continue;
						}
					}
				}
			}

		}

	}
}
