using System;
using System.Collections.Generic;
using System.Text;

namespace CLPVirtual
{

    abstract public class DefinicaoClpPadrao
    {
        abstract public List<Sinal> Variaveis { get; set; }
        abstract public void RunProcessCLP();


    }
    public class Sinal
    {
        public string sEndereco { get; set; }

        public string sNome { get; set; }

        public object oValor { get; set; }

        public Sinal(string sEnd, string sNome)
        {
            this.sEndereco = sEnd;
            this.sNome = sNome;
        }
    }
}
