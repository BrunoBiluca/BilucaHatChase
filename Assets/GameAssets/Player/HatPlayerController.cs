using Assets.UnityFoundation.Code.TimeUtils;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HatPlayerController : MonoBehaviourPunCallbacks
{
    public int Id { get; set; }

    [Header("Info")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;

    [SerializeField] private float repulsionCooldown;
    public float repulsionForce;
    public float repulsionRadius;

    [SerializeField] private CooldownIndicator repulsionCooldownIndicator;

    public Rigidbody rig;
    public Player photonPlayer;

    private GameObject hatObject;

    private Timer repulsionSkillTimer;
    public ParticleSystem repulsionFX;

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        Id = player.ActorNumber;

        hatObject.SetActive(false);
        GameManager.Instance.players.Add(this);

        if(!photonView.IsMine)
            rig.isKinematic = true;
    }

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
        hatObject = transform.Find("hat").gameObject;

        repulsionSkillTimer = new Timer(repulsionCooldown).Start();
        repulsionFX = transform.Find("energy_explosion")
            .GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        repulsionCooldownIndicator.Setup(repulsionSkillTimer);
    }

    void Update()
    {
        if(!photonView.IsMine) return;

        Move();
        if(Keyboard.current.spaceKey.wasPressedThisFrame)
            TryJump();

        if(Keyboard.current.fKey.wasPressedThisFrame)
            UseRepulsionSkill();
    }

    private void UseRepulsionSkill()
    {
        if(!repulsionSkillTimer.Completed)
            return;

        repulsionSkillTimer.Start();

        GameManager.Instance.ApplyRepulsionForce(Id);
    }

    void Move()
    {
        var directionX = Keyboard.current.dKey.isPressed ? 1f : 0f;
        directionX = Keyboard.current.aKey.isPressed ? -1f : directionX;

        var directionZ = Keyboard.current.wKey.isPressed ? 1f : 0f;
        directionZ = Keyboard.current.sKey.isPressed ? -1f : directionZ;

        var x = directionX * moveSpeed;
        var z = directionZ * moveSpeed;
        rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    void TryJump()
    {
        var ray = new Ray(
            transform.position + new Vector3(0f, 0.1f, 0f),
            Vector3.down
        );
        if(Physics.Raycast(ray, 0.7f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void GiveHat() => hatObject.SetActive(true);

    public void RemoveHat() => hatObject.SetActive(false);

    void OnCollisionEnter(Collision collision)
    {
        if(!photonView.IsMine)
            return;

        if(!collision.gameObject.CompareTag("Player"))
            return;

        if(!GameManager.Instance.IsPlayerWithHat(collision.gameObject))
            return;

        GameManager.Instance.TryCanGetHat(Id);
    }

}
