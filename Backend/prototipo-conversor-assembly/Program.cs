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
        "# Exemplo de código MIPS para testar LW, SW e LH",
        "",
        "# --- Setup Inicial ---",
        "addi $sp, $zero, 4096     # $sp = 4096 (0x1000) - Endereço base para memória de dados",
        "",
        "# --- Teste de SW (Store Word) ---",
        "addi $s1, $zero, 50      # $s1 = 50",
        "sw $s1, 0($sp)           # Armazena o valor de $s1 (50) no endereço ($sp + 0) = 4096.",
        "",
        "# --- Teste de LW (Load Word) ---",
        "addi $s2, $zero, 0       # Limpa $s2 para ter certeza que está 0 antes de carregar",
        "lw $s2, 0($sp)           # Carrega a palavra do endereço ($sp + 0) = 4096 para $s2",
        "",
        "# --- Teste de SW com Offset ---",
        "addi $s3, $zero, 100     # $s3 = 100",
        "sw $s3, 4($sp)           # Armazena o valor de $s3 (100) no endereço ($sp + 4) = 4100.",
        "",
        "# --- Teste de LW com Offset ---",
        "addi $t0, $zero, 0       # Limpa $t0",
        "lw $t0, 4($sp)           # Carrega a palavra do endereço ($sp + 4) = 4100 para $t0",
        "",
        "# --- Teste de LH (Load Halfword) ---",
        "addi $s4, $zero, 0x00FF  # $s4 = 255 (0x00FF)",
        "sw $s4, 8($sp)           # Armazena 0x00FF no endereço ($sp + 8) = 4104.",
        "",
        "addi $t1, $zero, 0       # Limpa $t1",
        "lh $t1, 8($sp)           # Carrega a halfword (2 bytes) do endereço ($sp + 8) = 4104 para $t1",
        "",
        "# --- Teste de LH com valor negativo para extensão de sinal ---",
        "addi $s5, $zero, -128    # $s5 = -128 (0xFFFFFF80 em 32 bits)",
        "sw $s5, 12($sp)          # Armazena -128 no endereço ($sp + 12) = 4108.",
        "",
        "addi $t2, $zero, 0       # Limpa $t2",
        "lh $t2, 12($sp)          # Carrega a halfword (2 bytes) de 4108 para $t2",
        "",
        "# --- Outras instruções de exemplo (mantidas) ---",
        "addi $a0, $zero, 1"
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