// Instructions/JumpRegisterInstruction.cs
using System;
using prototipo_conversor_assembly.Bases; // Mantenha se MipsInstruction estiver em prototipo_conversor_assembly.Bases

namespace prototipo_conversor_assembly
{
    public class JumpRegisterInstruction : MipsInstruction
    {
        private int _rsIndex; // Registrador que contém o endereço de destino

        public JumpRegisterInstruction(string assemblyLine, int address, int rsIndex)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.R; // JR é uma instrução do tipo R
            _rsIndex = rsIndex;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // Lê o valor do registrador rs, que é o endereço para onde saltar.
            // Usando seu padrão 'cpu.bancoDeRegistradores.GetValue()'
            int targetAddress = cpu.bancoDeRegistradores.GetValue(_rsIndex);

            // Em MIPS real, o endereço de destino de JR deve ser alinhado à palavra (múltiplo de 4).
            // Se não for, pode gerar uma exceção de alinhamento ou comportamento indefinido.
            if (targetAddress % 4 != 0)
            {
                // Em um simulador, podemos lançar uma exceção para avisar sobre o erro.
                throw new Exception($"Erro de alinhamento: Endereço de destino para JR ({targetAddress:X8}) não está alinhado à palavra (múltiplo de 4). Linha: '{AssemblyLine}'");
            }

            // O PC é simplesmente definido para o endereço de destino contido em rs.
            return targetAddress;
        }

        public override string ToBinaryString()
        {
            // Formato R-Type: Opcode (6) | Rs (5) | Rt (5) | Rd (5) | Shamt (5) | Funct (6)
            // Opcode para JR é 000000 (0)
            // Rt, Rd, Shamt são 00000 (0) para JR (não usados)
            // Funct para JR é 001000 (decimal 8)

            string opcodeBinary = Convert.ToString(0b000000, 2).PadLeft(6, '0'); // Opcode R-Type
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(0b00000, 2).PadLeft(5, '0'); // Rt é 0 para JR
            string rdBinary = Convert.ToString(0b00000, 2).PadLeft(5, '0'); // Rd é 0 para JR
            string shamtBinary = Convert.ToString(0b00000, 2).PadLeft(5, '0'); // Shamt é 0 para JR
            string functBinary = Convert.ToString(0b001000, 2).PadLeft(6, '0'); // Funct para JR

            return $"{opcodeBinary}{rsBinary}{rtBinary}{rdBinary}{shamtBinary}{functBinary}";
        }

        public override string ToHexString()
        {
            // Converte a string binária para um inteiro sem sinal e depois para hexadecimal
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            // O custo em ciclos de clock para uma instrução do tipo R (que inclui JR)
            return config.RTypeCycles;
        }
    }
}