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

public class InfluenceMap : GridData
{
	List<SimplePropagator> _propagators = new List<SimplePropagator>();
	
	float[,] _influences;
	float[,] _influencesBuffer;

	public float Decay { get; set; }
	public float Momentum { get; set; }

	public int Width { get{ return _influences.GetLength(0); } }
	public int Height { get{ return _influences.GetLength(1); } }
	public float GetValue(int x, int y) {
		return _influences [x, y];
	}
	
	public InfluenceMap(int size, float decay, float momentum) {
		_influences = new float[size, size];
		_influencesBuffer = new float[size, size];
		Decay = decay;
		Momentum = momentum;
	}
	
	public InfluenceMap(int width, int height, float decay, float momentum) {
		_influences = new float[width, height];
		_influencesBuffer = new float[width, height];
		Decay = decay;
		Momentum = momentum;
	}
	
	public void SetInfluence(int x, int y, float value) {
		if (x < Width && y < Height) {
			_influences[x, y] = value;
			_influencesBuffer[x, y] = value;
		}
	}

	public void SetInfluence(Vector2I pos, float value) {
		if (pos.x < Width && pos.y < Height) {
			_influences[pos.x, pos.y] = value;
			_influencesBuffer[pos.x, pos.y] = value;
		}
	}

	public void RegisterPropagator(SimplePropagator p) {
		_propagators.Add (p);
	}

	public void DeletePropagator(SimplePropagator p) {
		_propagators.Remove (p);	
	}

	public void Propagate() {
		UpdatePropagators();
		UpdatePropagation();
		UpdateInfluenceBuffer();
	}

	void UpdatePropagators() {
		for (int i = 0; i < _propagators.Count; ++i)
			SetInfluence (_propagators[i].GridPosition, _propagators[i].Value);
	}

	void UpdatePropagation() {
		for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx) {
			for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx) {
				float maxInf = -1;
				float minInf = 1;
				Vector2I[] neighbors = GetNeighbors(xIdx, yIdx);
				foreach (Vector2I n in neighbors) {
					float inf = _influencesBuffer[n.x, n.y] * Mathf.Exp(-Decay * n.d);
					maxInf = Mathf.Max(inf, maxInf);
					minInf = Mathf.Min(inf, minInf);
				}
				
				if (Mathf.Abs(minInf) > maxInf)
					_influences[xIdx, yIdx] = Mathf.Lerp(_influencesBuffer[xIdx, yIdx], minInf, Momentum);
				else 
					_influences[xIdx, yIdx] = Mathf.Lerp(_influencesBuffer[xIdx, yIdx], maxInf, Momentum);
			}
		}
	}

	void UpdateInfluenceBuffer() {
		for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx) 
			for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx) 
				_influencesBuffer[xIdx, yIdx] = _influences[xIdx, yIdx];
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
