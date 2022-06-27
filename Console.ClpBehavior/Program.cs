using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Timers;
using CLPVirtual;
using MachineBehavior;
using System.Reflection;

namespace Console.ClpBehavior
{
    class Program
    {
        static Timer ConfereEstadoVariaveis { get; set; }
        static DefinicaoClpPadrao oMachineClp { get; set; }
        static void Main(string[] args)
        {
            
            oMachineClp = (DefinicaoClpPadrao)CreateCLPInstance();

            if (ConfereEstadoVariaveis == null)
            ConfereEstadoVariaveis = new Timer();
            ConfereEstadoVariaveis.Interval = 1000;
            ConfereEstadoVariaveis.Enabled = true;
            ConfereEstadoVariaveis.Elapsed += ConfereEstadoVariaveis_Elapsed;
            ConfereEstadoVariaveis.Start();

            
            
            System.Console.ReadKey();
        }

        private static void ConfereEstadoVariaveis_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                oMachineClp.RunProcessCLP();
            }
            catch (Exception Falha)
            {
                System.Console.WriteLine(Falha.Message);
            }
            
        }

        private static object CreateCLPInstance()
        {
            try
            {
                string TemplateName = ConfigurationManager.AppSettings["TemplateCLP"];
                Assembly MyDALL = Assembly.Load("AllBehavior"); // DALL is name of my dll
                Type MyLoadClass = MyDALL.GetType("MachineBehavior." + TemplateName);
                return Activator.CreateInstance(MyLoadClass);
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return new CLPPadrao();
            }

            
        }

    }

    

    
}
