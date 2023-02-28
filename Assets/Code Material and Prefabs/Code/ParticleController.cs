using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Code
{
    public class ParticleController : MonoBehaviour
    {
        private XRGrabInteractable _grabInteractable;
        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
            _particleSystem = GetComponentInChildren<ParticleSystem>();
        }

        public void StartParticleSystem(XRBaseInteractor interactor)
        {
            _particleSystem.Play();
        }

        public void StopParticleSystem(XRBaseInteractor interactor)
        {
            _particleSystem.Stop();
        }
    }
}