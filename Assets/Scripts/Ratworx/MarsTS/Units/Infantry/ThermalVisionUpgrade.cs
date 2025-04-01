using Ratworx.MarsTS.Units.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units.Infantry
{
    public class ThermalVisionUpgrade : MonoBehaviour
    {
        private ISelectable _parent;
        private StealthSensor _vision;

        [FormerlySerializedAs("researchKey")] [SerializeField]
        private string _researchKey = "thermalVision";

        private void Awake()
        {
            _parent = GetComponentInParent<ISelectable>();
            _vision = GetComponent<StealthSensor>();
        }

        private void Start()
        {
            if (_vision != null) _vision.Detecting = true;
        }
    }
}