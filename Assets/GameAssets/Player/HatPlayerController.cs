using Assets.UnityFoundation.Code.TimeUtils;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class HatPlayerController : MonoBehaviourPunCallbacks
{
    public int Id { get; set; }

    [Header("Info")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;

    public Rigidbody rig;
    public Player photonPlayer;

    private GameObject hatObject;

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
    }

    void Update()
    {
        if(!photonView.IsMine) return;

        Move();
        if(Keyboard.current.spaceKey.wasPressedThisFrame)
            TryJump();
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
