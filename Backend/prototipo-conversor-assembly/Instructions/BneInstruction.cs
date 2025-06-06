using System;
using prototipo_conversor_assembly.Bases; 

namespace prototipo_conversor_assembly
{
    public class BneInstruction : MipsInstruction
    {
        private int _rsIndex;
        private int _rtIndex;
        private int _offset; // Offset em palavras (já calculado pelo parser)

        public BneInstruction(string assemblyLine, int address, int rsIndex, int rtIndex, int offset)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I; // BNE é uma instrução do tipo I
            _rsIndex = rsIndex;
            _rtIndex = rtIndex;
            _offset = offset;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // Lê os valores dos registradores rs e rt
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);
            int rtValue = cpu.bancoDeRegistradores.GetValue(_rtIndex);

            // Condição para BNE: ramifica se os valores NÃO forem iguais
            if (rsValue != rtValue)
            {
                // Calcula o novo PC. O offset é em palavras, então multiplicamos por 4 para bytes.
                // O PC é incrementado em 4 (para a próxima instrução sequencial) ANTES da ramificação ser calculada.
                // Ou seja, o novo PC será (endereço da instrução atual + 4) + (offset * 4).
                return Address + 4 + (_offset * 4);
            }
            else
            {
                // Se a condição for falsa (rsValue == rtValue),
                // o PC simplesmente avança para a próxima instrução sequencial.
                return Address + 4;
            }
        }

        public override string ToBinaryString()
        {
            // Implementação para converter a instrução BNE para sua representação binária
            // Formato I-Type: Opcode (6) | Rs (5) | Rt (5) | Immediate (16)
            // Opcode para BNE é 000101 (decimal 5)

            string opcodeBinary = Convert.ToString(0b000101, 2).PadLeft(6, '0'); // Opcode BNE
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');

            // O offset é um número com sinal de 16 bits.
            // É importante tratar o valor negativo corretamente em binário (complemento de dois).
            string offsetBinary = Convert.ToString((ushort)_offset, 2).PadLeft(16, '0');

            return $"{opcodeBinary}{rsBinary}{rtBinary}{offsetBinary}";
        }

        public override string ToHexString()
        {
            // Converte o binário para inteiro sem sinal e depois para hexadecimal
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            // O custo em ciclos de clock para uma instrução de branch
            return config.BranchCycles;
        }
    }
}