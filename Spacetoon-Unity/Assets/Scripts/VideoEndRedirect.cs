using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoEndRedirect : MonoBehaviour
{
    // Référence au VideoPlayer
    public VideoPlayer videoPlayer;

    // Nom de la scène vers laquelle rediriger
    public string nextSceneName;

    void Start()
    {
        // Vérifie si le VideoPlayer est assigné
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Ajoute un listener à l'événement de fin de vidéo
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    // Fonction appelée à la fin de la vidéo
    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene("menuDuJeu");
    }
}
