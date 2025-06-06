using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace prototipo_conversor_assembly;

public class BancoRegistradores
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
    private Dictionary<string, int> _registerNameToIndex = new(); 

    public BancoRegistradores()
    {
        //Inicializa o valor de todos os registradores como 0
        foreach(var r in Registradores)
        {
            Valores.Add(r, 0);
        }
        InitializeRegisterIndexMap();
    }

    private void InitializeRegisterIndexMap()
    {
        _registerNameToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "$zero", 0 }, { "$0", 0 },
            { "$at", 1 }, { "$1", 1 },
            { "$v0", 2 }, { "$2", 2 },
            { "$v1", 3 }, { "$3", 3 },
            { "$a0", 4 }, { "$4", 4 },
            { "$a1", 5 }, { "$5", 5 },
            { "$a2", 6 }, { "$6", 6 },
            { "$a3", 7 }, { "$7", 7 },
            { "$t0", 8 }, { "$8", 8 },
            { "$t1", 9 }, { "$9", 9 },
            { "$t2", 10 }, { "$10", 10 },
            { "$t3", 11 }, { "$11", 11 },
            { "$t4", 12 }, { "$12", 12 },
            { "$t5", 13 }, { "$13", 13 },
            { "$t6", 14 }, { "$14", 14 },
            { "$t7", 15 }, { "$15", 15 },
            { "$s0", 16 }, { "$16", 16 },
            { "$s1", 17 }, { "$17", 17 },
            { "$s2", 18 }, { "$18", 18 },
            { "$s3", 19 }, { "$19", 19 },
            { "$s4", 20 }, { "$20", 20 },
            { "$s5", 21 }, { "$21", 21 },
            { "$s6", 22 }, { "$22", 22 },
            { "$s7", 23 }, { "$23", 23 },
            { "$t8", 24 }, { "$24", 24 },
            { "$t9", 25 }, { "$25", 25 },
            { "$k0", 26 }, { "$26", 26 },
            { "$k1", 27 }, { "$27", 27 },
            { "$gp", 28 }, { "$28", 28 },
            { "$sp", 29 }, { "$29", 29 },
            { "$fp", 30 }, { "$30", 30 },
            { "$ra", 31 }, { "$31", 31 }
        };
    }

    public int GetRegisterIndex(string regName)
    {
        if (_registerNameToIndex.TryGetValue(regName, out int index))
        {
            return index;
        }
        throw new ArgumentException($"Nome de registrador desconhecido: {regName}");
    }

    //altera o valor do registrador
    public void SetValue(int index, int valor)
    {
        //Valida para não dexar alterar o $zero
        if(index == 0)
        {
            return;
        }

        var registrador = Registradores[index];
        this.Valores[registrador] = valor;
    }

    //Busca o valor do registrador
    public int GetValue(int index)
    {
        var registrador = Registradores[index];
        return Valores[registrador];
    }

    public void Reset()
    {
        foreach (var key in Valores.Keys.ToList()) // ToList para evitar modificação durante iteração
        {
            Valores[key] = 0;
        }
    }

}
