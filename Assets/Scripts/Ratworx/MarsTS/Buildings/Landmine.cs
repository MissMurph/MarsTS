using MarsTS.Events;
using MarsTS.Units;
using UnityEngine;

namespace MarsTS.Buildings
{
    public class Landmine : Building
    {
        private Explosion _explosion;

        [SerializeField] private int damage;

        protected override void Awake()
        {
            base.Awake();

            _explosion = transform.Find("Explosion").GetComponent<Explosion>();
            _explosion.gameObject.SetActive(false);
        }

        public void Detonate()
        {
            _explosion.Init(damage, Owner, this);
            _explosion.gameObject.SetActive(true);
            _explosion.transform.SetParent(null, true);

            Bus.Global(new UnitDeathEvent(Bus, this));

            Destroy(gameObject);
        }

        protected override void OnVisionUpdate(EntityVisibleEvent @event)
        {
            bool visible = @event.Visible;

            foreach (GameObject hideable in visionObjects)
            {
                hideable.SetActive(visible);
            }
        }
    }
}