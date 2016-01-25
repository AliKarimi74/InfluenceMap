using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct IMSpecs {
	public string name;
	public float decay;
	public float momentum;
	public int updateFreq;
}

[System.Serializable]
public struct DesireValEquationParam {
	public string basic_param;
	public int coefficient;
	public bool enemySquad;
	public bool higherIsBetter;
}

[System.Serializable]
public struct DesireValueIMSpecs {
	public IMSpecs spec;
	public DesireValEquationParam[] CalculattingEquation;
}

struct DisplaySpecs {
	public bool posIsOn;
	public bool negIsOn;
	public bool posIsLowMap;
	public bool negIsLowMap;
	public int mapIdxPos;
	public int mapIdxNeg;
}

public class InfluenceMapServer : MonoBehaviour {

	public Transform bottomLeft, upperRight;
	public GridDisplay display;

	public int squadNumbers;

	public float gridSize;
	int _width;
	int _high;

	public IMSpecs[] lowLevelMaps;
	public DesireValueIMSpecs[] highLevelMaps;

	List<List<InfluenceMapControl>> _lowLevelIMs;
	List<List<DesirabilityValMap>> _highLevelIMs;

	DisplaySpecs _displaySignals;
	
	void Awake() {
		_lowLevelIMs = new List<List<InfluenceMapControl>> ();
		_highLevelIMs = new List<List<DesirabilityValMap>> ();

		int _width = (int)(Mathf.Abs(upperRight.position.x - bottomLeft.position.x) / gridSize);
		int _height = (int)(Mathf.Abs(upperRight.position.z - bottomLeft.position.z) / gridSize);

		for (int s = 0; s < squadNumbers; ++s) {
			GameObject obj = new GameObject ("Squad " + s.ToString ());
			obj.transform.parent = gameObject.transform;
			obj.transform.position = Vector3.zero;

			_lowLevelIMs.Add (new List<InfluenceMapControl> ());
			for (int i = 0; i < lowLevelMaps.Length; ++i) {
				GameObject im_obj = new GameObject (lowLevelMaps [i].name);
				im_obj.transform.parent = obj.transform;
				im_obj.transform.position = Vector3.zero;
				_lowLevelIMs [s].Add (im_obj.AddComponent<InfluenceMapControl> ());
				InstallInfluenceMap (_lowLevelIMs [s] [i], i);
			}
		}

		for (int s = 0; s < squadNumbers; ++s) {
			_highLevelIMs.Add(new List<DesirabilityValMap>());
			for (int i = 0; i < highLevelMaps.Length; ++i) {
				DesParam[] parms = new DesParam[highLevelMaps[i].CalculattingEquation.Length];
				for (int j = 0; j < parms.Length; ++j) {
					parms[j].coeff = highLevelMaps[i].CalculattingEquation[j].coefficient;
					for (int k = 0; k < lowLevelMaps.Length; ++k) {
						if (highLevelMaps[i].CalculattingEquation[j].basic_param == lowLevelMaps[k].name) {
							if (highLevelMaps[i].CalculattingEquation[j].enemySquad)
								parms[j].im = _lowLevelIMs[1 - s][k].IM;
							else
								parms[j].im = _lowLevelIMs[s][k].IM;
							break;
						}
					}
					parms[j].enemyInflnc = highLevelMaps[i].CalculattingEquation[j].enemySquad;
					parms[j].higherIsBetter = highLevelMaps[i].CalculattingEquation[j].higherIsBetter;
				}

				_highLevelIMs[s].Add(new DesirabilityValMap(_width, _height, highLevelMaps[i].spec.decay, highLevelMaps[i].spec.momentum, parms) );

				StartCoroutine(UpdateDesMapCR(_highLevelIMs[s][i], highLevelMaps[i].spec.updateFreq));
			}
		}

		GridData[] _initData = { (GridData)_lowLevelIMs [0] [0].IM };
		display.SetGridData (_initData);
		display.CreateMesh (bottomLeft.position, gridSize);
	}

