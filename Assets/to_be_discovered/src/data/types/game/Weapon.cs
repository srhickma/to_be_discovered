using UnityEngine;

public class Weapon {

	public enum WeaponType {
		KNIFE, PISTOL, REVOLVER
	}

	private readonly RandomGenerator randomGenerator = new RandomGenerator();

	private WeaponType type{ get; set; }

	private readonly Range damage;
	public float accuracy { get; set; }
	public float firePeriod { get; set; }
	public float reloadPeriod { get; set; }
	public bool isAuto { get; set; }

	public int ammoPerMag { get; set; }
	public int ammoInMag { get; set; }
	public int ammoOutMag { get; set; }

    public string anim_idle { get; set; }
	public string anim_fire { get; set; }
	public string anim_reload { get; set; }
	public string anim_equip { get; set; }

	public GameObject obj { get; set; }

	public Weapon(WeaponType type, Range damage, float accuracy, float firePeriod, float reloadPeriod, bool isAuto, int ammoPerMag, string anim_idle, string anim_fire, string anim_reload, string anim_equip){
		this.type = type;
		this.damage = damage;
		this.accuracy = accuracy;
		this.firePeriod = firePeriod;
		this.reloadPeriod = reloadPeriod;
		this.isAuto = isAuto;
		this.ammoPerMag = ammoPerMag;
		this.anim_idle = anim_idle;
		this.anim_fire = anim_fire;
		this.anim_reload = anim_reload;
		this.anim_equip = anim_equip;
	}

	public int getDamage(){
		return randomGenerator.nextInt(damage);
	}

}
