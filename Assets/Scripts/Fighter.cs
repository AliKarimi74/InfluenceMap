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

[System.Serializable]
public enum Squad {
	SQUAD_A,
	SQUAD_B
}

public class Fighter : MonoBehaviour {

	public UnitSpecification unitProperties;
	public Squad squad;
	public Image hpImage;

	SimplePropagator _unit;
	SphereCollider _eye;
	Fighter _enemy;
	float _initHp;

	void Awake() {
		_unit = GetComponentInParent<SimplePropagator> ();
	}

	void Start () {
		_eye = GetComponent<SphereCollider> ();
		_eye.radius = unitProperties.range;
		_initHp = unitProperties.hp;
	}
	
	void Update () {
	}

	public void OnTriggerEnter(Collider other) {
		print (other.gameObject.name);
		if (other.gameObject.name == "ResourceNeg") 
			print ("entered");
		_enemy = other.gameObject.GetComponentInChildren<Fighter> ();
		if (_enemy != null) {
			if (_enemy.squad == squad)
				_enemy = null;
			else {
				StartCoroutine (HitEnemyCR ());
				if (other.transform.parent.gameObject.name == "ResourceNeg") 
					print ("setted");
			}
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
			if (_enemy.transform.parent.gameObject.name == "ResourceNeg") 
				print ("hitted");
			_enemy.GetDamage(unitProperties.power);
			yield return new WaitForSeconds(2f/unitProperties.attackSpeed);
		}
	}

	void Dead() {
		_unit.Dead ();
	}


}
