using UnityEngine;

namespace WarriorQuest.Audio
{

    public class AudioManager : MonoBehaviour
    {
        //싱글턴(Singleton) 디자인 패턴
        public static AudioManager Instance { get; private set; } //내부에서만 세팅 가능

        //오디오 데이터 SO
        public AudioDataSO audioData;

        //오디오 소스 컴포넌트 변수
        private AudioSource bgmAudioSource;
        private AudioSource sfxPlayerAudioSource;
        private AudioSource sfxEnemyAudioSource;



        #region 유니티 생명주기

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); //씬이 변경되어도 해당 gameObject를 제거하지 않는다.
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
            sfxEnemyAudioSource = this.gameObject.AddComponent<AudioSource>();

            sfxPlayerAudioSource.loop = false;
            sfxEnemyAudioSource.loop = false;

            //게임 시작시 BGM 실행
            PlayBGM(audioData.battleBGM);
        }

        #endregion

        #region 공통 메서드

        public void PlayBGM(AudioClip clip)
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