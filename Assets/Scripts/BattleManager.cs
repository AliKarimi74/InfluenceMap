using UnityEngine;
using System.Collections;

public enum Strategy {
	ATTACK,
	GATHER_RESOURCE
}

public class BattleManager : MonoBehaviour {

	static BattleManager _instance;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ChangeGameSpeed(int level) {
		int sp = Mathf.Clamp (level, 0, 5);
		Time.timeScale = sp;
	}

//	public Strategy GetStrategy(bool is_neg_squad) {
//	}
}
