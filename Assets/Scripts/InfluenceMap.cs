using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Vector2I {
	public int x;
	public int y;
	public float d;

	public Vector2I(int nx, int ny) {
		x = nx;
		y = ny;
		d = 1;
	}

	public Vector2I(int nx, int ny, float nd) {
		x = nx;
		y = ny;
		d = nd;
	}
}

public class DeadPropagator : IPropagator {
	Vector2I _pos;
	float _value;

	public DeadPropagator(Vector2I p, float v) {
		_pos = p;
		_value = v;
	}

	public float Value { get { return _value; } }
	public Vector2I GridPosition { get { return _pos; } }
}

public struct CellData {
	public float influence;
	public float defenceInfluence;
	public float resourceInfluence;
	public float deadUnits;
	public float AttackScore;
	public float DefenceScore;
	public float ResourceScore;
	public float SecurityScore;
	public CellData (float a, float b, float c, float d) {
		influence = a;
		defenceInfluence = b;
		resourceInfluence = c;
		deadUnits = d;
		AttackScore = 0f;
		DefenceScore = 0f;
		ResourceScore = 0f;
		SecurityScore = 0f;
	}
}

[System.Serializable]
public enum DisplayGridMode {
	Influence,
	DefenceInfluence,
	ResourceInfluence,
	DeadInfluence,
	AttackScore,
	DefenceScore,
	SecuratyScore,
	GatherResourceScore
}

public class InfluenceMap : GridData
{
	List<SimplePropagator> _propagators = new List<SimplePropagator>();
	List<SimplePropagator> _defencePropagators = new List<SimplePropagator>();
	List<SimplePropagator> _resourcePropagators = new List<SimplePropagator>();
	List<IPropagator> _deadPropagators = new List<IPropagator>();
	DisplayGridMode _mode;

	CellData[,] _influences;
	CellData[,] _influencesBuffer;

	public float Decay { get; set; }
	public float Momentum { get; set; }

	public int Width { get{ return _influences.GetLength(0); } }
	public int Height { get{ return _influences.GetLength(1); } }
	public float GetValue(int x, int y) {
		switch (_mode) {

		case DisplayGridMode.Influence:
			return _influences [x, y].influence;

		case DisplayGridMode.DefenceInfluence:
			return _influences [x, y].defenceInfluence;

		case DisplayGridMode.ResourceInfluence:
			return _influences [x, y].resourceInfluence;

		case DisplayGridMode.DeadInfluence:
			return _influences [x, y].deadUnits;

		case DisplayGridMode.AttackScore:
			return _influences[x, y].AttackScore;

		case DisplayGridMode.DefenceScore:
			return _influences[x, y].DefenceScore;

		case DisplayGridMode.SecuratyScore:
			return _influences[x, y].SecurityScore;

		case DisplayGridMode.GatherResourceScore:
			return _influences[x, y].ResourceScore;
		}
		return _influences [x, y].influence;
	}
	
	public InfluenceMap(int size, float decay, float momentum) {
		_influences = new CellData[size, size];
		_influencesBuffer = new CellData[size, size];
		Decay = decay;
		Momentum = momentum;
		_mode = DisplayGridMode.Influence;

	}
	
	public InfluenceMap(int width, int height, float decay, float momentum) {
		_influences = new CellData[width, height];
		_influencesBuffer = new CellData[width, height];
		Decay = decay;
		Momentum = momentum;
	}
	
	public void SetInfluence(int x, int y, float value) {
		if (x < Width && y < Height) {
			_influences[x, y].influence = value;
			_influencesBuffer[x, y].influence = value;
		}
	}

	public void SetInfluence(Vector2I pos, float value) {
		if (pos.x < Width && pos.y < Height) {
			_influences[pos.x, pos.y].influence = value;
			_influencesBuffer[pos.x, pos.y].influence = value;
		}
	}

	public void SetDefenceInfluence(Vector2I pos, float value) {
		if (pos.x < Width && pos.y < Height) {
			_influences[pos.x, pos.y].defenceInfluence = value;
			_influencesBuffer[pos.x, pos.y].defenceInfluence = value;
		}
	}

	public void SetResourceInfluence(Vector2I pos, float value) {
		if (pos.x < Width && pos.y < Height) {
			_influences[pos.x, pos.y].resourceInfluence = value;
			_influencesBuffer[pos.x, pos.y].resourceInfluence = value;
		}
	}

