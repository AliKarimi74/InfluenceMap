using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct DesParam {
	public int coeff;
	public InfluenceMap im;
	public bool enemyInflnc;
	public bool higherIsBetter;
}

public class DesirabilityValMap : GridData
{
	float[,] _influences;
	DesParam[] _parameters;

	public float Decay { get; set; }
	public float Momentum { get; set; }

	public int Width { get{ return _influences.GetLength(0); } }
	public int Height { get{ return _influences.GetLength(1); } }

	public float GetValue(int x, int y) {
		return _influences [x, y];
	}
	
	public DesirabilityValMap(int size, float decay, float momentum, DesParam[] p) {
		_influences = new float[size, size];
		Decay = decay;
		Momentum = momentum;
		_parameters = p;
	}
	
	public DesirabilityValMap(int width, int height, float decay, float momentum, DesParam[] p) {
		_influences = new float[width, height];
		Decay = decay;
		Momentum = momentum;
		_parameters = p;
	}
	
	public void SetInfluence(int x, int y, float value) {
		if (x < Width && y < Height) {
			_influences[x, y] = value;
		}
	}

	public void SetInfluence(Vector2I pos, float value) {
		if (pos.x < Width && pos.y < Height) {
			_influences[pos.x, pos.y] = value;
		}
	}

	float ReverseFunc(float val) {
		if (val >= 0)
			return 1 - val;
		else
			return -1 - val;
	}

	public void UpdateMap() {
		float val;
		int sum;
		float temp;
		for (int xIdx = 0; xIdx < _influences.GetLength(0); ++xIdx) {
			for (int yIdx = 0; yIdx < _influences.GetLength(1); ++yIdx) {
				val = 0;
				sum = 0;
				for (int i = 0; i < _parameters.Length; ++i) {
					sum += _parameters[i].coeff;

					temp = _parameters[i].im.GetValue(xIdx, yIdx);
					if (_parameters[i].enemyInflnc) temp *= -1;
					if (!_parameters[i].higherIsBetter) temp = ReverseFunc(temp);

					val += _parameters[i].coeff * temp;
				}
				_influences[xIdx, yIdx] = val / sum;
			}
		}
	}
}
