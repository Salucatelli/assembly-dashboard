// Instructions/AndiInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class AndiInstruction : MipsInstruction
    {
        private int _rtIndex;     // Registrador de destino
        private int _rsIndex;     // Registrador operando
        private int _immediate;   // Valor imediato (como int, mas tratado como unsigned 16-bit)

        public AndiInstruction(string assemblyLine, int address, int rtIndex, int rsIndex, int immediate)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _rtIndex = rtIndex;
            _rsIndex = rsIndex;
            _immediate = immediate;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // 1. Obter o valor do registrador fonte
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);

            // 2. O imediato é tratado como um valor de 16 bits SEM SINAL (zero-extended)
            // Para garantir que apenas os 16 bits sejam considerados e que não haja extensão de sinal de C#,
            // usamos um cast para ushort (unsigned short) e depois para int.
            // Isso efetivamente zero-estende os 16 bits para 32.
            int zeroExtendedImmediate = (int)(ushort)_immediate;

            // 3. Realizar a operação AND lógica bit a bit
            int result = rsValue & zeroExtendedImmediate;

            // 4. Escrever o resultado no registrador de destino
            cpu.bancoDeRegistradores.SetValue(_rtIndex, result);

            // Retorna o próximo PC sequencial
            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            // Formato I-Type: opcode (6) | rs (5) | rt (5) | immediate (16)
            // Opcode para ANDI é 001100 (12 em decimal)
            string opcode = "001100";
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');

            // O imediato para ANDI é sem sinal.
            // Para garantir a representação correta de 16 bits zero-extended,
            // podemos usar (ushort)_immediate e depois convertê-lo para string binária.
            // PadLeft com '0' é sempre o correto aqui.
            string immediateBinary = Convert.ToString((ushort)_immediate, 2).PadLeft(16, '0');

            return $"{opcode}{rsBinary}{rtBinary}{immediateBinary}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.ITypeCycles;
    }
}