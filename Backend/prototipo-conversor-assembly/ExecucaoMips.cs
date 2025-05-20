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
    public BancoRegistradores bancoDeRegistradores;
    public List<string> memoriaPrograma = new();
    public int pc { get; set; } = 0;

    public ExecucaoMips(List<string> comandos, ref BancoRegistradores bancoRegistradores) 
    { 
        Comandos = comandos;
        bancoDeRegistradores = bancoRegistradores;
    }

    public void Executar()
    {
        foreach (var com in Comandos)
        {
            Console.WriteLine($"\n============================|PC={pc}|============================\n");

            memoriaPrograma.Add(com);

            //Pega a instrução que foi escolhida
            string instrucao = com.Split(" ")[0];
            
            //pega os registradores e valores
            string[] regs = com.Split(" ")[1].Split(",");


            Console.WriteLine("Instrução escolhida: " + instrucao);
            
            if(instrucao == "add")
            {
                //Ele está adcionando o valor dos registradores se for um registrador, e adiciona apenas o valor numérico se for só um número
                bancoDeRegistradores.Valores[regs[0]] = (regs[1][0] == '$' ? bancoDeRegistradores.Valores[regs[1]] : Convert.ToInt32(regs[1])) + (regs[2][0] == '$' ? bancoDeRegistradores.Valores[regs[2]] : Convert.ToInt32(regs[2]));
            }

            Console.WriteLine($"O valor final de {regs[0]} é {bancoDeRegistradores.Valores[regs[0]]}");

            ExibirRegistradores();
            ExibirMemoriaPrograma();
            pc++;
        }   
    }

    public void ExibirRegistradores()
    {
        Console.WriteLine("REGISTRADORES DE CPU");
        foreach(var r in bancoDeRegistradores.Registradores)
        {
            Console.WriteLine($"\t|{r} : {bancoDeRegistradores.Valores[r]} |");
        }
    }

    public void ExibirMemoriaPrograma()
    {
        Console.WriteLine("MEMÓRIA DE PROGRAMA");
        foreach (var r in memoriaPrograma)
        {
            Console.WriteLine($"\t|{r}|");
        }
    }
}

