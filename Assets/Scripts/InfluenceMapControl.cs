using UnityEngine;
using System.Collections;

public class InfluenceMapControl : MonoBehaviour
{
	[SerializeField]
	Transform _bottomLeft;
	
	[SerializeField]
	Transform _upperRight;
	
	[SerializeField]
	float _gridSize;
	
	[SerializeField]
	float _decay = 0.3f;
	
	[SerializeField]
	float _momentum = 0.1f;
	
	[SerializeField]
	int _updateFrequency = 3;
	
	InfluenceMap _influenceMap;

	[SerializeField]
	GridDisplay _display;

	void CreateMap()
	{
		int width = (int)(Mathf.Abs(_upperRight.position.x - _bottomLeft.position.x) / _gridSize);
		int height = (int)(Mathf.Abs(_upperRight.position.z - _bottomLeft.position.z) / _gridSize);
		
		Debug.Log("Map Dimentional => " + width + " x " + height);
		
		_influenceMap = new InfluenceMap(width, height, _decay, _momentum);
		StartCoroutine (_influenceMap.upValCR ());
		
		_display.SetGridData(_influenceMap);
		_display.CreateMesh(_bottomLeft.position, _gridSize);
	}

	public void RegisterPropagator(SimplePropagator p, UnitType type)
	{
		_influenceMap.RegisterPropagator(p, type);
	}

	public Vector2I GetGridPosition(Vector3 pos)
	{
		int x = (int)((pos.x - _bottomLeft.position.x)/_gridSize);
		int y = (int)((pos.z - _bottomLeft.position.z)/_gridSize);

		return new Vector2I(x, y);
	}

	public void GetMovementLimits(out Vector3 bottomLeft, out Vector3 topRight) {
		bottomLeft = _bottomLeft.position;
		topRight = _upperRight.position;
	}

	public void DeadUnit(SimplePropagator p, float value) {
		_influenceMap.RegisterDeadPropagator (p);
		_influenceMap.DeletePropagator (p, p.type);
	}

	public void ChangeDisplayMode(int mode_idx) {
		DisplayGridMode new_mode = DisplayGridMode.Influence;
		if (mode_idx == 1)
			new_mode = DisplayGridMode.DefenceInfluence;
		else if (mode_idx == 2)
			new_mode = DisplayGridMode.ResourceInfluence;
		else if (mode_idx == 3)
			new_mode = DisplayGridMode.DeadInfluence;
		else if (mode_idx == 4)
			new_mode = DisplayGridMode.AttackScore;
		else if (mode_idx == 5)
			new_mode = DisplayGridMode.DefenceScore;
		else if (mode_idx == 6)
			new_mode = DisplayGridMode.GatherResourceScore;
		else if (mode_idx == 7)
			new_mode = DisplayGridMode.SecuratyScore;


		_influenceMap.ChangeDisplayMode (new_mode);
	}
	
	void Awake() {
		CreateMap();

		InvokeRepeating("PropagationUpdate", 0.001f, 1.0f/_updateFrequency);
	}

	void PropagationUpdate() {
		_influenceMap.Propagate();
	}

//	void SetInfluence(int x, int y, float value)
//	{
//		_influenceMap.SetInfluence(x, y, value);
//	}
//
//	void SetInfluence(Vector2I pos, float value)
//	{
//		_influenceMap.SetInfluence(pos, value);
//	}

	void Update()
	{
		_influenceMap.Decay = _decay;
		_influenceMap.Momentum = _momentum;
		
//		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
//		RaycastHit mouseHit;
//		if (Physics.Raycast(mouseRay, out mouseHit) && Input.GetMouseButton(0) || Input.GetMouseButton(1))
//		{
//			// is it within the grid
//			// if so, call SetInfluence in that grid position to 1.0
//			Vector3 hit = mouseHit.point;
//			if (hit.x > _bottomLeft.position.x && hit.x < _upperRight.position.x && hit.z > _bottomLeft.position.z && hit.z < _upperRight.position.z)
//			{
//				Vector2I gridPos = GetGridPosition(hit);
//
//				if (gridPos.x < _influenceMap.Width && gridPos.y < _influenceMap.Height)
//				{
//					SetInfluence(gridPos, (Input.GetMouseButton(0) ? 1.0f : -1.0f));
//				}
//			}
//		}
	}
}
