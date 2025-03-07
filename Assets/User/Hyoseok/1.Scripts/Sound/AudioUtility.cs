using UnityEngine;
using UnityEngine.Audio;
using BS.Audio;

namespace BS.Utility
{
    /// <summary>
    /// 이 클래스는 오디오 관련 유틸리티 기능을 제공
    /// AudioManager와 AudioMixer를 통해 오디오 그룹을 관리하고, 볼륨을 조절하거나,
    /// 특정 위치에서 사운드 효과를 재생하는 기능을 포함.
    /// </summary>
    public class AudioUtility
    {
        // AudioManager의 인스턴스를 저장하는 정적 변수.
        // AudioManager는 모든 오디오 믹싱 및 그룹 관리 기능을 담당합니다.
        static AudioManager s_AudioManager;

        // 게임 내에서 사용할 오디오 그룹을 정의합니다.
        public enum AudioGroups
        {
            Music,
            Sound,
            Skill,
            Explosion,
        }

        // 특정 위치에서 사운드 효과(AudioClip)를 생성하고 재생합니다.
        public static AudioSource CreateSFX(AudioClip clip, Vector3 position, AudioGroups audioGroup, float spatialBlend = 0f, float rolloffDistanceMin = 1f, float maxDistance = 15f, float pitch = 1.0f)
        {
            // 새 오브젝트 생성: 사운드 효과를 재생할 임시 오브젝트를 만듭니다.
            GameObject impactSfxInstance = new GameObject("SFX_" + clip.name);
            // 오브젝트 위치 설정 : 효과음이 발생한 곳에 생성
            impactSfxInstance.transform.position = position;

            //Hierarchy / SFXContainer 안으로 오브젝트를 생성
            impactSfxInstance.transform.SetParent(GameObject.Find("SFXContainer").transform);

            // AudioSource 컴포넌트를 추가하여 사운드를 재생합니다.
            AudioSource source = impactSfxInstance.AddComponent<AudioSource>();
            // 재생할 클립 설정
            source.clip = clip;
            // 3D 공간 사운드 블렌딩 설정 (0 이면 2D, 1 이면 3D)
            source.spatialBlend = spatialBlend;
            // 소리 감쇠가 시작되는 최소 거리
            source.minDistance = rolloffDistanceMin;
            source.maxDistance = maxDistance;

            // Custom Rolloff Curve 설정 
            // └ 이번 프로젝트에서는 플레이어 위치가 maxDistance 이상 멀어지게 되면
            // 소리가 아예 들리지 않도록 설정
            AnimationCurve customRolloff = new AnimationCurve();
            customRolloff.AddKey(0, 1f);              // 0 거리에서 볼륨 100%
            customRolloff.AddKey(rolloffDistanceMin, 1f);  // MinDistance까지는 유지
            customRolloff.AddKey(maxDistance, 0f);    // MaxDistance에서 볼륨 0%

            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customRolloff);

            // 출력 오디오 믹서 그룹 설정
            source.outputAudioMixerGroup = GetAudioGroup(audioGroup);

            // 사운드 재생 속도
            source.pitch = pitch;

            // 사운드 재생 시작
            source.Play();
            // 일정 시간이 지나면 오브젝트를 자동으로 제거하는 컴포넌트 추가
            TimedSelfDestruct timedSelfDestruct = impactSfxInstance.AddComponent<TimedSelfDestruct>();
            // 클립 길이만큼 유지 후 제거
            timedSelfDestruct.LifeTime = clip.length;
            return source;
        }

        // 오디오 그룹(AudioMixerGroup)을 반환합니다.
        public static AudioMixerGroup GetAudioGroup(AudioGroups group)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindAnyObjectByType<AudioManager>();

            // AudioManager를 통해 지정된 그룹 이름의 믹서 그룹을 찾습니다.
            var groups = s_AudioManager.FindMatchingGroups(group.ToString());
            if (groups.Length > 0)
                return groups[0];

            Debug.LogWarning("Didn't find audio group for " + group.ToString());
            return null;
        }

        /*
            Mathf.Log10을 사용하는 이유는 슬라이더의 선형 값을 로그 스케일로 변환하여 데시벨 단위의 볼륨 설정에 맞추고, 사용자에게 더 자연스러운 볼륨 변화를 제공
            Mathf.Log10(flaot value)함수는 value가 0일때 -Infinity를 반환하므로 슬라이더 값에서 Min Value를 0.001로 설정후 사용 또는 코드에 처리해줘야함
        */
        public static void SetMasterVolume(float value)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindAnyObjectByType<AudioManager>();

            if (value <= 0)
                value = 0.001f;
            float valueInDb = Mathf.Log10(value) * 20;

            s_AudioManager.SetFloat("MASTER", valueInDb);
        }

        public static float GetMasterVolume()
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindAnyObjectByType<AudioManager>();

            s_AudioManager.GetFloat("MASTER", out var valueInDb);
            return Mathf.Pow(10f, valueInDb / 20.0f);
        }

        public static void SetBGMVolume(float value)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindAnyObjectByType<AudioManager>();

            if (value <= 0)
                value = 0.001f;
            float valueInDb = Mathf.Log10(value) * 20;

            s_AudioManager.SetFloat("BGM", valueInDb);
        }

        public static float GetBGMVolume()
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindAnyObjectByType<AudioManager>();

            s_AudioManager.GetFloat("BGM", out var valueInDb);
            return Mathf.Pow(10f, valueInDb / 20.0f);
        }

        public static void SetSFXVolume(float value)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindAnyObjectByType<AudioManager>();

            if (value <= 0)
                value = 0.001f;
            float valueInDb = Mathf.Log10(value) * 20;

            s_AudioManager.SetFloat("EFFECT", valueInDb);
        }

        public static float GetSFXVolume()
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindAnyObjectByType<AudioManager>();

            s_AudioManager.GetFloat("EFFECT", out var valueInDb);
            return Mathf.Pow(10f, valueInDb / 20.0f);
        }

        public static float GetVolume(string parameterName)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindAnyObjectByType<AudioManager>();

            s_AudioManager.GetFloat(parameterName, out var valueInDb);
            //Debug.Log($"{valueInDb} 1");
            //Debug.Log($"{Mathf.Pow(10f, valueInDb / 20.0f)} 2");
            return Mathf.Pow(10f, valueInDb / 20.0f);
        }


        public static void SetVolume(float value, string parameterName)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindAnyObjectByType<AudioManager>();

            if (value <= 0)
                value = 0.001f;

            float valueInDb = Mathf.Log10(value) * 20;

            s_AudioManager.SetFloat(parameterName, valueInDb);
        }
    }
}
