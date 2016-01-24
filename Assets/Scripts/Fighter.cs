using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public struct UnitSpecification {
	public float hp;
	public float runSpeed;
	public float power;
	public float attackSpeed;
	public float range;
}

public class Fighter : MonoBehaviour {

	public UnitSpecification unitProperties;
	public Image hpImage;

	SimplePropagator _unit;
	SphereCollider _eye;
	Fighter _enemy;
	float _initHp;

	void Start () {
		_unit = GetComponentInParent<SimplePropagator> ();
		_eye = GetComponent<SphereCollider> ();
		_eye.radius = unitProperties.range;
		_initHp = unitProperties.hp;
	}
	
	void Update () {
	}

	public int Squad {
		get {
			return _unit.squadNo;
		}
	}

	public void OnTriggerEnter(Collider other) {
		_enemy = other.gameObject.GetComponentInChildren<Fighter> ();
		if (_enemy != null) {
			if (_enemy.Squad == Squad)
				_enemy = null;
			else 
				StartCoroutine (HitEnemyCR ());
		}
	}

	public void OnTriggerExit(Collider other) {
		if (_enemy != null) 
			_enemy = null;
	}

	bool IsAlive {
		get { return unitProperties.hp > 0;  }
	}

	public void GetDamage(float dam) {
		unitProperties.hp -= dam;
		hpImage.fillAmount = Mathf.Clamp(unitProperties.hp / _initHp, 0, 1);
		if (unitProperties.hp <= 0)
			Dead ();
	}

	IEnumerator HitEnemyCR() {
		while (_enemy != null) {
			_enemy.GetDamage(unitProperties.power);
			yield return new WaitForSeconds(2f/unitProperties.attackSpeed);
		}
	}

	void Dead() {
		_unit.Dead ();
	}


}
