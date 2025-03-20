using System.Collections.Generic;
using UnityEngine;

public class TriggerDice : MonoBehaviour
{
    public int diceValue = 0;
    private Dictionary<int, float> faceTimers = new Dictionary<int, float>(); // ✅ 각 면의 접촉 시간 저장

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            int faceNumber = GetFaceNumberFromName(gameObject.name);

            if (!faceTimers.ContainsKey(faceNumber))
            {
                faceTimers[faceNumber] = 0f; // ✅ 처음 닿은 면은 시간 초기화
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            int faceNumber = GetFaceNumberFromName(gameObject.name);

            if (faceTimers.ContainsKey(faceNumber))
            {
                faceTimers[faceNumber] += Time.deltaTime; // ✅ 닿아 있는 시간 증가
            }

            // ✅ 가장 오래 닿아 있는 면 찾기
            int bestFace = diceValue;
            float maxTime = faceTimers.ContainsKey(diceValue) ? faceTimers[diceValue] : 0;

            foreach (var face in faceTimers)
            {
                if (face.Value > maxTime)
                {
                    maxTime = face.Value;
                    bestFace = face.Key;
                }
            }

            diceValue = bestFace;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            int faceNumber = GetFaceNumberFromName(gameObject.name);
            if (faceTimers.ContainsKey(faceNumber))
            {
                faceTimers.Remove(faceNumber); // 바닥에서 떨어지면 해당 면 제거
            }

            // ✅ 다른 면들의 시간이 적어지면 diceValue 초기화
            if (faceTimers.Count == 0)
            {
                diceValue = 0;
            }
        }
    }

    private int GetFaceNumberFromName(string name)
    {
        // ✅ "DiceFace_1" 같은 이름에서 숫자 부분만 추출
        if (name.StartsWith("DiceFace_"))
        {
            string number = name.Replace("DiceFace_", "");
            if (int.TryParse(number, out int result))
            {
                return result;
            }
        }
        return 0;
    }
}
