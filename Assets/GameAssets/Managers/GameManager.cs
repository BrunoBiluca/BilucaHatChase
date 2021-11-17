using Assets.UnityFoundation.Code.TimeUtils;
using Assets.UnityFoundation.UI.Menus.GameOverMenu;
using Assets.UnityFoundation.UI.ProgressElements.ProgressCircle;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager
    : MonoBehaviourPunCallbacksSingleton<GameManager>, IPunObservable
{
    [SerializeField] private GameOverMenu gameOverMenu;
    [SerializeField] private ProgressCircle progressCircle;

    public CooldownIndicator RepulsionCooldownIndicator;

    [Header("Stats")]
    public bool gameEnded = false;
    public float timeToWin;
    public float invincibleDuration;

    [Header("Players")]
    public string playerPrefabLocation;
    public Transform[] spawnPoints;
    public int playerWithHat;
    private int playersInGame;

    public List<HatPlayerController> players;

    private Timer invincibleTimer;
    private float hatHolderTime;

    void Start()
    {
        playerWithHat = 1;
        players = new List<HatPlayerController>();

        invincibleTimer = new Timer(invincibleDuration);

        gameOverMenu.Setup("Go to Main Menu", GoBackToMenu);
        progressCircle.Setup(timeToWin);

        photonView.RPC(nameof(UpdatePlayersInGame), RpcTarget.AllBuffered);
    }

    private void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if(hatHolderTime >= timeToWin && !gameEnded)
            {
                gameEnded = true;
                photonView.RPC(nameof(WinGame), RpcTarget.All, playerWithHat);
            }
        }

        if(photonView.IsMine)
            hatHolderTime += Time.deltaTime;

        progressCircle.Display(hatHolderTime);
    }

    [PunRPC]
    private void UpdatePlayersInGame()
    {
        playersInGame++;
        if(playersInGame == PhotonNetwork.PlayerList.Length)
            SpawnPlayer();
    }

    [PunRPC]
    void WinGame(int playerId)
    {
        gameEnded = true;
        gameOverMenu.Show(
            $"Winner is {PhotonNetwork.PlayerList[playerId - 1].NickName}"
        );
    }

    void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.Instance.ChangeScene("main_menu_scene");
    }

    private void SpawnPlayer()
    {
        var playerObj = PhotonNetwork.Instantiate(
            playerPrefabLocation,
            spawnPoints[Random.Range(0, spawnPoints.Length)].position,
            Quaternion.identity,
            0
        );

        var hatPlayer = playerObj.GetComponent<HatPlayerController>();

        hatPlayer.photonView.RPC(
            nameof(HatPlayerController.Initialize),
            RpcTarget.All,
            PhotonNetwork.LocalPlayer
        );

        if(hatPlayer.Id == playerWithHat)
            photonView.RPC(nameof(ChangeHats), RpcTarget.All, hatPlayer.Id);
    }

    public HatPlayerController GetPlayer(int playerId)
        => players.First(x => x.Id == playerId);

    public HatPlayerController GetPlayer(GameObject playerObject)
        => players.First(x => x.gameObject == playerObject);

    public bool IsPlayerWithHat(GameObject playerObj)
        => GetPlayer(playerObj).Id == playerWithHat;

    public void TryCanGetHat(int id)
    {
        if(!invincibleTimer.Completed)
            return;

        photonView.RPC(nameof(ChangeHats), RpcTarget.All, id);
    }

    [PunRPC]
    private void ChangeHats(int id)
    {
        GetPlayer(playerWithHat).RemoveHat();
        GetPlayer(id).GiveHat();
        playerWithHat = id;
        hatHolderTime = 0f;
        invincibleTimer.Start();
    }

    public void ApplyRepulsionForce(int id)
    {
        photonView.RPC(
            nameof(ApplyRepulsionForceRPC),
            RpcTarget.All,
            id
        );
    }

    [PunRPC]
    private void ApplyRepulsionForceRPC(int id)
    {
        StartCoroutine(RepulsionSkill(GetPlayer(id)));
    }

    private IEnumerator RepulsionSkill(HatPlayerController player)
    {
        player.repulsionFX.gameObject.SetActive(true);
        player.repulsionFX.Play();

        yield return new WaitForSeconds(1f);

        var colliders = Physics.OverlapSphere(
            player.transform.position, player.repulsionRadius
        );
        foreach(var hit in colliders)
        {
            if(hit.gameObject == player.gameObject)
                continue;

            if(!hit.TryGetComponent(out Rigidbody rg))
                continue;

            rg.AddForce(
                (rg.position - player.transform.position).normalized 
                * player.repulsionForce,
                ForceMode.Impulse
            );
        }

        yield return new WaitForSeconds(1f);

        player.repulsionFX.gameObject.SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(hatHolderTime);
        }
        else if(stream.IsReading)
        {
            hatHolderTime = (float)stream.ReceiveNext();
        }
    }
}
