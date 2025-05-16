using prototipo_conversor_assembly;

BancoRegistradores bancoRegistradores = new BancoRegistradores();

List<string> MemoriaPrograma = new() { "add $s1,1,2" };

ExecucaoMips mips = new(MemoriaPrograma, ref bancoRegistradores);

mips.Executar();

//while (true)
//{
//    Console.WriteLine("Digite uma linha em assembly");

//    string linha = Console.ReadLine().ToString() ?? "";

//    if (!string.IsNullOrWhiteSpace(linha))
//    {
        
//    }
//}