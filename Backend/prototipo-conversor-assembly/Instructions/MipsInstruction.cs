// Em Instructions/MipsInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    // Enum para os tipos de instrução MIPS
    public enum MipsInstructionType { R, I, J, Unknown }

    // Classe base abstrata para todas as instruções MIPS
    public abstract class MipsInstruction
    {
        public MipsInstructionType Type { get; protected set; }
        public string AssemblyLine { get; protected set; } // Linha original do código assembly
        public int Address { get; protected set; }         // Endereço da instrução na memória de programa

        protected MipsInstruction(string assemblyLine, int address)
        {
            AssemblyLine = assemblyLine;
            Address = address;
        }

        // Método abstrato para executar a instrução.
        // As classes derivadas devem implementá-lo para modificar o estado da CPU e da memória.
        // O método retorna o próximo PC. Isso permite que jumps/branches alterem o fluxo.
        public abstract int Execute(MipsCPU cpu, MemoryMips dataMemory);

        // Métodos para obter a representação binária e hexadecimal da instrução.
        public abstract string ToBinaryString();
        public abstract string ToHexString();

        // Método para obter os ciclos de clock que esta instrução consome.
        public abstract int GetClockCycles(CpuConfig config);
    }
}
