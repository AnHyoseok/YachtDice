using UnityEngine;
using BS.Audio;

namespace BS.Utility
{
    /// <summary>
    /// 오디오(SFX[Effect]) 효과 재생시 AudioUtility.cs -> AudioCreateSFX()를 통해
    /// 빈 오브젝트를 만들어 AudioSource(Audio Clip)를 붙여주고 사운드를 재생함
    /// 사운드의 길이만큼 재생을 하고 만들어졌던 빈 오브젝트는 Destroy해주는 역할
    /// LifeTime -> AudioClip.length 를 통해 변수를 초기화함
    /// </summary>
    public class TimedSelfDestruct : MonoBehaviour
    {
        public float LifeTime = 1f;

        float m_SpawnTime;

        void Awake()
        {
            m_SpawnTime = Time.time;
        }

        void Update()
        {
            if (Time.time > m_SpawnTime + LifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
}