using prototipo_conversor_assembly.Bases; 

namespace prototipo_conversor_assembly;

public class MipsCPU 
{
    public BancoRegistradores bancoDeRegistradores; 
    public MemoryMips dataMemory; 
    public CpuConfig config; 

    public List<MipsInstruction> LoadedInstructions { get; private set; } 
    public int pc { get; set; } = 0; 
    public long TotalClockCycles { get; private set; } 

    public MipsCPU(BancoRegistradores bancoRegistradores)
    {
        bancoDeRegistradores = bancoRegistradores;
        dataMemory = new MemoryMips(8192); 
        config = new CpuConfig(); 
        LoadedInstructions = new List<MipsInstruction>();
        TotalClockCycles = 0;
    }

    public void LoadProgram(string filePath)
    {
        bancoDeRegistradores.Reset();
        dataMemory.Reset();
        pc = 0;
        TotalClockCycles = 0;
        LoadedInstructions.Clear();

        MipsProgramParser parser = new MipsProgramParser(bancoDeRegistradores);
        parser.LoadAndParse(filePath);
        LoadedInstructions = parser.ParsedInstructions;

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
        }
        Console.WriteLine("\n==== EXECUÇÃO DO PROGRAMA FINALIZADA ====\n");
        ExibirResultadosFinais();
    }

    public void ExecuteNextInstruction()
    {
        // Encontra a instrução certa pelo PC
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

        int nextPC = currentInstruction.Execute(this, dataMemory);

        // Adiciona os ciclos de clock da instrução
        TotalClockCycles += currentInstruction.GetClockCycles(config);

        // Atualiza o PC
        pc = nextPC;

        ExibirRegistradores();
        ExibirMemoriaDados(); 
        Console.WriteLine($"Tempo total de execução: {CalculateExecutionTime().ToString("F9")} segundos");
        Console.WriteLine("Pressione uma tecla para o próximo ciclo...");
        Console.ReadKey();
        Console.Clear();
    }

    public MipsInstruction GetInstructionAtPC()
    {
        int instructionIndex = pc / 4;
        if (instructionIndex >= 0 && instructionIndex < LoadedInstructions.Count)
        {
            return LoadedInstructions[instructionIndex];
        }
        return null;
    }

    public bool IsProgramFinished()
    {
        if (LoadedInstructions.Count == 0) return true;
        return pc >= LoadedInstructions.Last().Address + 4; 
    }

    public double CalculateExecutionTime()
    {
        if (config.ClockFrequencyMHz <= 0) return 0;
        return (double)TotalClockCycles / (config.ClockFrequencyMHz * 1_000_000); 
    }

    public void ExibirRegistradores()
    {
        Console.WriteLine("\n--- REGISTRADORES DE CPU ---");
        int counter = 1;
        foreach (var rName in bancoDeRegistradores.Registradores) 
        {
            Console.Write($"\t|{rName} : {bancoDeRegistradores.Valores[rName],-10} | (0x{bancoDeRegistradores.Valores[rName]:X8})");
            if(counter % 2 == 0)
            {
                Console.WriteLine("");
            }
            counter++;
        }
    }

    public void ExibirMemoriaDados() 
    {
        Console.WriteLine("\n--- MEMÓRIA DE DADOS (Primeiras 40 bytes) ---");
        
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