using UnityEngine;

namespace ScaryIslands.Game
{
    public enum ObjectiveItem { ChapelKey, SaltBell }
    public sealed class ObjectivePickup : MonoBehaviour
    {
        [SerializeField] private ObjectiveItem item;
        public void Collect()
        {
            if (RunState.Instance == null) return;
            if (item == ObjectiveItem.ChapelKey) RunState.Instance.TakeKey();
            else RunState.Instance.TakeBell();
            gameObject.SetActive(false);
        }
        private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) Collect(); }
    }
}