	void Start() {
		_displaySignals.negIsOn = true;
		_displaySignals.posIsOn = true;
		_displaySignals.negIsLowMap = true;
		_displaySignals.posIsLowMap = true;
		_displaySignals.mapIdxNeg = 1;
		_displaySignals.mapIdxPos = 0;
		ChangeDisplayMode ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void InstallInfluenceMap(InfluenceMapControl im, int idx) {
		im._bottomLeft = bottomLeft;
		im._upperRight = upperRight;
		im._gridSize = gridSize;

		im._decay = lowLevelMaps[idx].decay;
		im._momentum = lowLevelMaps[idx].momentum;
		im._updateFrequency = lowLevelMaps[idx].updateFreq;

		im.StartWork ();
	}

	IEnumerator UpdateDesMapCR(DesirabilityValMap map, float up_freq) {
		while (true) {
			map.UpdateMap();
			yield return new WaitForSeconds(1 / up_freq);
		}
	}

	public Vector2I GetGridPosition(Vector3 pos) {
		int x = (int)((pos.x - bottomLeft.position.x) / gridSize);
		int y = (int)((pos.z - bottomLeft.position.z) / gridSize);
		
		return new Vector2I(x, y);
	}
	
	public void GetMovementLimits(out Vector3 downLeft, out Vector3 topRight) {
		downLeft = bottomLeft.position;
		topRight = upperRight.position;
	}

	public void RegisterPropagator(SimplePropagator p, string type, int squad_no) {
		bool found = false;
		for (int i = 0; i < lowLevelMaps.Length; ++i) {
			if (type == lowLevelMaps[i].name) {
				_lowLevelIMs[squad_no][i].RegisterPropagator(p);
				found = true;
				break;
			}
		}
		if (!found) 
			print ("Err: Type not found for register: " + type);
	}

	public void DeadPropagator(SimplePropagator p, string type, int squad_no) {
		bool found = false;
		for (int i = 0; i < lowLevelMaps.Length; ++i) {
			if (type == lowLevelMaps[i].name) {
				_lowLevelIMs[squad_no][i].DeadUnit(p);
				found = true;
				break;
			}
		}
		if (!found) 
			print ("Err: Type not found for dead: " + type);
	}

	public void ChangeDisplayMode() {
		if (_displaySignals.posIsOn && !_displaySignals.negIsOn) {
			if (_displaySignals.posIsLowMap) {
				GridData[] data = { (GridData)_lowLevelIMs [0] [_displaySignals.mapIdxPos].IM };
				display.SetGridData (data);
			} else {
				GridData[] data = { (GridData)_highLevelIMs [0] [_displaySignals.mapIdxPos] };
				display.SetGridData (data);
			}
		} else if (!_displaySignals.posIsOn && _displaySignals.negIsOn) {
			if (_displaySignals.negIsLowMap) {
				GridData[] data = { (GridData)_lowLevelIMs [1] [_displaySignals.mapIdxNeg].IM };
				display.SetGridData (data);
			} else {
				GridData[] data = { (GridData)_highLevelIMs [1] [_displaySignals.mapIdxNeg] };
				display.SetGridData (data);
			}
		} 
		else {
			GridData[] data = new GridData[2];
			if (_displaySignals.posIsLowMap) 
				data[0] = (GridData)_lowLevelIMs [0] [_displaySignals.mapIdxPos].IM;
			else 
				data[0] = (GridData)_highLevelIMs [0] [_displaySignals.mapIdxPos];
			if (_displaySignals.negIsLowMap) 
				data[1] = (GridData)_lowLevelIMs [1] [_displaySignals.mapIdxNeg].IM;
			else
				data[1] = (GridData)_highLevelIMs [1] [_displaySignals.mapIdxNeg];
			display.SetGridData (data);
		}
	}

	public void ToggleDisplayFlag(bool is_pos) {
		if (is_pos)
			_displaySignals.posIsOn = !_displaySignals.posIsOn;
		else
			_displaySignals.negIsOn = !_displaySignals.negIsOn;
		ChangeDisplayMode ();
	}

	public void ChangPosMapIdxLL(int idx) {
		_displaySignals.posIsLowMap = true;
		_displaySignals.mapIdxPos = idx;
		ChangeDisplayMode ();
	}

	public void ChangPosMapIdxHL(int idx) {
		_displaySignals.posIsLowMap = false;
		_displaySignals.mapIdxPos = idx;
		ChangeDisplayMode ();
	}

	public void ChangNegMapIdxLL(int idx) {
		_displaySignals.negIsLowMap = true;
		_displaySignals.mapIdxNeg = idx;
		ChangeDisplayMode ();
	}
	
	public void ChangNegMapIdxHL(int idx) {
		_displaySignals.negIsLowMap = false;
		_displaySignals.mapIdxNeg = idx;
		ChangeDisplayMode ();
	}
}
