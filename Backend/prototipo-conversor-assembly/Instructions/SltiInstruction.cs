// Instructions/SltiInstruction.cs
using System;
using prototipo_conversor_assembly.Bases; // Mantenha se MipsInstruction estiver em prototipo_conversor_assembly.Bases

namespace prototipo_conversor_assembly
{
    public class SltiInstruction : MipsInstruction
    {
        private int _rtIndex;      // Registrador de destino
        private int _rsIndex;      // Registrador para comparação
        private short _immediate;  // Valor imediato (16 bits com sinal)

        public SltiInstruction(string assemblyLine, int address, int rtIndex, int rsIndex, short immediate)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I; // SLTI é uma instrução do tipo I
            _rtIndex = rtIndex;
            _rsIndex = rsIndex;
            _immediate = immediate;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // Lê o valor do registrador rs
            // Usando seu padrão 'cpu.bancoDeRegistradores.GetValue()'
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);

            int result = 0; // Por padrão, assume que rs não é menor que immediate

            // Compara os valores com sinal: se rsValue < _immediate, o resultado é 1, caso contrário é 0
            if (rsValue < _immediate)
            {
                result = 1;
            }
            // Se rsValue >= _immediate, result permanece 0

            // Escreve o resultado no registrador rt
            // Usando seu padrão 'cpu.bancoDeRegistradores.SetValue()'
            cpu.bancoDeRegistradores.SetValue(_rtIndex, result);

            // Retorna o próximo endereço do PC (instrução tipo I avança PC em 4)
            return Address + 4;
        }

        public override string ToBinaryString()
        {
            // Formato I-Type: Opcode (6) | Rs (5) | Rt (5) | Immediate (16)
            // Opcode para SLTI é 001010 (decimal 10)

            string opcodeBinary = Convert.ToString(0b001010, 2).PadLeft(6, '0'); // Opcode SLTI
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');

            // O imediato é um número com sinal de 16 bits.
            // Convert.ToString((ushort)_immediate, 2) lida com o complemento de dois para valores negativos.
            string immediateBinary = Convert.ToString((ushort)_immediate, 2).PadLeft(16, '0');

            return $"{opcodeBinary}{rsBinary}{rtBinary}{immediateBinary}";
        }

        public override string ToHexString()
        {
            // Converte a string binária para um inteiro sem sinal e depois para hexadecimal
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            // O custo em ciclos de clock para uma instrução do tipo I
            return config.ITypeCycles;
        }
    }
}