using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.UI;
using MonsterArena.Interfaces;

namespace MonsterArena
{
    public class MarketZone : MonoBehaviour, IZone
    {
        [SerializeField] private Wallet _wallet = null;
        [SerializeField] private int _coinsPerResource = 0;

        public void Sell(int count)
        {
            _wallet.Add(transform, _coinsPerResource * count);
        }
    }
}
