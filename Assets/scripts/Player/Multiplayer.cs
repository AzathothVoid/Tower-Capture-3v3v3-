using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;
using System.Collections;

public class Multiplayer : MonoBehaviour, IPunObservable
{
    public float movementSpeed = 10f;
    private Rigidbody rigidbody;

    public float fireRate = 0.75f;
    public GameObject bulletPrefab;
    public Transform bulletPosition;
    public GameObject bulletFiringEffect;
    private float nextFire;

    [HideInInspector]
    public int health = 100;
    public Slider healthBar;
    public Text playerNameText; // UI Text for player name display

    public AudioClip playerShootingAudio;

    private PhotonView photonView;

    // Variables for network synchronization
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    // References to ability scripts
    private PlayerShooting playerShooting;
    private PlayerDash playerDash;
    private AoEAbility aoeAbility;
    private PlayerDashWithBuff playerDashWithBuff;
    private PlayerDashWithProjectile playerDashWithProjectile;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        playerShooting = GetComponent<PlayerShooting>();
        playerDash = GetComponent<PlayerDash>();
        playerDashWithBuff = GetComponent<PlayerDashWithBuff>();
        playerDashWithProjectile = GetComponent<PlayerDashWithProjectile>();
        aoeAbility = GetComponent<AoEAbility>();

        // Set player name based on Photon owner
        playerNameText.text = photonView.Owner.NickName;

        // Determine local player's team from Photon custom properties
        int localTeam = -1;
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("teamID"))
        {
            localTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamID"];
        }

        // Set color based on team relation (friendly green, enemy red)
        PlayerTeam pt = GetComponent<PlayerTeam>();
        if (pt != null)
        {
            if (pt.teamID == localTeam)
            {
                playerNameText.color = Color.green;
                // Optionally, set the health bar fill color (assuming you have a reference to the fill Image)
                // healthBar.fillRect.GetComponent<Image>().color = Color.green;
            }
            else
            {
                playerNameText.color = Color.red;
                // healthBar.fillRect.GetComponent<Image>().color = Color.red;
            }
        }
        else
        {
            playerNameText.color = Color.white;
        }

        Debug.LogError("This message will make the console appear in Development Builds");

        // Initialize network sync values
        networkPosition = transform.position;
        networkRotation = transform.rotation;

        if (photonView.IsMine)
        {
            // Let the camera follow your player
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                CameraTracking cameraFollow = mainCamera.GetComponent<CameraTracking>();
                if (cameraFollow != null)
                {
                    cameraFollow.target = transform;
                }
            }
        }
        else
        {
            rigidbody.isKinematic = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        UpdateRotationFromMouse();

        // Process abilities (keys updated as needed)
        if (Input.GetKeyDown(KeyCode.F) && playerDash != null && playerDash.enabled && playerDash.CanDash())
        {
            Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            StartCoroutine(playerDash.Dash(inputDirection));
        }
        if (Input.GetKeyDown(KeyCode.F) && playerDashWithBuff != null && playerDashWithBuff.enabled && playerDashWithBuff.CanDash())
        {
            Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            StartCoroutine(playerDashWithBuff.Dash(inputDirection));
        }
        if (Input.GetKeyDown(KeyCode.F) && playerDashWithProjectile != null && playerDashWithProjectile.enabled && playerDashWithProjectile.CanDash())
        {
            Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            StartCoroutine(playerDashWithProjectile.Dash(inputDirection));
        }
        if (Input.GetKeyDown(KeyCode.F) && aoeAbility != null && aoeAbility.enabled)
        {
            aoeAbility.TriggerAoE();
        }

        // Shooting handled in PlayerShooting script (using left mouse)
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (playerDash == null || !playerDash.IsDashing())
            {
                Move();
            }
        }
        else
        {
            rigidbody.MovePosition(Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10));
            rigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10));
        }
    }

    void UpdateRotationFromMouse()
{
    // Only allow local player to control rotation
    if (!photonView.IsMine)
        return;

    // Cast a ray from the mouse position to the game world
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
    float rayDistance;

    if (groundPlane.Raycast(ray, out rayDistance))
    {
        // Get the point where the ray hits the ground plane
        Vector3 pointToLook = ray.GetPoint(rayDistance);

        // Calculate direction from player to mouse position
        Vector3 direction = pointToLook - transform.position;
        direction.y = 0f; // Keep rotation horizontal

        if (direction != Vector3.zero)
        {
            // Rotate player to face the mouse cursor
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}


    void Move()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDir = new Vector3(horizontalInput, 0, verticalInput);
        if (movementDir.magnitude > 1)
            movementDir.Normalize();

        rigidbody.MovePosition(rigidbody.position + movementDir * movementSpeed * Time.deltaTime);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(health);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            health = (int)stream.ReceiveNext();
            healthBar.value = health;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            MultiplayerBulletController bullet = collision.gameObject.GetComponent<MultiplayerBulletController>();
            TakeDamage(bullet);
        }
    }

    void TakeDamage(MultiplayerBulletController bullet)
    {
        float finalDamage = bullet.damage;

        if (playerDashWithBuff != null && playerDashWithBuff.IsBuffActive())
        {
            finalDamage = playerDashWithBuff.ApplyDamageReduction(bullet.damage);
        }

        health -= (int)finalDamage;
        healthBar.value = health;

        if (health <= 0)
        {
            bullet.owner.AddScore(1);
            PlayerDied();
        }
    }

    public void ApplyAoEDamage(float damage, Photon.Realtime.Player caster)
    {
        health -= (int)damage;
        healthBar.value = health;
        Debug.Log($"[Multiplayer] {playerNameText.text} took {damage} AoE damage. Remaining health: {health}");
    
        if (health <= 0)
        {
            if (caster != null && caster != photonView.Owner)
            {
                caster.AddScore(1);
                Debug.Log($"[Multiplayer] {playerNameText.text} died. {caster.NickName} scores!");
            }
            PlayerDied();
        }
    }

    void PlayerDied()
{
    if (!photonView.IsMine)
        return;

    // Get the player's team ID
    int teamID = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamID"];

    // Find the spawn point from RoomManager
    RoomManager roomManager = FindObjectOfType<RoomManager>();
    if (roomManager != null && roomManager.teamSpawnPoints.Length > teamID)
    {
        transform.position = roomManager.teamSpawnPoints[teamID].position;
        transform.rotation = roomManager.teamSpawnPoints[teamID].rotation;
    }

    // Reset health
    health = 100;
    healthBar.value = health;

    Debug.Log($"{playerNameText.text} respawned at team {teamID} spawn point.");
}


    void OnEnable()
    {
        Debug.Log($"{GetType().Name} enabled");
    }

    void OnDisable()
    {
        Debug.Log($"{GetType().Name} disabled");
    }
}
