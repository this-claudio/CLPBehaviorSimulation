using System;
using System.Collections.Generic;
using CLPVirtual;
using OpcUaCommon;
using Opc.Ua.Client;
using Opc.Ua;
using Opc.Ua.Configuration;

namespace MachineBehavior
{
    public class CLPPadrao : DefinicaoClpPadrao
    {
        public override List<Sinal> Variaveis { get; set; }

        public CLPPadrao()
        {
           
        }
        public override void RunProcessCLP()
        {
            throw new NotImplementedException();
        }

    }

}
