using System;
using UnityEngine;

namespace ScaryIslands.Economy
{
    /// <summary>Persistent soft-currency wallet. The only soft currency in Scary Islands is Dots.</summary>
    [DefaultExecutionOrder(-200)]
    public sealed class DotWallet : MonoBehaviour
    {
        private const string BalanceKey = "scary_islands_dots";
        public static DotWallet Instance { get; private set; }

        [SerializeField, Min(0)] private int startingDots = 100;

        public int Balance { get; private set; }
        public event Action<int> Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            new GameObject("Dot Wallet").AddComponent<DotWallet>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!PlayerPrefs.HasKey(BalanceKey))
                PlayerPrefs.SetInt(BalanceKey, startingDots);

            Balance = Mathf.Max(0, PlayerPrefs.GetInt(BalanceKey, startingDots));
            Save();
        }

        public void AddDots(int amount)
        {
            if (amount <= 0) return;
            long next = (long)Balance + amount;
            Balance = next > int.MaxValue ? int.MaxValue : (int)next;
            Save();
            Changed?.Invoke(Balance);
        }

        public bool TrySpendDots(int amount)
        {
            if (amount < 0 || Balance < amount) return false;
            Balance -= amount;
            Save();
            Changed?.Invoke(Balance);
            return true;
        }

        private void Save()
        {
            PlayerPrefs.SetInt(BalanceKey, Balance);
            PlayerPrefs.Save();
        }
    }
}
