using prototipo_conversor_assembly;

BancoRegistradores bancoRegistradores = new BancoRegistradores();

MipsCPU mips = new MipsCPU(bancoRegistradores);

// Caminho para o arquivo assembly de teste
string assemblyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assembly.txt");

// verifica se existe o arquivo
if (!File.Exists(assemblyFilePath))
{
    Console.WriteLine("Arquivo assembly.txt não encontrado...");
}
else
{
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

}

Console.WriteLine("\nPressione qualquer tecla para sair.");
Console.ReadKey();