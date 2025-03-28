using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class AbilityUIManager : MonoBehaviour
{
    [Header("Ability Script References")]
    public PlayerDash playerDash;
    public PlayerDashWithBuff playerDashWithBuff;
    public PlayerDashWithProjectile playerDashWithProjectile;
    public AoEAbility aoeAbility;
    public PlayerShooting shootingAbility;

    [Header("UI References")]
    public GameObject abilityPanel; // Assign in inspector
    public Button dashButton;
    public Button dashWithBuffButton;
    public Button dashWithProjectileButton;
    public Button aoeButton;
    public Button shootingButton;

    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            abilityPanel.SetActive(true); // Open ability panel only for local player
            AssignButtonListeners();
        }
        else
        {
            abilityPanel.SetActive(false); // Hide ability panel for others
        }
    }

    void AssignButtonListeners()
    {
        dashButton.onClick.AddListener(EnableDash);
        dashWithBuffButton.onClick.AddListener(EnableDashWithBuff);
        dashWithProjectileButton.onClick.AddListener(EnableDashWithProjectile);
        aoeButton.onClick.AddListener(EnableAOE);
        shootingButton.onClick.AddListener(EnableShooting);
    }

    void EnableDash()
    {
        DisableAllAbilities();
        playerDash.enabled = true;
    }

    void EnableDashWithBuff()
    {
        DisableAllAbilities();
        playerDashWithBuff.enabled = true;
    }

    void EnableDashWithProjectile()
    {
        DisableAllAbilities();
        playerDashWithProjectile.enabled = true;
    }

    void EnableAOE()
    {
        DisableAllAbilities();
        aoeAbility.enabled = true;
    }

    void EnableShooting()
    {
        DisableAllAbilities();
        shootingAbility.enabled = true;
    }

    void DisableAllAbilities()
    {
        playerDash.enabled = false;
        playerDashWithBuff.enabled = false;
        playerDashWithProjectile.enabled = false;
        aoeAbility.enabled = false;
    }
}
