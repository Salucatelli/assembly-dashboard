using System;
using prototipo_conversor_assembly.Bases; 

namespace prototipo_conversor_assembly
{
    public class SltInstruction : MipsInstruction
    {
        private int _rdIndex; // Registrador de destino
        private int _rsIndex; // Primeiro operando (registrador)
        private int _rtIndex; // Segundo operando (registrador)

        public SltInstruction(string assemblyLine, int address, int rdIndex, int rsIndex, int rtIndex)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.R; // SLT é uma instrução do tipo R
            _rdIndex = rdIndex;
            _rsIndex = rsIndex;
            _rtIndex = rtIndex;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // Lê os valores dos registradores rs e rt
            // CORREÇÃO: Usando seu padrão 'cpu.bancoDeRegistradores.GetValue()'
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);
            int rtValue = cpu.bancoDeRegistradores.GetValue(_rtIndex);

            int result = 0; // Por padrão, assume que rs não é menor que rt

            // Compara os valores: se rsValue < rtValue, o resultado é 1, caso contrário é 0
            if (rsValue < rtValue)
            {
                result = 1;
            }
            // Se rsValue >= rtValue, result permanece 0

            // Escreve o resultado no registrador rd
            // CORREÇÃO: Usando seu padrão 'cpu.bancoDeRegistradores.SetValue()'
            cpu.bancoDeRegistradores.SetValue(_rdIndex, result);

            // Retorna o próximo endereço do PC (instrução tipo R avança PC em 4)
            return Address + 4;
        }

        public override string ToBinaryString()
        {
            // Formato R-Type: Opcode (6) | Rs (5) | Rt (5) | Rd (5) | Shamt (5) | Funct (6)
            // Opcode para SLT (R-Type) é 000000 (0)
            // Shamt para SLT é 00000 (0)
            // Funct para SLT é 101010 (decimal 42)

            string opcodeBinary = Convert.ToString(0b000000, 2).PadLeft(6, '0'); // Opcode R-Type
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string rdBinary = Convert.ToString(_rdIndex, 2).PadLeft(5, '0');
            string shamtBinary = Convert.ToString(0b00000, 2).PadLeft(5, '0'); // Shamt é 0 para SLT
            string functBinary = Convert.ToString(0b101010, 2).PadLeft(6, '0'); // Funct para SLT

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
            // O custo em ciclos de clock para uma instrução do tipo R
            return config.RTypeCycles;
        }
    }
}