	public void SetDeadInfluence(Vector2I pos, float value) {
		if (pos.x < Width && pos.y < Height) {
			_influences[pos.x, pos.y].deadUnits = value;
			_influencesBuffer[pos.x, pos.y].deadUnits = value;
		}
	}

	public void RegisterPropagator(SimplePropagator p, UnitType type) {
		if (type == UnitType.Soldier)
			_propagators.Add (p);
		else if (type == UnitType.Defencer)
			_defencePropagators.Add (p);
		else if (type == UnitType.Resource)
			_resourcePropagators.Add (p);
	}

	public void RegisterDeadPropagator(IPropagator p) {
		DeadPropagator d = new DeadPropagator (p.GridPosition, p.Value);
		_deadPropagators.Add ((IPropagator)d);
	}

	public void DeletePropagator(SimplePropagator p, UnitType type) {
		if (type == UnitType.Soldier)
			_propagators.Remove (p);
		else if (type == UnitType.Defencer)
			_defencePropagators.Remove (p);
		else if (type == UnitType.Resource)
			_resourcePropagators.Remove (p);
	}

	public void ChangeDisplayMode(DisplayGridMode new_mode) {
		_mode = new_mode;
	}

	public void Propagate() {
		UpdatePropagators();
		UpdatePropagation();
		UpdateInfluenceBuffer();
		UpdateDesirabilityValues ();
	}

	public IEnumerator upValCR() {
		while (true) {
			yield return new WaitForSeconds(5);
			UpdateDesirabilityValues ();
		}
	}

	void UpdatePropagators() {
		for (int i = 0; i < _propagators.Count; ++i)
			SetInfluence (_propagators[i].GridPosition, _propagators[i].Value);
		for (int i = 0; i < _defencePropagators.Count; ++i)
			SetDefenceInfluence (_defencePropagators [i].GridPosition, _defencePropagators [i].Value);
		for (int i = 0; i < _resourcePropagators.Count; ++i)
			SetResourceInfluence (_resourcePropagators [i].GridPosition, _resourcePropagators [i].Value);
		for (int i = 0; i < _deadPropagators.Count; ++i)
			SetDeadInfluence (_deadPropagators [i].GridPosition, _deadPropagators [i].Value);
	}

