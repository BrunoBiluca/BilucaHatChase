using Assets.UnityFoundation.SceneFader;
using Photon.Pun;

public class PhotonSceneFader : SceneFader
{
    protected override void OnLoadScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}
