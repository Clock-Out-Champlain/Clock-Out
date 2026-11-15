using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip metallicThump;
    public AudioClip buttonWork;
    public AudioClip buttonFail;
    public AudioClip footsteps;
    public AudioClip elecZap;
    public AudioClip powerOut;
    public AudioClip altPowerOut;
    public AudioClip gunLoad;
    public AudioClip gunshot;
    public AudioClip placeholder;

    public float doormanVoiceClipCooldown;
    public int doormanVoiceClipCooldownLength;
    bool jumpscared = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (doormanVoiceClipCooldown > 0)
        {
            doormanVoiceClipCooldown -= Time.deltaTime;
        }
    }
    public IEnumerator PlayOneShotDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip);
    }
    public void PlayDoormanVoiceClip()
    {
        if (doormanVoiceClipCooldown <= 0)
        {
            audioSource.PlayOneShot(gunLoad, 1f);
            StartCoroutine(PlayOneShotDelayed(placeholder, gunLoad.length));
            doormanVoiceClipCooldown = doormanVoiceClipCooldownLength;
        }
        else
        {
            audioSource.PlayOneShot(gunLoad, 1f);
        }
    }
    public IEnumerator Jumpscare(string entity)
    {
        if (!jumpscared)
        {
            jumpscared = true;
            if (entity == "Doorman")
            {
                audioSource.PlayOneShot(gunshot, 1f);
                yield return new WaitForSeconds(gunshot.length);
                SceneManager.LoadScene("DeathScreenDoorman");
            }
        }
    }
    public void PowerDrain()
    {
        audioSource.PlayOneShot(elecZap, 0.5f);
    }
    public void PowerOut()
    {
        audioSource.PlayOneShot(powerOut, 0.5f);
    }
    public void PlayDoorThump()
    {
        audioSource.PlayOneShot(metallicThump, 1f);
    }

    public void DoorButtonPress(bool buttonMoveDoor)
    {
        if (buttonMoveDoor)
        {
            audioSource.PlayOneShot(buttonWork, 1f);
        }
        else
        {
            audioSource.PlayOneShot(buttonFail, 1f);
        }
    }

    public void playFootSteps(bool heavyFootsteps)
    {
        //float footstepsLength = footsteps.clip.length;
        if (heavyFootsteps)
        {
            audioSource.PlayOneShot(footsteps, 0.1f);
        }
        else
        {
            audioSource.PlayOneShot(footsteps, 0.025f);
        }
    }
}
