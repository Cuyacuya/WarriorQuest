using UnityEngine;

namespace WarriorQuest.Audio
{
    
    public class AudioManager : MonoBehaviour
    {
        //싱글턴 디자인 패턴
        public static AudioManager Instance { get; private set; }
    
        //오디오 소스 컴포넌트 변수
        private AudioSource bgmAudioSource;
        private AudioSource sfxPlayerAudioSource;
        private AudioSource sfxEnemyAudioSource;
    
        //오디오 데이터 SO
        [SerializeField] public AudioDataSO audioData;

        #region 유니티 생명주기
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            //컴포넌트 생성
            bgmAudioSource = this.gameObject.AddComponent<AudioSource>();
            sfxPlayerAudioSource = this.gameObject.AddComponent<AudioSource>();
            sfxEnemyAudioSource =  gameObject.AddComponent<AudioSource>();
        
            sfxPlayerAudioSource.loop = false;
            sfxEnemyAudioSource.loop = false;
            //게임 시작 시 BGM 실행
            PlayerBGM(audioData.battleBGM);
        }
    
        #endregion

        #region 공통 메서드

        public void PlayerBGM(AudioClip clip)
        {
            bgmAudioSource.clip = clip;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }

        public void PlayerSFX(AudioClip clip)
        {
            sfxPlayerAudioSource.PlayOneShot(clip);        
        }

        public void StopPlayerSFX()
        {
            sfxPlayerAudioSource.Stop();
        }

        public void EnemySFX(AudioClip clip)
        {
            sfxEnemyAudioSource.PlayOneShot(clip); 
        }

        public void StopEnemySFX()
        {
            sfxEnemyAudioSource.Stop();
        }
        #endregion
    }

}