using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "Warrior/AudioData")]
public class AudioDataSO : ScriptableObject
{
    [Header("BGM Clips")] 
    public AudioClip mainBGM;
    public AudioClip battleBGM;

    [Header("SFX Clips - Attacks")] 
    public AudioClip playerAttackSFX;
    public AudioClip EnemyAttackSFX;
    
    [Header("SFX Clips - Items")] 
    public AudioClip itemPickupSFX;
}
