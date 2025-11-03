using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Transform currentWeapon;
    public KeyCode dropKey = KeyCode.Q;

    [Header("Debug")]
    public bool showDebug = true;

    void Update()
    {
        if (Input.GetKeyDown(dropKey) && currentWeapon != null)
        {
            DropWeapon();
        }
    }

    public void OnWeaponPickedUp(Transform weapon)
    {
        if (currentWeapon != null)
        {
            DropWeapon();
        }

        currentWeapon = weapon;

        if (showDebug) Debug.Log("🎯 PlayerWeaponManager: Silah eklendi - " + weapon.name);
    }

    public void DropWeapon()
    {
        if (currentWeapon == null) return;

        Weapon weaponScript = currentWeapon.GetComponent<Weapon>();
        if (weaponScript != null)
        {
            weaponScript.DropWeapon();
        }

        currentWeapon = null;
        if (showDebug) Debug.Log("🗑️ PlayerWeaponManager: Silah bırakıldı");
    }

    public bool HasWeapon()
    {
        return currentWeapon != null;
    }
}