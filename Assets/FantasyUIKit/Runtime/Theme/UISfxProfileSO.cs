using UnityEngine;

namespace FantasyUI.Theme
{
    public enum UISfxCue
    {
        Click,
        Hover,
        ScrollOpen,
        StoneOpen,
        LeatherOpen,
        Close,
        Toggle,
        CoinClink,
        Victory,
        Defeated,
        Equip
    }

    [CreateAssetMenu(menuName = "Fantasy UI/SFX Profile", fileName = "UISfxProfile_Default")]
    public class UISfxProfileSO : ScriptableObject
    {
        [Range(0f, 1f)] public float volume = 0.75f;
        public AudioClip click;
        public AudioClip hover;
        public AudioClip scrollOpen;
        public AudioClip stoneOpen;
        public AudioClip leatherOpen;
        public AudioClip close;
        public AudioClip toggle;
        public AudioClip coinClink;
        public AudioClip victory;
        public AudioClip defeated;
        public AudioClip equip;

        public AudioClip GetClip(UISfxCue cue)
        {
            return cue switch
            {
                UISfxCue.Click       => click,
                UISfxCue.Hover       => hover,
                UISfxCue.ScrollOpen  => scrollOpen,
                UISfxCue.StoneOpen   => stoneOpen,
                UISfxCue.LeatherOpen => leatherOpen,
                UISfxCue.Close       => close,
                UISfxCue.Toggle      => toggle,
                UISfxCue.CoinClink   => coinClink,
                UISfxCue.Victory     => victory,
                UISfxCue.Defeated    => defeated,
                UISfxCue.Equip       => equip,
                _                    => null
            };
        }
    }
}
