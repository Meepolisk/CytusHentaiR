using UnityEngine;
using UnityEditor;
using RTool.Attribute;
using System.Collections.Generic;

namespace RTool.AudioAnalyze
{
    [CreateAssetMenu(fileName = "RhymSetting", menuName = "Rhym Setting", order = 1)]
    public class RhymSetting : ScriptableObject
    {
        [Header("Global config")]
        [SerializeField, Range(0.1f, 2f)]
        private float bfDownScale = 0.5f;
        public float BfDownScale => bfDownScale;
        [SerializeField, Range(1f, 2f)]
        private float bfSafeScale = 1.25f;
        public float BfSafeScale => bfSafeScale;
        [SerializeField, Range(1, 10)]
        private float totalBoost = 5f;
        public float TotalBoost => totalBoost;

        [Header("Band config")]
        [SerializeField]
        private BandProfile SubBass = new BandProfile(20, 60, 1.5f);
        [SerializeField]
        private BandProfile Bass = new BandProfile(60, 200, 1f);
        [SerializeField]
        private BandProfile LowMid = new BandProfile(200, 1000, 1.2f);
        [SerializeField]
        private BandProfile HighMid = new BandProfile(1000, 5000, 8f);
        [SerializeField]
        private BandProfile Treble = new BandProfile(5000, 10000, 13f);
        
        public Dictionary<BandType, BandProfile> GetBands
        {
            get
            {
                var daDick = new Dictionary<BandType, BandProfile>();
                daDick.Add(BandType.SubBass, SubBass);
                daDick.Add(BandType.Bass, Bass);
                daDick.Add(BandType.LowMid, LowMid);
                daDick.Add(BandType.HighMid, HighMid);
                daDick.Add(BandType.Treble, Treble);
                return daDick;
            }
        }
    }
}