	void UpdatePropagation() {
		for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx) {
			for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx) {
				CellData maxInf = new CellData(0, 0, 0, 0);
				CellData minInf = new CellData(0, 0, 0, 0);
				Vector2I[] neighbors = GetNeighbors(xIdx, yIdx);
				foreach (Vector2I n in neighbors) {
					float inf = _influencesBuffer[n.x, n.y].influence * Mathf.Exp(-Decay * n.d); //* Decay;
					maxInf.influence = Mathf.Max(inf, maxInf.influence);
					minInf.influence = Mathf.Min(inf, minInf.influence);

					float def_inf = _influencesBuffer[n.x, n.y].defenceInfluence * Mathf.Exp(-Decay * n.d); //* Decay;
					maxInf.defenceInfluence = Mathf.Max(def_inf, maxInf.defenceInfluence);
					minInf.defenceInfluence = Mathf.Min(def_inf, minInf.defenceInfluence);

					float res_inf = _influencesBuffer[n.x, n.y].resourceInfluence * Mathf.Exp(-Decay * n.d); //* Decay;
					maxInf.resourceInfluence = Mathf.Max(res_inf, maxInf.resourceInfluence);
					minInf.resourceInfluence = Mathf.Min(res_inf, minInf.resourceInfluence);

					float dead_inf = _influencesBuffer[n.x, n.y].deadUnits * Mathf.Exp(-Decay * n.d); //* Decay;
					maxInf.deadUnits = Mathf.Max(dead_inf, maxInf.deadUnits);
					minInf.deadUnits = Mathf.Min(dead_inf, minInf.deadUnits);
				}
				
				if (Mathf.Abs(minInf.influence) > maxInf.influence)
					_influences[xIdx, yIdx].influence = Mathf.Lerp(_influencesBuffer[xIdx, yIdx].influence, minInf.influence, Momentum);
				else 
					_influences[xIdx, yIdx].influence = Mathf.Lerp(_influencesBuffer[xIdx, yIdx].influence, maxInf.influence, Momentum);

				if (Mathf.Abs(minInf.defenceInfluence) > maxInf.defenceInfluence)
					_influences[xIdx, yIdx].defenceInfluence = Mathf.Lerp(_influencesBuffer[xIdx, yIdx].defenceInfluence, minInf.defenceInfluence, Momentum);
				else 
					_influences[xIdx, yIdx].defenceInfluence = Mathf.Lerp(_influencesBuffer[xIdx, yIdx].defenceInfluence, maxInf.defenceInfluence, Momentum);

				if (Mathf.Abs(minInf.resourceInfluence) > maxInf.resourceInfluence)
					_influences[xIdx, yIdx].resourceInfluence = Mathf.Lerp(_influencesBuffer[xIdx, yIdx].resourceInfluence, minInf.resourceInfluence, Momentum);
				else 
					_influences[xIdx, yIdx].resourceInfluence = Mathf.Lerp(_influencesBuffer[xIdx, yIdx].resourceInfluence, maxInf.resourceInfluence, Momentum);

				if (Mathf.Abs(minInf.deadUnits) > maxInf.deadUnits)
					_influences[xIdx, yIdx].deadUnits = Mathf.Lerp(_influencesBuffer[xIdx, yIdx].deadUnits, minInf.deadUnits, Momentum);
				else 
					_influences[xIdx, yIdx].deadUnits = Mathf.Lerp(_influencesBuffer[xIdx, yIdx].deadUnits, maxInf.deadUnits, Momentum);
			}
		}
	}

	void UpdateInfluenceBuffer() {
		for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx) {
			for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx) {
				_influencesBuffer[xIdx, yIdx].influence = _influences[xIdx, yIdx].influence;
				_influencesBuffer[xIdx, yIdx].defenceInfluence = _influences[xIdx, yIdx].defenceInfluence;
				_influencesBuffer[xIdx, yIdx].resourceInfluence = _influences[xIdx, yIdx].resourceInfluence;
				_influencesBuffer[xIdx, yIdx].deadUnits = _influences[xIdx, yIdx].deadUnits;
			}
		}
	}

	void UpdateDesirabilityValues() {
		for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx) 
			for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx) 
				CalculateDesirabilityValues (xIdx, yIdx);
	}

	float MyReverseFunction(float x) {
		float EPS = 0.1f;
		if (x < -EPS)
			return -x - 1;
		else if (x > EPS)
			return -x + 1;
		return x;
	}

	void CalculateDesirabilityValues(int i, int j) {

		_influences [i, j].SecurityScore = (1 * _influences [i, j].influence +
											2 * _influences [i, j].defenceInfluence) / (1 + 2);

		_influences [i, j].AttackScore = (	-3 * MyReverseFunction(_influences[i, j].SecurityScore) +
		                                  -5 * _influences [i, j].resourceInfluence ) / (8);

		_influences [i, j].DefenceScore = (	5 * _influences [i, j].resourceInfluence +
		                                   	2 * MyReverseFunction (_influences [i, j].SecurityScore) +
		                                  	-1 * _influences [i, j].deadUnits)
											/ (5 + 2 + 1); 

		_influences [i, j].ResourceScore = (-3 * _influences [i, j].resourceInfluence +
											1 * _influences [i, j].SecurityScore) / (3);

	}
	
	Vector2I[] GetNeighbors(int x, int y) {
		List<Vector2I> retVal = new List<Vector2I>();
		
		if (x > 0) retVal.Add(new Vector2I(x-1, y));
		if (x < _influences.GetLength(0)-1) retVal.Add(new Vector2I(x+1, y));
		if (y > 0) retVal.Add(new Vector2I(x, y-1));
		if (y < _influences.GetLength(1)-1) retVal.Add(new Vector2I(x, y+1));

		// diagonals
		if (x > 0 && y > 0) retVal.Add(new Vector2I(x-1, y-1, 1.4142f));
		if (x < _influences.GetLength(0)-1 && y < _influences.GetLength(1)-1) retVal.Add(new Vector2I(x+1, y+1, 1.4142f));
		if (x > 0 && y < _influences.GetLength(1)-1) retVal.Add(new Vector2I(x-1, y+1, 1.4142f));
		if (x < _influences.GetLength(0)-1 && y > 0) retVal.Add(new Vector2I(x+1, y-1, 1.4142f));

		return retVal.ToArray();
	}
}
