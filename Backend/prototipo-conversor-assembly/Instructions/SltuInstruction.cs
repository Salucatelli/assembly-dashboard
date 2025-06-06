using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly
{
    public class SltuInstruction : MipsInstruction
    {
        private int _rdIndex; // Registrador de destino
        private int _rsIndex; // Primeiro operando (registrador)
        private int _rtIndex; // Segundo operando (registrador)

        public SltuInstruction(string assemblyLine, int address, int rdIndex, int rsIndex, int rtIndex)
            : base(assemblyLine, address) // Chama o construtor da classe base MipsInstruction
        {
            Type = MipsInstructionType.R; // SLTU é uma instrução do tipo R
            _rdIndex = rdIndex;
            _rsIndex = rsIndex;
            _rtIndex = rtIndex;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // Lê os valores dos registradores rs e rt.
            // Eles são lidos como int, mas para a comparação SLTU, devem ser tratados como unsigned.
            // Usando seu padrão 'cpu.bancoDeRegistradores.GetValue()'
            int rsValueSigned = cpu.bancoDeRegistradores.GetValue(_rsIndex);
            int rtValueSigned = cpu.bancoDeRegistradores.GetValue(_rtIndex);

            // Converte para uint para realizar a comparação sem sinal
            uint rsValueUnsigned = (uint)rsValueSigned;
            uint rtValueUnsigned = (uint)rtValueSigned;

            int result = 0; // Por padrão, assume que rs não é menor que rt (unsigned)

            // Compara os valores sem sinal: se rsValueUnsigned < rtValueUnsigned, o resultado é 1, caso contrário é 0
            if (rsValueUnsigned < rtValueUnsigned)
            {
                result = 1;
            }
            // Se rsValueUnsigned >= rtValueUnsigned, result permanece 0

            // Escreve o resultado no registrador rd.
            // Usando seu padrão 'cpu.bancoDeRegistradores.SetValue()'
            cpu.bancoDeRegistradores.SetValue(_rdIndex, result);

            // Retorna o próximo endereço do PC (instrução tipo R avança PC em 4)
            return Address + 4;
        }

        public override string ToBinaryString()
        {
            // Formato R-Type: Opcode (6) | Rs (5) | Rt (5) | Rd (5) | Shamt (5) | Funct (6)
            // Opcode para SLTU (R-Type) é 000000 (0)
            // Shamt para SLTU é 00000 (0)
            // Funct para SLTU é 101011 (decimal 43)

            string opcodeBinary = Convert.ToString(0b000000, 2).PadLeft(6, '0'); // Opcode R-Type
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string rdBinary = Convert.ToString(_rdIndex, 2).PadLeft(5, '0');
            string shamtBinary = Convert.ToString(0b00000, 2).PadLeft(5, '0'); // Shamt é 0 para SLTU
            string functBinary = Convert.ToString(0b101011, 2).PadLeft(6, '0'); // Funct para SLTU

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