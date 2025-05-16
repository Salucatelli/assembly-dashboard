using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace prototipo_conversor_assembly;

internal class ExecucaoMips
{
    //Lista com os comandos a serem executados
    public List<string> Comandos = new();
    public BancoRegistradores BancoDeRegistradores;

    public ExecucaoMips(List<string> comandos, ref BancoRegistradores bancoRegistradores) 
    { 
        Comandos = comandos;
        BancoDeRegistradores = bancoRegistradores;
    }

    public void Executar()
    {
        foreach (var com in Comandos)
        {
            //Pega a instrução que foi escolhida
            string instrucao = com.Split(" ")[0];
            
            //pega os registradores e valores
            string[] regs = com.Split(" ")[1].Split(",");


            Console.WriteLine("Instrução escolhida: " + instrucao);
            
            if(instrucao == "add")
            {
                BancoDeRegistradores.Valores[regs[0]] = (regs[1][0] == '$' ? 0 : Convert.ToInt32(regs[1])) + (regs[2][0] == '$' ? 0 : Convert.ToInt32(regs[2]));
            }

            Console.WriteLine($"O valor final de {regs[0]} é {BancoDeRegistradores.Valores[regs[0]]}");

            ExibirRegistradores();
        }   
    }

    public void ExibirRegistradores()
    {
        foreach(var r in BancoDeRegistradores.Registradores)
        {
            Console.WriteLine($"\t|{r} : {BancoDeRegistradores.Valores[r]} |");
        }
    }
}
