
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public void PlaySound()
    {
        Debug.Log("Se ha pulsado el botón de sonido");

        if (GameAudio.Instance != null)
        {
            GameAudio.Instance.PlayButtonSound();
        }
        else
        {
            Debug.LogError("GameAudio.Instance es null");
        }
    }
}
