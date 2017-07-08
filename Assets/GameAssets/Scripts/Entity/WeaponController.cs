using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

	private Weapon pistol = new Weapon(Weapon.WeaponType.PISTOL, 
		new Range(0, 1), 1.0f, 0.0f, 2.0f, false, 8,
		"W_Pistol", null, null, null
	);
	public GameObject pistolGO;

	private Weapon currentWeapon;
	private float cooldown = 0.0f;
	private bool isReloading = false;

	private Animator animator;

	void Awake(){
		animator = GetComponent<Animator>();
		pistol.obj = pistolGO;

		pistol.ammoInMag = pistol.ammoPerMag * 100;
		pistol.ammoOutMag = pistol.ammoPerMag;
	}

	void Start(){
		changeWeapon(pistol);
	}

	void Update(){
		if(cooldown > 0.0f){
			cooldown -= Time.deltaTime;
		}
	}

	void changeWeapon(Weapon weapon){
		if(!isReloading){
			if(currentWeapon != null){
				currentWeapon.obj.SetActive(false);
			}
			currentWeapon = weapon;
			currentWeapon.obj.SetActive(true);
			animator.Play(currentWeapon.anim_idle);
			cooldown = 0.0f;
		}
	}

	public void tryShootAuto(){
		if(currentWeapon.isAuto){
			tryShoot();
		}
	}

	public void tryShootSemi(){
		tryShoot();
	}

	private void tryShoot(){
		if(currentWeapon.ammoInMag > 0 && cooldown <= 0.0f){
			animator.Play(currentWeapon.anim_fire);
			BroadcastMessage("shoot", currentWeapon);
			cooldown = currentWeapon.firePeriod;
			currentWeapon.ammoInMag--;
		}
		else if(currentWeapon.ammoInMag == 0 && currentWeapon.ammoOutMag > 0 && !isReloading){
			isReloading = true;
			StartCoroutine(reload());
		}
	}

	private IEnumerator reload(){
		animator.Play(currentWeapon.anim_reload);
		yield return new WaitForSeconds(currentWeapon.reloadPeriod);
		if(currentWeapon.ammoOutMag > currentWeapon.ammoPerMag){
			currentWeapon.ammoInMag = currentWeapon.ammoPerMag;
		}
		else{
			currentWeapon.ammoInMag = currentWeapon.ammoOutMag;
		}
		currentWeapon.ammoOutMag -= currentWeapon.ammoInMag;
		isReloading = false;
	}

}
