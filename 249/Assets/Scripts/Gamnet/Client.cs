using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamnet
{
    public class Client : MonoBehaviour
    {
        public ClientSession session = new ClientSession();
        private void Update()
        {
            session.Update();
        }
    }
}
