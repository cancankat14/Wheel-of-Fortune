using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Wheel
{
    public class InventoryManager : Singleton<InventoryManager>, IManager
    {
        private readonly Dictionary<Reward, int> _stash = new();
        private readonly List<Pickup> _stashHistory = new();
        private readonly Dictionary<Reward, int> _bank = new();

        // gold tracked separately so multiple gold rewards all sum up
        private int _goldStash = 0;
        private int _goldBank = 0;

        public event Action OnStashChanged;
        public event Action OnBankChanged;

        [Serializable]
        public struct Pickup
        {
            public Reward reward;
            public int amount;

            public Pickup(Reward r, int a)
            {
                reward = r;
                amount = a;
            }
        }

        public int GetGoldTotal() => _goldBank + _goldStash;
        public bool TrySpendGoldAny(int amount)
        {
            amount = Mathf.Max(0, amount);
            int remaining = amount;

            int fromBank = Mathf.Min(_goldBank, remaining);
            _goldBank -= fromBank;
            remaining -= fromBank;

            if (remaining > 0)
            {
                if (_goldStash < remaining)
                {
                    OnBankChanged?.Invoke();
                    OnStashChanged?.Invoke();
                    return false;
                }

                _goldStash -= remaining;
            }

            OnBankChanged?.Invoke();
            OnStashChanged?.Invoke();
            return true;
        }
        public void ClearStash()
        {
            _stash.Clear();
            _stashHistory.Clear();
            _goldStash = 0; // <- also clear gold stash
            OnStashChanged?.Invoke();
        }
        public void AddToStash(Reward reward, int amount)
        {
            if (!reward) return;

            switch (reward.type)
            {
                case RewardType.Bomb:
                    return;

                case RewardType.Gold:
                    amount = Mathf.Max(0, amount);
                    _goldStash += amount; // <- gold goes to gold stash
                    _stashHistory.Add(new Pickup(reward, amount)); // still show individual pickups
                    OnStashChanged?.Invoke();
                    return;

                case RewardType.Currency:
                case RewardType.Chest:
                    amount = Mathf.Max(0, amount);
                    break;

                case RewardType.Cosmetic:
                    amount = 1;
                    break;

                case RewardType.Upgrade:
                    amount = Mathf.Max(1, amount);
                    break;
            }

            if (_stash.TryGetValue(reward, out var current))
                _stash[reward] = current + amount;
            else
                _stash[reward] = amount;

            _stashHistory.Add(new Pickup(reward, amount));
            OnStashChanged?.Invoke();
        }

        public IReadOnlyList<(Reward reward, int amount)> GetStashEntries()
        {
            var list = new List<(Reward, int)>(_stash.Count);
            foreach (var kv in _stash) list.Add((kv.Key, kv.Value));
            return list;
        }

        public IReadOnlyList<Pickup> GetStashHistory() => _stashHistory;

        public void BankStash()
        {
            // bank all non-gold
            foreach (var kv in _stash)
            {
                if (!kv.Key) continue;
                _bank[kv.Key] = (_bank.TryGetValue(kv.Key, out var cur) ? cur : 0) + kv.Value;
            }

            _stash.Clear();

            // bank gold
            if (_goldStash > 0)
            {
                _goldBank += _goldStash;
                _goldStash = 0;
            }

            _stashHistory.Clear();
            OnStashChanged?.Invoke();
            OnBankChanged?.Invoke();
        }
        public int GetGoldStash() => _goldStash; // for UI previews if needed
    }
}