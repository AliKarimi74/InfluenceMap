using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class BattleManager : MonoBehaviour {

	public RectTransform posPanel, negPanel;
	bool _isPosOpen = false, _isNegOpen = false;
	
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

	public void TogglePanel(bool is_pos) {
		if (is_pos) {
			if (_isPosOpen) 
				HOTween.To(posPanel, 0.5f, new TweenParms().Prop("anchoredPosition", new Vector2(-100, posPanel.anchoredPosition.y)));
			else 
				HOTween.To(posPanel, 0.5f, new TweenParms().Prop("anchoredPosition", new Vector2(100, posPanel.anchoredPosition.y)));
			_isPosOpen = !_isPosOpen;
		} else {
			if (_isNegOpen) 
				HOTween.To(negPanel, 0.5f, new TweenParms().Prop("anchoredPosition", new Vector2(100, posPanel.anchoredPosition.y)));
			else 
				HOTween.To(negPanel, 0.5f, new TweenParms().Prop("anchoredPosition", new Vector2(-100, posPanel.anchoredPosition.y)));
			_isNegOpen = !_isNegOpen;
		}
	}
	
}
