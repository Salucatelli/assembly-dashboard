using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices; // Não é mais necessário aqui
using System.Text;
using System.Threading.Tasks;
using System.IO;
using prototipo_conversor_assembly.Bases; // Para carregar arquivo

namespace prototipo_conversor_assembly;

public class MipsCPU // Renomeado de ExecucaoMips
{
    public BancoRegistradores bancoDeRegistradores; // Seu banco de registradores
    public MemoryMips dataMemory; // Novo: memória de dados
    public CpuConfig config; // Novo: configurações da CPU

    public List<MipsInstruction> LoadedInstructions { get; private set; } // Agora armazena objetos de instrução
    public int pc { get; set; } = 0; // Program Counter
    public long TotalClockCycles { get; private set; } // Contador de ciclos

    // Não precisamos mais do Comandos no construtor
    public MipsCPU(BancoRegistradores bancoRegistradores)
    {
        bancoDeRegistradores = bancoRegistradores;
        dataMemory = new MemoryMips(4096); // Ex: 4KB de memória de dados
        config = new CpuConfig(); // Configurações padrão da CPU
        LoadedInstructions = new List<MipsInstruction>();
        TotalClockCycles = 0;
    }

    // Método para carregar e iniciar um novo programa
    public void LoadProgram(string filePath)
    {
        // Reseta o estado da CPU e memória
        bancoDeRegistradores.Reset();
        dataMemory.Reset();
        pc = 0;
        TotalClockCycles = 0;
        LoadedInstructions.Clear();

        // Usa o parser para carregar e transformar o código Assembly em objetos de instrução
        MipsProgramParser parser = new MipsProgramParser(bancoDeRegistradores);
        parser.LoadAndParse(filePath);
        LoadedInstructions = parser.ParsedInstructions;

        // Opcional: ajustar o PC inicial se houver uma label 'main'
        if (parser.Labels.ContainsKey("main"))
        {
            pc = parser.Labels["main"];
        }
    }


    public void ExecutarProgramaCompleto()
    {
        Console.WriteLine("\n==== INICIANDO EXECUÇÃO DO PROGRAMA ====\n");
        while (!IsProgramFinished())
        {
            ExecuteNextInstruction();
            //Console.ReadKey(); // Pausa a cada instrução, útil para depuração
        }
        Console.WriteLine("\n==== EXECUÇÃO DO PROGRAMA FINALIZADA ====\n");
        ExibirResultadosFinais();
    }

    public void ExecuteNextInstruction()
    {
        // Encontra a instrução correta pelo PC
        MipsInstruction currentInstruction = GetInstructionAtPC();

        if (currentInstruction == null)
        {
            Console.WriteLine("Erro: PC fora dos limites do programa ou fim do programa.");
            return;
        }

        Console.WriteLine($"\n============================|PC=0x{pc:X8}|============================\n");
        Console.WriteLine($"Instrução: {currentInstruction.AssemblyLine}");
        Console.WriteLine($"Binário: {currentInstruction.ToBinaryString()}");
        Console.WriteLine($"Hexadecimal: {currentInstruction.ToHexString()}");

        // Executa a instrução, que retorna o NOVO valor do PC
        // Isso permite que JUMP e BRANCH controlem o fluxo
        int nextPC = currentInstruction.Execute(this, dataMemory);

        // Adiciona os ciclos de clock da instrução
        TotalClockCycles += currentInstruction.GetClockCycles(config);

        // Atualiza o PC
        pc = nextPC;

        ExibirRegistradores();
        ExibirMemoriaDados(); // Novo método para exibir memória de dados
        Console.WriteLine($"Tempo total de execução: {CalculateExecutionTime().ToString("F9")} segundos");
        Console.WriteLine("Pressione uma tecla para o próximo ciclo...");
        Console.ReadKey();
    }

    public MipsInstruction GetInstructionAtPC()
    {
        // Opcional: Converter PC para índice na lista se as instruções não estiverem em endereços contínuos
        // Para nossa simulação simplificada, PC / 4 é o índice.
        int instructionIndex = pc / 4;
        if (instructionIndex >= 0 && instructionIndex < LoadedInstructions.Count)
        {
            return LoadedInstructions[instructionIndex];
        }
        return null;
    }

    public bool IsProgramFinished()
    {
        // Se o PC está além do último endereço da última instrução, o programa terminou.
        if (LoadedInstructions.Count == 0) return true;
        return pc >= LoadedInstructions.Last().Address + 4; // Ou pc / 4 >= LoadedInstructions.Count
    }

    public double CalculateExecutionTime()
    {
        if (config.ClockFrequencyMHz <= 0) return 0;
        return (double)TotalClockCycles / (config.ClockFrequencyMHz * 1_000_000); // Em segundos
    }

    public void ExibirRegistradores()
    {
        Console.WriteLine("\n--- REGISTRADORES DE CPU ---");
        foreach (var rName in bancoDeRegistradores.Registradores) // Itera sobre a lista de nomes para ordem
        {
            Console.WriteLine($"\t|{rName} : {bancoDeRegistradores.Valores[rName],-10} | (0x{bancoDeRegistradores.Valores[rName]:X8})");
        }
    }

    public void ExibirMemoriaDados() // Novo método para exibir memória de dados
    {
        Console.WriteLine("\n--- MEMÓRIA DE DADOS (Primeiras 40 bytes) ---");
        // Exibe as primeiras 10 palavras (40 bytes) para ter uma noção
        for (int i = 0; i < 40; i += 4)
        {
            try
            {
                int value = dataMemory.ReadWord(i);
                Console.WriteLine($"\t|Endereço 0x{i:X4}: 0x{value:X8} ({value})|");
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine($"\t|Endereço 0x{i:X4}: Não acessível/Vazio|");
            }
        }
        // TODO: Você pode adicionar lógica para mostrar endereços específicos se souber que foram alterados.
    }

    public void ExibirResultadosFinais()
    {
        Console.WriteLine("\n==== RESULTADOS FINAIS ====\n");
        ExibirRegistradores();
        ExibirMemoriaDados();
        Console.WriteLine($"\nCiclos totais: {TotalClockCycles}");
        Console.WriteLine($"Tempo total de execução: {CalculateExecutionTime().ToString("F9")} segundos");
    }
}