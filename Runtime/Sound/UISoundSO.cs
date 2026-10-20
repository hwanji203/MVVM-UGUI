using UnityEngine;

namespace MVVM.UGUI.Sound
{
    /// <summary>
    /// UI 가 내는 소리 하나. Element 는 Play() 만 부른다 — 소리를 어떻게 내는지는 이 SO 를 구현한 쪽이 정한다 (DESIGN 3.5).
    /// 프로젝트는 자기 사운드 시스템으로 이걸 상속해 쓰면 된다.
    /// </summary>
    public abstract class UISoundSO : ScriptableObject
    {
        public abstract void Play();
    }
}
