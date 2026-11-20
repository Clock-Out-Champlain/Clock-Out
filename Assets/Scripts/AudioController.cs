using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AudioController : MonoBehaviour
{
    [Header("Sounds")]
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
    public AudioClip bodyThump;
    public AudioClip neckSnap;
    public AudioClip ramJumpscare;
    public AudioClip malJumpscare;

    public AudioClip placeholder;
    [Header("Red filter")]
    public GameObject redFilter;
    [Header("Blur Settings")]
    public Volume blurVolume;
    [Header("Camera")]
    public float camStartX;
    public float camStartY;
    public bool camMoveStarted;
    public bool camMoveFinished;
    public bool neckSnapStarted;
    public bool neckSnapPlayed;
    public bool neckSnapFinished;
    static bool eyeDeathSoroutineStarted = false;
    float camMoveTimer = 0;
    public GameObject camera;
    [Header("Doorman voice clips")]
    public float doormanVoiceClipCooldown;
    public int doormanVoiceClipCooldownLength;
    bool jumpscared = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        redFilter.SetActive(false);
        blurVolume.enabled = false;
        camStartX = camera.transform.position.x;
        camStartY = camera.transform.position.y;
    }
    void Update()
    {
        if (doormanVoiceClipCooldown > 0)
        {
            doormanVoiceClipCooldown -= Time.deltaTime;
        }
        if (camMoveStarted)
        {
            camMoveTimer += 2 * Time.deltaTime;
            MoveCameraInCircleXY(camMoveTimer);
        }
        if (camMoveFinished && !eyeDeathSoroutineStarted)
        {
            eyeDeathSoroutineStarted = true;
            StartCoroutine(DeathEyesore());
        }
        if (neckSnapStarted)
        {
            if (!neckSnapPlayed)
            {
                audioSource.PlayOneShot(neckSnap, 1f);
                neckSnapPlayed = true;
            }
            SnapNeck();
        }
        if (neckSnapFinished)
        {
            camMoveStarted = true;
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
    public IEnumerator DeathEyesore()
    {
        audioSource.PlayOneShot(bodyThump, 1f);
        yield return new WaitForSeconds(bodyThump.length);
        SceneManager.LoadScene("DeathScreenEyesore");
    }
    public IEnumerator Jumpscare(string entity)
    {
        var tablet = FindAnyObjectByType<TabletController>();
        tablet.CloseTablet();
        if(!jumpscared)
        {
            jumpscared = true;
            if (entity == "Doorman")
            {
                audioSource.PlayOneShot(gunshot, 1f);
                redFilter.SetActive(true);
                blurVolume.enabled = true;
                yield return new WaitForSeconds(gunshot.length);
                SceneManager.LoadScene("DeathScreenDoorman");
            }
            else if (entity == "Eyesore")
            {
                camera.transform.Rotate(0f, 180 * Time.deltaTime, 0f);
                redFilter.SetActive(true);
                blurVolume.enabled = true;
                neckSnapStarted = true;
            }
            else if (entity == "Malwerewolf")
            {
                audioSource.PlayOneShot(malJumpscare, 1f);
                yield return new WaitForSeconds(malJumpscare.length);
                SceneManager.LoadScene("DeathScreenMalWerewolf");
            }
            else if (entity == "Rampage")
            {
                audioSource.PlayOneShot(ramJumpscare, 1f);
                yield return new WaitForSeconds(ramJumpscare.length);
                SceneManager.LoadScene("DeathScreenRampage");
            }
        }
    }
    public void SnapNeck()
    {
        float turnSpeed = 360;
        Quaternion targetRotation = Quaternion.Euler(0f, 90, 0f);
        camera.transform.rotation = Quaternion.RotateTowards(camera.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        if (camera.transform.rotation == targetRotation)
        {
            neckSnapFinished = true;
        }
    }
    public void MoveCameraInCircleXY(float t)
    {
        t = t * t * 0.75f;
        float camAngleZ = -t * (180 / Mathf.PI);
        float x = camStartX + camStartY * Mathf.Sin(t);
        float y = camStartY * Mathf.Cos(t);
        camera.transform.position = new Vector3(x, y, camera.transform.position.z);
        camera.transform.eulerAngles = new Vector3(0f, 90f, camAngleZ);
        if (y <= 1.5f)
        {
            camMoveFinished = true;
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
