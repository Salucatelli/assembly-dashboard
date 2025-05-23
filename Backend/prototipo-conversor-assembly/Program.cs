using prototipo_conversor_assembly;
using System;
using System.IO; // Para usar File.Exists

// Instancia o banco de registradores uma vez
BancoRegistradores bancoRegistradores = new BancoRegistradores();

// Instancia a CPU MIPS
MipsCPU mips = new MipsCPU(bancoRegistradores); // Passamos o bancoRegistradores

// Caminho para o arquivo assembly de teste
string assemblyFilePath = "codigo_mips_teste.txt";

// Cria um arquivo de teste se não existir
if (!File.Exists(assemblyFilePath))
{
    Console.WriteLine($"Criando arquivo de exemplo: {assemblyFilePath}");
    File.WriteAllLines(assemblyFilePath, new[] {
        "addi $s1, $zero, 10      # $s1 = 10",
        "addi $s2, $s1, 4        # $s2 = $s1 + 4 = 14",
        "sub $s2, $s2, $s1       # $s2 = $s2 - $s1 = 14 - 10 = 4",
        "addi $a0, $zero, 1",
        "lw $t0, 0($sp)          # Exemplo de load (SP padrão é 0, pode dar erro se não setar SP)"
    });
}

// Carrega e parseia o programa do arquivo
try
{
    mips.LoadProgram(assemblyFilePath);
    mips.ExecutarProgramaCompleto();
}
catch (Exception ex)
{
    Console.WriteLine($"\nOcorreu um erro durante o carregamento ou execução: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

Console.WriteLine("\nPressione qualquer tecla para sair.");
Console.ReadKey();