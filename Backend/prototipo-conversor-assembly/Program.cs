using prototipo_conversor_assembly;
using System;
using System.IO; // Para usar File.Exists

// Instancia o banco de registradores uma vez
BancoRegistradores bancoRegistradores = new BancoRegistradores();

// Instancia a CPU MIPS
MipsCPU mips = new MipsCPU(bancoRegistradores); // Passamos o bancoRegistradores

// Caminho para o arquivo assembly de teste
string assemblyFilePath = "D:\\Prog\\Faculdade\\Arquitetura de Computadores\\assembly-dashboard\\Backend\\prototipo-conversor-assembly\\assembly.txt";

// Cria um arquivo de teste se não existir
if (!File.Exists(assemblyFilePath))
{
    Console.WriteLine($"Criando arquivo de exemplo: {assemblyFilePath}");
    File.WriteAllLines(assemblyFilePath, new[] {
         "# Código MIPS para testar a instrução LW (Load Word)",
        "",
        "# --- Setup Inicial ---",
        "addi $sp, $zero, 4096     # $sp = 4096 (0x1000)",
        "",
        "# --- Armazenando um valor na memória para ser carregado ---",
        "addi $s0, $zero, 12345    # $s0 = 12345",
        "sw $s0, 0($sp)            # Memória[4096] = 12345",
        "",
        "# --- Teste da instrução LW ---",
        "addi $t0, $zero, 0        # $t0 = 0",
        "lw $t0, 0($sp)            # $t0 = Memória[4096] (Esperado: $t0 = 12345)",
        "",
        "# --- Teste de LW com offset ---",
        "addi $s1, $zero, 54321    # $s1 = 54321",
        "sw $s1, 4($sp)            # Memória[4100] = 54321",
        "",
        "addi $t1, $zero, 0        # $t1 = 0",
        "lw $t1, 4($sp)            # $t1 = Memória[4100] (Esperado: $t1 = 54321)",
        "",
        "# Fim do programa"
        // Fim do novo código assembly

        //"addi $s1, $zero, 10      # $s1 = 10",
        //"addi $s2, $s1, 4        # $s2 = $s1 + 4 = 14",
        //"sub $s2, $s2, $s1       # $s2 = $s2 - $s1 = 14 - 10 = 4",
        //"addi $a0, $zero, 1",
        //"lw $t0, 0($sp)          # Exemplo de load (SP padrão é 0, pode dar erro se não setar SP)"
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