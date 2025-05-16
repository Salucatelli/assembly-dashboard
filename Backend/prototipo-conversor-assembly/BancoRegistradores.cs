using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace prototipo_conversor_assembly;

internal class BancoRegistradores
{
    //Lista com os registradores do MIPS
    public List<string> Registradores = new() 
    { 
        "$zero","$at","$v0","$v1","$a0","$a1","$a2","$a3",
        "$t0","$t1","$t2","$t3","$t4","$t5","$t6","$t7",
        "$s0","$s1","$s2","$s3","$s4","$s5","$s6","$s7",
        "$t8","$t9","$k0","$k1","$gp","$sp","$fp","$ra" 
    };

    //Dicionário com os valores armazenados em cada registrador
    public Dictionary<string, int> Valores = new();

    public BancoRegistradores()
    {
        //Inicializa o valor de todos os registradores como 0
        foreach(var r in Registradores)
        {
            Valores.Add(r, 0);
        }
    }

    //altera o valor do registrador
    public void SalvaRegistrador(string registrador, int valor)
    {
        //Valida para não dexar alterar o $zero
        if(registrador == "$zero")
        {
            return;
        }

        this.Valores[registrador] = valor;
    }

    //Busca o valor do registrador
    public int BuscaRegistrador(string registrador)
    {
        return Valores[registrador];
    }

}
