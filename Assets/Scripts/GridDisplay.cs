using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface GridData
{
	int Width { get; }
	int Height { get; }
	float GetValue(int x, int y);
}

public class GridDisplay : MonoBehaviour
{
	MeshRenderer _meshRenderer;
	MeshFilter _meshFilter;
	Mesh _mesh;

	GridData[] _data;

	[SerializeField]
	Material _material;
	
	[SerializeField]
	Color _neutralColor = Color.white;
	
	[SerializeField]
	Color _positiveColor = Color.red;
	
	[SerializeField]
	Color _positive2Color = Color.red;
	
	[SerializeField]
	Color _negativeColor = Color.blue;
	
	[SerializeField]
	Color _negative2Color = Color.blue;
	
	Color[] _colors;

	public void SetGridData(GridData[] m) {
		_data = m;
	}

	public void CreateMesh(Vector3 bottomLeftPos, float gridSize)
	{
		_mesh = new Mesh();
		_mesh.name = name;
		_meshFilter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		_meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		
		_meshFilter.mesh = _mesh;
		_meshRenderer.material = _material;
		
		float _objectHeight = transform.position.y;
		float _staX = 0;//bottomLeftPos.x;
		float _staZ = 0;//bottomLeftPos.z;
		
		// create squares starting at bottomLeftPos
		List<Vector3> verts = new List<Vector3>();
		for (int yIdx = 0; yIdx < _data[0].Height; ++yIdx) {
			for (int xIdx = 0; xIdx < _data[0].Width; ++xIdx) {
				Vector3 bl = new Vector3(_staX + (xIdx * gridSize), _objectHeight, _staZ + (yIdx * gridSize));
				Vector3 br = new Vector3(_staX + ((xIdx+1) * gridSize), _objectHeight, _staZ + (yIdx * gridSize));
				Vector3 tl = new Vector3(_staX + (xIdx * gridSize), _objectHeight, _staZ + ((yIdx+1) * gridSize));
				Vector3 tr = new Vector3(_staX + ((xIdx+1) * gridSize), _objectHeight, _staZ + ((yIdx+1) * gridSize));

				verts.Add(bl);
				verts.Add(br);
				verts.Add(tl);
				verts.Add(tr);
			}
		}

		List<Color> colors = new List<Color>();
		for (int yIdx = 0; yIdx < _data[0].Height; ++yIdx) {
			for (int xIdx = 0; xIdx < _data[0].Width; ++xIdx) {
				colors.Add(Color.white);
				colors.Add(Color.white);
				colors.Add(Color.white);
				colors.Add(Color.white);
			}
		}
		_colors = colors.ToArray();
		
		List<Vector3> norms = new List<Vector3>();
		for (int yIdx = 0; yIdx < _data[0].Height; ++yIdx) {
			for (int xIdx = 0; xIdx < _data[0].Width; ++xIdx) {
				norms.Add(Vector3.up);
				norms.Add(Vector3.up);
				norms.Add(Vector3.up);
				norms.Add(Vector3.up);
			}
		}
		
		List<Vector2> uvs = new List<Vector2>();
		for (int yIdx = 0; yIdx < _data[0].Height; ++yIdx) {
			for (int xIdx = 0; xIdx < _data[0].Width; ++xIdx) {
				uvs.Add(new Vector2(0, 0));
				uvs.Add(new Vector2(1, 0));
				uvs.Add(new Vector2(0, 1));
				uvs.Add(new Vector2(1, 1));
			}
		}
		
		List<int> tris = new List<int>();
		for (int idx = 0; idx < verts.Count; idx+=4) {
			int bl = idx;
			int br = idx+1;
			int tl = idx+2;
			int tr = idx+3;

			tris.Add(bl);
			tris.Add(tl);
			tris.Add(br);
			
			tris.Add(tl);
			tris.Add(tr);
			tris.Add(br);
			
		}

		_mesh.vertices = verts.ToArray();
		_mesh.normals = norms.ToArray();
		_mesh.uv = uvs.ToArray();
		_mesh.colors = _colors;
		_mesh.triangles = tris.ToArray();
	}
	
	void SetColor(int x, int y, Color c) {
		int idx = ((y * _data[0].Width) + x) * 4;
		_colors[idx] = c;
		_colors[idx+1] = c;
		_colors[idx+2] = c;
		_colors[idx+3] = c;
	}

	void Update()
	{
		if (_data != null) {
			for (int yIdx = 0; yIdx < _data[0].Height; ++yIdx) {
				for (int xIdx = 0; xIdx < _data[0].Width; ++xIdx) {
					float value = 0;
					for (int i = 0; i < _data.Length; ++i) 
						value += _data [i].GetValue (xIdx, yIdx);
					value /= _data.Length;
					Color c = _neutralColor;
					if (value > 0.5f)
						c = Color.Lerp (_positiveColor, _positive2Color, (value - 0.5f) / 0.5f);
					else if (value > 0)
						c = Color.Lerp (_neutralColor, _positiveColor, value / 0.5f);
					else if (value < -0.5f)
						c = Color.Lerp (_negativeColor, _negative2Color, -(value + 0.5f) / 0.5f);
					else
						c = Color.Lerp (_neutralColor, _negativeColor, -value / 0.5f);
					SetColor (xIdx, yIdx, c);
				}
			}
		
			_mesh.colors = _colors;
		}
	}
}
