using UnityEngine;

namespace LOS
{
    /// <summary>
    /// Disables a gameobjects renderer if the object is outside the line of sight
    /// </summary>
    [RequireComponent(typeof(LOS.LOSCuller))]
    [AddComponentMenu("Line of Sight/LOS Object Hider")]
    public class TankHider : MonoBehaviour
    {
        private TankCuller m_Culler;
        private LOSVisibilityInfo m_VisibilityInfo;

        private void OnEnable()
        {
            m_Culler = GetComponent<TankCuller>();
        }

        private void Start()
        {
            //_chassis = transform.Find("TankChassis");
            //_left = transform.Find("TankTracksLeft");
            //_right = transform.Find("TankTracksRight");
            //_turret = transform.Find("TankTurret");
        }

        private void LateUpdate()
        {
            if (m_Culler.enabled)
            {
                foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
                    r.enabled = m_Culler.Visibile;
                }
            }
        }
    }
}