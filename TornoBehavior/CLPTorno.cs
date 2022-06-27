using System;
using System.Collections.Generic;
using CLPVirtual;
using OpcUaCommon;
using Opc.Ua.Client;
using Opc.Ua;
using Opc.Ua.Configuration;

namespace MachineBehavior
{
    public class CLPTorno : DefinicaoClpPadrao
    {
        private bool bfirst;

        private int CondicaoInicialFerramenta;

        private int TempoCiclo;

        private string MensagemAtual;
        public ClassOPCClient MachineConnection { get; set; }
        public override List<Sinal> Variaveis { get; set; }

        private DateTime HoroInicioCiclo;

        public CLPTorno()
        {
            Variaveis = new List<Sinal>();
            Variaveis.Add(new Sinal("ns=3;i=1015", "Ciclo"));
            Variaveis.Add(new Sinal("ns=3;i=1017", "CondicaoFerramenta"));
            Variaveis.Add(new Sinal("ns=3;i=1020", "Gatilho"));
            Variaveis.Add(new Sinal("ns=3;i=1018", "Mensagem"));
            Variaveis.Add(new Sinal("ns=3;i=1016", "Parada"));
            Variaveis.Add(new Sinal("ns=3;i=1019", "Processo"));
            Variaveis.Add(new Sinal("ns=3;i=1021", "Comando"));

            bfirst = true;
            TempoCiclo = 20;
            MensagemAtual = "";
            HoroInicioCiclo = new DateTime();
        }
        

        public override void RunProcessCLP()
        {
            if (bfirst)
                Console.WriteLine("Inicializando");

            if(MachineConnection == null)
                MachineConnection = new ClassOPCClient("SQO-053.energpower.com.br", "4840");

            if (MachineConnection.SessioStatus != ClassOPCClient.EstadoConexao.Conectado)
            {
                Console.WriteLine("Conectando");
                try
                {
                    MachineConnection.Conectar();
                    Console.WriteLine("Conectado");
                }
                catch(Exception e)
                {
                    Console.WriteLine("Não foi possivel conectar devido à: "+ e.Message);
                    return;
                }
            }

            if (bfirst)
                Console.WriteLine("Atualizando Valores em Memoria");

            foreach(var Sinal in Variaveis)
            {
                Sinal.oValor = MachineConnection.GetNodeValue(Sinal.sEndereco);
            }

            var Ciclo = Variaveis.Find(x => x.sNome == "Ciclo");
            var Parada = Variaveis.Find(x => x.sNome == "Parada");
            var Gatilho = Variaveis.Find(x => x.sNome == "Gatilho");
            var CondicaoFerramenta = Variaveis.Find(x => x.sNome == "CondicaoFerramenta");
            var Mensagem = Variaveis.Find(x => x.sNome == "Mensagem");
            var Processo = Variaveis.Find(x => x.sNome == "Processo");
            var Comando = Variaveis.Find(x => x.sNome == "Comando");

            if (bfirst)
            {
                MachineConnection.SetNodeValue(Ciclo.sEndereco, false);
                MachineConnection.SetNodeValue(Parada.sEndereco, true);
                MachineConnection.SetNodeValue(Gatilho.sEndereco, false);
                MachineConnection.SetNodeValue(Mensagem.sEndereco, "Aguardando Comando");
                MachineConnection.SetNodeValue(CondicaoFerramenta.sEndereco, 100);
                MachineConnection.SetNodeValue(Processo.sEndereco, 0);
                Console.WriteLine("Aguardando Comando");
                MensagemAtual = "";
                bfirst = false;
            }

            if ((bool)Gatilho.oValor || Comando.oValor.ToString() == "Start")
            {
                MachineConnection.SetNodeValue(Comando.sEndereco, "");
                MachineConnection.SetNodeValue(Gatilho.sEndereco, false);
                if (Convert.ToInt32(CondicaoFerramenta.oValor) <= 0)
                {
                    MensagemAtual = "Não é possível Iniciar o Processo sem uma boa ferramenta";
                }
                else
                {
                    
                    HoroInicioCiclo = DateTime.Now;

                    MachineConnection.SetNodeValue(Ciclo.sEndereco, true);
                    MachineConnection.SetNodeValue(Parada.sEndereco, false);
                    CondicaoInicialFerramenta = Convert.ToInt32(CondicaoFerramenta.oValor);

                    MachineConnection.SetNodeValue(Mensagem.sEndereco, "Processo Iniciado");
                    Console.WriteLine("Processo Iniciado");
                    MensagemAtual = "";
                }
            }

            if ((bool)Ciclo.oValor)
            {
                var TempoDecorrido = (int)(DateTime.Now.Subtract(HoroInicioCiclo)).TotalSeconds;
                var ProcessoValor = (int)(((double)TempoDecorrido / (double)TempoCiclo) * 100.0);
                var CondicaoFerramentaValor = Convert.ToInt32(CondicaoInicialFerramenta - (TempoDecorrido));

                ProcessoValor = ProcessoValor >= 100 ? 100 : ProcessoValor;
                ProcessoValor = ProcessoValor < 0 ? 0 : ProcessoValor;
                CondicaoFerramentaValor = CondicaoFerramentaValor >= 100 ? 100 : CondicaoFerramentaValor;
                CondicaoFerramentaValor = CondicaoFerramentaValor <= 0 ? 0 : CondicaoFerramentaValor;

                MachineConnection.SetNodeValue(CondicaoFerramenta.sEndereco, CondicaoFerramentaValor);
                MachineConnection.SetNodeValue(Processo.sEndereco, ProcessoValor);

                if(TempoDecorrido > TempoCiclo)
                {
                    MachineConnection.SetNodeValue(Ciclo.sEndereco, false);
                    MachineConnection.SetNodeValue(Parada.sEndereco, true);
                    MensagemAtual = "Processo Finalizado";
                }
            }

            if(Convert.ToInt32(CondicaoFerramenta.oValor) <= 0)
            {
                MensagemAtual = "Ferramenta desgastada, deve ser feita a troca";
            }

            if(Comando.oValor.ToString() == "ChangeTool")
            {
                MachineConnection.SetNodeValue(Comando.sEndereco, "");
                MachineConnection.SetNodeValue(CondicaoFerramenta.sEndereco, 100);
                MensagemAtual = "Ferramenta Substituida";
            }

            if (Comando.oValor.ToString().Contains("ChangeTime="))
            {
                var NovoTempo = Comando.oValor.ToString().Remove(0, 11);
                TempoCiclo = Convert.ToInt32(NovoTempo);

                MachineConnection.SetNodeValue(Comando.sEndereco, "");
                MensagemAtual = "Novo Tempo Ciclo configurado: " + NovoTempo + "s";
            }

            if(!string.IsNullOrEmpty(MensagemAtual))
                SendMesageValue(MensagemAtual);

        }

        private void SendMesageValue(string sMen)
        {
            var Mensagem = Variaveis.Find(x => x.sNome == "Mensagem");
            MachineConnection.SetNodeValue(Mensagem.sEndereco, sMen);
            Console.WriteLine(sMen);
        }
    }

}
